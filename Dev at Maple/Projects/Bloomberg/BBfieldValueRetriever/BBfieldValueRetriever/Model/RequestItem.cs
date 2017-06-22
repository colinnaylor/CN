using System;
using System.Collections.Generic;
using Shared;

namespace BBfieldValueRetriever.Model
{
    public class RequestItem
    {
        public BloombergDataInstrument.eRequestType RequestType { get; set; }

        public int ID { get; set; }

        public string OriginalInputTicker { get; set; }

        public string BBTicker { get; set; }

        public string TickerDownloadAssetClass { get; set; }

        public string BBFieldList { get; set; }

        public string UserId { get; set; }

        public DateTime InsertedWhen { get; set; }

        public DateTime DateFrom { get; set; }

        public DateTime DateTo { get; set; }

        public DateTime CacheMaxAgeTolerance { get; set; }

        public string Periodicity { get; set; }

        public string Errors { get; set; }

        public string EventType { get; set; }

        public bool SendToBloomberg { get; set; }

        // Placeholder for the fields that are appropriate to this RequestItem
        public Dictionary<string, RequestItemField> riFields = new Dictionary<string, RequestItemField>();

        // A dic keyed on date, each date containing an array of values
        public Dictionary<DateTime, string[]> Data = new Dictionary<DateTime, string[]>();

        public BloombergDatawarehouseData WarehouseData;
    }

    public class RequestItemField
    {
        public RequestItemField(string name)
        {
            Name = name;
        }

        public string Name { get; set; }

        public List<OverrideField> OverrideFields = new List<OverrideField>();

        public string Key
        {
            get
            {
                string ret = Name.ToLower();
                //if (OverrideFields.Count > 0) {
                //    ret += "[]";
                //}
                return ret;
            }
        }
    }

    public class OverrideField
    {
        public OverrideField(string name, string value)
        {
            Name = name;
            Value = value;
        }

        public string Name { get; set; }

        public string Value { get; set; }
    }
}