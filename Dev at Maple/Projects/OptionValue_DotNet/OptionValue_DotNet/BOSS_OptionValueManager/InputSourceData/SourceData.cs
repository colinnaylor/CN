using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BOSS_OptionValueManager.InputSourceData
{
    /// <summary>
    /// base class for all import datasources
    /// </summary>
    public abstract class SourceData
    {
        /// <summary>
        /// Initialise the source as missing
        /// </summary>
        public SourceData()
        {
            Source = InputSource.Missing;
            CaptureTime = null;
        }

        /// <summary>
        /// where has this data come from?
        /// </summary>
        public InputSource Source { get; set; }

        /// <summary>
        /// What time was the data captured?
        /// </summary>
        public DateTime? CaptureTime { get; set; }
    }
}
