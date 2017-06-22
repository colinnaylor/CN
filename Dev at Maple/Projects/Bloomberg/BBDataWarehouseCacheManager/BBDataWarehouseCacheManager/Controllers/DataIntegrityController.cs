using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Maple.Database;

namespace BBDataWarehouseCacheManager.Controllers
{
    public class DataIntegrityController
    {
        public bool CheckDataIntegrityByConsumer(string consumerName, DateTime effDate, double maxAcceptableErrorRate)
        {
            //get ticker field list by consumer based on historical usage

            var list = Utils.DbController.GetListOfKeyValuePairs<string, string>(string.Format("selecT ticker, field from (selecT dense_rank() over(partition by userid order by retrievetime desc) as rank ,  * from  CostReportByConsumer  where retrievetime between DATEADD(day,-5 ,'{0:ddMMMyy}') and '{0:ddMMMyy}' and datasource = 'warehouse' and UserId = '{1}') a where rank = 1", effDate, consumerName));

            var sqls = list.Select(item => string.Format("select {0} from BloombergDataWarehouse where berg_moniker='{1}' and EFFECTIVE_DATE = '{2:ddMMMyy}'", item.Value, item.Key, effDate)).ToList();

            var errorRate = GetErrorRateOfFields(sqls);
            Utils.Logger.Info("Bad values rate: {0} : {1:P1}.", consumerName, errorRate);
            if (!(errorRate > maxAcceptableErrorRate)) return true;
            var msg = string.Format("There is a high percentage of bad values ({0:P1}) in the fields required by consumer: {1} (Tolerance {2:P1}). Please check warehouse datasources. (Effective date: {3:ddd dd MMM yyyy})", errorRate, consumerName, maxAcceptableErrorRate, effDate);
            Utils.Logger.Fatal(msg);

            EmailUsingLegacyDatabaseTable(msg);

            return false;
        }

        public bool CheckDataIntegrityForEntireWarehouse(DateTime thisDate, double maxAcceptableErrorRate)
        {
            var list = Utils.DbController.GetListOfKeyValuePairs<string, string>(string.Format("select berg_moniker,REQ_DATA_FIELD_LIST from BloombergDataWarehouse where EFFECTIVE_DATE = '{0:ddMMMyyy}'", thisDate));

            var sqls = (from item in list where !string.IsNullOrWhiteSpace(item.Value) && !string.IsNullOrWhiteSpace(item.Key) select string.Format("select {0} from BloombergDataWarehouse where berg_moniker='{1}' and EFFECTIVE_DATE = '{2:ddMMMyy}'", item.Value, item.Key, thisDate)).ToList();

            var errorRate = GetErrorRateOfFields(sqls);
            Utils.Logger.Info("Bad values rate: Whole warehouse : {0:P1}.", errorRate);

            if (errorRate > maxAcceptableErrorRate)
            {
                var msg = string.Format("There is a high percentage of bad values ({0:P1}) in the warehouse in general (Tolerance {1:P1}). Please check warehouse datasources. (Effective date: {2:ddd dd MMM yyyy})", errorRate, maxAcceptableErrorRate, thisDate);
                Utils.Logger.Fatal(msg);

                EmailUsingLegacyDatabaseTable(msg);

                return false;
            }
            return true;
        }

        public bool CheckDataIntegrityForLocalCacheOfTorontoViews(DateTime thisDate, double maxAcceptableErrorRate)
        {
            foreach (var pricingView in new List<string> { "TorViewBloombergPerSecurityPull" })
            //foreach (var pricingView in new DatawarehouseManager().GetRequiredLocalCachedPricingViews())
            {
                var errorRate = GetErrorRateOfFields(new List<string> { string.Format("SELECT * FROM {0} WHERE effectivedate='{1:ddMMMyyyy}'", pricingView, thisDate) });
                Utils.Logger.Info("Bad values rate: Local cache of Toronto View ({0}): {1:P1}.", pricingView, errorRate);

                if (errorRate > maxAcceptableErrorRate)
                {
                    var msg = string.Format("There is a high percentage of bad values ({0:P1}) in the London cached copy of Toronto View ({1}) (Tolerance {2:P1}). Please check Toronto data!. (Effective date: {3:ddd dd MMM yyyy})", errorRate, pricingView, maxAcceptableErrorRate, thisDate);
                    Utils.Logger.Fatal(msg);
                    //                    EmailUsingLegacyDatabaseTable(msg);
                }
            }
            return true;
        }

        private static void EmailUsingLegacyDatabaseTable(string msg)
        {
            //emailing using Maple legacy database queue
            var paramList = new List<SqlParameter>();
            paramList.Add(new SqlParameter("@To", SqlDbType.VarChar) { Value = ConfigurationManager.AppSettings["To"] });
            paramList.Add(new SqlParameter("@FromAddress", SqlDbType.VarChar) { Value = ConfigurationManager.AppSettings["FromAddress"] });
            paramList.Add(new SqlParameter("@Subject", SqlDbType.VarChar) { Value = string.Format("Berg ({0}) data integrity check breached ({1})", ConfigurationManager.AppSettings["EnvName"], Environment.MachineName) });
            paramList.Add(new SqlParameter("@Body", SqlDbType.VarChar) { Value = msg });
            var connectionString = ConfigurationManager.ConnectionStrings["LegacyEmailConnectionString"].ToString();
            new DatabaseController(connectionString).GetScalarFromStoredProc<int>("InsertEmail", paramList);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="sqls">list of sqls to execute</param>
        /// <returns>rate of nulls and nas versus total fields</returns>
        private double GetErrorRateOfFields(IEnumerable<string> sqls)
        {
            double fails = 0;
            double total = 0;
            var connectionString = ConfigurationManager.ConnectionStrings["BloombergConnectionString"].ToString();
            foreach (var sql in sqls)
            {
                using (var sqlConn = new SqlConnection(connectionString))
                {
                    using (var sqlCommand = new SqlCommand(sql, sqlConn))
                    {
                        sqlConn.Open();

                        using (SqlDataReader rdr = sqlCommand.ExecuteReader())
                        {
                            while (rdr.Read())
                            {
                                for (int i = 0; i < rdr.FieldCount; i++)
                                {
                                    total += 1;
                                    if ((rdr[i] == null) || (rdr[i] == DBNull.Value) || rdr[i].ToString().Trim().Equals("N.A."))
                                    {
                                        fails += 1;
                                        Utils.Logger.Info("\t\tFound nulls/nas: {0} , {1}", sql, rdr[0]);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            Utils.Logger.Info("GetErrorRateOfFields: {0} bad values out of possible {1}", fails, total);

            return fails / total;
        }
    }
}