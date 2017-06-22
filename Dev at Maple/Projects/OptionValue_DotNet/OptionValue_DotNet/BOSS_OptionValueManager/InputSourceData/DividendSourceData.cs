using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BOSS_OptionValueManager
{
    /// <summary>
    /// The list of dividends to use along with information about when/where the data has come from
    /// </summary>
    public class DividendSourceData : InputSourceData.SourceData
    {
        /// <summary>
        /// instantiate the divideds list
        /// </summary>
        public DividendSourceData():base()
        {
            Dividends = new List<DividendWithCurrency>();
        }

        /// <summary>
        /// The dividends
        /// </summary>
        public List<DividendWithCurrency> Dividends { get; set; }

        /// <summary>
        /// return the currency of the first div if there is one
        /// </summary>
        public string DividendCurrency 
        {
            get
            {
                if (Dividends.Count == 0)
                    return "";
                else
                    return Dividends[0].Currency;
            }
        }

        /// <summary>
        /// return the fxrate of the first div if there is one
        /// </summary>
        public double DividendFXRate
        {
            get
            {
                if (Dividends.Count == 0)
                    return 1;
                else
                    return Dividends[0].FXRate;
            }
        }

        public override string ToString()
        {            
            return String.Format("{0}:{1}", Source, CaptureTime.ToString());            
        }


    }
}
