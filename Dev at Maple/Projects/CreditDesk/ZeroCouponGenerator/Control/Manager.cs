using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZeroCouponGenerator.Properties;
using Maple;

namespace ZeroCouponGenerator
{
    class Manager
    {

        public void Action(int runID = 0)
        {
            Utils.Log("In Action");
            Database db = new Database();

            try
            {
                if (runID == 0)
                {
                    db.GetTickerRecordSet();
                }

                Utils.Log("Getting list of currencies ... ");
                List<string> currencies = db.GetCurrency(runID);

                foreach (string currency in currencies)
                {
                    try {
                        Utils.Log(string.Format("Processing currency: {0}", currency));

                        Utils.Log(string.Format("Getting curve data: {0}", currency));

                        InputData input = db.GetCurveData(currency, runID);

                        Utils.Log(string.Format("Getting conventions: {0}", currency));
                        Dictionary<string, string> conventions = db.GetConvention(currency);

                        input.DayCountConvention = ZeroCouponGenerator.eNumConvert.GetDayCountEnum(conventions["DayCountConvention"]);
                        input.NextWorkingDay = ZeroCouponGenerator.eNumConvert.GetNextWorkingDayEnum(conventions["NextWorkingDay"]);
                        input.HolidayCentre = conventions["HolidayCentre"];
                        input.MaxFutureTermDays = int.Parse(conventions["MaxFutureTermInDays"]);
                        input.SettleDaysForFutures = int.Parse(conventions["SettleDaysForFutures"]);
                        input.SettleDaysForSwaps = int.Parse(conventions["SettleDaysForSwaps"]);
                        input.SettleDaysForLibors = int.Parse(conventions["SettleDaysForLibor"]);
                        input.LiborDayCountConvention = ZeroCouponGenerator.eNumConvert.GetDayCountEnum(conventions["LiborDayCountConvention"]);
                        input.FutureDayCountConvention = ZeroCouponGenerator.eNumConvert.GetDayCountEnum(conventions["FutureDayCountConvention"]);
                        input.SwapFixedDayCountConvention = ZeroCouponGenerator.eNumConvert.GetDayCountEnum(conventions["SwapFixedDayCountConvention"]);
                        input.SwapFloatPaymentFrequency = int.Parse(conventions["SwapFloatPaymentFrequency"]);
                        input.SwapFixedPaymentFrequency = int.Parse(conventions["SwapFixedPaymentFrequency"]);

                        Utils.Log(string.Format("Getting holidays: {0}", input.HolidayCentre));
                        input.Holidays = Functions.GetHolidays(input.HolidayCentre);

                        Utils.Log(string.Format("Creating curve: {0}", currency));

                        ZeroCouponCalc calc = new ZeroCouponCalc();
                        calc.Calc(input);

                        List<OutputPoint> output = calc.Output;

                        StringBuilder sbSql = new StringBuilder("INSERT tblRateTerm (Currency, Duration, Type, Forward, Spot, LastUpdated, UpdatedBy, UpdatedFrom, DayCountBasis) \r\n VALUES ");
                        string eol = ",";
#if DEBUG
                        eol = ",\r\n";
#endif
                        bool first = true;

                        foreach (OutputPoint point in output) {
                            if (first)
                                first = false;
                            else {
                                sbSql.Append(eol);
                            }

                            sbSql.AppendFormat("('{0}', {1}, 'ZR', 0, {2}, GETDATE(),'{3}','{4}',{5})",
                                        currency, point.Term, point.Rate, Environment.UserName, Environment.MachineName, conventions["DayCountConvention"]);
                        }
                        Utils.Log(string.Format("Writing curve points - Calling MMA DSN: {0} {1}", Settings.Default.MMADSN, sbSql.ToString()));
                        db.ExecSql(sbSql.ToString(), "Error inserting bbg data into tblRateTerm", Settings.Default.MMADSN, timeout: 30);
                    } catch (Exception ex) {
                        Utils.Log(ex.Message);
                        Utils.Log(ex.StackTrace);
                        Utils.Log("Aborting {0}".Args(currency));
                        string to = "London_RiskControl@mpuk.com";
#if DEBUG
                        to = "colin.naylor@mpuk.com";
#endif
                        string userMessage = "Please contact IT if rates need to be copied over. Currencies that did not generate an email, did not fail.";
                        // Limit email to between 15:15 and 15:45
                        if(DateTime.Now.Hour == 15){
                            if (DateTime.Now.Minute >= 15 && DateTime.Now.Minute < 45) {
                                Maple.Notifier.SendEmail(to, "Zero coupon generator error for {0}.".Args(currency),
                                    "{0}\r\nSee IT logs for detail\r\nS:\\dev\\Logs\\ZeroCouponGenerator\r\n\r\n{1}".Args(ex.Message, userMessage));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Utils.Log(ex.Message);
                Utils.Log(ex.StackTrace);
            }
        }
    }
}
