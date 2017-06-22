using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BOSS_OptionValueManager
{
    /// <summary>
    /// The volatility to use along with information about when/where the data has come from
    /// </summary>
    public class VolatilitySourceData : InputSourceData.SourceData
    {
        /// <summary>
        /// What BB ticker was used to download the volatility (provided if source is bloomberg)
        /// </summary>
        public BBData BBData { get; set; }

        public double Volatility { get; set; }

        public override string ToString()
        {
            if (Source == InputSourceData.InputSource.Override)
                return String.Format("{0}:{1}", Source, CaptureTime.ToString());
            else if (Source == InputSourceData.InputSource.Bloomberg)
                return String.Format("{0}:{1}:{2}", Source, CaptureTime.ToString(), BBData.ToString());
            else if (Source == InputSourceData.InputSource.Missing)
                return "MISSING";
            else
                return base.ToString();
        }
    }
}
