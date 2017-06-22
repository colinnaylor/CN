using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using BBDataWarehouseCacheManager.Models;

namespace BBDataWarehouseCacheManager.Controllers
{
    public class DatawarehouseManager
    {
        private readonly TorontoViewManager _torontoController = new TorontoViewManager();

        /// <summary>
        /// mapping of field names to their Bloomberg pricing category
        /// </summary>
        public Dictionary<string, string> BloombergFieldPricingCategoryMapping { get; set; }

        public DatawarehouseManager()
        {
            //fill this BloombergFieldPricingCategoryMapping
            BloombergFieldPricingCategoryMapping = Utils.DbController.ExecuteReaderDictionary(
                "select distinct field,DataLicenseCategory from bloombergFieldCategoryMapping"
                              );
        }

        public void RunCostReportActual(DateTime effectiveDate)
        {
            //run cost report

            var listWarehouseCalls = Utils.DbController.GetObjects<BloombergDatawarehouseData>(string.Format(File.ReadAllText("sql\\CostReportActual.sql"), effectiveDate));

            foreach (var item in listWarehouseCalls)
            {
                var logentry = string.Format("INSERT INTO CostReportActual (RetrieveTime,Datasource,Ticker,dl_asset_class,FieldPriceCategory) VALUES ('{0:ddMMMyyyy}','Warehouse','{1}','{2}','{3}');",
                      item.Effective_Date,
                      item.Berg_Moniker,
                      item.DL_Asset_Class,
                      item.DataLicense);

                Utils.Logger.Info(logentry);
                Utils.DbController.ExecuteNonQuery(logentry);
            }
        }

        public void InsertTickersIntoDatawarehouse(Dictionary<string, string> tickersWithFields, DateTime positionDate)
        {
            Utils.DbController.ExecuteNonQuery(String.Format("delete bloombergdatawarehouse where effective_date = '{0:ddMMMyyyy}';", positionDate));
            foreach (var ticker in tickersWithFields.Keys)
            {
                StringBuilder varname1 = new StringBuilder();
                varname1.Append("INSERT BloombergDataWarehouse \n");
                varname1.Append("       (BERG_MONIKER, \n");
                varname1.Append("        effective_date, \n");
                varname1.Append("        INSERTEDWHEN, \n");
                varname1.Append("        REQ_DATA_FIELD_LIST) \n");
                varname1.Append("VALUES('{0}', \n");
                varname1.Append("       '{1:ddMMMyyyy}', \n");
                varname1.Append("       Getdate(), \n");
                varname1.Append("       '{2}');");

                Utils.DbController.ExecuteNonQuery(String.Format(varname1.ToString(),
                                        ticker,
                                        positionDate,
                                        tickersWithFields[ticker]));
            }
        }

        /// <summary>
        /// generate same list of tickers as the berg consumers.
        /// </summary>
        /// <param name="T"></param>
        /// <returns></returns>
        public Dictionary<string, string> GetLiveTickersUsingBergConsumerSqls(DateTime T)
        {
            var sql = new StringBuilder();
            sql.Append(File.ReadAllText("Sql\\GetTickerListUsingBergConsumerSqls.sql"));
            sql.Replace("{Position_Date}", T.ToString("ddMMMyyyy"));

            var ret = Utils.DbController.ExecuteReaderDictionary(sql.ToString());

            //log
            foreach (var ticker in ret)
                Utils.Logger.Info("Received ticker: {0}", ticker.ToString());
            return ret;
        }

        public void CategoriseBergTickersIntoTorontoAssetTypes(DateTime effectiveDate)
        {
            var mgr = new TorontoViewManager();
            var tickers = mgr.GetBergMonikersFromDataWarehouse(effectiveDate);
            foreach (var ticker in tickers)
            {
                var thisBergId = new BergTicker(ticker.Key);
                var fieldList = ticker.Value;
                var sql = new StringBuilder();
                sql.AppendFormat("update bloombergdatawarehouse set tor_longtype = '{0}',", thisBergId.TorLongType);
                sql.AppendFormat("tor_moniker_lookup = '{0}',", thisBergId.TorTickerLookup);
                sql.AppendFormat("berg_id_type = '{0}',", thisBergId.BergIdType);
                sql.AppendFormat("TOR_ID = '{0}',", thisBergId.TorId);
                sql.AppendFormat("TOR_pulltype = '{0}',", thisBergId.TorPullType);
                sql.AppendFormat("tor_moniker_lookup2 = '{0}',", thisBergId.TorTickerLookup2);
                sql.AppendFormat("REQ_DATA_SECURITY_MASTER = '{0}',", RequiresBloombergLicenseSecurityMaster(fieldList, thisBergId.TorPullType == "E") ? "Y" : "N");
                sql.AppendFormat("REQ_DATA_DERIVED_INTRADAY= '{0}',", RequiresBloombergLicenseDerivedDataIntraday(fieldList) ? "Y" : "N");
                sql.AppendFormat("REQ_DATA_DERIVED_EOD     = '{0}',", RequiresBloombergLicenseDerivedDataEod(fieldList) ? "Y" : "N");
                sql.AppendFormat("REQ_DATA_PRICING_INTRADAY= '{0}',", RequiresBloombergLicensePricingDataIntraday(fieldList, thisBergId.TorPullType == "E") ? "Y" : "N");
                sql.AppendFormat("REQ_DATA_PRICING_EOD     = '{0}',", RequiresBloombergLicensePricingDataEod(fieldList) ? "Y" : "N");
                sql.AppendFormat("REQ_DATA_ESTIMATES       = '{0}',", RequiresBloombergLicenseEstimates(fieldList) ? "Y" : "N");
                sql.AppendFormat("REQ_DATA_CREDIT_RISK     = '{0}'", RequiresBloombergLicenseCreditRisk(fieldList) ? "Y" : "N");
                sql.AppendFormat(" where berg_moniker = '{0}' and effective_date='{1:ddMMMyyyy}'", ticker.Key, effectiveDate);

                Utils.DbController.ExecuteNonQuery(sql.ToString());
            }
        }

        ///  <summary>
        /// 
        ///  </summary>
        ///  <param name="listOfFields">comma delimited field names</param>
        /// <param name="forEquity"></param>
        /// <returns></returns>
        public bool RequiresBloombergLicenseSecurityMaster(string listOfFields, bool forEquity)
        {
            if (forEquity)
                return RequiresBloombergPricingCategory(listOfFields, "SecurityMaster", _torontoController.getFieldsForVe_BloombergDescription());
            return RequiresBloombergPricingCategory(listOfFields, "SecurityMaster", new List<string>());
        }

        public bool RequiresBloombergLicenseDerivedDataIntraday(string listOfFields)
        {
            return RequiresBloombergPricingCategory(listOfFields, "Derived-Intraday", new List<string>());
        }

        public bool RequiresBloombergLicenseDerivedDataEod(string listOfFields)
        {
            return RequiresBloombergPricingCategory(listOfFields, "Derived-EndofDay", new List<string>());
        }

        public bool RequiresBloombergLicensePricingDataIntraday(string listOfFields, bool forEquity)
        {
            if (forEquity)
                return RequiresBloombergPricingCategory(listOfFields, "Pricing-Intraday", _torontoController.getFieldsForVe_BloombergNorthAmericanPrice());
            return RequiresBloombergPricingCategory(listOfFields, "Pricing-Intraday", new List<string>());
        }

        public bool RequiresBloombergLicensePricingDataEod(string listOfFields)
        {
            return RequiresBloombergPricingCategory(listOfFields, "Pricing-EndofDay", new List<string>());
        }

        public bool RequiresBloombergLicenseEstimates(string listOfFields)
        {
            return RequiresBloombergPricingCategory(listOfFields, "Estimates", new List<string>());
        }

        public bool RequiresBloombergLicenseCreditRisk(string listOfFields)
        {
            return RequiresBloombergPricingCategory(listOfFields, "CreditRisk", _torontoController.getFieldsForVe_BloombergCreditRisk());
        }

        private bool RequiresBloombergPricingCategory(string listOfFields, string thisCategory, IEnumerable<string> fieldsAlreadyAvailable)
        {
            foreach (var fieldName in listOfFields.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                var cleaned = fieldName.ToUpper().Trim();
                if (BloombergFieldPricingCategoryMapping.ContainsKey(cleaned))
                    if (BloombergFieldPricingCategoryMapping[cleaned].Equals(thisCategory) && !fieldsAlreadyAvailable.Contains(cleaned))
                        return true;
            }
            return false;
        }

        public List<string> GetRequiredLocalCachedPricingViews()
        {
            return new List<string>
            {
                "TorViewBloombergPerSecurityPull",
                "TorViewBloombergNonNorthAmericanPrice",
                "TorViewBloombergNorthAmericanPrice",
                "TorViewBloombergFixedIncome"
            };
        }

        public bool DataAvailableInAllPricingViews(DateTime asOfDate)
        {
            return GetRequiredLocalCachedPricingViews().All(pricingView => HasDataInWarehouseTableForDate(pricingView, asOfDate));
        }

        public bool HasDataInWarehouseTableForDate(string tableName, DateTime asOfDate)
        {
            var sql = string.Format("select top 1 1 from [{0}] where effectivedate ='{1:ddMMMyyyy}'", tableName, asOfDate);

            try
            {
                if (Utils.DbController.ExecuteExists(sql))
                {
                    Utils.Logger.Info("Has data in warehouse table for date: {0} for date {1:ddMMMyyyy}", tableName, asOfDate);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Utils.Logger.Info("Sql error - treat as NO data in warehouse table for date: {0} for date {1:ddMMMyyyy} {2}", tableName, asOfDate, ex);
                return false;
            }

            Utils.Logger.Info("Has NO data in warehouse table for date: {0} for date {1:ddMMMyyyy}", tableName, asOfDate);
            return false;
        }

        public bool HasBeenEnrichedForDate(DateTime asOfDate)
        {
            var sql = "select count(*) from bloombergdatawarehouse where effective_date ='{0:ddMMMyyyy}' and downloaded is {1} null";
            var enriched = string.Format(sql, asOfDate, "NOT");
            var notEnriched = string.Format(sql, asOfDate, "");

            var rowsEnriched = Utils.DbController.GetScalar<int>(enriched);
            var rowsNotEnriched = Utils.DbController.GetScalar<int>(notEnriched);

            Utils.Logger.Info("Rows enriched:{0} not enriched: {1} , {2:ddMMMyyyy}", rowsEnriched, rowsNotEnriched, asOfDate);

            if (rowsEnriched == 0 && rowsNotEnriched == 0)
                return true;
            if (rowsEnriched > rowsNotEnriched)
            {
                Utils.Logger.Info("Already enriched. Rows enriched:{0} not enriched: {1} , {2:ddMMMyyyy}", rowsEnriched, rowsNotEnriched, asOfDate);
                return true;
            }
            return false;
        }
    }
}