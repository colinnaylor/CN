using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BB = Bloomberglp.Blpapi;

namespace Shared {

	#region Testing
	public class Testing {
		public void TestForReference() {
			List<BloombergDataInstrument> bbdis = new List<BloombergDataInstrument>();
			BloombergDataInstrument bbdi = new BloombergDataInstrument();

			bbdi.ID = 0;
			bbdi.Ticker = "VOD LN Equity";
			//bbdi.Ticker = "BBG002626686 BUID";
			//bbdi.SecurityType = "Equity";
			bbdi.Type = BloombergDataInstrumentType.Security;
			bbdi.BBFields = new Dictionary<string, BloombergDataInstrumentField>();
			bbdi.BBFields.Add("PX_LAST", new BloombergDataInstrumentField("PX_LAST"));
			bbdis.Add(bbdi);
			/*
						bbdi.ID = 0;
						bbdi.Ticker = "US0200021014 EQUITY";
						bbdi.SecurityType = "EQUITY";
						bbdi.Type = BloombergDataInstrumentType.Security;
						bbdi.BBFields = new Dictionary<string, object>();
						bbdi.BBFields.Add("ID_ISIN", null);
						bbdi.BBFields.Add("BID", null);
						bbdi.BBFields.Add("ASK", null);
						bbdi.BBFields.Add("PX_CLOSE_DT", null);
						bbdis.Add(bbdi);

						bbdi = new BloombergDataInstrument();
						bbdi.ID = 1;
						bbdi.Ticker = "RBS LN EQUITY";
						bbdi.SecurityType = "EQUITY";
						bbdi.Type = BloombergDataInstrumentType.Security;
						bbdi.BBFields = new Dictionary<string, object>();
						bbdi.BBFields.Add("ID_GAVIN", null);
						bbdi.BBFields.Add("PX_LAST", null);
						bbdis.Add(bbdi);
			*/
            // BloombergData bbd = new BloombergData(new System.Collections.Specialized.StringCollection(), "gavinh@mpuk.com", "testing Bloomberg v3 api", ";");
            //  OR
            BloombergData bbd = new BloombergData();

			bbd.InstrumentCompleteChanged += new BloombergData.InstrumentComplete(bbd_InstrumentCompleteChanged);
			bbd.PercentCompleteChanged += new BloombergData.PercentComplete(bbd_PercentCompleteChanged);
			bbd.ProcessCompleted += new BloombergData.ProcessStatus(bbd_ProcessCompleted);
			bbd.StatusChanged += new BloombergData.StatusUpdate(bbd_StatusChanged);
			bbd.GetBloombergData(bbdis);
		}

		public void TestForHistory() {
			List<BloombergDataInstrument> bbdis = new List<BloombergDataInstrument>();
			BloombergDataInstrument bbdi = new BloombergDataInstrument();

			bbdi.ID = 0;
			bbdi.Ticker = "EUR001M INDEX";
			bbdi.SecurityType = "INDEX";

			bbdi.BBFields = new Dictionary<string, BloombergDataInstrumentField>();
			bbdi.BBFields.Add("PX_LAST", new BloombergDataInstrumentField("PX_LAST"));
			bbdis.Add(bbdi);

			BloombergData bbd = new BloombergData(new System.Collections.Specialized.StringCollection(), "hansa@mpuk.com", "testing Bloomberg v3 api", ";");
			bbd.InstrumentCompleteChanged += new BloombergData.InstrumentComplete(bbd_InstrumentCompleteChanged);
			bbd.PercentCompleteChanged += new BloombergData.PercentComplete(bbd_PercentCompleteChanged);
			bbd.ProcessCompleted += new BloombergData.ProcessStatus(bbd_ProcessCompleted);
			bbd.StatusChanged += new BloombergData.StatusUpdate(bbd_StatusChanged);
			bbd.GetBloombergData(bbdis, DateTime.Today.AddDays(-1));
		}

		static void bbd_StatusChanged(string status) {
			Console.WriteLine(status);
		}

		static void bbd_PercentCompleteChanged(int percentComplete) {
			Console.WriteLine("Completed " + percentComplete.ToString() + "%");
		}

		static void bbd_InstrumentCompleteChanged(BloombergDataInstrument instr) {
			string ticker = instr.Ticker;
			string value = string.Empty;
			string error = string.Empty;
			Console.WriteLine("BloombergDataInstrument completed " + ticker);
			foreach (string key in instr.BBFields.Keys) {
				if (instr.BBFields[key].Value == null) {
					value = "[NULL]";
				} else {
					value = instr.BBFields[key].Value.ToString();
				}
				error = instr.BBFields[key].Error;
				Console.WriteLine(string.Format("Ticker - {0} : Key - {1} : Value - {2} : Error - {3}", ticker, key, value));

			}

		}

		static void bbd_ProcessCompleted(List<BloombergDataInstrument> instruments) {
			Console.WriteLine("Process completed");
		}
	}

	#endregion

	/// <summary>
	/// Methods to extract data returned by Bloomberg.
	/// </summary>
	public static class Conversion {

		#region internal methods
		/// <summary>
		/// Gets the field value from the BloombergData BBField collection.
		/// </summary>
		/// <param name="instrument">The instrument.</param>
		/// <param name="fieldName">Name of the field.</param>
		/// <returns></returns>
		private static object GetFieldValue(BloombergDataInstrument instrument, string fieldName) {
			object fieldValue = null;
			try {
				if (instrument.BBFields != null) {
					if (instrument.BBFields.ContainsKey(fieldName)) {
						fieldValue = instrument.BBFields[fieldName].Value;
					}
				}
			} catch {
				fieldValue = null;
			}
			return fieldValue;
		}
		#endregion

		#region String
		/// <summary>
		/// Gets the string value from the field value object.
		/// </summary>
		/// <param name="fieldValue">The field value.</param>
		/// <returns></returns>
		public static string GetString(object fieldValue) {
			string actualValue = null;
			if (fieldValue != null) {
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
		public static string GetString(BloombergDataInstrument instrument, string fieldName) {
			object fieldValue = GetFieldValue(instrument, fieldName);
			return GetString(fieldValue);
		}
		#endregion

		#region Decimal
		/// <summary>
		/// Gets the decimal value from the field value object.
		/// </summary>
		/// <param name="fieldValue">The field value.</param>
		/// <returns></returns>
		public static decimal? GetDecimal(object fieldValue) {
			decimal? actualValue = null;
			if (fieldValue != null) {
				decimal temp;
				Decimal.TryParse(fieldValue.ToString(), out temp);
				actualValue = (decimal?)temp;
			}
			return actualValue;
		}
		/// <summary>
		/// Gets the decimal value from the BloombergData BBField collection.
		/// </summary>
		/// <param name="instrument">The instrument.</param>
		/// <param name="fieldName">Name of the field.</param>
		/// <returns></returns>
		public static decimal? GetDecimal(BloombergDataInstrument instrument, string fieldName) {
			object fieldValue = GetFieldValue(instrument, fieldName);
			return GetDecimal(fieldValue);
		}
		#endregion

		#region DateTime
		/// <summary>
		/// Gets the date time value from the field value object.
		/// </summary>
		/// <param name="fieldValue">The field value.</param>
		/// <returns></returns>
		public static DateTime? GetDateTime(object fieldValue) {
			DateTime? actualValue = null;
			if (fieldValue != null) {
				DateTime temp;
				if (DateTime.TryParseExact(fieldValue.ToString(), "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AdjustToUniversal, out temp)) {
					actualValue = (DateTime?)temp;
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
		public static DateTime? GetDateTime(BloombergDataInstrument instrument, string fieldName) {
			object fieldValue = GetFieldValue(instrument, fieldName);
			return GetDateTime(fieldValue);
		}
		#endregion

		#region Long
		/// <summary>
		/// Gets the long value from the field value object.
		/// </summary>
		/// <param name="fieldValue">The field value.</param>
		/// <returns></returns>
		public static long? GetLong(object fieldValue) {
			long? actualValue = null;
			if (fieldValue != null) {
				long temp;
				long.TryParse(fieldValue.ToString(), out temp);
				actualValue = (long?)temp;
			}
			return actualValue;
		}

		/// <summary>
		/// Gets the long value from the BloombergData BBField collection.
		/// </summary>
		/// <param name="instrument">The instrument.</param>
		/// <param name="fieldName">Name of the field.</param>
		/// <returns></returns>
		public static long? GetLong(BloombergDataInstrument instrument, string fieldName) {
			object fieldValue = GetFieldValue(instrument, fieldName);
			return GetLong(fieldValue);
		}
		#endregion

		#region Int
		/// <summary>
		/// Gets the int value from the field value object.
		/// </summary>
		/// <param name="fieldValue">The field value.</param>
		/// <returns></returns>
		public static int? GetInt(object fieldValue) {
			int? actualValue = null;
			if (fieldValue != null) {
				int temp;
				int.TryParse(fieldValue.ToString(), out temp);
				actualValue = (int?)temp;
			}
			return actualValue;
		}

		/// <summary>
		/// Gets the int value from the BloombergData BBField collection.
		/// </summary>
		/// <param name="instrument">The instrument.</param>
		/// <param name="fieldName">Name of the field.</param>
		/// <returns></returns>
		public static int? GetInt(BloombergDataInstrument instrument, string fieldName) {
			object fieldValue = GetFieldValue(instrument, fieldName);
			return GetInt(fieldValue);
		}
		#endregion

        #region Dictionary<string, string>
        public static Dictionary<string, string> GetValuePairs(object fieldValue) {
            // string because the values are returned by Bloomberg as a delimited string
            Dictionary<string, string> ret = new Dictionary<string, string>();

            string[] pairs = fieldValue.ToString().Split(new char[] { '{', '}' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string pair in pairs) {
                string[] vals = pair.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

                ret.Add(vals[0], vals[1]);
            }

            return ret;
        }
        #endregion
    }
	
	public enum BloombergDataInstrumentType {
		Security,
		Currency,
		ReferenceRate,
		Company
	}

	public class BloombergDataInstrumentField {

		/// <summary>
		/// Initializes a new instance of the <see cref="BloombergDataInstrumentField"/> class.
		/// </summary>
		/// <param name="name">The name of the Bloomberg field.</param>
		public BloombergDataInstrumentField(string name) {
			this.Name = name;
			this.Value = null;
			this.Error = null;
			this.FieldOverrides = new Dictionary<string, object>();
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
	}
	public class BloombergDataInstrument {

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
	}

	public class BloombergData {

		#region instantiation

		/// <summary>
		/// Initializes a new instance of the <see cref="BloombergData"/> class.
		/// Retrieves Bloomberg data via the API
		/// </summary>
		public BloombergData() {
			UpdateStatus("BloombergData constructor");
			this.limits = new System.Collections.Specialized.StringCollection { "DAILY_LIMIT_REACHED" };
			this.statusSystemName = System.AppDomain.CurrentDomain.FriendlyName;
			this.emailErrorsTo = "duoc@mpuk.com";
			this.arrayDelimiter = ";";
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="BloombergData"/> class.
		/// Retrieves Bloomberg data via the API
		/// </summary>
		/// <param name="limitMsgs">The limit messages to search for.</param>
		/// <param name="emailTo">The email address for notification purposes - usually for the developers.</param>
		/// <param name="statusSystemName">Name of the system.</param>
		public BloombergData(System.Collections.Specialized.StringCollection limitMsgs, string emailTo, string statusSystemName, string arrayDelimiter) {
			UpdateStatus("BloombergData constructor");
			this.limits = limitMsgs;
			this.emailErrorsTo = emailTo;
			this.statusSystemName = statusSystemName;
			this.arrayDelimiter = arrayDelimiter;
		}

		/// <summary>
		/// Releases unmanaged resources and performs other cleanup operations before the
		/// <see cref="BloombergData"/> is reclaimed by garbage collection.
		/// </summary>
		~BloombergData() {
			UpdateStatus("BloombergData destructor");
			Dispose();
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Releases unmanaged and - optionally - managed resources
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		protected virtual void Dispose(bool disposing) {
			if (!disposed) {
				UpdateStatus("Disposing the Bloomberg session.");
				if (disposing) {
					// Dispose managed resources.
					if (session != null) {
						UpdateStatus("Stopping the active Bloomberg session.");
						session.Stop(BB.Session.StopOption.SYNC);
						session.Dispose();
					}
				}
				session = null;
				disposed = true;
			}
		}
		#endregion

		#region public events

		public delegate void ProcessStatus(List<BloombergDataInstrument> instruments);
		/// <summary>
		/// Occurs when process of retrieving data from Bloomberg has completed.
		/// </summary>
		public event ProcessStatus ProcessCompleted;
		private void ProcessComplete() {
			UpdateStatus("Completed.");
			ProcessCompleted(sentToBB);
			Dispose();
		}

		public delegate void InstrumentComplete(BloombergDataInstrument instr);
		/// <summary>
		/// Occurs when each each security completes - can be used to process the data by security.
		/// </summary>
		public event InstrumentComplete InstrumentCompleteChanged;
		private void ShowCompletedInstrument(BloombergDataInstrument instr) {
			if (InstrumentCompleteChanged != null) {
				InstrumentCompleteChanged(instr);
			}
		}

		public delegate void PercentComplete(int percentComplete);
		/// <summary>
		/// Occurs when each security completes, shows completion percentage.
		/// </summary>
		public event PercentComplete PercentCompleteChanged;
		private void ShowCompletionPercentage(int count, int total) {
			if (PercentCompleteChanged != null) {
				int pc = (int)(((double)count / (double)total) * 100);
				PercentCompleteChanged(pc);
			}
		}

		public delegate void StatusUpdate(string status);
		/// <summary>
		/// Occurs when the status changes - used to display messages.
		/// </summary>
		public event StatusUpdate StatusChanged;
		private void UpdateStatus(string status) {
			if (StatusChanged != null) {
				StatusChanged(status);
			}
		}
		#endregion

		#region private properties

		private BB.SessionOptions sessionOptions;
		private BB.Session session;

		private const string BLP_REFERENCE_REQUEST = "ReferenceDataRequest";
		private const string BLP_HISTORICAL_REQUEST = "HistoricalDataRequest";
		private const string BLP_REFERENCE_RESPONSE = "ReferenceDataResponse";
		private const string BLP_HISTORICAL_RESPONSE = "HistoricalDataResponse";
		private const string BLP_SECURITIES = "securities";
		private const string BLP_FIELDS = "fields";
		private const string BLP_OVERRIDES = "overrides";
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
		private System.Collections.Specialized.StringCollection limits;
		private string emailErrorsTo = "duoc@mpuk.com";
		private string statusSystemName = string.Empty;
		private string arrayDelimiter = ";";
		private bool sentLimitEmail = false;
		private Object lockObject = new Object();
		private bool disposed = false;

		#endregion

		#region public methods

		/// <summary>
		/// Gets the Bloomberg data for the security.
		/// </summary>
		/// <param name="instruments">The instruments (securities) that we need to get data for.</param>
		public void GetBloombergData(List<BloombergDataInstrument> instruments) {

			GetBloombergData(instruments, DateTime.Today, BLP_REFERENCE_REQUEST);
		}

		/// <summary>
		/// Gets the Bloomberg data for the security.
		/// </summary>
		/// <param name="instruments">The instruments (securities) that we need to get data for.</param>
		/// <param name="dateOfData">The date of the data to retrieve.</param>
		public void GetBloombergData(List<BloombergDataInstrument> instruments, DateTime dateOfData) {
			GetBloombergData(instruments, dateOfData, BLP_HISTORICAL_REQUEST);
		}

		public void GetBloombergData(List<BloombergDataInstrument> instruments, DateTime dataFromDate, DateTime dataToDate) {
			GetBloombergData(instruments, dataFromDate, dataToDate, BLP_HISTORICAL_REQUEST);
		}

		#endregion

		#region private methods
		/// <summary>
		/// Initialises the specified URI session and opens the service.
		/// </summary>
		/// <param name="uri">The URI.</param>
		/// <returns></returns>
		private bool Initialise(string uri, int requestCount) {
			sessionOptions = new BB.SessionOptions();
			sessionOptions.ServerHost = "localhost";
			sessionOptions.ServerPort = 8194;
			if (requestCount > sessionOptions.MaxPendingRequests) {
				sessionOptions.MaxPendingRequests = requestCount;
			}

			UpdateStatus("Starting the Bloomberg session.");
			session = new BB.Session(sessionOptions, new BB.EventHandler(processEvent));

			if (session.Start()) {
				if (!session.OpenService(uri)) {
					throw new Exception("Bloomberg failed to open session " + uri);
				}
				return true;
			} else {
				throw new Exception("An error occurred starting the Bloomberg session. Ensure Bloomberg is installed.");
			}
		}

		/// <summary>
		/// Gets the Bloomberg data via the specified request type
		/// </summary>
		/// <param name="instruments">The list of instruments to retrieve data for</param>
		/// <param name="dateOfData">The date of the data being requested</param>
		/// <param name="requestType">The type of request i.e. Reference/Historical</param>
		private void GetBloombergData(List<BloombergDataInstrument> instruments, DateTime dateOfData, string requestType) {
			GetBloombergData(instruments, dateOfData, dateOfData, requestType);
		}

		/// <summary>
		/// Gets the Bloomberg data via the specified request type
		/// </summary>
		/// <param name="instruments">The list of instruments to retrieve data for</param>
		/// <param name="dataFromDate">The start date of the data being requested</param>
		/// <param name="dataToDate">The end date of the data being requested</param>
		/// <param name="requestType">The type of request i.e. Reference/Historical</param>
		private void GetBloombergData(List<BloombergDataInstrument> instruments, DateTime dataFromDate, DateTime dataToDate, string requestType) {
			try {
				sentLimitEmail = false;
				Initialise(BLP_REF, instruments.Count);
				BB.Service service = session.GetService(BLP_REF);

				guids = new List<Guid>();
				sentToBB = instruments; // new List<BloombergDataInstrument>();
				ShowCompletionPercentage(0, instruments.Count);

				foreach (BloombergDataInstrument bbdi in instruments) {
					BB.Request request = service.CreateRequest(requestType);

					if (requestType == BLP_HISTORICAL_REQUEST) {
						request.Set("startDate", dataFromDate.ToString("yyyyMMdd"));
						request.Set("endDate", dataToDate.ToString("yyyyMMdd"));
					}
					BB.Element securities = request.GetElement(BLP_SECURITIES);

					// check for sedol ticker which must be in the correct format
					string ticker = bbdi.Ticker;
					if (ticker.EndsWith("SEDOL1")) {
						ticker = @"/SEDOL1/" + ticker.Replace(" SEDOL1", string.Empty);
					}

					// set all the securities to fetch
					securities.AppendValue(ticker);

					// set all the fields
					BB.Element fields = request.GetElement(BLP_FIELDS);
					foreach (string field in bbdi.BBFields.Keys) {
						fields.AppendValue(field);

						// now do the overrides - if they exist
						if (bbdi.BBFields[field].FieldOverrides != null) {
							BB.Element requestOverrides = request.GetElement(BLP_OVERRIDES);
							foreach (string oField in bbdi.BBFields[field].FieldOverrides.Keys) {
								object oValue = bbdi.BBFields[field].FieldOverrides[oField];
								// now add in the override oField and oValue
								BB.Element ovr = requestOverrides.AppendElement();
								ovr.SetElement(FIELD_ID, oField);
                                ovr.SetElement(VALUE, oValue.ToString());
							}
						}
					}
					session.SendRequest(request, new BB.CorrelationID(bbdi.GUID));
				}

				UpdateStatus(string.Format("Sent {0} instruments\\requests to Bloomberg", sentToBB.Count));

			} catch (Exception ex) {
				UpdateStatus(ex.Message);
				throw new Exception("An error occurred whilst sending requests to Bloomberg - " + ex.Message, ex);
			}
		}

		/// <summary>
		/// Processes the Bloomberg event.
		/// </summary>
		/// <param name="eventObj">The event obj.</param>
		/// <param name="session">The session.</param>
		private void processEvent(BB.Event eventObj, BB.Session session) {
			try {
				switch (eventObj.Type) {
					case BB.Event.EventType.RESPONSE:
					case BB.Event.EventType.PARTIAL_RESPONSE:
						processSubscriptionDataEvent(eventObj, session);
						break;
					default:
						processMiscEvents(eventObj, session);
						break;
				}
			} catch (System.Exception e) {
				Maple.Logger.Log(e.Message);
				Maple.Logger.Log(e.StackTrace);
			}
		}

		/// <summary>
		/// Processes misc Bloomberg events.
		/// </summary>
		/// <param name="eventObj">The event obj.</param>
		/// <param name="session">The session.</param>
		private void processMiscEvents(BB.Event eventObj, BB.Session session) {
			foreach (BB.Message msg in eventObj.GetMessages()) {

				UpdateStatus(msg.MessageType.ToString());

				switch (msg.MessageType.ToString()) {
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
						UpdateStatus("Error message body : " + msg.ToString());

						bool hasGUID = false;
						bool hasResponseError = false;
						try {
							Guid reqGUID = (Guid)msg.CorrelationID.Object;
							hasGUID = true;
							hasResponseError = msg.HasElement("responseError");
						} catch {
							hasGUID = false;
							hasResponseError = false;
						}

						if (hasGUID & hasResponseError) {
							UpdateStatus("GUID and responseError found - handling normally");

							// has both a GUID and a response error so can be handled normally
							processSubscriptionDataEvent(eventObj, session);

						} else if (hasGUID) {
							UpdateStatus("GUID found - updating GUID list.");

							Guid reqGUID = (Guid)msg.CorrelationID.Object;
							// only has a GUID so add to the list of returned GUIDs
							if (!guids.Contains(reqGUID)) {
								guids.Add(reqGUID);
							}

						} else {
							// ok, email out that a timeout has occurred
							Maple.Email email = new Maple.Email();
							email.SendEmail(emailErrorsTo,
								"Bloomberg Request Failure",
								"A RequestFailure response has been returned from Bloomberg." + Environment.NewLine + reasonText + Environment.NewLine + msg.ToString(),
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
		private BloombergDataInstrument FindSentInstrumentByGuid(Guid guid) {
			BloombergDataInstrument instr = null;
			foreach (BloombergDataInstrument bbdi in sentToBB) {
				if (bbdi.GUID == guid) {
					instr = bbdi;
					break;
				}
			}
			return instr;
		}

		/// <summary>
		/// Processes the Bloomberg subscription data event.
		/// </summary>
		/// <param name="eventObj">The event obj.</param>
		/// <param name="session">The session.</param>
		private void processSubscriptionDataEvent(BB.Event eventObj, BB.Session session) {
			try {
				// process message
				foreach (BB.Message msg in eventObj.GetMessages()) {

					#region find instrument
					Guid reqGUID = (Guid)msg.CorrelationID.Object;

					// check for duplicate replies just in case
					if (guids.Contains(reqGUID)) {
						return;
					} else {
						guids.Add(reqGUID);
					}

					// find the correct instrument
					BloombergDataInstrument bbdi = FindSentInstrumentByGuid(reqGUID);
					if (bbdi == null) {
						UpdateStatus("Unable to find received instrument by Guid - " + reqGUID.ToString());
						continue;
					}
					#endregion

					UpdateStatus(string.Format("Received {0} of {1} requests : {2}", guids.Count, sentToBB.Count, bbdi.Ticker));

					if (msg.HasElement("responseError")) {
						BB.Element error = msg.GetElement(RESPONSE_ERROR);
						string responseError = error.GetElementAsString(SUBCATEGORY);
						UpdateStatus("Response error : " + error.GetElementAsString(MESSAGE));
						CheckForLimits(responseError);
						bbdi.HasFieldErrors = true;
						continue;
					}
					BB.Element secDataArray = msg.GetElement(SECURITY_DATA);

					#region process security data

					// process security data
					int numberOfSecurities = secDataArray.NumValues;
					for (int index = 0; index < numberOfSecurities; index++) {

						if (msg.MessageType.Equals(Bloomberglp.Blpapi.Name.GetName(BLP_REFERENCE_RESPONSE))) {

							// just contains the one element, which is actually a sequence
							BB.Element secData = secDataArray.GetValueAsElement(index);
							BB.Element fields = secData.GetElement(FIELD_DATA);

							GetData(bbdi, fields, secData);

						} else if (msg.MessageType.Equals(Bloomberglp.Blpapi.Name.GetName(BLP_HISTORICAL_RESPONSE))) {
							// Historical is handled slightly different.  Can contain many elements each with possibly multiple values
							foreach (BB.Element secData in secDataArray.Elements) {
								if (secData.Name != FIELD_DATA)
									continue;

								for (int pointIndex = 0; pointIndex < secData.NumValues; pointIndex++) {
									BB.Element fields = secData.GetValueAsElement(pointIndex);

									GetData(bbdi, fields, secData);
								}
							}
						}
					}
					#endregion

					ShowCompletionPercentage(guids.Count, sentToBB.Count);
					ShowCompletedInstrument(bbdi);
				}
			} catch (Exception ex) {
				UpdateStatus("Error occurred processing reply : " + ex.Message);
			} finally {
				if (guids.Count == sentToBB.Count) { // we are done
					ShowCompletionPercentage(1, 1);
					ProcessComplete();
				}
			}
		}

		private void GetData(BloombergDataInstrument instrument, BB.Element fields, BB.Element secData) {

			#region security errors
			if (secData.HasElement(SECURITY_ERROR)) {
				instrument.IsSecurityValid = false;
				BB.Element error = secData.GetElement(SECURITY_ERROR);
				UpdateStatus(string.Format("Security error for ticker {0} : {1}", instrument.Ticker, error.GetElementAsString(MESSAGE)));
                instrument.SecurityErrors += (error.GetElementAsString(MESSAGE) + "; ");
			}
			#endregion

			#region field errors
			if (secData.HasElement(FIELD_EXCEPTIONS)) {
				instrument.HasFieldErrors = true;
				// process error
				BB.Element error = secData.GetElement(FIELD_EXCEPTIONS);
				for (int errorIndex = 0; errorIndex < error.NumValues; errorIndex++) {
					BB.Element errorException = error.GetValueAsElement(errorIndex);
					BB.Element errorInfo = errorException.GetElement(ERROR_INFO);
					
					instrument.BBFields[errorException.GetElementAsString(FIELD_ID)].Error = errorInfo.GetElementAsString(MESSAGE);
					instrument.HasFieldErrors = true;
					string msg = string.Format("Field error for ticker {0} : Field {1}: {2}",
						instrument.Ticker,
						errorException.GetElementAsString(FIELD_ID),
						errorInfo.GetElementAsString(MESSAGE));
					UpdateStatus(msg);
				}
			}
			#endregion

			#region get the data
			if (instrument.BBFields != null) {
				lock (lockObject) {
					foreach (string bbField in instrument.BBFields.Keys.ToList()) {
						if (fields.HasElement(bbField)) {
							BB.Element item = fields.GetElement(bbField);
							if (item.IsArray) {
								instrument.BBFields[bbField].Value = processBulkData(item);
							} else {
								// set the value in the instrument field item
								instrument.BBFields[bbField].Value = item.GetValue();
							}
						}
					}
				}
			}
			#endregion
		}

		private string processBulkData(BB.Element data) {
			string ret = null;

			try {
				if (data.NumValues > 0) {
					for (int index = 0; index < data.NumValues; index++) {
						BB.Element bulk = data.GetValueAsElement(index);

						if (bulk.NumElements > 1) {
							ret += "{";
						}
						foreach (BB.Element item in bulk.Elements) {
							ret += item.GetValueAsString() + arrayDelimiter;
						}
						if (bulk.NumElements > 1) {
							ret += "}";
						}
					}
				}

			} catch (Exception ex) {
				UpdateStatus("Error occurred processing array field : " + ex.Message);
				ret = null;
			}
			return ret;
		}

		private void CheckForLimits(string message) {
			foreach (string msg in limits) {
				if (message.Contains(msg)) {
					// there is a limit message

					#region system status
					Maple.ApplicationStatus.SetStatus(statusSystemName, "Error", "Bloomberg limit has been reached - " + msg + ".", 1);
					#endregion

					#region email
					if (!sentLimitEmail) {
						Maple.Email email = new Maple.Email();
						string html = string.Format("<HTML><BODY><P>{0}</P><P>This automated email sent from {1} : </BR> {2} </P></BODY><HTML>",
							"Please check, get the limit reset by Bloomberg ASAP, and if necessary run the app on a backup machine.</BR></BR>" + message,
							Environment.MachineName,
							Environment.GetCommandLineArgs()[0]);
						email.SendEmail(emailErrorsTo,
							"Bloomberg limit has been reached on machine " + Environment.MachineName,
							html,
							true);
					}
					#endregion

					sentLimitEmail = true;
					break;
				}
			}
		}
		#endregion

	}
}
