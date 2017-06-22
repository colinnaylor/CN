using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BOSS_OptionValueManager
{
    /// <summary>
    /// Store the original ccy of a dividend along with the FX rate applied
    /// </summary>
    public class DividendWithCurrency: OptionValue_DotNet.Dividend
    {

        /// <summary>
        /// Just pass in the fx'ed number to the base class - it is not care that the figure has been fx'ed
        /// </summary>
        /// <param name="exdate"></param>
        /// <param name="amount"></param>
        /// <param name="currency"></param>
        /// <param name="fxrate"></param>
        public DividendWithCurrency(DateTime exdate, double amount, string currency, double fxrate)
            : base(exdate, amount / fxrate)
        {
            Currency = currency;
            FXRate = fxrate;
            BaseAmount = amount;
        }

        public string Currency { get; set; }
        public double FXRate { get; set; }
        public double BaseAmount { get; set; }

        /// <summary>
        /// show fx details if fx'ed
        /// </summary>
        /// <returns></returns>
        public override string DisplayDividendString()
        {
            if (FXRate == 1)
                return base.DisplayDividendString();
            else
                return string.Format("{0},BaseDiv={1},Ccy={2},FX={3}", base.DisplayDividendString(), BaseAmount.ToString("0.000000"), Currency, FXRate.ToString("0.000000"));
        }
    }
}
