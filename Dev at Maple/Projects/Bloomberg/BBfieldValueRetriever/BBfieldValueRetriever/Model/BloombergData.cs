using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using BBfieldValueRetriever;
using Maple;
using BB = Bloomberglp.Blpapi;

namespace Shared
{
    /// <summary>
    /// Methods to extract data returned by Bloomberg.
    /// </summary>
    public static class Conversion
    {
        #region internal methods

        /// <summary>
        /// Gets the field value from the BloombergData BBField collection.
        /// </summary>
        /// <param name="instrument">The instrument.</param>
        /// <param name="fieldName">Name of the field.</param>
        /// <returns></returns>
        private static object GetFieldValue(BloombergDataInstrument instrument, string fieldName)
        {
            object fieldValue = null;
            try
            {
                if (instrument.BBFields != null)
                {
                    if (instrument.BBFields.ContainsKey(fieldName))
                    {
                        fieldValue = instrument.BBFields[fieldName].Value;
                    }
                }
            }
            catch
            {
                fieldValue = null;
            }
            return fieldValue;
        }

        #endregion internal methods

        #region String

        /// <summary>
        /// Gets the string value from the field value object.
        /// </summary>
        /// <param name="fieldValue">The field value.</param>
        /// <returns></returns>
        public static string GetString(object fieldValue)
        {
            string actualValue = null;
            if (fieldValue != null)
            {
                actualValue = fieldValue.ToString();
            }
            return actualValue;
        }

        /// <summary>
        /// Gets the string value from the BloombergData BBField collection.
        /// </summary>
        /// <param name="instrument">The instrument.</param>
        /// <param name="fieldName">Name of the field.</param>
        /// <returns></returns>
        public static string GetString(BloombergDataInstrument instrument, string fieldName)
        {
            object fieldValue = GetFieldValue(instrument, fieldName);
            return GetString(fieldValue);
        }

        #endregion String

        #region Decimal

        /// <summary>
        /// Gets the decimal value from the field value object.
        /// </summary>
        /// <param name="fieldValue">The field value.</param>
        /// <returns></returns>
        public static decimal? GetDecimal(object fieldValue)
        {
            decimal? actualValue = null;
            if (fieldValue != null)
            {
                decimal temp;
                Decimal.TryParse(fieldValue.ToString(), out temp);
                actualValue = temp;
            }
            return actualValue;
        }

        /// <summary>
        /// Gets the decimal value from the BloombergData BBField collection.
        /// </summary>
        /// <param name="instrument">The instrument.</param>
        /// <param name="fieldName">Name of the field.</param>
        /// <returns></returns>
        public static decimal? GetDecimal(BloombergDataInstrument instrument, string fieldName)
        {
            object fieldValue = GetFieldValue(instrument, fieldName);
            return GetDecimal(fieldValue);
        }

        #endregion Decimal

        #region DateTime

        /// <summary>
        /// Gets the date time value from the field value object.
        /// </summary>
        /// <param name="fieldValue">The field value.</param>
        /// <returns></returns>
        public static DateTime? GetDateTime(object fieldValue)
        {
            DateTime? actualValue = null;
            if (fieldValue != null)
            {
                DateTime temp;
                if (DateTime.TryParseExact(fieldValue.ToString(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out temp))
                {
                    actualValue = temp;
                }
            }
            return actualValue;
        }

        /// <summary>
        /// Gets the date time value from the BloombergData BBField collection.
        /// </summary>
        /// <param name="instrument">The instrument.</param>
        /// <param name="fieldName">Name of the field.</param>
        /// <returns></returns>
        public static DateTime? GetDateTime(BloombergDataInstrument instrument, string fieldName)
        {
            object fieldValue = GetFieldValue(instrument, fieldName);
            return GetDateTime(fieldValue);
        }

        #endregion DateTime

        #region Long

        /// <summary>
        /// Gets the long value from the field value object.
        /// </summary>
        /// <param name="fieldValue">The field value.</param>
        /// <returns></returns>
        public static long? GetLong(object fieldValue)
        {
            long? actualValue = null;
            if (fieldValue != null)
            {
                long temp;
                long.TryParse(fieldValue.ToString(), out temp);
                actualValue = temp;
            }
            return actualValue;
        }

        /// <summary>
        /// Gets the long value from the BloombergData BBField collection.
        /// </summary>
        /// <param name="instrument">The instrument.</param>
        /// <param name="fieldName">Name of the field.</param>
        /// <returns></returns>
        public static long? GetLong(BloombergDataInstrument instrument, string fieldName)
        {
            object fieldValue = GetFieldValue(instrument, fieldName);
            return GetLong(fieldValue);
        }

        #endregion Long

        #region Int

        /// <summary>
        /// Gets the int value from the field value object.
        /// </summary>
        /// <param name="fieldValue">The field value.</param>
        /// <returns></returns>
        public static int? GetInt(object fieldValue)
        {
            int? actualValue = null;
            if (fieldValue != null)
            {
                int temp;
                int.TryParse(fieldValue.ToString(), out temp);
                actualValue = temp;
            }
            return actualValue;
        }

        /// <summary>
        /// Gets the int value from the BloombergData BBField collection.
        /// </summary>
        /// <param name="instrument">The instrument.</param>
        /// <param name="fieldName">Name of the field.</param>
        /// <returns></returns>
        public static int? GetInt(BloombergDataInstrument instrument, string fieldName)
        {
            object fieldValue = GetFieldValue(instrument, fieldName);
            return GetInt(fieldValue);
        }

        #endregion Int

        #region Dictionary<string, string>

        public static Dictionary<string, string> GetValuePairs(object fieldValue)
        {
            // string because the values are returned by Bloomberg as a delimited string
            Dictionary<string, string> ret = new Dictionary<string, string>();

            string[] pairs = fieldValue.ToString().Split(new[] { '{', '}' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string pair in pairs)
            {
                string[] vals = pair.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

                ret.Add(vals[0], vals[1]);
            }

            return ret;
        }

        #endregion Dictionary<string, string>
    }

    public enum BloombergDataInstrumentType
    {
        Security,
        Currency,
        ReferenceRate,
        Company
    }

    public class BloombergDataInstrumentField
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BloombergDataInstrumentField"/> class.
        /// </summary>
        /// <param name="name">The name of the Bloomberg field.</param>
        public BloombergDataInstrumentField(string name)
        {
            Name = name;
            Value = null;
            Error = null;
            FieldOverrides = new Dictionary<string, object>();
        }

        /// <summary>
        /// Gets or sets the name of the Bloomberg field.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the value of the Bloomberg field.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public object Value { get; set; }

        /// <summary>
        /// Gets or sets the error string if returned from Bloomberg.
        /// </summary>
        /// <value>
        /// The error.
        /// </value>
        public string Error { get; set; }

        /// <summary>
        /// Gets or sets the field override values to pass to Bloomberg.
        /// </summary>
        /// <value>
        /// The field overrides.
        /// </value>
        public Dictionary<string, object> FieldOverrides { get; set; }

        /// <summary>
        /// Gets or sets the field override values to pass to Bloomberg.
        /// </summary>
        /// <value>
        /// The field overrides.
        /// </value>
        public int ID { get; set; }

        /// <summary>
        /// Indicates that the field was an array of data values
        /// </summary>
        public bool IsArray { get; set; }
    }

    public class BloombergDataInstrument
    {
        public enum eRequestType { Reference, Historic, IntraDayBar, IntraDayTick, NotSet }

        public BloombergDataInstrument()
        {
            BBFields = new Dictionary<string, BloombergDataInstrumentField>();
        }

        /// <summary>
        /// Gets or sets the ID - usually the primary key, what the developer will use to identify the instrument.
        /// </summary>
        /// <value>
        /// The ID.
        /// </value>
        public int ID { get; set; }

        /// <summary>
        /// Gets or sets the ticker - the unique code that Bloomberg uses to identify a security etc.
        /// </summary>
        /// <value>
        /// The ticker.
        /// </value>
        public string Ticker { get; set; }

        private Guid guid = Guid.NewGuid();

        /// <summary>
        /// Gets the GUID - unique identifier that the BloombergData class will use, identifies the instrument across any type.
        /// </summary>
        public Guid GUID { get { return guid; } }

        /// <summary>
        /// Gets or sets the BB fields - the fields that we are requesting from Bloomberg.
        /// </summary>
        /// <value>
        /// The BB fields.
        /// </value>
        public Dictionary<string, BloombergDataInstrumentField> BBFields { get; set; }

        /// <summary>
        /// Gets or sets the type of the security - this is the text name that the developer can use to help identify the instrument.
        /// </summary>
        /// <value>
        /// The type of the security.
        /// </value>
        public string SecurityType { get; set; }

        /// <summary>
        /// Gets or sets the type - the type of instrument.
        /// </summary>
        /// <value>
        /// The type.
        /// </value>
        public BloombergDataInstrumentType Type { get; set; }

        private bool isSecurityValid = true;

        /// <summary>
        /// Gets or sets a value indicating whether a security is valid - if Bloomberg returns a security error then this is set to false.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is security valid; otherwise, <c>false</c>.
        /// </value>
        public bool IsSecurityValid { get { return isSecurityValid; } set { isSecurityValid = value; } } // is the security valid

        /// <summary>
        /// Gets or sets the security errors - populates with the results from Bloomberg.
        /// </summary>
        /// <value>
        /// The security errors.
        /// </value>
        public string SecurityErrors { get; set; }

        /// <summary>
        /// Indicates whether a response error was received from Bloomberg
        /// </summary>
        public string ResponseError { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether there are field errors - if Bloomberg returns a field error then this is set to true.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance has field errors; otherwise, <c>false</c>.
        /// </value>
        public bool HasFieldErrors { get; set; } // did any of the fields have an problem

        /// <summary>
        /// Gets or sets the field errors - populates with the results from Bloomberg.
        /// </summary>
        /// <value>
        /// The field errors.
        /// </value>
        public bool IsCompanyCreditActive { get; set; }

        /// <summary>
        /// Gets or sets the security tag string.
        /// </summary>
        /// <value>
        /// The tag string.
        /// </value>
        public string Tag { get; set; }

        /// <summary>
        /// The date of an historical request or the start date of an historical date range request
        /// </summary>
        public DateTime DateFrom { get; set; }

        /// <summary>
        /// The end date of an historical date range request
        /// </summary>
        public DateTime DateTo { get; set; }

        /// <summary>
        /// Determine the frequency of the output
        /// </summary>
        public string Periodicity { get; set; }

        /// <summary>
        /// The type of request that you are asking for
        /// </summary>
        public eRequestType RequestType { get; set; }

        /// <summary>
        /// The event type used for an Intra Day request. BID, ASK, TRADE etc.
        /// </summary>
        public string EventType { get; set; }

        public override string ToString()
        {
            return "BloombergDataInstrument = " + Ticker;
        }
    }

    public class BloombergData
    {
        #region instantiation

        /// <summary>
        /// Initializes a new instance of the <see cref="BloombergData"/> class.
        /// Retrieves Bloomberg data via the API
        /// </summary>
        public BloombergData()
        {
            UpdateStatus("BloombergData constructor");
            limits = new StringCollection { "DAILY_LIMIT_REACHED" };
            statusSystemName = AppDomain.CurrentDomain.FriendlyName;
            emailErrorsTo = "duoc@mpuk.com";
            arrayDelimiter = ";";
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BloombergData"/> class.
        /// Retrieves Bloomberg data via the API
        /// </summary>
        /// <param name="limitMsgs">The limit messages to search for.</param>
        /// <param name="emailTo">The email address for notification purposes - usually for the developers.</param>
        /// <param name="statusSystemName">Name of the system.</param>
        public BloombergData(StringCollection limitMsgs, string emailTo, string statusSystemName, string arrayDelimiter)
        {
            UpdateStatus("BloombergData constructor");
            limits = limitMsgs;
            emailErrorsTo = emailTo;
            this.statusSystemName = statusSystemName;
            this.arrayDelimiter = arrayDelimiter;
        }

        /// <summary>
        /// Releases unmanaged resources and performs other cleanup operations before the
        /// <see cref="BloombergData"/> is reclaimed by garbage collection.
        /// </summary>
        ~BloombergData()
        {
            UpdateStatus("BloombergData destructor");
            Dispose();
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                UpdateStatus("Disposing the Bloomberg session.");
                if (disposing)
                {
                    // Dispose managed resources.
                    if (session != null)
                    {
                        UpdateStatus("Stopping the active Bloomberg session.");
                        session.Stop(BB.Session.StopOption.SYNC);
                        session.Dispose();
                    }
                }
                session = null;
                disposed = true;
            }
        }

        #endregion instantiation

        #region public events

        public delegate void ProcessStatus(List<BloombergDataInstrument> instruments);

        /// <summary>
        /// Occurs when process of retrieving data from Bloomberg has completed.
        /// </summary>
        public event ProcessStatus ProcessCompleted;

        private void ProcessComplete()
        {
            UpdateStatus("Completed.");
            ProcessCompleted(sentToBB);
            Dispose();
        }

        public delegate void InstrumentComplete(BloombergDataInstrument instr);

        /// <summary>
        /// Occurs when each each security completes - can be used to process the data by security.
        /// </summary>
        public event InstrumentComplete InstrumentCompleteChanged;

        private void ShowCompletedInstrument(BloombergDataInstrument instr)
        {
            if (InstrumentCompleteChanged != null)
            {
                InstrumentCompleteChanged(instr);
            }
        }

        public delegate void PercentComplete(int percentComplete);

        /// <summary>
        /// Occurs when each security completes, shows completion percentage.
        /// </summary>
        public event PercentComplete PercentCompleteChanged;

        private void ShowCompletionPercentage(int count, int total)
        {
            if (PercentCompleteChanged != null)
            {
                int pc = (int)((count / (double)total) * 100);
                PercentCompleteChanged(pc);
            }
        }

        public delegate void StatusUpdate(string status);

        /// <summary>
        /// Occurs when the status changes - used to display messages.
        /// </summary>
        public event StatusUpdate StatusChanged;

        private void UpdateStatus(string status)
        {
            if (StatusChanged != null)
            {
                StatusChanged(status);
            }
        }

        #endregion public events

        #region private properties

        private BB.SessionOptions sessionOptions;
        private BB.Session session;

        private const string BLP_SECURITIES = "securities";
        private const string BLP_FIELDS = "fields";
        private const string BLP_OVERRIDES = "overrides";
        private const string BLP_DATE_FIELD = "date";
        private const string BLP_TIME_FIELD = "time";
        private readonly BB.Name SECURITY_DATA = new BB.Name("securityData");
        private readonly BB.Name SECURITY = new BB.Name("security");
        private readonly BB.Name FIELD_DATA = new BB.Name("fieldData");
        private readonly BB.Name RESPONSE_ERROR = new BB.Name("responseError");
        private readonly BB.Name MESSAGE = new BB.Name("message");
        private readonly BB.Name SECURITY_ERROR = new BB.Name("securityError");
        private readonly BB.Name FIELD_EXCEPTIONS = new BB.Name("fieldExceptions");
        private readonly BB.Name ERROR_INFO = new BB.Name("errorInfo");
        private readonly BB.Name FIELD_ID = new BB.Name("fieldId");
        private readonly BB.Name REASON = new BB.Name("reason");
        private readonly BB.Name ERROR_CODE = new BB.Name("errorCode");
        private readonly BB.Name SOURCE = new BB.Name("source");
        private readonly BB.Name CATEGORY = new BB.Name("category");
        private readonly BB.Name SUBCATEGORY = new BB.Name("subcategory");
        private readonly BB.Name DESCRIPTION = new BB.Name("description");
        private readonly BB.Name VALUE = new BB.Name("value");

        private const string BLP_REF = "//blp/refdata";
        private const string BLP_MKT = "//blp/mktdata";
        private const string BLP_VWAP = "//blp/mktvwap";
        private const string BLP_APIFLDS = "//blp/apiflds";

        private List<Guid> guids;
        private List<BloombergDataInstrument> sentToBB;
        private StringCollection limits;
        private string emailErrorsTo = "duoc@mpuk.com";
        private string statusSystemName = string.Empty;
        private string arrayDelimiter = ";";
        private bool sentLimitEmail;
        private Object lockObject = new Object();
        private bool disposed;

        #endregion private properties

        #region public methods

        /// <summary>
        /// Gets the Bloomberg data for the security.
        /// </summary>
        /// <param name="instruments">The instruments (securities) that we need to get data for.</param>
        public void GetBloombergData(List<BloombergDataInstrument> instruments)
        {
            getBloombergData(instruments);
        }

        #endregion public methods

        #region private methods

        /// <summary>
        /// Initialises the specified URI session and opens the service.
        /// </summary>
        /// <param name="uri">The URI.</param>
        /// <returns></returns>
        private bool Initialise(string uri, int requestCount)
        {
            sessionOptions = new BB.SessionOptions();
            sessionOptions.ServerHost = "localhost";
            sessionOptions.ServerPort = 8194;
            if (requestCount > sessionOptions.MaxPendingRequests)
            {
                sessionOptions.MaxPendingRequests = requestCount;
            }

            UpdateStatus("Starting the Bloomberg session.");
            session = new BB.Session(sessionOptions, processEvent);

            string msg;
            if (session.Start())
            {
                if (!session.OpenService(uri))
                {
                    msg = "Bloomberg failed to open session " + uri;
                    NLogger.Instance.Fatal(msg);
                    throw new Exception(msg);
                }
                return true;
            }
            msg = "An error occurred starting the Bloomberg session. Ensure Bloomberg is installed.";
            NLogger.Instance.Fatal(msg);
            throw new Exception(msg);
        }

        /// <summary>
        /// Gets the Bloomberg data via the specified request type
        /// </summary>
        /// <param name="requests">The list of instruments to retrieve data for</param>
        /// <param name="dataFromDate">The start date of the data being requested</param>
        /// <param name="dataToDate">The end date of the data being requested</param>
        /// <param name="requestType">The type of request i.e. Reference/Historical</param>
        private void getBloombergData(List<BloombergDataInstrument> requests)
        {
            try
            {
                sentLimitEmail = false;
                // Create session and open service
                Initialise(BLP_REF, requests.Count);
                BB.Service service = session.GetService(BLP_REF);

                guids = new List<Guid>();
                sentToBB = requests;
                ShowCompletionPercentage(0, requests.Count);

                foreach (BloombergDataInstrument bbdi in requests)
                {
                    BB.Request request = createRequest(bbdi, service);

                    switch (bbdi.RequestType)
                    {
                        case BloombergDataInstrument.eRequestType.Reference:
                            SetReferenceProperties(request, bbdi);
                            break;

                        case BloombergDataInstrument.eRequestType.Historic:
                            SetHistoricalProperties(request, bbdi);
                            break;

                        case BloombergDataInstrument.eRequestType.IntraDayBar:
                            SetIntraDayBarProperties(request, bbdi);
                            break;
                    }

                    session.SendRequest(request, new BB.CorrelationID(bbdi.GUID));
                }

                UpdateStatus(string.Format("Sent {0} instruments\\requests to Bloomberg", sentToBB.Count));
            }
            catch (Exception ex)
            {
                UpdateStatus(ex.ToString());
                throw new Exception("An error occurred whilst sending requests to Bloomberg - " + ex, ex);
            }
        }

        private void SetReferenceProperties(BB.Request request, BloombergDataInstrument bbdi)
        {
            BB.Element securities = request.GetElement(BLP_SECURITIES);

            string ticker = bbdi.Ticker;

            // set all the securities to fetch
            securities.AppendValue(ticker);

            // set all the fields
            BB.Element fields = request.GetElement(BLP_FIELDS);
            foreach (string field in bbdi.BBFields.Keys)
            {
                fields.AppendValue(field);

                // now do the overrides - if they exist
                BloombergDataInstrumentField bdif = bbdi.BBFields[field];
                if (bdif.FieldOverrides != null)
                {
                    BB.Element requestOverrides = request.GetElement(BLP_OVERRIDES);
                    foreach (string oField in bdif.FieldOverrides.Keys)
                    {
                        object oValue = bdif.FieldOverrides[oField];
                        // now add in the override oField and oValue
                        BB.Element ovr = requestOverrides.AppendElement();
                        ovr.SetElement(FIELD_ID, oField);
                        ovr.SetElement(VALUE, oValue.ToString());
                    }
                }
            }
        }

        private void SetIntraDayBarProperties(BB.Request request, BloombergDataInstrument bbdi)
        {
            DateTime dt = bbdi.DateFrom;
            BB.Datetime t = new BB.Datetime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second, 0);
            request.Set("startDateTime", t);
            dt = bbdi.DateTo;
            t = new BB.Datetime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second, 0);
            request.Set("endDateTime", t);

            request.Set("gapFillInitialBar", false);
            request.Set("interval", bbdi.Periodicity);

            request.Set("security", bbdi.Ticker);

            request.Set("eventType", bbdi.EventType);
        }

        private void SetHistoricalProperties(BB.Request request, BloombergDataInstrument bbdi)
        {
            // set historical request properties
            request.Set("periodicityAdjustment", "ACTUAL");

            if (bbdi.Periodicity.Trim() == "") { bbdi.Periodicity = "DAILY"; }
            request.Set("periodicitySelection", bbdi.Periodicity.ToUpper());
            // request.Set("currency", textBoxCurrencyCode.Text.Trim()); Not set and default to security

            request.Set("startDate", bbdi.DateFrom.ToString("yyyyMMdd"));
            // A second date is not mandatory
            if (bbdi.DateTo > DateTime.MinValue)
            {
                request.Set("endDate", bbdi.DateTo.ToString("yyyyMMdd"));
            }

            request.Set("nonTradingDayFillOption", "NON_TRADING_WEEKDAYS");
            request.Set("nonTradingDayFillMethod", "PREVIOUS_VALUE");
            request.Set("overrideOption", "OVERRIDE_OPTION_CLOSE");
            //request.Set("maxDataPoints", 1000);  No limit applied yet

            // returnEids. returns the entitlement identifiers associated with security
            request.Set("returnEids", false);

            BB.Element securities = request.GetElement(BLP_SECURITIES);

            // set all the securities to fetch
            securities.AppendValue(bbdi.Ticker);

            // set all the fields
            BB.Element fields = request.GetElement(BLP_FIELDS);
            foreach (string field in bbdi.BBFields.Keys)
            {
                fields.AppendValue(field);

                // now do the overrides - if they exist
                BloombergDataInstrumentField bdif = bbdi.BBFields[field];
                if (bdif.FieldOverrides != null)
                {
                    BB.Element requestOverrides = request.GetElement(BLP_OVERRIDES);
                    foreach (string oField in bdif.FieldOverrides.Keys)
                    {
                        object oValue = bdif.FieldOverrides[oField];
                        // now add in the override oField and oValue
                        BB.Element ovr = requestOverrides.AppendElement();
                        ovr.SetElement(FIELD_ID, oField);
                        ovr.SetElement(VALUE, oValue.ToString());
                    }
                }
            }
        }

        private BB.Request createRequest(BloombergDataInstrument bbdi, BB.Service service)
        {
            BB.Request req;
            switch (bbdi.RequestType)
            {
                case BloombergDataInstrument.eRequestType.Reference:
                    req = service.CreateRequest("ReferenceDataRequest");
                    break;

                case BloombergDataInstrument.eRequestType.Historic:
                    req = service.CreateRequest("HistoricalDataRequest");
                    break;

                case BloombergDataInstrument.eRequestType.IntraDayBar:
                    req = service.CreateRequest("IntradayBarRequest");
                    break;

                default:
                    throw new Exception("Unhandled request type in createRequest method.");
            }
            return req;
        }

        /// <summary>
        /// Processes the Bloomberg event.
        /// </summary>
        /// <param name="eventObj">The event obj.</param>
        /// <param name="session">The session.</param>
        private void processEvent(BB.Event eventObj, BB.Session session)
        {
            try
            {
                switch (eventObj.Type)
                {
                    case BB.Event.EventType.RESPONSE:
                    case BB.Event.EventType.PARTIAL_RESPONSE:
                        //processSubscriptionDataEvent(eventObj, session);
                        processRequestDataEvent(eventObj, session);
                        break;

                    default:
                        processMiscEvents(eventObj, session);
                        break;
                }
            }
            catch (Exception e)
            {
                NLogger.Instance.Error(e.ToString());
                throw;
            }
        }

        /// <summary>
        /// Processes misc Bloomberg events.
        /// </summary>
        /// <param name="eventObj">The event obj.</param>
        /// <param name="session">The session.</param>
        private void processMiscEvents(BB.Event eventObj, BB.Session session)
        {
            foreach (BB.Message msg in eventObj.GetMessages())
            {
                UpdateStatus(msg.MessageType.ToString());

                switch (msg.MessageType.ToString())
                {
                    case "SessionStarted":
                        break;

                    case "SessionTerminated":
                    case "SessionStopped":
                        break;

                    case "ServiceOpened":
                        break;

                    case "RequestFailure":
                        UpdateStatus("*** REQUEST FAILURE ***");

                        BB.Element reason = msg.GetElement(REASON);
                        string reasonText = string.Format("Error: Source-[{0}], Code-[{1}], Category-[{2}], Desc-[{3}]",
                            reason.GetElementAsString(SOURCE),
                            reason.GetElementAsString(ERROR_CODE),
                            reason.GetElementAsString(CATEGORY),
                            reason.GetElementAsString(DESCRIPTION));
                        UpdateStatus(reasonText);
                        UpdateStatus("Error message body : " + msg);

                        bool hasGUID = false;
                        bool hasResponseError = false;
                        try
                        {
                            Guid reqGUID = (Guid)msg.CorrelationID.Object;
                            hasGUID = true;
                            hasResponseError = msg.HasElement("responseError");
                        }
                        catch
                        {
                            hasGUID = false;
                            hasResponseError = false;
                        }

                        if (hasGUID & hasResponseError)
                        {
                            UpdateStatus("GUID and responseError found - handling normally");

                            // has both a GUID and a response error so can be handled normally
                            processRequestDataEvent(eventObj, session);
                        }
                        else if (hasGUID)
                        {
                            UpdateStatus("GUID found - updating GUID list.");

                            Guid reqGUID = (Guid)msg.CorrelationID.Object;
                            // only has a GUID so add to the list of returned GUIDs
                            if (!guids.Contains(reqGUID))
                            {
                                guids.Add(reqGUID);
                            }
                        }
                        else
                        {
                            // ok, email out that a timeout has occurred
                            Email email = new Email();
                            email.SendEmail(emailErrorsTo,
                                "Bloomberg Request Failure",
                                "A RequestFailure response has been returned from Bloomberg." + Environment.NewLine + reasonText + Environment.NewLine + msg,
                                false);
                        }
                        break;

                    default:
                        UpdateStatus("*** Unhandled Misc Event ***");
                        break;
                }
            }
        }

        /// <summary>
        /// Finds the sent instrument by GUID.
        /// </summary>
        /// <param name="guid">The GUID.</param>
        /// <returns></returns>
        private BloombergDataInstrument FindSentInstrumentByGuid(Guid guid)
        {
            BloombergDataInstrument instr = null;
            foreach (BloombergDataInstrument bbdi in sentToBB)
            {
                if (bbdi.GUID == guid)
                {
                    instr = bbdi;
                    break;
                }
            }
            return instr;
        }

        /// <summary>
        /// Process subscription data
        /// </summary>
        /// <param name="eventObj"></param>
        /// <param name="session"></param>
        private void processRequestDataEvent(BB.Event eventObj, BB.Session session)
        {
            string securityName = string.Empty;

            try
            {
                // process message
                foreach (BB.Message msg in eventObj.GetMessages())
                {
                    #region find instrument

                    Guid reqGUID = (Guid)msg.CorrelationID.Object;

                    // check for duplicate replies just in case
                    if (guids.Contains(reqGUID))
                    {
                        return;
                    }
                    else
                    {
                        guids.Add(reqGUID);
                    }

                    // find the correct instrument
                    BloombergDataInstrument bbdi = FindSentInstrumentByGuid(reqGUID);
                    if (bbdi == null)
                    {
                        UpdateStatus("Unable to find received instrument by Guid - " + reqGUID);
                        continue;
                    }

                    #endregion find instrument

                    UpdateStatus(string.Format("Received {0} of {1} requests : {2}", guids.Count, sentToBB.Count, bbdi.Ticker));

                    if (msg.HasElement(RESPONSE_ERROR))
                    {
                        BB.Element error = msg.GetElement(RESPONSE_ERROR);
                        string responseError = error.GetElementAsString(SUBCATEGORY);
                        UpdateStatus("Response error : " + error.GetElementAsString(MESSAGE));
                        CheckForLimits(responseError);
                        bbdi.ResponseError = responseError;
                        continue;
                    }

                    BB.Element secDataArray = null;
                    int numberOfSecurities = 0;

                    switch (msg.MessageType.ToString())
                    {
                        case "ReferenceDataResponse":
                            secDataArray = msg.GetElement(SECURITY_DATA);
                            numberOfSecurities = secDataArray.NumValues;

                            // process securities data
                            for (int index = 0; index < numberOfSecurities; index++)
                            {
                                BB.Element secData = secDataArray.GetValueAsElement(index);
                                if (!hasSecurityError(secData, bbdi))
                                {
                                    hasFieldError(secData, bbdi);

                                    GetData(bbdi, secData);
                                }
                            }
                            break;

                        case "HistoricalDataResponse":
                            secDataArray = msg.GetElement(SECURITY_DATA);
                            numberOfSecurities = secDataArray.NumValues;

                            if (!hasSecurityError(secDataArray, bbdi))
                            {
                                hasFieldError(secDataArray, bbdi);

                                // process securities data
                                for (int index = 0; index < numberOfSecurities; index++)
                                {
                                    foreach (BB.Element secData in secDataArray.Elements)
                                    {
                                        switch (secData.Name.ToString())
                                        {
                                            case "eidData":
                                                // process security eid data here
                                                break;

                                            case "security":
                                                // security name
                                                securityName = secData.GetValueAsString();
                                                break;

                                            case "fieldData":
                                            case "fieldExceptions":
                                                GetData(bbdi, secData);
                                                break;
                                        }
                                    }
                                }
                            }
                            break;

                        case "IntradayBarResponse":
                            secDataArray = msg.GetElement("barData");
                            numberOfSecurities = secDataArray.NumValues;

                            if (!hasSecurityError(secDataArray, bbdi))
                            {
                                foreach (BB.Element barData in secDataArray.Elements)
                                {
                                    if (barData.Name.ToString() == "barTickData")
                                    {
                                        GetData(bbdi, barData);
                                    }
                                }
                            }
                            break;

                        case "":
                            break;
                    }
                    ShowCompletionPercentage(guids.Count, sentToBB.Count);
                    ShowCompletedInstrument(bbdi);
                }
            }
            catch (Exception ex)
            {
                UpdateStatus("Error occurred processing reply : " + ex);
                throw;
            }
            finally
            {
                if (guids.Count == sentToBB.Count)
                { // we are done
                    ShowCompletionPercentage(1, 1);
                    ProcessComplete();
                }
            }
        }

        private bool hasSecurityError(BB.Element secDataArray, BloombergDataInstrument bbdi)
        {
            bool ret = false;
            if (secDataArray.HasElement(SECURITY_ERROR))
            {
                ret = true;
                bbdi.IsSecurityValid = false;
                BB.Element error = secDataArray.GetElement(SECURITY_ERROR);
                UpdateStatus(string.Format("Security error for ticker {0} : {1}", bbdi.Ticker, error.GetElementAsString(MESSAGE)));
                bbdi.SecurityErrors += (error.GetElementAsString(MESSAGE) + "; ");
            }
            //  int c = secDataArray.NumValues;
            //if (secDataArray.HasElement("delayedSecurity")) {
            //    ret = true;
            //    bbdi.IsSecurityValid = false;
            //    bbdi.SecurityErrors += ("delayedSecurity; ");
            //}

            // Standardise the errors here so that they can be more easily used further down the line
            bbdi.SecurityErrors = StandardiseError(bbdi.SecurityErrors);

            return ret;
        }

        private string StandardiseError(string ErrorMessage)
        {
            string ret = ErrorMessage;
            if (ErrorMessage != null)
            {
                if (ErrorMessage.SubStringSafe(0, 24) == "Unknown/Invalid security")
                {
                    ret = "Unknown/Invalid security";
                }
            }

            return ret;
        }

        /// <summary>
        /// Determines whether there are field errors. Field errors should not stop us from attempting to read field values
        /// however because there maybe field values even though there are field errors.
        /// </summary>
        /// <param name="secData"></param>
        /// <param name="bbdi"></param>
        /// <returns></returns>
        private bool hasFieldError(BB.Element secData, BloombergDataInstrument bbdi)
        {
            if (secData.HasElement(FIELD_EXCEPTIONS))
            {
                // process error element
                BB.Element error = secData.GetElement(FIELD_EXCEPTIONS);

                if (error.NumValues > 0)
                {
                    bbdi.HasFieldErrors = true;

                    for (int errorIndex = 0; errorIndex < error.NumValues; errorIndex++)
                    {
                        BB.Element errorException = error.GetValueAsElement(errorIndex);
                        BB.Element errorInfo = errorException.GetElement(ERROR_INFO);

                        bbdi.BBFields[errorException.GetElementAsString(FIELD_ID)].Error = errorInfo.GetElementAsString(MESSAGE);
                        string msg = string.Format("Field error for ticker {0} : Field {1}: {2}",
                            bbdi.Ticker,
                            errorException.GetElementAsString(FIELD_ID),
                            errorInfo.GetElementAsString(MESSAGE));
                        UpdateStatus(msg);
                    }
                }
            }

            return bbdi.HasFieldErrors;
        }

        private void GetData(BloombergDataInstrument bbdi, BB.Element secData)
        {
            #region get the data

            if (bbdi.BBFields != null)
            {
                BB.Element fields = null;
                switch (bbdi.RequestType)
                {
                    case BloombergDataInstrument.eRequestType.Reference:
                        fields = secData.GetElement(FIELD_DATA);

                        lock (lockObject)
                        {
                            foreach (string bbField in bbdi.BBFields.Keys.ToList())
                            {
                                string key = bbField.ToLower();
                                if (fields.HasElement(key))
                                {
                                    BB.Element item = fields.GetElement(key);
                                    if (item.IsArray)
                                    {
                                        bbdi.BBFields[key].Value = processBulkData(item);
                                        bbdi.BBFields[key].IsArray = true;
                                    }
                                    else
                                    {
                                        // set the value in the instrument field item
                                        bbdi.BBFields[key].Value = item.GetValue();
                                    }
                                }
                            }
                        }
                        break;

                    case BloombergDataInstrument.eRequestType.Historic:
                        // get field data
                        for (int pointIndex = 0; pointIndex < secData.NumValues; pointIndex++)
                        {
                            fields = secData.GetValueAsElement(pointIndex);

                            if (fields.HasElement(BLP_DATE_FIELD) && !bbdi.BBFields.ContainsKey(BLP_DATE_FIELD))
                            {
                                bbdi.BBFields.Add(BLP_DATE_FIELD, new BloombergDataInstrumentField(BLP_DATE_FIELD));
                            }

                            foreach (BloombergDataInstrumentField fld in bbdi.BBFields.Values)
                            {
                                string key = fld.Name.ToLower();
                                try
                                {
                                    if (fields.HasElement(key))
                                    {
                                        if (bbdi.BBFields[key].Value == null)
                                        {
                                            // We need a list for multiple values
                                            bbdi.BBFields[key].Value = new List<object>();
                                        }
                                        BB.Element item = fields.GetElement(key);
                                        List<object> list = (List<object>)bbdi.BBFields[key].Value;
                                        if (item.IsArray)
                                        {
                                            // bulk field data
                                            list.Add(processBulkData(item));
                                            bbdi.BBFields[key].IsArray = true;
                                        }
                                        else
                                        {
                                            // field data
                                            list.Add(item.GetValueAsString());
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    // display error
                                    bbdi.BBFields[key].Error = ex.Message;
                                }
                            } // end foreach
                        } // end for

                        break;

                    case BloombergDataInstrument.eRequestType.IntraDayBar:
                        // get field data
                        if (secData.NumValues == 0)
                        {
                            bbdi.IsSecurityValid = false;
                            UpdateStatus(string.Format("Security error for ticker {0} : No data", bbdi.Ticker));
                            bbdi.SecurityErrors += "No data; ";
                        }
                        else
                        {
                            for (int pointIndex = 0; pointIndex < secData.NumValues; pointIndex++)
                            {
                                fields = secData.GetValueAsElement(pointIndex);

                                if (fields.HasElement(BLP_TIME_FIELD) && !bbdi.BBFields.ContainsKey(BLP_TIME_FIELD))
                                {
                                    bbdi.BBFields.Add(BLP_TIME_FIELD, new BloombergDataInstrumentField(BLP_TIME_FIELD));
                                }

                                foreach (BloombergDataInstrumentField fld in bbdi.BBFields.Values)
                                {
                                    string key = fld.Name.ToLower();
                                    try
                                    {
                                        // Bar data columns come in lower case
                                        if (fields.HasElement(key))
                                        {
                                            if (bbdi.BBFields[key].Value == null)
                                            {
                                                // We need a list for multiple values
                                                bbdi.BBFields[key].Value = new List<object>();
                                            }
                                            BB.Element item = fields.GetElement(key);
                                            List<object> list = (List<object>)bbdi.BBFields[key].Value;
                                            if (item.IsArray)
                                            {
                                                // bulk field data
                                                list.Add(processBulkData(item));
                                                bbdi.BBFields[key].IsArray = true;
                                            }
                                            else
                                            {
                                                // field data
                                                list.Add(item.GetValueAsString());
                                            }
                                        }
                                        else
                                        {
                                            fld.Error = "Invalid field";
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        // display error
                                        bbdi.BBFields[key].Error = ex.Message;
                                    }
                                } // end foreach
                            } // end for
                        }

                        break;

                    default:
                        throw new Exception(string.Format("RequestType {0} not handled in GetData method", bbdi.RequestType));
                }
            }

            #endregion get the data
        }

        private void CheckForNoDataReturned(BB.Element secData, BloombergDataInstrument bbdi)
        {
            if (secData.NumValues == 0)
            {  // No values returned
                foreach (BloombergDataInstrumentField fld in bbdi.BBFields.Values)
                {
                    if (fld.Value == null)
                    {
                        fld.Value = new List<object>();
                    }
                    List<object> list = (List<object>)bbdi.BBFields[fld.Name].Value;
                    list.Add("no data");
                }
            }
        }

        private string processBulkData(BB.Element data)
        {
            string ret = null;

            try
            {
                if (data.NumValues > 0)
                {
                    for (int index = 0; index < data.NumValues; index++)
                    {
                        BB.Element bulk = data.GetValueAsElement(index);

                        if (bulk.NumElements > 1)
                        {
                            ret += "{";
                        }
                        foreach (BB.Element item in bulk.Elements)
                        {
                            ret += item.GetValueAsString() + arrayDelimiter;
                        }
                        if (bulk.NumElements > 1)
                        {
                            ret += "}";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                UpdateStatus("Error occurred processing array field : " + ex);
                ret = "Error in processBulkData";
            }
            return ret;
        }

        private void CheckForLimits(string message)
        {
            foreach (string msg in limits)
            {
                if (message.Contains(msg))
                {
                    // there is a limit message

                    #region system status

                    ApplicationStatus.SetStatus(statusSystemName, "Error", "Bloomberg limit has been reached - " + msg + ".", 1);

                    #endregion system status

                    #region email

                    if (!sentLimitEmail)
                    {
                        Email email = new Email();
                        string html = string.Format("<HTML><BODY><P>{0}</P><P>This automated email sent from {1} : </BR> {2} </P></BODY><HTML>",
                            "Please check, get the limit reset by Bloomberg ASAP, and if necessary run the app on a backup machine.</BR></BR>" + message,
                            Environment.MachineName,
                            Environment.GetCommandLineArgs()[0]);
                        email.SendEmail(emailErrorsTo,
                            "Bloomberg limit has been reached on machine " + Environment.MachineName,
                            html,
                            true);
                    }

                    #endregion email

                    sentLimitEmail = true;
                    break;
                }
            }
        }

        #endregion private methods
    }
}