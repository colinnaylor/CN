using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Xml.Linq;
using Maple;
using BBGShared;

namespace OTCOptionValuation_BBImporter.Data
{
    internal class DataLayer : IDisposable
    {
        SQLServer db;

        internal DataLayer()
        {
            Connect();
        }

        #region GetTickerData
        public delegate System.Data.DataSet GetTickerDataDel(DateTime PriceDate);

        internal DataSet GetTickerData_Rate(DateTime PriceDate)
        {
            SqlCommand sqlCom = new SqlCommand("spOTCOptionPrice_GetBBTickers_RiskFreeRate");
            sqlCom.CommandType = CommandType.StoredProcedure;
            sqlCom.Parameters.Add(new SqlParameter("@PriceDate", PriceDate));
            DataSet ds = db.FetchData(sqlCom);
            return ds;
        }

        internal DataSet GetTickerData_Volatility(DateTime PriceDate)
        {
            SqlCommand sqlCom = new SqlCommand("spOTCOptionPrice_GetBBTickers_Volatility");
            sqlCom.CommandType = CommandType.StoredProcedure;
            sqlCom.Parameters.Add(new SqlParameter("@PriceDate", PriceDate));
            DataSet ds = db.FetchData(sqlCom);
            return ds;
        }

        /// <summary>
        /// Only import vols that we have not alreadys got.
        /// We can run this in the morning to pick up any late booked trades i.e. ones that were entered after the evening import at 5 PM
        /// </summary>
        /// <param name="PriceDate"></param>
        /// <returns></returns>
        internal DataSet GetTickerData_Volatility_MissingOnly(DateTime PriceDate)
        {
            SqlCommand sqlCom = new SqlCommand("spOTCOptionPrice_GetBBTickers_Volatility_MissingOnly");
            sqlCom.CommandType = CommandType.StoredProcedure;
            sqlCom.Parameters.Add(new SqlParameter("@PriceDate", PriceDate));
            DataSet ds = db.FetchData(sqlCom);
            return ds;
        }

        internal DataSet GetTickerData_Dividend(DateTime PriceDate)
        {
            SqlCommand sqlCom = new SqlCommand("spOTCOptionPrice_GetBBTickers_Dividend");
            sqlCom.CommandType = CommandType.StoredProcedure;
            sqlCom.Parameters.Add(new SqlParameter("@PriceDate", PriceDate));
            DataSet ds = db.FetchData(sqlCom);
            return ds;
        }

        #endregion

        #region SaveTickerData
        public delegate void SaveTickerDataDel(List<BloombergDataInstrument> Instruments, int Reference);

        /// <summary>
        /// Write vols back to DB - update if there already
        /// </summary>
        /// <param name="ins"></param>
        /// <param name="PriceDate"></param>
        internal void SaveTickerData_Volatility(List<BloombergDataInstrument> instruments, int Reference)
        {
            string sql = string.Format("OptionValueManagerVolatilityInsert {0}", Reference);

            string ret = db.ExecSql(sql);
            if (ret != "") { throw new Exception(string.Format("Error in {0}.\r\n{1}", "SaveTickerData_Volatility", ret)); }
        }

        /// <summary>
        /// Write rates back to DB - update if there already
        /// </summary>
        /// <param name="ins"></param>
        /// <param name="PriceDate"></param>
        internal void SaveTickerData_Rate(List<BloombergDataInstrument> instruments, int Reference)
        {
            string sql = string.Format("OptionValueManagerRateInsert {0}", Reference);

            string ret = db.ExecSql(sql);
            if (ret != "") { throw new Exception(string.Format("Error in {0}.\r\n{1}", "SaveTickerData_Rate", ret)); }
        }

        /// <summary>
        /// Write divs back to DB - update if there already
        /// </summary>
        /// <param name="ins"></param>
        /// <param name="PriceDate"></param>
        internal void SaveTickerData_Dividend(List<BloombergDataInstrument> instruments, int Reference)
        {
            string sql = string.Format("OptionValueManagerDividendInsert {0}", Reference);

            string ret = db.ExecSql(sql);
            if (ret != "") { throw new Exception(string.Format("Error in {0}.\r\n{1}", "SaveTickerData_Dividend", ret)); }
        }

        #endregion

        /// <summary>
        /// unpack the dataset into a list of Tickers/fields
        /// </summary>
        /// <param name="PriceDate"></param>
        /// <param name="ds"></param>
        /// <returns></returns>
        internal static List<BloombergDataInstrument> GetTickers(DateTime PriceDate, DataSet ds)
        {
            Dictionary<string, BloombergDataInstrument> ticker_map = new Dictionary<string, BloombergDataInstrument>();
            List<BloombergDataInstrument> tickers = new List<BloombergDataInstrument>();

            if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {

                string ticker;
                string field;
                int i = 0;
                BloombergDataInstrument ins;
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    ticker = dr["BBTicker"].ToString();
                    field = dr["BBField"].ToString();

                    //add if we do not have already
                    if (!ticker_map.ContainsKey(ticker))
                    {
                        ins = new BloombergDataInstrument()
                        {
                            ID = i++,
                            Ticker = ticker,
                            BBFields = new Dictionary<string, BloombergDataInstrumentField>(),
                            //DataFrom = PriceDate,   //this means that we only get a single point corresponding to pricedate when making historical requests
                            //DataTo = PriceDate
                        };

                        ticker_map.Add(ticker, ins);
                        tickers.Add(ins);
                    }
                    else
                    {
                        ins = ticker_map[ticker];
                    }

                    //add the field
                    ins.BBFields.Add(field, null);
                }
            }
            return tickers;
        }

        #region Connection Methods

        private void Connect()
        {
            string dsn = Properties.Settings.Default.LiveDSN; ;
#if DEBUG
            dsn = Properties.Settings.Default.TestDSN;
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

        internal string GetConnectionInfo()
        {
            return string.Format("Server = {0}\r\nDatabase = {1}", db.Server, db.Database);
        }
    }
}
