using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Xml.Linq;
using OptionValue_DotNet;
using Maple;

namespace BOSS_OptionValueManager.Data
{
    internal class DataLayer : IDisposable
    {
        SQLServer db;

        internal DataLayer()
        {
            Connect();
        }
        
        /// <summary>
        /// get a list of all of the options that we want to price (i.e. ones we have a balance in)
        /// </summary>
        /// <param name="ValuationDate"></param>
        /// <returns></returns>
        internal List<BOSSOption> GetBOSSOptionsToPrice(DateTime ValuationDate)
        {
            List<BOSSOption> options = new List<BOSSOption>();
            SqlCommand sqlCom = new SqlCommand("spOTCOptionPrice_GetSecuritiesToPrice");
            sqlCom.CommandType = CommandType.StoredProcedure;         
            sqlCom.Parameters.Add(new SqlParameter("@PriceDate", ValuationDate));            
            DataSet ds = db.FetchData(sqlCom);
            if (ds.Tables[0].Rows.Count != 0)
            {
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    //get static data
                    int SecurityID =int.Parse(dr["SecurityID"].ToString());
                    BOSSOption o = GetBOSSOption(SecurityID,ValuationDate);
                    options.Add(o);
                }

            }
            return options;            
        }

        /// <summary>
        /// retrieve the static/current data associated with the option from BOSS
        /// </summary>
        /// <param name="SecurityID"></param>
        /// <returns></returns>
        private BOSSOption GetBOSSOption(int securityID, DateTime ValuationDate)
        {
            SqlCommand sqlCom = new SqlCommand("spOTCOptionPrice_GetSecurityData");
            sqlCom.CommandType = CommandType.StoredProcedure;
            sqlCom.Parameters.Add(new SqlParameter("@SecurityID", securityID));
            DataSet ds = db.FetchData(sqlCom);
            if (ds.Tables[0].Rows.Count == 0)
                return null;
            DataRow dr = ds.Tables[0].Rows[0];
            BOSSOption o = new BOSSOption()
            {
                SecurityID = securityID,
                SecurityName = dr["Description"].ToString(),
                UnderlyingSecurityName = dr["UnderlyingDescription"].ToString(),
                Strike = double.Parse(dr["StrikePrice"].ToString()),
                Maturity = DateTime.Parse(dr["MatDate"].ToString()),
                Type = (dr["SecuritySubTypeID"].ToString() == "6")  ? Option.OptionType.Call : Option.OptionType.Put,
                Style = (dr["OptionStyleID"].ToString() == "1") ? Option.OptionStyle.American: Option.OptionStyle.European,
                FirstTraded = DateTime.Parse(dr["FirstTradeDate"].ToString())

            };

            //get underlying price
            o.UnderlyingPrice = GetUnderlyingPrice(securityID, ValuationDate);

            //get remaining data - can come from any of (up to) three sources, try the each one in the order specified below...

            //get dividends            
            o.DividendSource = (DividendSourceData)SourceDataChooser(securityID, ValuationDate, new List<GetSourceDataDel>() { 
                new GetSourceDataDel(GetDividends_ManualOverride), 
                new GetSourceDataDel(GetDividends_BOSS), 
                new GetSourceDataDel(GetDividends_Bloomberg) });

            //get volatility
            o.VolatilitySource = (VolatilitySourceData)SourceDataChooser(securityID, ValuationDate, new List<GetSourceDataDel>() { 
                new GetSourceDataDel(GetVolatility_ManualOverride),                 
                new GetSourceDataDel(GetVolatility_Bloomberg) });

            //get rate
            o.RateSource = (RateSourceData)SourceDataChooser(securityID, ValuationDate, new List<GetSourceDataDel>() { 
                new GetSourceDataDel(GetRate_ManualOverride),                 
                new GetSourceDataDel(GetRate_Bloomberg) });


            return o;

        }

        /// <summary>
        /// clear out all valuation data for the date
        /// </summary>
        /// <param name="ValuationDate"></param>
        internal void DeleteOptionPrices(DateTime ValuationDate)
        {
            SqlCommand sqlComDelete = new SqlCommand("spOTCOptionPrice_DeletePrice_All");
            sqlComDelete.CommandType = CommandType.StoredProcedure;
            sqlComDelete.Parameters.Add(new SqlParameter("@PriceDate", ValuationDate));
            ExecSql(sqlComDelete);

        }

        /// <summary>
        /// clear out valuation data for the security/date
        /// </summary>
        /// <param name="SecurityID"></param>
        /// <param name="ValuationDate"></param>
        internal void DeleteOptionPrice(int SecurityID,DateTime ValuationDate)
        {
            SqlCommand sqlComDelete = new SqlCommand("spOTCOptionPrice_DeletePrice");
            sqlComDelete.CommandType = CommandType.StoredProcedure;
            sqlComDelete.Parameters.Add(new SqlParameter("@SecurityID", SecurityID));
            sqlComDelete.Parameters.Add(new SqlParameter("@PriceDate", ValuationDate));
            ExecSql(sqlComDelete);

        }

        /// <summary>
        /// write the valuation data for this security/date
        /// </summary>
        /// <param name="ValuationDate"></param>
        /// <param name="Options"></param>
        internal void SaveOptionPrice(BOSSOption option, DateTime ValuationDate)
        {

            //delete if it is there already
            DeleteOptionPrice(option.SecurityID, ValuationDate);

            //insert
            SqlCommand sqlComInsert = new SqlCommand("spOTCOptionPrice_InsertPrice");
            sqlComInsert.CommandType = CommandType.StoredProcedure;

            sqlComInsert.Parameters.Add(new SqlParameter("@SecurityID", option.SecurityID));
            sqlComInsert.Parameters.Add(new SqlParameter("@PriceDate", ValuationDate));
            sqlComInsert.Parameters.Add(new SqlParameter("@UnderlyingPrice", option.UnderlyingPrice));

            sqlComInsert.Parameters.Add(new SqlParameter("@UnderlyingVolatility", option.UnderlyingVolatility));
            sqlComInsert.Parameters.Add(new SqlParameter("@VolatilitySource", option.VolatilitySource.Source.ToString()));
            sqlComInsert.Parameters.Add(new SqlParameter("@VolatilityCaptureTime", option.VolatilitySource.CaptureTime));

            sqlComInsert.Parameters.Add(new SqlParameter("@VolatilityBBTicker", option.VolatilitySource.BBData.Ticker));
            sqlComInsert.Parameters.Add(new SqlParameter("@VolatilityBBField", option.VolatilitySource.BBData.Field));

            sqlComInsert.Parameters.Add(new SqlParameter("@RiskFreeRate", option.Rate));
            sqlComInsert.Parameters.Add(new SqlParameter("@RiskFreeRateSource", option.RateSource.Source.ToString()));
            sqlComInsert.Parameters.Add(new SqlParameter("@RiskFreeRateCaptureTime", option.RateSource.CaptureTime));

            sqlComInsert.Parameters.Add(new SqlParameter("@RiskFreeRateBBTicker_Previous", option.RateSource.BBData_PreviousTerm.Ticker));
            sqlComInsert.Parameters.Add(new SqlParameter("@RiskFreeRateBBTicker_Next", option.RateSource.BBData_NextTerm.Ticker));
            sqlComInsert.Parameters.Add(new SqlParameter("@RiskFreeRateBBTicker_Previous_Value", option.RateSource.BBData_PreviousTerm.Value));
            sqlComInsert.Parameters.Add(new SqlParameter("@RiskFreeRateBBTicker_Next_Value", option.RateSource.BBData_NextTerm.Value));
            
            sqlComInsert.Parameters.Add(new SqlParameter("@DividendsSource", option.DividendSource.Source.ToString()));
            if (option.DividendSource.Source != BOSS_OptionValueManager.InputSourceData.InputSource.Missing)
            {
                sqlComInsert.Parameters.Add(new SqlParameter("@DivDetails", option.DisplayDividendString));
                sqlComInsert.Parameters.Add(new SqlParameter("@DividendCurrency", option.DividendSource.DividendCurrency));
                sqlComInsert.Parameters.Add(new SqlParameter("@DividendFXRate", option.DividendSource.DividendFXRate));
            }

            sqlComInsert.Parameters.Add(new SqlParameter("@Price", option.OptionValue));

            ExecSql(sqlComInsert);
        }

        /// <summary>
        /// Write saved option prices to BOSS
        /// </summary>
        /// <param name="ValuationDate"></param>
        internal void SaveOptionPricesToBOSS(DateTime ValuationDate)
        {
            SqlCommand sqlComInsert = new SqlCommand("spOTCOptionPrice_InsertBOSSPrices");
            sqlComInsert.CommandType = CommandType.StoredProcedure;            
            sqlComInsert.Parameters.Add(new SqlParameter("@PriceDate", ValuationDate));
            ExecSql(sqlComInsert);
        }

        /// <summary>
        /// get the price of the underlying security at valuation date
        /// </summary>
        /// <param name="SecurityID"></param>
        /// <param name="ValuationDate"></param>
        /// <returns></returns>
        private double GetUnderlyingPrice(int SecurityID, DateTime ValuationDate)
        {
            SqlCommand sqlCom = new SqlCommand("spOTCOptionPrice_GetSecurityUnderlyingPrice");
            sqlCom.CommandType = CommandType.StoredProcedure;
            sqlCom.Parameters.Add(new SqlParameter("@SecurityID", SecurityID));
            sqlCom.Parameters.Add(new SqlParameter("@PriceDate", ValuationDate));
            DataSet ds = db.FetchData(sqlCom);
            double price =0;
            if (ds.Tables[0].Rows.Count != 0)
            {
                DataRow dr = ds.Tables[0].Rows[0];
                price = double.Parse(dr["Price"].ToString());
            }
            return price;
        }


        #region SourceDataChooser
        public delegate InputSourceData.SourceData GetSourceDataDel(int SecurityID, DateTime ValuationDate);
        /// <summary>
        /// encapsulate the logic of trying each of a prioritised list of sources until one returns a non MISSING source...
        /// </summary>
        /// <param name="SecurityID"></param>
        /// <param name="ValuationDate"></param>
        /// <param name="Sources"></param>
        /// <returns></returns>
        public InputSourceData.SourceData SourceDataChooser(int SecurityID, DateTime ValuationDate, List<GetSourceDataDel> Sources)
        {
            //try each of the sources until one returns a non MISSING value
            InputSourceData.SourceData sd = null;
            foreach (GetSourceDataDel sdd in Sources)
            {
                sd = sdd.Invoke(SecurityID, ValuationDate);
                if (sd.Source != InputSourceData.InputSource.Missing)
                    return sd;
            }

            //otherwise return the last one...
            return sd;
        } 
        #endregion

        #region Dividends

        private DividendSourceData GetDividends_ManualOverride(int SecurityID, DateTime ValuationDate)
        {
            DividendSourceData dsd = new DividendSourceData();
            SqlCommand sqlCom = new SqlCommand("spOTCOptionPrice_GetSecurityDividends_Default");
            sqlCom.CommandType = CommandType.StoredProcedure;
            sqlCom.Parameters.Add(new SqlParameter("@SecurityID", SecurityID));
            DataSet ds = db.FetchData(sqlCom);
            if (ds.Tables[0].Rows.Count != 0)
            {                
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    //dividend data is stored in the form exdate(amount) ie. 01/12/09(213.345)
                    string divdata = dr["DefaultDividends"].ToString();
                    string ccy = dr["Currency"].ToString();
                    DateTime Maturity = DateTime.Parse(dr["MatDate"].ToString());
                    if (divdata.Length != 0)
                    {
                        DateTime exdate = DateTime.ParseExact(divdata.Substring(0, 8), "dd/MM/yy", null);
                        double amount = Double.Parse(divdata.Substring(9).TrimEnd(")".ToCharArray()));

                        //only add if before maturity
                        if (exdate <= Maturity)
                        {
                            dsd.Source = InputSourceData.InputSource.Override;
                            dsd.CaptureTime = DateTime.ParseExact(dr["CaptureTime"].ToString(), "dd/MM/yyyy HH:mm:ss", null);
                            dsd.Dividends.Add(new DividendWithCurrency(exdate, amount, ccy,1));
                        }
                    }
                }
            }
            return dsd;
        }

        private DividendSourceData GetDividends_BOSS(int SecurityID, DateTime ValuationDate)
        {
            DividendSourceData dsd = new DividendSourceData();            
            SqlCommand sqlCom = new SqlCommand("spOTCOptionPrice_GetSecurityDividends_BOSS");
            sqlCom.CommandType = CommandType.StoredProcedure;
            sqlCom.Parameters.Add(new SqlParameter("@SecurityID", SecurityID));
            sqlCom.Parameters.Add(new SqlParameter("@PriceDate", ValuationDate));            
            DataSet ds = db.FetchData(sqlCom);            
            if (ds.Tables[0].Rows.Count != 0)
            {
                dsd.Source = InputSourceData.InputSource.BOSS;                
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    dsd.CaptureTime = DateTime.ParseExact(dr["CaptureTime"].ToString(), "dd/MM/yyyy HH:mm:ss", null);
                    dsd.Dividends.Add(new DividendWithCurrency(DateTime.Parse(dr["ExDate"].ToString()), Double.Parse(dr["Amount"].ToString()), dr["Currency"].ToString(), Double.Parse(dr["FX"].ToString())));
                }
                    
            }            

            return dsd;
        }

        private DividendSourceData GetDividends_Bloomberg(int SecurityID, DateTime ValuationDate)
        {
            DividendSourceData dsd = new DividendSourceData();
            SqlCommand sqlCom = new SqlCommand("spOTCOptionPrice_GetSecurityDividends_Bloomberg");
            sqlCom.CommandType = CommandType.StoredProcedure;
            sqlCom.Parameters.Add(new SqlParameter("@SecurityID", SecurityID));
            sqlCom.Parameters.Add(new SqlParameter("@PriceDate", ValuationDate));
            DataSet ds = db.FetchData(sqlCom);
            if (ds.Tables[0].Rows.Count != 0)
            {
                dsd.Source = InputSourceData.InputSource.Bloomberg;                
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    dsd.CaptureTime = DateTime.ParseExact(dr["CaptureTime"].ToString(), "dd/MM/yyyy HH:mm:ss", null);
                    dsd.Dividends.Add(new DividendWithCurrency(DateTime.Parse(dr["ExDate"].ToString()), Double.Parse(dr["Amount"].ToString()), dr["Currency"].ToString(), Double.Parse(dr["FX"].ToString())));
                }

            }

            return dsd;
        } 

        #endregion

        #region Volatilities

        private VolatilitySourceData GetVolatility_ManualOverride(int SecurityID, DateTime ValuationDate)
        {
            SqlCommand sqlCom = new SqlCommand("spOTCOptionPrice_GetVolatility_Default");
            sqlCom.CommandType = CommandType.StoredProcedure;
            sqlCom.Parameters.Add(new SqlParameter("@SecurityID", SecurityID));
            DataSet ds = db.FetchData(sqlCom);
            VolatilitySourceData vsd  = new VolatilitySourceData();
            if (ds.Tables[0].Rows.Count != 0)
            {
                DataRow dr = ds.Tables[0].Rows[0];

                //if the value is null ignore (i.e. is missing)
                if (dr["DefaultUnderlyingVolatility"] != DBNull.Value)
                {
                    vsd.Source = InputSourceData.InputSource.Override;
                    vsd.CaptureTime = DateTime.ParseExact(dr["CaptureTime"].ToString(), "dd/MM/yyyy HH:mm:ss", null);
                    vsd.Volatility = double.Parse(dr["DefaultUnderlyingVolatility"].ToString());
                }
            }
            return vsd;
        }

        private VolatilitySourceData GetVolatility_Bloomberg(int SecurityID, DateTime ValuationDate)
        {
            SqlCommand sqlCom = new SqlCommand("spOTCOptionPrice_GetVolatility_Bloomberg");
            sqlCom.CommandType = CommandType.StoredProcedure;
            sqlCom.Parameters.Add(new SqlParameter("@SecurityID", SecurityID));
            sqlCom.Parameters.Add(new SqlParameter("@PriceDate", ValuationDate));
            DataSet ds = db.FetchData(sqlCom);
            VolatilitySourceData vsd = new VolatilitySourceData();
            if (ds.Tables[0].Rows.Count != 0)
            {
                DataRow dr = ds.Tables[0].Rows[0];

                try {
                    vsd.Source = InputSourceData.InputSource.Bloomberg;
                    vsd.Volatility = double.Parse(dr["Volatility"].ToString());
                    vsd.CaptureTime = DateTime.ParseExact(dr["CaptureTime"].ToString(), "dd/MM/yyyy HH:mm:ss", null);
                    vsd.BBData = new BBData() { Ticker = dr["BBGTicker"].ToString(), Field = dr["BBField"].ToString(), Value = double.Parse(dr["Volatility"].ToString()) };
                } catch {
                    // Problem with data so don't return half a class, return as if nothing was obtained from db
                    vsd = new VolatilitySourceData();
                }
            }
            return vsd;
        } 
        #endregion

        #region Rates
 
        private RateSourceData GetRate_ManualOverride(int SecurityID, DateTime ValuationDate)
        {
            SqlCommand sqlCom = new SqlCommand("spOTCOptionPrice_GetRiskFreeRate_Default");
            sqlCom.CommandType = CommandType.StoredProcedure;
            sqlCom.Parameters.Add(new SqlParameter("@SecurityID", SecurityID));
            DataSet ds = db.FetchData(sqlCom);
            RateSourceData rsd = new RateSourceData();
            if (ds.Tables[0].Rows.Count != 0)
            {
                DataRow dr = ds.Tables[0].Rows[0];
               
                //if the value is null ignore (i.e. is missing)
                if (dr["DefaultRiskFreeRate"] != DBNull.Value)
                {
                    rsd.Source = InputSourceData.InputSource.Override;
                    rsd.Rate = double.Parse(dr["DefaultRiskFreeRate"].ToString());
                    rsd.CaptureTime = DateTime.ParseExact(dr["CaptureTime"].ToString(), "dd/MM/yyyy HH:mm:ss", null);
                }
            }
            return rsd;
        }

        private RateSourceData GetRate_Bloomberg(int SecurityID, DateTime ValuationDate)
        {
            SqlCommand sqlCom = new SqlCommand("spOTCOptionPrice_GetRiskFreeRate_Bloomberg");
            sqlCom.CommandType = CommandType.StoredProcedure;
            sqlCom.Parameters.Add(new SqlParameter("@SecurityID", SecurityID));
            sqlCom.Parameters.Add(new SqlParameter("@PriceDate", ValuationDate));
            DataSet ds = db.FetchData(sqlCom);
            RateSourceData rsd = new RateSourceData();
            if (ds.Tables[0].Rows.Count != 0)
            {
                DataRow dr = ds.Tables[0].Rows[0];
                try {
                    rsd.Source = InputSourceData.InputSource.Bloomberg;
                    rsd.Rate = double.Parse(dr["MaturityRate"].ToString());
                    rsd.CaptureTime = DateTime.ParseExact(dr["CaptureTime"].ToString(), "dd/MM/yyyy HH:mm:ss", null);
                    rsd.BBData_PreviousTerm = new BBData() { Ticker = dr["PreviousTicker"].ToString(), Field = "PX_YEST_CLOSE", Value = double.Parse(dr["PreviousRate"].ToString()) };
                    rsd.BBData_NextTerm = new BBData() { Ticker = dr["NextTicker"].ToString(), Field = "PX_YEST_CLOSE", Value = double.Parse(dr["NextRate"].ToString()) };
                } catch {
                    // Problem with data so don't return half a class, return as if nothing was obtained from db
                    rsd = new RateSourceData();
                }

            }
            return rsd;
        } 
        #endregion


        public DataTable GetManualOverrides()
        {
            SqlCommand sqlCom = new SqlCommand("spOTCOptionPrice_GetDefaults");
            sqlCom.CommandType = CommandType.StoredProcedure;            
            return db.FetchData(sqlCom).Tables[0]; 
        }

        public void SaveManualOverride(int SecurityID, double DefaultUnderlyingVolatility, double DefaultRate, string DefaultDividend)
        {
            SqlCommand sqlComInsert = new SqlCommand("spOTCOptionPrice_InsertDefaults");
            sqlComInsert.CommandType = CommandType.StoredProcedure;

            sqlComInsert.Parameters.Add(new SqlParameter("@SecurityID", SecurityID));

            if(DefaultUnderlyingVolatility != 0)
                sqlComInsert.Parameters.Add(new SqlParameter("@DefaultUnderlyingVolatility", DefaultUnderlyingVolatility));

            if(DefaultRate !=0)
                sqlComInsert.Parameters.Add(new SqlParameter("@DefaultRiskFreeRate", DefaultRate));

            if(DefaultDividend.Length != 0)
                sqlComInsert.Parameters.Add(new SqlParameter("@DefaultDividends", DefaultDividend));

            ExecSql(sqlComInsert);

        }

        public void DeleteManualOverride(int SecurityID)
        {
            SqlCommand sqlComInsert = new SqlCommand("spOTCOptionPrice_DeleteDefaults");
            sqlComInsert.CommandType = CommandType.StoredProcedure;

            sqlComInsert.Parameters.Add(new SqlParameter("@SecurityID", SecurityID));
            ExecSql(sqlComInsert);

        }

        public DataTable GetBloombergData_Volatilities(DateTime ValuationDate)
        {
            SqlCommand sqlCom = new SqlCommand("spOTCOptionPrice_GetBBData_Volatilities");
            sqlCom.CommandType = CommandType.StoredProcedure;
            sqlCom.Parameters.Add(new SqlParameter("@PriceDate", ValuationDate));
            return db.FetchData(sqlCom).Tables[0];
        }

        public DataTable GetBloombergData_Rates(DateTime ValuationDate)
        {
            SqlCommand sqlCom = new SqlCommand("spOTCOptionPrice_GetBBData_Rates");
            sqlCom.CommandType = CommandType.StoredProcedure;
            sqlCom.Parameters.Add(new SqlParameter("@PriceDate", ValuationDate));
            return db.FetchData(sqlCom).Tables[0];
        }

        public void CopyVolatilitiesAndRates(DateTime PriceDateFrom, DateTime PriceDateTo)
        {
            SqlCommand sqlComInsert = new SqlCommand("spOTCOptionPrice_CopyVolsAndRates");
            sqlComInsert.CommandType = CommandType.StoredProcedure;

            sqlComInsert.Parameters.Add(new SqlParameter("@PriceDateFrom", PriceDateFrom));
            sqlComInsert.Parameters.Add(new SqlParameter("@PriceDateTo", PriceDateTo));
            ExecSql(sqlComInsert);
        }

        /// <summary>
        /// call the dm.ExecSQL method and raise an error if an error string is returned
        /// </summary>
        /// <param name="sqlCommand"></param>
        public void ExecSql(SqlCommand sqlCommand)
        {
            string err = db.ExecSql(sqlCommand);
            if (err != "")
                throw new System.Exception("Could not execute command: " + err);
        }

        #region Connection Methods

        private void Connect()
        {
            string dsn = "BossLive"; ;

            #if DEBUG
                        dsn = "BossTest";
            #endif

            db = new SQLServer(dsn);
        }

        private void Disconnect()
        {
            if (db != null)
                db.Close();
            db = null;
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            Disconnect();
        }

        #endregion
    }
}