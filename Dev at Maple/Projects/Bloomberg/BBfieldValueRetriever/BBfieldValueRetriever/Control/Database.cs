using System;
using System.Collections.Generic;
using System.Text;
using BBfieldValueRetriever.Model;
using BBfieldValueRetriever.Properties;
using Maple;
namespace BBfieldValueRetriever.Control
{
    public class Database
    {
        public string GetDsn()
        {
            return Settings.Default.DSN;
        }

        public List<RequestItemRoutingRule> GetRoutingRules()
        {
            return Utils.DbController.GetObjects<RequestItemRoutingRule>("SELECT [Datasource],[UserIdMatchRegex],[FieldListMatchRegex] FROM [dbo].[BloombergRoutingTable] order by Applyorder");
        }

        public Dictionary<string, string> GetFieldPricingCategories()
        {
            var sql = "SELECT field,datalicensecategory FROM BloombergFieldCategoryMapping";
            var ret = Utils.DbController.GetListOfKeyValuePairs<string, string>(sql);
            var retDict = new Dictionary<string, string>();
            foreach (var item in ret)
            {
                if (!retDict.ContainsKey(item.Key))
                    retDict.Add(item.Key, item.Value);
            }

            return retDict;
        }

        public List<DateTime> GetCalendarNonSetttlementDates(CalendarNonSettlementDateRequest request)
        {
            var sql = string.Format("select distinct holidaydate from [HELIUM].[marketdata].[dbo].ve_Holiday where [holiday description] = '{2}' and holidaydate between '{0:ddMMMyyyy}' and '{1:ddMMMyyyy}' order by 1", request.CalendarStartDate, request.CalendarEndDate, new TorontoViewController().MapBloombergCalendarCodeToTorontoHolidayDescription(request.SettlementCalendarCode));
            var ret = Utils.DbController.GetList<DateTime>(sql);
            return ret;
        }

        public List<RequestItem> GetTickerItemsToProcess(string sql)
        {
            //The sql needs to return request items with columns same name as the RequestItem members
            List<RequestItem> requestItems = Utils.DbController.GetObjects<RequestItem>(sql);

            requestItems.ForEach(x =>
            {
                x.Errors = "";
                x.OriginalInputTicker = x.BBTicker;
                // check for sedol ticker which must be in the correct format
                if (x.BBTicker.EndsWith("SEDOL1"))
                    x.BBTicker = @"/SEDOL1/" + x.BBTicker.Replace(" SEDOL1", string.Empty);

                BloombergApiController.AddFieldFromFieldList(x);
                BloombergApiController.ValidateRequest(x);
            });

            return requestItems;
        }

        public void SaveValues(List<RequestItem> requestItems)
        {
            string header = "<DataValues>";
            string footer = "</DataValues>";

            int maxColumnCount = 0;

            foreach (RequestItem ri in requestItems)
            {
                StringBuilder xml = new StringBuilder();
                xml.AppendLine(header);

                bool handled = false;

                // Special handling for index weights that return many results in just one field.
                if (ri.BBTicker.ToLower().EndsWith(" index") && ri.BBFieldList.ToUpper() == "INDX_MWEIGHT" && ri.Data.Keys.Count == 1)
                {
                    foreach (DateTime key in ri.Data.Keys)
                    {
                        string[] data = ri.Data[key];
                        try
                        {
                            //INDX_MWEIGHT return by BLAPI format
                            string[] items = data[0].Split(new[] { '{', '}' }, StringSplitOptions.RemoveEmptyEntries);
                            if (items.Length > 1)
                            {
                                // Looks like it was in the format expected.
                                for (int c = 1; c <= items.Length; c++)
                                {
                                    StringBuilder line = new StringBuilder();
                                    line.Append("<DataValue DataTime=\"" + key.ToString("yyyyMMdd HH:mm:ss") + "\" ");

                                    string col = "c1";
                                    line.Append(col + "=\"" + items[c - 1] + "\" ");

                                    line.Append("/>");

                                    xml.AppendLine(line.ToString());
                                }

                                handled = true;
                            }
                            else
                            {
                                items = data[0].Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                                if (items.Length > 1)
                                {
                                    // Looks like it was in the format expected.
                                    for (int c = 4; c < items.Length; c = c + 4)
                                    {
                                        StringBuilder line = new StringBuilder();
                                        line.Append("<DataValue DataTime=\"" + key.ToString("yyyyMMdd HH:mm:ss") + "\" ");

                                        string col = "c1";
                                        line.Append(col + "=\"" + items[c] + ";" + items[c + 2] + ";\"");

                                        line.Append("/>");

                                        xml.AppendLine(line.ToString());
                                    }

                                    handled = true;
                                }
                            }
                        }
                        catch (NullReferenceException)
                        {
                            //data, or an element in data is null - do nothing
                        }
                    }
                }

                if (!handled)
                {
                    foreach (DateTime key in ri.Data.Keys)
                    {
                        string[] data = ri.Data[key];
                        // ColumnCount is used just in case we exceed the number of columns available in the db
                        if (data.Length > maxColumnCount) { maxColumnCount = data.Length; }

                        StringBuilder line = new StringBuilder();
                        line.Append("<DataValue DataTime=\"" + key.ToString("yyyyMMdd HH:mm:ss") + "\" ");

                        for (int c = 1; c <= data.Length; c++)
                        {
                            string attributeString = string.Format("c{0}=\"{1}\" ", c, data[c - 1]);
                            if (data[c - 1] != null)
                            {
                                line.Append(attributeString);
                            }
                        }

                        line.Append("/>");

                        xml.AppendLine(line.ToString());
                    }
                }

                xml.AppendLine(footer);

                xml = xml.Replace("&", "&amp;");
                xml = xml.Replace("'", "&apos;");

                string sql = string.Format("EXEC BloombergDataResultInsert {0}, '{1}', {2}, '{3}'",
                    ri.ID, xml, maxColumnCount, ri.Errors);

                Utils.DbController.ExecuteNonQuery(sql);
            }

            NLogger.Instance.Info("Process completed");
        }

        internal void ReadSettings(out int hitsWarningLevel, out int hitsLimit, out int hitsToday)
        {
            hitsWarningLevel = int.Parse(Settings.Default.hitsWarningLevel);
            hitsLimit = int.Parse(Settings.Default.hitsLimit);
            hitsToday = 0;

            string sql = string.Format("SELECT isnull(sum(Hits),0) FROM BloombergDataHits " + "WHERE Machine = '{0}' AND Inserted > '{1}'", Environment.MachineName, DateTime.Now.ToString("yyyyMMdd"));
            hitsToday = Utils.DbController.GetScalar<int>(sql);

            sql = string.Format("SELECT top 1 HitsWarning, MaxHits FROM BloombergDataControl c " + "WHERE c.Machine = '{0}' and Environment = '{1}'", Environment.MachineName, Settings.Default.EnvName);

            var ret = Utils.DbController.GetListOfKeyValuePairs<int, int>(sql);
            if (ret.Count > 0)
            {
                hitsWarningLevel = ret[0].Key;
                hitsLimit = ret[0].Value;
            }
        }

        internal void RecordHits(int hits)
        {
            string sql = string.Format("INSERT BloombergDataHits(Machine, Hits) SELECT '{0}', {1}", Environment.MachineName, hits);
            Utils.DbController.ExecuteNonQuery(sql);
        }
    }
}