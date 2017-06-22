using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using BBfieldValueRetriever.Model;
using Maple;
using NLog;
using Shared;

namespace BBfieldValueRetriever.Control
{
    public class BloombergApiController : BloombergDatasourceController
    {
        public event MessageEventDelegate MessageEvent;

        private int _hitsWarningLevel;
        private int _hitsLimit;
        private int _newHits;
        private int _totalHitsToday;
        private DateTime _currentDay;
        protected AutoResetEvent Completed = new AutoResetEvent(true);

        public BloombergApiController(BergController mainController)
        {
            Datasource = "BLAPI";
            this.MainController = mainController;
            Db = mainController.Db;
            Db.ReadSettings(out _hitsWarningLevel, out _hitsLimit, out _totalHitsToday);
            _currentDay = DateTime.Now.Date;
            ReportHits(_hitsWarningLevel, _hitsLimit, _totalHitsToday);
        }

        /// <summary>
        /// Controls when to set the system status
        /// </summary>

        private List<RequestItem> _requestItems = new List<RequestItem>();

        public List<RequestItem> RetrieveSynchronously(List<RequestItem> theseRequestItems)
        {
            _requestItems = theseRequestItems;
            var bbdis = ComposeBBobjects(_requestItems);
            GetBbgData(bbdis);
            return _requestItems;
        }

        public void ProcessDataRequests(List<RequestItem> theseRequestItems)
        {
            if (_totalHitsToday >= _hitsLimit || _currentDay != DateTime.Now.Date)
            {
                // Check limits
                NLogger.Instance.Info("Checking hits counters. Currently: hits: {0} warning: {1} limit: {2}", _totalHitsToday, _hitsWarningLevel, _hitsLimit);
                Db.ReadSettings(out _hitsWarningLevel, out _hitsLimit, out _totalHitsToday);
                NLogger.Instance.Info("Checking hits counters. Now: hits: {0} warning: {1} limit: {2}", _totalHitsToday, _hitsWarningLevel, _hitsLimit);
                ReportHits(_hitsWarningLevel, _hitsLimit, _totalHitsToday);

                _currentDay = DateTime.Now.Date;
            }

            RaiseMessageEvent(string.Format("Checking at {0}", DateTime.Now.ToString("HH:mm:ss")));

            _requestItems = theseRequestItems;

            List<BloombergDataInstrument> bbdis = null;
            if (_requestItems.Count > 0)
            {
                RaiseMessageEvent(string.Format("Processing {0} requests", _requestItems.Count));
                // Unique items to BB
                bbdis = ComposeBBobjects(_requestItems);
            }

            if (bbdis == null || bbdis.Count == 0)
            {
                if (_requestItems.Count > 0)
                {
                    // Save any error messages
                    NLogger.Instance.Info("Saving errors...");
                    Db.SaveValues(_requestItems);
                    CostReportUserAttribution(_requestItems);
                }
                // Nothing to do , ok to restart next process
            }
            else
            {
                _newHits = CalcHits(bbdis);

                if (HitsExceeded())
                {
                    // Show down timer, no need to work too hard if the limit has been breached
                    // Allows a developer to adjust the limits if needed and will be picked up after this interval
                }
                else
                {
                    RaiseMessageEvent("Recording Hits");  // Record before going to BB to be on the safe side
                    Db.RecordHits(_newHits);
                    GetBbgData(bbdis); //sychronous
                }
            }
        }

        private void ReportHits(int hitsWarningLevel, int hitsLimit, int totalHitsToday)
        {
            RaiseMessageEvent(string.Format("hitsWarning {0}", hitsWarningLevel.ToString("#,##0")));
            RaiseMessageEvent(string.Format("hitsLimit   {0}", hitsLimit.ToString("#,##0")));
            RaiseMessageEvent(string.Format("hitsTotal   {0}", totalHitsToday.ToString("#,##0")));
        }

        private DateTime _warningEmailSent = DateTime.MinValue;
        private DateTime _limitEmailSent = DateTime.MinValue;

        private bool HitsExceeded()
        {
            bool ret = false;

            _totalHitsToday += _newHits;
            ReportHits(_hitsWarningLevel, _hitsLimit, _totalHitsToday);

            if (_totalHitsToday >= _hitsWarningLevel)
            {
                if (_warningEmailSent != DateTime.Now.Date)
                {
                    string msg = string.Format("Total Bloomberg hits today on {0} is now at {1}. The warning level is {2} " +
                        "and the hit limit for {0} is {3}", Environment.MachineName, _totalHitsToday, _hitsWarningLevel, _hitsLimit);
                    Static.Email(string.Format("BB Hits warning on {0}", Environment.MachineName), msg);
                    NLogger.Instance.Info("BB Hits warning on {0}", Environment.MachineName);
                    _warningEmailSent = DateTime.Now.Date;
                }
            }

            if (_totalHitsToday >= _hitsLimit)
            {
                ret = true;

                if (_limitEmailSent != DateTime.Now.Date)
                {
                    string msg = string.Format("Total Bloomberg hits today on {0} would exceed the hit limit of {1}. No more Bloomberg requests " +
                        "will be made today on {0} by the BBfieldValueRetriever", Environment.MachineName, _hitsLimit);
                    Static.Email(string.Format("BB Hits warning on {0}", Environment.MachineName), msg);
                    NLogger.Instance.Info("BB Hits warning on {0}", Environment.MachineName);

                    _limitEmailSent = DateTime.Now.Date;
                }
            }

            return ret;
        }

        public int CalcHits(List<BloombergDataInstrument> bbdis)
        {
            int ret = 0;
            foreach (BloombergDataInstrument bdi in bbdis)
            {
                switch (bdi.RequestType)
                {
                    case BloombergDataInstrument.eRequestType.Reference:
                    case BloombergDataInstrument.eRequestType.Historic:
                        foreach (string fld in bdi.BBFields.Keys)
                        {
                            ret++;
                        }
                        break;

                    case BloombergDataInstrument.eRequestType.IntraDayBar:
                        ret++;   // We believe this is just one, being the eventType field
                        //foreach (string fld in bdi.BBFields.Keys) {
                        //    ret++;
                        //}
                        break;

                    default:
                        Static.Email("CalcHits switch default", string.Format("Unhandled RequestType of [{0}] in CalcHits.", bdi.RequestType));
                        break;
                }
            }

            return ret;
        }

        private void bbd_ProcessCompleted(List<BloombergDataInstrument> instruments)
        {
            try
            {
                RaiseMessageEvent("Retrieving results");
                AddResultsToRequestItems(instruments);

                RaiseMessageEvent("Saving to db");
                Db.SaveValues(_requestItems);
                CostReportUserAttribution(_requestItems);
                RaiseMessageEvent(string.Format("Completed {0} items.", _requestItems.Count));
            }
            catch (Exception ex)
            {
                RaiseMessageEvent("Error: " + ex);
                Static.Email("Error encountered in BBFieldValueRetriever in bbd_ProcessCompleted", ex.ToString());
            }
            finally
            {
                Completed.Set();
                // Set var so that the next process can start
            }
        }

        public List<BloombergDataInstrument> ComposeBBobjects(List<RequestItem> requestItems)
        {
            List<BloombergDataInstrument> tickerList = new List<BloombergDataInstrument>();
            int lastId = 0;
            string lastTicker = "";
            BloombergDataInstrument.eRequestType lastRequestType = BloombergDataInstrument.eRequestType.NotSet;
            BloombergDataInstrument bdi = null;

            foreach (RequestItem ri in requestItems)
            {
                if (ri.SendToBloomberg)
                {
                    bool newBdi = false;
                    // For reference types, we can place the fields into the same bdi for speed
                    if (lastRequestType == ri.RequestType
                        && ri.RequestType == BloombergDataInstrument.eRequestType.Reference)
                    {
                        if (string.Compare(lastTicker, ri.BBTicker, true) != 0)
                        {
                            newBdi = true;
                        }
                    }
                    else
                    {
                        if (lastId != ri.ID)
                        {
                            newBdi = true;
                        }
                    }
                    lastRequestType = ri.RequestType;
                    lastTicker = ri.BBTicker;
                    lastId = ri.ID;

                    if (newBdi)
                    {
                        // Next instrument class
                        bdi = new BloombergDataInstrument();
                        bdi.ResponseError = "";

                        bdi.RequestType = ri.RequestType;
                        bdi.ID = ri.ID;

                        bdi.EventType = ri.EventType == null ? null : ri.EventType.ToUpper();

                        bdi.Ticker = ri.BBTicker;

                        if (ri.DateFrom > DateTime.MinValue)
                        {
                            bdi.DateFrom = ri.DateFrom;
                        }
                        if (ri.DateTo > DateTime.MinValue)
                        {
                            bdi.DateTo = ri.DateTo;
                        }
                        else
                        {
                            // Just one day
                            bdi.DateTo = bdi.DateFrom;
                        }

                        bdi.Periodicity = ri.Periodicity;
                    }

                    foreach (RequestItemField field in ri.riFields.Values)
                    {
                        AddBBfield(bdi, field, ri.ID);
                    }

                    if (newBdi)
                    {
                        tickerList.Add(bdi);
                    }
                }
            }

            return tickerList;
        }

        private void AddBBfield(BloombergDataInstrument bdi, RequestItemField Field, int id)
        {
            // Ignore duplicate field names
            if (!bdi.BBFields.ContainsKey(Field.Key))
            {
                BloombergDataInstrumentField field = new BloombergDataInstrumentField(Field.Key);
                field.ID = id;

                foreach (OverrideField oField in Field.OverrideFields)
                {
                    field.FieldOverrides.Add(oField.Name, oField.Value);
                }

                bdi.BBFields.Add(Field.Key, field);
            }
        }

        /// <summary>
        /// Indicates when the retrieval process has completed.
        /// </summary>

        virtual public void GetBbgData(List<BloombergDataInstrument> bbdis)
        {
            BloombergData bbd = new BloombergData();

            RaiseMessageEvent("Retrieving data");

            bbd.PercentCompleteChanged += bbd_PercentCompleteChanged;
            bbd.ProcessCompleted += bbd_ProcessCompleted;
            bbd.StatusChanged += bbd_StatusChanged;

            //wait here ! look for completed to be set

            bbd.GetBloombergData(bbdis);
            Completed.Reset(); Completed.WaitOne();

            NLogger.Instance.Info("Completed");
        }

        private static void bbd_PercentCompleteChanged(int percentComplete)
        {
            NLogger.Instance.Info("Completed " + percentComplete + "%");
        }

        private void AddResultsToRequestItems(List<BloombergDataInstrument> instruments)
        {
            // Now for all requests, find the data
            foreach (RequestItem ri in _requestItems)
            {
                if (!ri.SendToBloomberg) continue;

                ri.Errors = "";
                BloombergDataInstrument source = null;

                source = instruments.FirstOrDefault(x =>
                    (ri.RequestType == BloombergDataInstrument.eRequestType.Reference && x.Ticker.ToUpper() == ri.BBTicker.ToUpper()) ||
                    (ri.RequestType != BloombergDataInstrument.eRequestType.Reference && x.ID == ri.ID));

                if (source != null)
                {
                    if (source.ResponseError != "")
                    {
                        ri.Errors = source.ResponseError;
                    }
                    else
                    {
                        if (source.IsSecurityValid)
                        {
                            // If we have dates coming back from Bloomberg, create an entry for each date returned
                            // otherwise time stamp with Now
                            if (source.BBFields.ContainsKey("date"))
                            {
                                List<object> list = (List<object>)source.BBFields["date"].Value;
                                if (list != null)
                                {
                                    foreach (object val in list)
                                    {
                                        DateTime timeStamp;
                                        if (DateTime.TryParse(val.ToString(), out timeStamp))
                                        {
                                            ri.Data.Add(timeStamp, new string[source.BBFields.Count - 1]);
                                        }
                                        else
                                        {
                                            throw new Exception("Expected date not found in AddResultsToRequestItems");
                                        }
                                    }
                                }
                            }
                            else if (source.BBFields.ContainsKey("time"))
                            {
                                List<object> list = (List<object>)source.BBFields["time"].Value;
                                if (list != null)
                                {
                                    foreach (object val in list)
                                    {
                                        DateTime timeStamp;
                                        if (DateTime.TryParse(val.ToString(), out timeStamp))
                                        {
                                            ri.Data.Add(timeStamp, new string[source.BBFields.Count - 1]);
                                        }
                                        else
                                        {
                                            throw new Exception("Expected time not found in AddResultsToRequestItems");
                                        }
                                    }
                                }
                            }
                            else
                            {
                                ri.Data.Add(DateTime.Now, new string[source.BBFields.Count]);
                            }

                            // As we need to get a list of values from a list of fields and add it to a dictionary,
                            // it is easier to do using numbers and arrays
                            List<string[]> array = new List<string[]>();
                            foreach (string[] sa in ri.Data.Values)
                            {
                                array.Add(sa);
                            }

                            // Loop through each field and find the values
                            int fieldPos = 0;
                            foreach (RequestItemField field in ri.riFields.Values)
                            {
                                BloombergDataInstrumentField fld = source.BBFields[field.Key];
                                if (fld.Name != "date" && fld.Name != "time")
                                { // handled earlier and is the key in the ri.Data dictionary
                                    if (fld.Value != null)
                                    {
                                        if (fld.Value.GetType().Name == "List`1")
                                        {
                                            List<object> list = (List<object>)fld.Value;

                                            for (int timeLine = 0; timeLine < list.Count; timeLine++)
                                            {
                                                array[timeLine][fieldPos] = GetStringValue(list[timeLine]);
                                            }
                                        }
                                        else
                                        {
                                            array[0][fieldPos] = GetStringValue(fld.Value);
                                        }
                                    }
                                    else
                                    {
                                        string err = fld.Error;
                                        // Sometimes no error is returned even when the Value is null
                                        if (err == null)
                                        {
                                            err = "N.A.";
                                        }
                                        ri.Errors += string.Format("[{0}|{1}]", fld.Name, err);
                                    }
                                    fieldPos++;
                                }
                            }
                        }
                        else
                        {
                            // Invalid security
                            ri.Errors = source.SecurityErrors;
                        }
                    }
                    NLogger.Instance.Info("at end");
                }
                else
                {
                    // No data returned
                    ri.Errors = "No data returned from BB";
                }
            }
        }

        private string GetStringValue(object value)
        {
            string ret = "";
            string dataType = value.GetType().ToString();
            try
            {
                switch (dataType)
                {
                    case "Bloomberglp.Blpapi.Datetime":
                        DateTime d = DateTime.Parse(value.ToString());
                        ret = d.ToString("yyyyMMdd HH:mm:ss");
                        break;

                    case "System.String":
                    case "System.Char":
                    case "System.Double":
                    case "System.Int32":
                        ret = value.ToString();
                        break;

                    default:
                        ret = value.ToString();
                        Static.Email("Missing data type in BBfieldValueRetriever", "The GetStringValue in the Bloomberg.cs needs to be altered " +
                            "to include an unhandled Bloomberg data type of " + dataType + " in the Switch statement.");
                        break;
                }
            }
            catch
            {
                Static.Email("Missing data type in BBfieldValueRetriever", "The GetStringValue in the Bloomberg.cs needs to be altered " +
                    "to include an unhandled Bloomberg data type of " + dataType + " in the Switch statement.");
            }
            return ret;
        }

        private void bbd_StatusChanged(string status)
        {
            //Use maple log functionality
            NLogger.Instance.Info(status);
        }

        protected void RaiseMessageEvent(string message)
        {
            /// To avoid a race condition where the last handler can be removed
            /// between the null check and the invocation of the event, event
            /// sources should also create a copy of the event before performing
            /// the null check and raising the event.
            MessageEventDelegate temp = MessageEvent;
            if (temp != null)
            {
                MessageEventArgs e = new MessageEventArgs(message);
                temp(this, e);
            }
        }

        internal static void ValidateRequest(RequestItem ri)
        {
            ri.SendToBloomberg = true;

            Dictionary<BloombergDataInstrument.eRequestType, bool> requestTypes = new Dictionary<BloombergDataInstrument.eRequestType, bool>();
            // Handled request types
            requestTypes.Add(BloombergDataInstrument.eRequestType.Reference, true);
            requestTypes.Add(BloombergDataInstrument.eRequestType.Historic, true);
            requestTypes.Add(BloombergDataInstrument.eRequestType.IntraDayBar, true);

            if (!requestTypes.ContainsKey(ri.RequestType))
            {
                ri.Errors += "Request type specified is either missing or invalid. ";
            }

            if (ri.DateFrom > DateTime.Now || ri.DateTo > DateTime.Now)
            {
                ri.Errors += "Date specified is in the future. ";
            }

            // Blank defaults to DAILY
            var periodicities = new List<string> { "", "DAILY", "WEEKLY", "MONTHLY", "QUARTERLY", "SEMI_ANNUALLY", "YEARLY" };

            if (ri.RequestType == BloombergDataInstrument.eRequestType.Historic
                && !periodicities.Contains(ri.Periodicity))
            {
                ri.Errors = "Invalid periodicity. Choices are DAILY WEEKLY MONTHLY QUARTERLY SEMI_ANNUALLY YEARLY. ";
            }
            if (ri.RequestType == BloombergDataInstrument.eRequestType.IntraDayBar)
            {
                int p;
                if (!int.TryParse(ri.Periodicity, out p))
                {
                    ri.Errors = "Invalid periodicity. Must be an integer. ";
                }

                if (ri.EventType == null || !_intraDayEventTypeFields.ContainsKey(ri.EventType))
                {
                    ri.Errors = "First column in the FieldList must be one of ";

                    foreach (string f in _intraDayEventTypeFields.Keys)
                    {
                        ri.Errors += f + " ";
                    }
                    ri.Errors = ri.Errors.Substring(0, ri.Errors.Length - 1) + ". ";
                }
            }

            if (ri.Errors != "")
            {
                ri.SendToBloomberg = false;
            }
        }

        private static Dictionary<string, bool> _intraDayEventTypeFields = new Dictionary<string, bool>();

        internal static void AddFieldFromFieldList(RequestItem ri)
        {
            switch (ri.RequestType)
            {
                case BloombergDataInstrument.eRequestType.IntraDayBar:
                case BloombergDataInstrument.eRequestType.IntraDayTick:
                    if (_intraDayEventTypeFields.Count == 0)
                    {
                        _intraDayEventTypeFields.Add("bid", true);
                        _intraDayEventTypeFields.Add("ask", true);
                        _intraDayEventTypeFields.Add("trade", true);
                        _intraDayEventTypeFields.Add("ask_best", true);
                        _intraDayEventTypeFields.Add("bid_best", true);
                    }
                    break;
            }

            string fields = ri.BBFieldList;

            // Fields may be a single field or may be a comma seperated list
            // or they may contains Override fields within square brackets
            while (fields != "")
            {
                RequestItemField fld = null;
                int comma = fields.IndexOf(",");
                int bracket = fields.IndexOf("[");

                if (comma == -1 && bracket == -1)
                {
                    // a lonesome field
                    fld = new RequestItemField(fields);
                    fields = "";
                }
                else if (comma == -1)
                {
                    // Just a bracket found
                    int end = fields.IndexOf("]");
                    string overrides = fields.Substring(0, end + 1);
                    fields = fields.Substring(end + 1);
                    fld = ProcessOverrides(ri, overrides);
                }
                else if (bracket == -1)
                {
                    // Just a comma found
                    fld = new RequestItemField(fields.Substring(0, comma));
                    fields = fields.Substring(comma + 1);
                }
                else
                {
                    // Both a bracket and commas, process in order.
                    if (comma < bracket)
                    {
                        fld = new RequestItemField(fields.Substring(0, comma));
                        fields = fields.Substring(comma + 1);
                    }
                    else
                    {
                        int end = fields.IndexOf("]");
                        string overrides = fields.Substring(0, end + 1);
                        fields = fields.Substring(end + 1);
                        fld = ProcessOverrides(ri, overrides);
                    }
                }

                if (ri.RequestType == BloombergDataInstrument.eRequestType.IntraDayBar
                    && _intraDayEventTypeFields.ContainsKey(fld.Key))
                {
                    ri.EventType = fld.Key;
                }
                else
                {
                    ri.riFields.Add(fld.Key, fld);
                }
            }
        }

        private static RequestItemField ProcessOverrides(RequestItem ri, string overrides)
        {
            // Overrides come in the form "FieldName[fieldName,fieldValue,fieldName,fieldValue]"
            // with as many name/value pairs within the brackets as required
            int start = overrides.IndexOf("[");
            // The last character will be the ]
            if (!overrides.EndsWith("]"))
            {
                throw new Exception("Override fields passed to ProcessOverrides() that didn't end with ]");
            }

            RequestItemField ret = new RequestItemField(overrides.Substring(0, start));

            string[] nameValues = overrides.Substring(start + 1, overrides.Length - start - 2).Split(',');
            for (int i = 0; i < nameValues.Length; i += 2)
            {
                string field = nameValues[i];
                string value = nameValues[i + 1];

                ret.OverrideFields.Add(new OverrideField(field, value));
            }

            return ret;
        }
    }
}