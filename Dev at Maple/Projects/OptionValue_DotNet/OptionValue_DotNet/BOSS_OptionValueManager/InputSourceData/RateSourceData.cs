using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BOSS_OptionValueManager
{
    /// <summary>
    /// The rate to use along with information about when/where the data has come from.
    /// If the rate is from Bloomberg the prev/next points indicate the values used to interpolate the rate at maturity
    /// </summary>
    public class RateSourceData : InputSourceData.SourceData
    {
        /// <summary>
        /// What BB ticker was used as the lower point in the rate interpolation i.e EE0001M INDEX, BP0004M INDEX...
        /// </summary>
        public BBData BBData_PreviousTerm { get; set; }

        /// <summary>
        /// What BB ticker was used as the upper point in the interpolation i.e EE0002M INDEX, BP0005M INDEX
        /// </summary>
        public BBData BBData_NextTerm { get; set; }

        /// <summary>
        /// The rate used
        /// </summary>
        public double Rate { get; set; }

        public override string ToString()
        {
            if (Source == InputSourceData.InputSource.Override)
                return String.Format("{0}:{1}", Source, CaptureTime.ToString());
            else if (Source == InputSourceData.InputSource.Bloomberg)
                return String.Format("{0}:{1}:{2}:{3}", Source, CaptureTime.ToString(),BBData_PreviousTerm.ToString(),BBData_NextTerm.ToString());
            else if (Source == InputSourceData.InputSource.Missing)
                return "MISSING";
            else
                return base.ToString();
        }

    }
}
