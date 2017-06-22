using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BOSS_OptionValueManager
{
    /// <summary>
    /// A saved Bloomberg data item - the source data for volatilites and rates in the calculation
    /// </summary>
    public struct BBData
    {
        public String Ticker { get; set; }
        public String Field { get; set; }
        public double Value { get; set; }
        public override string ToString()
        {
            return String.Format("{0}.{1}({2})", Ticker, Field, Value);
        }
    }
}
