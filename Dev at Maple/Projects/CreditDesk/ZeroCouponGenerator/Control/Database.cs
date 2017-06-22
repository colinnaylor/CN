using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Maple;
using System.Data.SqlClient;
using System.Data;
using ZeroCouponGenerator.Properties;
using System.Windows.Forms;
using ZeroCouponGenerator.Model;

namespace ZeroCouponGenerator
{
    class Database
    {
        private Settings settings = ZeroCouponGenerator.Properties.Settings.Default;

        public void ExecSql(string sql, string failMessage, string DSN = "", int timeout = 0)
        {
            SqlCommand cmd = new SqlCommand(sql);

            if (DSN == "")
            {
                DSN = Settings.Default.DSN;
            }

            SQLServer db = new SQLServer(DSN);
            Utils.Log(string.Format("Calling DSN: {0} {1}", DSN, sql));
            db.ConnectionTimeout = timeout;
            db.Timeout = timeout;
            //SQLServer db = new SQLServer(Settings.Default.DSN);
            try
            {
                string ret = db.ExecSql(cmd);

                if (ret != "")
                {
                    Utils.Log(failMessage);
                    throw new Exception(failMessage + " " + ret);
                }

            }
            finally
            {
                db.Close();
            }
        }

        public void GetTickerRecordSet()
        {
            Utils.Log("Doing Generating Curve Data");

            ExecSql("EXEC ZeroCouponGenerator", "Error - Generating Curve Data (stored proc ZeroCouponGenerator)", timeout: 900);

            Utils.Log("Done Generating Curve Data");
        }



        internal void Wait(double Seconds)
        {
            DateTime timeOut = DateTime.Now.AddMilliseconds(Seconds * 1000);
            while (timeOut > DateTime.Now)
            {
                Application.DoEvents();
            }
        }

        public InputData GetCurveData(String currency, int runID)
        {
            Utils.Log("Creating List");
            string sql = "EXEC CurveGeneratorGetLatestData '" + currency + "', " + runID;
            SQLServer db = new SQLServer(Settings.Default.DSN);
            Utils.Log(string.Format("Calling DSN: {0} {1}", Settings.Default.DSN, sql));
            SqlDataReader dr = db.FetchDataReader(sql);

            InputData input = new InputData();
            input.LiborData = new List<Rate>();
            input.SwapData = new List<Rate>();
            input.FutureData = new List<Rate>();



            while (dr.Read())
            {
                input.StartDate = DateTime.Parse(dr["TimeStamp"].ToString()); // DateTime.Now;

                Rate rate = new Rate();
                //rate.StartDate = DateTime.Now;
                rate.Ask = (double)dr["Ask"];
                rate.Bid = (double)dr["Bid"];
                rate.SecType = dr["TypeName"].ToString();
                if (rate.SecType == "Future")
                {
                    rate.Expiry = DateTime.Parse(dr["Maturity"].ToString());
                }
                else
                {
                    rate.TermCode = dr["Type"].ToString();
                }

                switch (rate.SecType)
                {
                    case "Libor":
                        input.LiborData.Add(rate);
                        break;
                    case "Swap":
                        input.SwapData.Add(rate);
                        break;
                    case "Future":
                        input.FutureData.Add(rate);
                        break;
                }
                Utils.Log(string.Format("{0} Curve data used: {1}", currency, rate.ToString()));
            }
            dr.Close();
            db.Close();

            return input;

        }

        public List<String> GetCurrency(int runID)
        {
            Utils.Log("Start GetCurrency");
            string sql;
            if (runID == 0)
            {
                sql = "SELECT Currency FROM CurveGeneratorCurrency WHERE CalcCurve = 1";
            }
            else
            {
                sql = "SELECT DISTINCT C.Currency FROM CurveGeneratorDate T JOIN CurveGeneratorCurrency C ON T.Currency = C.Currency WHERE CalcCurve = 1 AND ID = " + runID;
            }

            SQLServer db = new SQLServer(Settings.Default.DSN);
            Utils.Log(string.Format("Calling DSN: {0} {1} ", Settings.Default.DSN, sql));
            SqlDataReader dr = db.FetchDataReader(sql);

            List<String> ccy = new List<string>();

            while (dr.Read())
            {
                Utils.Log(dr["Currency"].ToString());
                ccy.Add(dr["Currency"].ToString());
            }
            dr.Close();
            db.Close();

            return ccy;
        }

        public Dictionary<string, string> GetConvention(String Currency)
        {
            string sql = "SELECT * FROM CurveGeneratorConvention WHERE Currency = '" + Currency + "'";
            SQLServer db = new SQLServer(Settings.Default.DSN);
            Utils.Log(string.Format("Calling DSN: {0} {1}", Settings.Default.DSN, sql));
            SqlDataReader dr = db.FetchDataReader(sql);

            Dictionary<string, string> convention = new Dictionary<string, string>();

            while (dr.Read())
            {
                convention.Add("DayCountConvention", dr["DayCountConvention"].ToString());
                convention.Add("NextWorkingDay", dr["NextWorkingDay"].ToString());
                convention.Add("HolidayCentre", dr["HolidayCentre"].ToString());
                convention.Add("MaxFutureTermInDays", dr["MaxFutureTermInDays"].ToString());
                convention.Add("SettleDaysForFutures", dr["SettleDaysForFutures"].ToString());
                convention.Add("SettleDaysForSwaps", dr["SettleDaysForSwaps"].ToString());
                convention.Add("SettleDaysForLibor", dr["SettleDaysForLibor"].ToString());
                convention.Add("LiborDayCountConvention", dr["LiborDayCountConvention"].ToString());
                convention.Add("FutureDayCountConvention", dr["FutureDayCountConvention"].ToString());
                convention.Add("SwapFixedDayCountConvention", dr["SwapFixedDayCountConvention"].ToString());
                convention.Add("SwapFloatPaymentFrequency", dr["SwapFloatPaymentFrequency"].ToString());
                convention.Add("SwapFixedPaymentFrequency", dr["SwapFixedPaymentFrequency"].ToString());
            }
            dr.Close();
            db.Close();

            return convention;
        }

    }
}
