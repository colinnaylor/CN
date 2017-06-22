using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OptionValue_DotNet
{
    /// <summary>
    /// A dividend: ex-date and value.
    /// </summary>
    public class Dividend
    {
        private Dividend() { }
        public Dividend(DateTime exdate, double amount)
        {
            ExDate = exdate;
            Amount = amount;
        }
        public DateTime ExDate { get; private set; }
        public double Amount { get; private set; }
        private double YearsToExDate(DateTime ValuationDate)
        {
            return ((ExDate - ValuationDate).Days) / 365.25d;
        }

        /// <summary>
        /// this is the format we pass the dividends into the VB OptionValuation function
        /// i.e. 0.55/0.0045
        /// </summary>
        /// <param name="ValuationDate"></param>
        /// <returns></returns>
        public string DividendString(DateTime ValuationDate)
        {
            return string.Format("{0}/{1}", YearsToExDate(ValuationDate), Amount);
        }

        /// <summary>
        /// this is the format we are using to display divs to users
        /// i.e. 01/01/09(0.0045)
        /// </summary>
        /// <returns></returns>
        public  virtual string DisplayDividendString()
        {
            return string.Format("{0}({1})", ExDate.ToString("dd/MM/yy"), Amount.ToString("0.000000"));
        }
    }
}
