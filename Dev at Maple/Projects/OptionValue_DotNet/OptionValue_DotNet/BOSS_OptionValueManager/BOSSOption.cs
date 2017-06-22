using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OptionValue_DotNet;

namespace BOSS_OptionValueManager
{
    /// <summary>
    /// Extends the option class to include some BOSS specific data.
    /// Also handles retrieving vol/rate/dividend information
    /// </summary>
    public class BOSSOption:Option
    {
        /// <summary>
        /// The BOSS SecurityID
        /// </summary>
        public int SecurityID { get; set; }

        /// <summary>
        /// The BOSS Security Name
        /// </summary>
        public string SecurityName { get; set; }

        /// <summary>
        /// The BOSS Underlying Security Name
        /// </summary>
        public string UnderlyingSecurityName { get; set; }

        /// <summary>
        /// When was this option first traded in BOSS
        /// </summary>
        public DateTime FirstTraded { get; set; }

        private VolatilitySourceData volatilitySource;
        /// <summary>
        /// if a manual override has been provided by the user, use that, otherwise we get the vol from the stored bloomberg data (captured daily)
        /// </summary>
        public VolatilitySourceData VolatilitySource 
        {
            get { return volatilitySource; }
            set 
            { 
                volatilitySource = value;
                base.UnderlyingVolatility = value.Volatility;
            }
        }

        private RateSourceData rateSource;
        /// <summary>
        /// if a manual override has been provided by the user, use that, otherwise we get the rate from the stored bloomberg data (captured daily)
        /// </summary>
        public RateSourceData RateSource
        {
            get { return rateSource; }
            set
            {
                rateSource = value;
                base.Rate = value.Rate;
            }
        }

        private DividendSourceData dividendSource;
        /// <summary>
        /// if a manual override has been provided by the user, use that, otherwise we get the divs from BOSS
        /// </summary>
        public DividendSourceData DividendSource
        {
            get { return dividendSource; }
            set
            {
                dividendSource = value;
                base.UnderlyingDividends.Clear();
                foreach (DividendWithCurrency dwc in value.Dividends)
                {
                    base.UnderlyingDividends.Add(dwc);                    
                }                
            }
        }



        #region WingDing Characters - for binding in grid...!
        public string DividendSourceCharacter
        {
            get
            {
                // as dividends are an optional input - do not show missing if not present
                if (dividendSource.Dividends.Count == 0)
                    return "";
                else
                    return Utilities.GetSourceCharacter(DividendSource.Source);
            }
        }
        public string VolatilitySourceCharacter
        {
            get
            {
                return Utilities.GetSourceCharacter(VolatilitySource.Source);
                
            }
        }
        public string RateSourceCharacter
        {
            get
            {
                return Utilities.GetSourceCharacter(RateSource.Source);
            }
        }

        public string StatusCharacter
        {
            get
            {
                if (base.OptionValue <0)
                    return "û";
                else
                    return "ü";                
            }
        }

        public string IsNewlyTradedCharacter
        {
            get
            {
                //new option
                if (FirstTraded == ValuationDate)
                    return "µ";
                else
                    return "";
            }
        }

        #endregion


    }
}
