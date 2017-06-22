using BBDataWarehouseCacheManager.Controllers;

namespace BBDataWarehouseCacheManager.Models
{
    public class BergTicker
    {
        public BergTicker(string inputTicker)
        {
            var mgr = new TickerManager();
            RawValue = inputTicker;
            BergTickerClean = inputTicker.ToUpper().Trim().Replace("  ", " ");
            TorLongType = mgr.GetTorontoLongType(BergTickerClean);
            if (TorLongType == "UNKNOWN")
                Utils.Logger.Info(TorLongType + " - " + inputTicker);
            else
                Utils.Logger.Info("\t\t " + TorLongType + " - " + inputTicker);

            TorTickerLookup = mgr.GetPrimaryLookUpString(BergTickerClean, TorLongType);
            TorTickerLookup2 = mgr.GetSecondaryLookUpString(BergTickerClean, TorLongType);
            BergIdType = mgr.GetBergIdType(BergTickerClean, TorLongType);

            TorId = mgr.GetTorontoId(this);

            TorPullType = mgr.GetTorontoPullType(TorLongType);
        }

        public string RawValue;
        public string BergIdType;
        public string BergTickerClean;
        public string TorTickerLookup;
        public string TorTickerLookup2;

        public string TorId;
        public string TorLongType;
        public string TorPullType;

        public string BloombergSuffix;
    }
}