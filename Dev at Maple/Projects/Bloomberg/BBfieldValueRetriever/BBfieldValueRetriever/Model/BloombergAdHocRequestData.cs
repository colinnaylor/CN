using System;
using System.Text.RegularExpressions;

namespace BBfieldValueRetriever.Model
{
    public class BloombergAdHocRequestData
    {
        public string MappedFriendlyTicker()
        {
            if (OriginalRequestItem.OriginalInputTicker.EndsWith("SEDOL1", StringComparison.OrdinalIgnoreCase))
                return OriginalRequestItem.OriginalInputTicker;
            return Regex.Replace(OriginalRequestItem.BBTicker, " CURNCY", " Crncy", RegexOptions.IgnoreCase);
        }

        public BloombergAdHocRequestData()
        {
        }

        public BloombergAdHocRequestData(RequestItem requestItem)
        {
            OriginalRequestItem = requestItem;
        }

        public int bloombergAdHocId { get; set; }

        public RequestItem OriginalRequestItem { get; set; }

        public string ReturnData { get; set; }

        public string SecurityIdentifier { get; set; }

        public string FieldsToPull { get; set; }
    }
}