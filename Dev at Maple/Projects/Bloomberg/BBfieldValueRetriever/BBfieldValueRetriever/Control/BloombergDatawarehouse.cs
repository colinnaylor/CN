using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using BBfieldValueRetriever.Model;
using Shared;
using Maple;
namespace BBfieldValueRetriever.Control
{
    public class BloombergDatasourceController
    {
        protected Database Db;
        protected BergController MainController;
        protected string Datasource = "BloombergDatasourceController";

        public virtual void CostReportUserAttribution(List<RequestItem> requestItems)
        {
            //cost reporting actuals
            var sql = new StringBuilder();
            //cost reporting attribution
            //userid, datasource, ticker, ticker asset class, field, field pricing cat
            foreach (RequestItem ri in requestItems)
            {
                foreach (DateTime key in ri.Data.Keys.ToList())
                {
                    foreach (var field in Static.SplitWithStringDelimeters(ri.BBFieldList, ',', '[', ']'))
                    {
                        var fieldPriceCat = MainController.FieldPricingCategories.ContainsKey(field.Trim()) ? MainController.FieldPricingCategories[field.Trim()] : "Unknown";
                        try
                        {
                            sql.Clear();
                            sql.Append("INSERT INTO [dbo].[CostReportByConsumer] ");
                            sql.Append(" ([RetrieveTime] ");
                            sql.Append(" ,[UserId] ");
                            sql.Append(" ,[Datasource] ");
                            sql.Append(" ,[Ticker] ");
                            sql.Append(" ,[dl_asset_class] ");
                            sql.Append(" ,[Field] ");
                            sql.Append(" ,[FieldPriceCategory] ");
                            sql.Append(" ,[FieldSourcedFromPerSec] ");
                            sql.Append(" ,[Value]) ");
                            sql.Append(" VALUES ");
                            sql.AppendFormat(" ('{0:yyyy-MMM-dd HH:mm:ss}'", DateTime.Now);
                            sql.AppendFormat(" ,'{0}' ", ri.UserId);
                            sql.AppendFormat(" ,'{0}' ", Datasource);
                            sql.AppendFormat(" ,'{0}' ", ri.OriginalInputTicker);
                            sql.AppendFormat(" ,'{0}' ", ri.TickerDownloadAssetClass);
                            sql.AppendFormat(" ,'{0}' ", field.Trim().Substring(0, Math.Min(field.Trim().Length, 100)));
                            sql.AppendFormat(" ,'{0}' ", fieldPriceCat);
                            sql.Append(" ,1");
                            sql.AppendFormat(" ,'{0}')", string.Join("|", ri.Data[key]).Replace("'", "''"));

                            Utils.DbController.ExecuteNonQuery(sql.ToString());
                        }
                        catch (Exception ex)
                        {
                            NLogger.Instance.Error("Error in writing to table CostReportByConsumer {0}", ex.ToString());
                        }
                    }
                }
            }
        }
    }

    public class BloombergDatawarehouseController : BloombergDatasourceController
    {
        public BloombergDatawarehouseController(BergController mainController)
        {
            Db = mainController.Db;
            this.MainController = mainController;
            Datasource = "Warehouse";
        }

        private bool FieldWasRequestedFromPerSecurityLicense(string field, BloombergDatawarehouseData data)
        {
            var cat = MainController.FieldPricingCategories[field.Trim()];
            if (cat == "Packaged" || cat == "SecurityMaster" || cat == "OpenSource")
                if (data.REQ_DATA_SECURITY_MASTER == "Y")
                    return true;

            if (cat == "CreditRisk")
                if (data.REQ_DATA_CREDIT_RISK == "Y")
                    return true;

            if (cat == "Estimates")
                if (data.REQ_DATA_ESTIMATES == "Y")
                    return true;

            if (cat == "Pricing-Intraday")
                if (data.REQ_DATA_PRICING_INTRADAY == "Y")
                    return true;

            if (cat == "Pricing-EndofDay")
                if (data.REQ_DATA_PRICING_EOD == "Y")
                    return true;

            if (cat == "Derived-Intraday")
                if (data.REQ_DATA_DERIVED_INTRADAY == "Y")
                    return true;

            if (cat == "Derived-EndofDay")
                if (data.REQ_DATA_DERIVED_EOD == "Y")
                    return true;

            return false;
        }

        public override void CostReportUserAttribution(List<RequestItem> requestItems)
        {
            //cost reporting actuals

            //cost reporting attribution
            //userid, datasource, ticker, ticker asset class, field, field pricing cat
            StringBuilder sql = new StringBuilder();

            foreach (RequestItem ri in requestItems)
            {
                foreach (DateTime key in ri.Data.Keys.ToList())
                {
                    foreach (var field in Static.SplitWithStringDelimeters(ri.BBFieldList, ',', '[', ']'))
                    {
                        try
                        {
                            sql.Clear();
                            sql.Append("INSERT INTO [dbo].[CostReportByConsumer] ");
                            sql.Append(" ([RetrieveTime] ");
                            sql.Append(" ,[UserId] ");
                            sql.Append(" ,[Datasource] ");
                            sql.Append(" ,[Ticker] ");
                            sql.Append(" ,[dl_asset_class] ");
                            sql.Append(" ,[Field] ");
                            sql.Append(" ,[FieldPriceCategory] ");
                            sql.Append(" ,[FieldSourcedFromPerSec] ");
                            sql.Append(" ,[Value]) ");
                            sql.Append(" VALUES ");
                            sql.AppendFormat(" ('{0:yyyy-MMM-dd HH:mm:ss}'", ri.WarehouseData.EFFECTIVE_DATE);
                            sql.AppendFormat(" ,'{0}' ", ri.UserId);
                            sql.AppendFormat(" ,'{0}' ", Datasource);
                            sql.AppendFormat(" ,'{0}' ", ri.OriginalInputTicker);
                            sql.AppendFormat(" ,'{0}' ", ri.TickerDownloadAssetClass);
                            sql.AppendFormat(" ,'{0}' ", field.Trim().Substring(0, Math.Min(field.Trim().Length, 100)));
                            sql.AppendFormat(" ,'{0}' ", MainController.FieldPricingCategories[field.Trim()]);
                            sql.AppendFormat(" ,{0} ", Convert.ToInt16(FieldWasRequestedFromPerSecurityLicense(field, ri.WarehouseData)));
                            sql.AppendFormat(" ,'{0}')", string.Join("|", ri.Data[key]));

                            Utils.DbController.ExecuteNonQuery(sql.ToString());
                        }
                        catch (Exception ex)
                        {
                            NLogger.Instance.Error("Error in writing to table CostReportByConsumer {0}", ex.ToString());
                        }
                    }
                }
            }
        }

        public virtual void ProcessDataRequests(IEnumerable<RequestItem> requestItems)
        {
            int tickerCount = 0;
            int tickerCountHits = 0;
            int fieldCount = 0;
            int fieldCountHits = 0;
            foreach (var item in requestItems)
            {
                tickerCount++;
                //calendar holidays
                if (item.BBFieldList.StartsWith("CALENDAR_NON_SETTLEMENT_DATES["))
                {
                    var list = Db.GetCalendarNonSetttlementDates(new CalendarNonSettlementDateRequest(item.BBFieldList));
                    var listString = new List<string>();
                    foreach (var thisDate in list) listString.Add(thisDate.ToString("yyyy-MM-dd"));
                    var returnedValuesFromDatawarehouse = new List<string> { string.Concat(string.Join(";", listString.ToArray()), ";") };
                    item.Data.Add(DateTime.Now, returnedValuesFromDatawarehouse.ToArray());
                }
                else
                {
                    var t = GetDataFromDatawarehouse(item);
                    item.WarehouseData = t;
                    if (t == null)
                        item.Errors += "Ticker not found in warehouse";
                    else
                    {
                        item.TickerDownloadAssetClass = t.DL_ASSET_CLASS;
                        if (!t.TickerFoundInBackOfficeFiles && !t.TickerFoundInPerSecurityPull)
                            item.Errors += "Ticker not found in Bloomberg per security pull or overnight back office files.";
                        else if (t.BLOOMBERGERRORCODE != null && !t.BLOOMBERGERRORCODE.Equals("0") && !t.BLOOMBERGERRORCODE.Equals("10"))
                            item.Errors += string.Format("Bloomberg returned an error code: {0}", t.BLOOMBERGERRORCODE);
                        else
                        {
                            tickerCountHits++;
                            var returnedValuesFromDatawarehouse = new List<string>();
                            foreach (var field in item.riFields)
                            {
                                fieldCount++;
                                var cleanField = field.Key.ToUpper().Trim();
                                FieldInfo myf = t.GetType().GetField(cleanField);

                                if (myf != null && myf.GetValue(t) != null)
                                {
                                    fieldCountHits++;
                                    var returnedValue = myf.GetValue(t).ToString();

                                    //Clean values
                                    returnedValue = Static.CleanValueReturnedFromBloomberg(item.OriginalInputTicker, cleanField, returnedValue);

                                    //dont write N.A.
                                    if (returnedValue.Equals("N.A.") || returnedValue.Equals("N.S.") || returnedValue.Trim().Equals(string.Empty))
                                    {
                                        returnedValuesFromDatawarehouse.Add(null);
                                        item.Errors += string.Format("[{0}|cache returned {1}]", field.Key, returnedValue.Trim().Equals(string.Empty) ? "blank string" : returnedValue);
                                    }
                                    else
                                        returnedValuesFromDatawarehouse.Add(returnedValue);
                                }
                                else
                                {
                                    returnedValuesFromDatawarehouse.Add(null);
                                    item.Errors += string.Format("[{0}|{1}]", field.Key, "field not found in cache");
                                }
                            }

                            item.Data.Add(DateTime.Now, returnedValuesFromDatawarehouse.ToArray());
                        }
                    }
                    CostReportUserAttribution(new List<RequestItem> { item });
                }
                //save
                Db.SaveValues(new List<RequestItem> { item });
            }

            NLogger.Instance.Info("Hit rate: Tickers: " + tickerCountHits + "/" + tickerCount + " Fields: " + fieldCountHits + "/" + fieldCount);
        }

        public BloombergDatawarehouseData GetDataFromDatawarehouse(RequestItem item)
        {
            var bergMoniker = item.OriginalInputTicker;
            string sql;
            if (item.RequestType.Equals(BloombergDataInstrument.eRequestType.Historic) && item.DateFrom != null)
                sql = File.ReadAllText("Control\\GetBloombergDataFromDatawarehouse.Historic.sql").Replace("{Berg_Moniker}", bergMoniker).Replace("{date}", item.DateFrom.ToString("ddMMMyyyy"));
            else
                sql = File.ReadAllText("Control\\GetBloombergDataFromDatawarehouse.sql").Replace("{Berg_Moniker}", bergMoniker);

            return Utils.DbController.GetFirstObject<BloombergDatawarehouseData>(sql);
        }
    }
}