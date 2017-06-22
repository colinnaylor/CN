using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZeroCouponGenerator {
    class InputData {
        public List<Rate> LiborData;
        public List<Rate> FutureData;
        public List<Rate> SwapData;
        public List<Rate> Rates;

        public DateTime StartDate;

        public DayCountConvention DayCountConvention;
        public DayCountConvention LiborDayCountConvention;
        public DayCountConvention FutureDayCountConvention;
        public DayCountConvention SwapFixedDayCountConvention;

        public NextWorkingDayConvention NextWorkingDay;

        public string HolidayCentre;
        public Dictionary<DateTime, DateTime> Holidays;

        public int MaxFutureTermDays;
        public int SettleDaysForFutures;
        public int SettleDaysForSwaps;
        public int SettleDaysForLibors;
        public int SwapFloatPaymentFrequency;
        public int SwapFixedPaymentFrequency;


        internal void PrintRates(List<Rate> theRates) {

            foreach (Rate rate in theRates) {
                double theRate = rate.SpotRate;
                if (theRate == 0) { theRate = rate.SwapRate; }
                if (theRate == 0) { theRate = rate.RawSwapRate; }
                string data = rate.TermDate.ToString("dd MMM yy") + "\t" + rate.Term + "\t" + theRate.ToString("0.000000");
                Console.Out.WriteLine(data);
            }
        }
    }
}
