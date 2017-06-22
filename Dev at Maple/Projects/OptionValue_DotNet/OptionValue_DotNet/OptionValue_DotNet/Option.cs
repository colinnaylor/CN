using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OptionValue_DotNet
{
    /// <summary>
    /// An option to be valued (using the underlying C DLL - via a VB DLL!).
    /// Set the properties, call CalculateOptionValue for a given date and then inspect the OptionValue property
    /// </summary>
    public class Option
    {
        public OptionType Type { get; set; }
        public OptionStyle Style { get; set; }
        public double Strike { get; set; }
        public DateTime Maturity { get; set; }        
        public double UnderlyingPrice { get; set; }
        public double UnderlyingVolatility { get; set; }
        public List<Dividend> UnderlyingDividends { get; set; }        
        public double Rate { get; set; }
        private double optionValue;
        private DateTime valuationDate;

        public Option()
        {
            UnderlyingDividends = new List<Dividend>();
        }

        /// <summary>
        /// The value of the option - call CalculateOptionValue to recalculate this value
        /// </summary>
        public double OptionValue
        {
            get { return optionValue; }
        }

        /// <summary>
        /// The valuation date applied last, set when CalculateOptionValue is called
        /// </summary>
        public DateTime ValuationDate
        {
            get { return valuationDate; }
        }

        /// <summary>
        /// Re-calculate the value based on the current properties
        /// </summary>
        /// <param name="ValuationDate"></param>
        public void CalculateOptionValue(DateTime ValuationDate)
        {
            optionValue = OptionValueWrapper(ValuationDate);
            valuationDate = ValuationDate;
        }

        private double OptionValueWrapper(DateTime ValuationDate)
        {
            //unpack the dividends into the required format and pass to COM dll
            OptionValue32_CS.Pricer Pricer = new OptionValue32_CS.Pricer();

            return Pricer.OptionValue(
                Strike,
                UnderlyingPrice,
                UnderlyingVolatility,
                YearsToMaturity(ValuationDate),
                Rate,
                (short)Type,
                (short)Style,
                DividendString(ValuationDate));
        }

        private double YearsToMaturity(DateTime ValuationDate)
        {
            return ((Maturity - ValuationDate).Days) / 365.25d;
        }

        /// <summary>
        /// this is the format we pass the dividends into the VB OptionValuation function
        /// </summary>
        /// <param name="ValuationDate"></param>
        /// <returns></returns>
        private string DividendString(DateTime ValuationDate)
        {
            string divs = "";
            foreach (Dividend div in UnderlyingDividends)
            {
                divs += (div.DividendString(ValuationDate) + ",");

                //CA:   Exit after first dividend - there is a bug that causes the calculation
                //      to fail if for more than one dividend...need to fix in VB DLL
                break;
            }
            divs.TrimEnd(",".ToCharArray());
            return divs;            
            
        }


        /// <summary>
        /// The value of the option - call CalculateOptionValue to recalculate this value
        /// </summary>
        public string DisplayDividendString
        {
            get 
            {
                string divs = "";
                foreach (Dividend div in UnderlyingDividends)
                {
                    divs += (div.DisplayDividendString() + " ");
                }
                divs.TrimEnd(" ".ToCharArray());
                return divs;                
            }
        }

        public enum OptionType
        {
            Put = 0,
            Call = 1            
        }

        public enum OptionStyle
        {
            European = 0,
            American = 1
        }
    }
}
