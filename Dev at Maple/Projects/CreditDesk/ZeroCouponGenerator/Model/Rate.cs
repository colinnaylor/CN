using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZeroCouponGenerator
{
    class Rate
    {
        public string SecType { get; set; }
        public string TermCode { get; set; }
        public double Bid { get; set; }
        public double Ask { get; set; }
        public double Price { get; set; }
        public DateTime Expiry { get; set; }

        public DateTime StartDate;
        public DateTime TermDate;
        public int Term;
        public int DaysInYear;
        public double YearFrac;
        public double SpotRate;
        public double RawSwapRate;
        public double SwapRate;
        public double DiscountFactor;


        public override string ToString()
        {

            string dateString = "";
            if (Expiry != null)
            {
                dateString = Expiry.ToString("ddMMMyyyy");
            }

            return string.Format("bid:{0} ask:{1} sectype:{2} termcode:{3} expiry{4}", Bid, Ask, SecType, TermCode, dateString);
        }

        public bool NotWanted { get; set; }
    }
}
