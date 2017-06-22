using System;
using System.Collections.Generic;
using System.Threading;

namespace BBDataWarehouseCacheManager.Controllers
{
    public class TorontoViewManager
    {
        public List<string> ListOfTorontoViews()
        {
            return new List<string>
            {  "ve_BloombergCreditRisk",
                "ve_BloombergDescription"
                    };
        }

        public bool DataIsReadyInViewForDate(string viewName, DateTime asOfDate)
        {
            //extra check for ve_BloombergPerSecurityPull
            if (viewName.Equals("ve_BloombergPerSecurityPull"))
            {
                //get latest data indicator.
                var res = Utils.DbController.GetScalar<DateTime>("select top 1 FileDate from helium.bloombergdatalicense.dbo.BloombergPerSecurityfiledate");
                //looking for reasons to fail - null, or the date is wrong.
                if (res.Equals(DateTime.MinValue) || !res.Equals(asOfDate))
                {
                    Utils.Logger.Info("Filedate in Bloombergfiledatenna ({0}) does not match. {1} vs {2}", viewName, asOfDate, res);
                    Utils.Logger.Info("Data is NOT ready in view for date {0} for date {1:ddMMMyyyy} ", viewName, asOfDate);
                    return false;
                }
            }
            else if (viewName.Equals("ve_BloombergNonNorthAmericanPrice"))
            {
                //get latest data indicator.
                var res = Utils.DbController.GetScalar<DateTime>("select filedate from helium.BloombergDataLicense.dbo.Bloombergfiledatenna a inner join helium.BloombergDataLicense.dbo.luBloombergFileType b on a.bloombergfiletypeid = b.bloombergfiletypeid where description = 'European'");
                //looking for reasons to fail - null, or the date is wrong.
                if (res.Equals(DateTime.MinValue) || !res.Equals(asOfDate))
                {
                    Utils.Logger.Info("Filedate in Bloombergfiledatenna ({0}) does not match. {1} vs {2}", viewName, asOfDate, res);
                    Utils.Logger.Info("Data is NOT ready in view for date {0} for date {1:ddMMMyyyy} ", viewName, asOfDate);
                    return false;
                }

                res = Utils.DbController.GetScalar<DateTime>("select filedate from helium.BloombergDataLicense.dbo.Bloombergfiledatenna a inner join helium.BloombergDataLicense.dbo.luBloombergFileType b on a.bloombergfiletypeid = b.bloombergfiletypeid where description = 'Asian 1'");
                //looking for reasons to fail - null, or the date is wrong.
                if (res.Equals(DateTime.MinValue) || !res.Equals(asOfDate))
                {
                    Utils.Logger.Info("Filedate in Bloombergfiledatenna ({0}) does not match. {1} vs {2}", viewName, asOfDate, res);
                    Utils.Logger.Info("Data is NOT ready in view for date {0} for date {1:ddMMMyyyy} ", viewName, asOfDate);
                    return false;
                }

                res = Utils.DbController.GetScalar<DateTime>("select filedate from helium.BloombergDataLicense.dbo.Bloombergfiledatenna a inner join helium.BloombergDataLicense.dbo.luBloombergFileType b on a.bloombergfiletypeid = b.bloombergfiletypeid where description = 'Asian 2'");
                //looking for reasons to fail - null, or the date is wrong.
                if (res.Equals(DateTime.MinValue) || !res.Equals(asOfDate))
                {
                    Utils.Logger.Info("Filedate in Bloombergfiledatenna ({0}) does not match. {1} vs {2}", viewName, asOfDate, res);
                    Utils.Logger.Info("Data is NOT ready in view for date {0} for date {1:ddMMMyyyy} ", viewName, asOfDate);
                    return false;
                }
            }
            else if (viewName.Equals("ve_BloombergNorthAmericanPrice"))
            {
                //get latest data indicator.
                var res = Utils.DbController.GetScalar<DateTime>("select filedate from helium.BloombergDataLicense.dbo.Bloombergfiledate");
                //looking for reasons to fail - null, or the date is wrong.
                if (res.Equals(DateTime.MinValue) || !res.Equals(asOfDate))
                {
                    Utils.Logger.Info("Filedate in Bloombergfiledate ({0}) does not match. {1} vs {2}", viewName, asOfDate, res);
                    Utils.Logger.Info("Data is NOT ready in view for date {0} for date {1:ddMMMyyyy} ", viewName, asOfDate);
                    return false;
                }
            }
            else
            {
                var rowCount = GetRowCountInViewForDate(viewName, asOfDate);
                if (rowCount == 0)
                {
                    Utils.Logger.Info("Data is NOT ready in view for date {0} for date {1:ddMMMyyyy} ", viewName, asOfDate);
                    return false;
                }
                Thread.Sleep(10000);
                var checkAgain = GetRowCountInViewForDate(viewName, asOfDate);
                if (rowCount != checkAgain)
                {
                    Utils.Logger.Info("Data is NOT ready in view for date {0} for date {1:ddMMMyyyy} ", viewName, asOfDate);
                    return false;
                }
            }
            Utils.Logger.Info("Data is ready in view for date {0} for date {1:ddMMMyyyy} ", viewName, asOfDate);
            return true;
        }

        private int GetRowCountInViewForDate(string viewName, DateTime asOfDate)
        {
            var sql = string.Format("select count(*) from [HELIUM].[BloombergDataLicense].[dbo].[{0}] where effectivedate ='{1:ddMMMyyyy}'", viewName, asOfDate);
            return Utils.DbController.GetScalar<int>(sql);
        }

        public List<KeyValuePair<string, string>> GetBergMonikersFromDataWarehouse(DateTime effectiveDate)
        {
            var sql = string.Format("SELECT distinct berg_moniker,REQ_DATA_FIELD_LIST FROM bloombergdatawarehouse where effective_date='{0:ddMMMyyyy}'", effectiveDate);
            return Utils.DbController.GetListOfKeyValuePairs<string, string>(sql);
        }

        public List<string> getFieldsForVe_BloombergDescription()
        {
            return new List<string>
            {
                                "144A_FLAG",
                                "ADR_ADR_PER_SH",
                                "ADR_SH_PER_ADR",
                                "ADR_UNDL_CMPID",
                                "ADR_UNDL_CRNCY",
                                "ADR_UNDL_SECID",
                                "ADR_UNDL_TICKER",
                                "BloombergDescriptionId",
                                "BloombergFileTypeId",
                                "CDR_COUNTRY_CODE",
                                "CDR_EXCH_CODE",
                                "CDR_SETTLE_CODE",
                                "CFI_CODE",
                                "CNTRY_ISSUE_ISO",
                                "CNTRY_OF_DOMICILE",
                                "CNTRY_OF_INCORPORATION",
                                "COUNTRY",
                                "CRNCY",
                                "Description",
                                "DTC_ELIGIBLE",
                                "DVD_CRNCY",
                                "DVD_DECLARED_DT",
                                "DVD_EX_DT",
                                "DVD_FREQ",
                                "DVD_PAY_DT",
                                "DVD_RECORD_DT",
                                "DVD_SH_12M",
                                "DVD_SH_LAST",
                                "DVD_TYP_LAST",
                                "EffectiveDate",
                                "EQY_DVD_EX_FLAG",
                                "EQY_DVD_PCT_FRANKED",
                                "EQY_DVD_SH_12M_NET",
                                "EQY_FREE_FLOAT_PCT",
                                "EQY_FUND_CRNCY",
                                "EQY_FUND_TICKER",
                                "EQY_INIT_PO_DT",
                                "EQY_INIT_PO_SH_PX",
                                "EQY_OPT_AVAIL",
                                "EQY_PO_DT",
                                "EQY_PRIM_EXCH",
                                "EQY_PRIM_EXCH_SHRT",
                                "EQY_PRIM_SECURITY_COMP_EXCH",
                                "EQY_PRIM_SECURITY_PRIM_EXCH",
                                "EQY_PRIM_SECURITY_TICKER",
                                "EQY_SH_OUT",
                                "EQY_SH_OUT_DT",
                                "EQY_SH_OUT_REAL",
                                "EQY_SH_OUT_TOT_MULT_SH",
                                "EQY_SIC_CODE",
                                "EQY_SIC_NAME",
                                "EQY_SPLIT_ADJ_INIT_PO_PX",
                                "EQY_SPLIT_DT",
                                "EQY_SPLIT_RATIO",
                                "EXCH_CODE",
                                "ID_BB_COMPANY",
                                "ID_BB_CONNECT",
                                "ID_BB_PARENT_CO",
                                "ID_BB_PRIM_SECURITY",
                                "ID_BB_PRIM_SECURITY_FLAG",
                                "ID_BB_SECURITY",
                                "ID_BB_UNIQUE",
                                "ID_BELGIUM",
                                "ID_COMMON",
                                "ID_CUSIP",
                                "ID_DUTCH",
                                "ID_EXCH_SYMBOL",
                                "ID_FRENCH",
                                "ID_ISIN",
                                "ID_LOCAL",
                                "ID_MIC_LOCAL_EXCH",
                                "ID_MIC_PRIM_EXCH",
                                "ID_NAICS_CODE",
                                "ID_SEDOL1",
                                "ID_SEDOL2",
                                "ID_VALOREN",
                                "ID_WERTPAPIER",
                                "INDUSTRY_GROUP",
                                "INDUSTRY_SECTOR",
                                "INDUSTRY_SUBGROUP",
                                "INDUSTRY_SUBGROUP_NUM",
                                "IS_SETS",
                                "IS_STK_MARGINABLE",
                                "LAST_DPS_GROSS",
                                "LONG_COMP_NAME",
                                "MARKET_SECTOR_DES",
                                "MARKET_STATUS",
                                "MULTIPLE_SHARE",
                                "NAME",
                                "PAR_AMT",
                                "PAR_VAL_CRNCY",
                                "PARENT_COMP_NAME",
                                "PARENT_COMP_TICKER",
                                "PARENT_INDUSTRY_GROUP",
                                "PARENT_INDUSTRY_SECTOR",
                                "PARENT_INDUSTRY_SUBGROUP",
                                "PX_QUOTE_LOT_SIZE",
                                "PX_ROUND_LOT_SIZE",
                                "PX_TRADE_LOT_SIZE",
                                "REL_INDEX",
                                "SEC_RESTRICT",
                                "SECURITY_TYP",
                                "SECURITY_TYP2",
                                "SEDOL1_COUNTRY_ISO",
                                "SEDOL2_COUNTRY_ISO",
                                "TICKER",
                                "TICKER_AND_EXCH_CODE",
                                "TOTAL_NON_VOTING_SHARES_VALUE",
                                "TOTAL_VOTING_SHARES_VALUE",
                                "TRANSFER_AGENT",
                                "VOTING_RIGHTS",
                                "WHEN_ISSUED",
                                "WHICH_JAPANESE_SECTION"};
        }

        public List<string> getFieldsForVe_BloombergNorthAmericanPrice()
        {
            return new List<string>
            {
                            "BloombergnorthamericanpriceId",
                    "CNTRY_ISSUE_ISO",
                    "CNTRY_OF_DOMICILE",
                    "CNTRY_OF_INCORPORATION",
                    "COMPOSITE_EXCH_CODE",
                    "CRNCY",
                    "CUR_MKT_CAP",
                    "EffectiveDate",
                    "EQY_BETA",
                    "EQY_DVD_YLD_12M",
                    "EQY_DVD_YLD_12M_NET",
                    "EQY_DVD_YLD_IND",
                    "EQY_FLOAT",
                    "EQY_PRIM_EXCH",
                    "EQY_PRIM_EXCH_SHRT",
                    "EQY_PRIM_SECURITY_COMP_EXCH",
                    "EQY_SH_OUT",
                    "EQY_SH_OUT_REAL",
                    "EXCH_CODE",
                    "HIGH_52WEEK",
                    "HIGH_DT_52WEEK",
                    "ID_BB_COMPANY",
                    "ID_BB_CONNECT",
                    "ID_BB_PRIM_SECURITY_FLAG",
                    "ID_BB_SECURITY",
                    "ID_BB_UNIQUE",
                    "ID_BELGIUM",
                    "ID_COMMON",
                    "ID_CUSIP",
                    "ID_DUTCH",
                    "ID_EXCH_SYMBOL",
                    "ID_FRENCH",
                    "ID_ISIN",
                    "ID_LOCAL",
                    "ID_MIC_LOCAL_EXCH",
                    "ID_MIC_PRIM_EXCH",
                    "ID_SEDOL1",
                    "ID_SEDOL2",
                    "ID_VALOREN",
                    "ID_WERTPAPIER",
                    "LAST_UPDATE",
                    "LAST_UPDATE_DT_EXCH_TZ",
                    "LOW_52WEEK",
                    "LOW_DT_52WEEK",
                    "MARKET_SECTOR_DES",
                    "MARKET_STATUS",
                    "MKT_CAP_LISTED",
                    "NAME",
                    "PRICING_SOURCE",
                    "PX_ASK",
                    "PX_BID",
                    "PX_FIXING",
                    "PX_HIGH",
                    "PX_LAST",
                    "PX_LOW",
                    "PX_MID",
                    "PX_NASDAQ_CLOSE",
                    "PX_OPEN",
                    "PX_QUOTE_LOT_SIZE",
                    "PX_ROUND_LOT_SIZE",
                    "PX_TRADE_LOT_SIZE",
                    "PX_VOLUME",
                    "SECURITY_TYP",
                    "SEDOL1_COUNTRY_ISO",
                    "SEDOL2_COUNTRY_ISO",
                    "TICKER",
                    "TICKER_AND_EXCH_CODE"        };
        }

        public List<string> getFieldsForVe_BloombergCreditRisk()
        {
            return new List<string>
            {
                                            "ACQUIRED_BY_PARENT",
                            "BloombergCreditRiskId",
                            "BloombergFileTypeId",
                            "CNTRY_OF_DOMICILE",
                            "CNTRY_OF_INCORPORATION",
                            "CNTRY_OF_RISK",
                            "COMPANY_ADDRESS",
                            "COMPANY_CORP_TICKER",
                            "COMPANY_LEGAL_NAME",
                            "COMPANY_TO_PARENT_RELATIONSHIP",
                            "EffectiveDate",
                            "ID_BB_COMPANY",
                            "ID_BB_PARENT_CO",
                            "ID_BB_ULTIMATE_PARENT_CO",
                            "INDUSTRY_GROUP",
                            "INDUSTRY_SECTOR",
                            "INDUSTRY_SUBGROUP",
                            "INDUSTRY_SUBGROUP_NUM",
                            "IS_ULT_PARENT",
                            "ISSUER_NAME_TYPES",
                            "LONG_COMP_NAME",
                            "LONG_PARENT_COMP_NAME",
                            "LONG_ULT_PARENT_COMP_NAME",
                            "OBLIG_INDUSTRY_SUBGROUP",
                            "STATE_OF_DOMICILE",
                            "STATE_OF_INCORPORATION",
                            "ULT_PARENT_CNTRY_DOMICILE",
                            "ULT_PARENT_CNTRY_INCORPORATION",
                            "ULT_PARENT_CNTRY_OF_RISK",
                            "ULT_PARENT_TICKER_EXCHANGE"
            };
        }
    }
}