using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BOSS_OptionValueManager.InputSourceData
{
    /// <summary>
    /// The source of rate/vol/div data.    
    /// </summary>
    public enum InputSource
    {
        /// <summary>
        /// Manually entered by user
        /// </summary>
        Override,
        /// <summary>
        /// Captured from Bloomberg in the daily download
        /// </summary>
        Bloomberg,
        /// <summary>
        /// From BOSS (Dividends only at present)
        /// </summary>
        BOSS,
        /// <summary>
        /// Data not found
        /// </summary>
        Missing
    }

}
