using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Timers;
using System.Windows.Forms;
using BBfieldValueRetriever.Model;
using BBfieldValueRetriever.Properties;
using Maple;
using Timer = System.Timers.Timer;

namespace BBfieldValueRetriever.Control
{
    public class BergController
    {
        private BloombergAdHocController _adHocQueueController;
        private BloombergDatawarehouseController _datawarehouseController;

        public BloombergApiController ApiController { get; set; }

        private List<RequestItemRoutingRule> _routingRules;
        public Dictionary<string, string> FieldPricingCategories;

        public event MessageEventDelegate MessageEvent;

        private Timer _localQueueTimer = new Timer();

        private DateTime _setStatusTime = DateTime.Now;

        public Database Db;

        private void RaiseMessageEvent(string message)
        {
            // To avoid a race condition where the last handler can be removed
            // between the null check and the invocation of the event, event
            // sources should also create a copy of the event before performing
            // the null check and raising the event.
            MessageEventDelegate temp = MessageEvent;
            if (temp != null)
            {
                MessageEventArgs e = new MessageEventArgs(message);
                temp(this, e);
            }
        }

        public BergController()
        {
            Db = new Database();
            _routingRules = Db.GetRoutingRules();
            FieldPricingCategories = Db.GetFieldPricingCategories();
            _adHocQueueController = new BloombergAdHocController(this);
            _datawarehouseController = new BloombergDatawarehouseController(this);
            ApiController = new BloombergApiController(this);

            _localQueueTimer.Elapsed += mainTimer_Elapsed;
            _localQueueTimer.Interval = Settings.Default.MillisecondTimerInterval;

            //deal with stale messages on startup before starting timers
            OnResurrect();
        }

        public IEnumerable<RequestItem> PeekStaleMessagesFromLocalQueue()
        {
            //peek local q for my hostname stale messages
            NLogger.Instance.Info("PeekStaleMessagesFromLocalQueue...");

            var list = Db.GetTickerItemsToProcess(File.ReadAllText("Control\\PeekStaleMessagesForThisServer.sql"));
            foreach (var item in list)
                NLogger.Instance.Info("Stale found: {0} {1} (Inserted at {2})", item.BBTicker, item.BBFieldList, item.InsertedWhen);
            return list;
        }

        public void OnResurrect()
        {
            NLogger.Instance.Info("OnResurrect - Dealing with stale messages first before starting all timers.");

            var staleLocalMessages = PeekStaleMessagesFromLocalQueue();

            if (!staleLocalMessages.Any())
                NLogger.Instance.Info("No stale messages found on resurrect");
            else
                ProcessDataRequests(staleLocalMessages, true);

            NLogger.Instance.Info("OnResurrect - Done with stale messages.");
        }

        public string Start()
        {
            //todo Sort this Stop method out
            _localQueueTimer.Start();
            _adHocQueueController.StartTimer();
            RaiseMessageEvent("Started");

            return string.Empty;
        }

        public string Stop()
        {
            //todo Sort this Stop method out
            _localQueueTimer.Stop();
            _adHocQueueController.StopTimer();
            RaiseMessageEvent("Stopped");

            return string.Empty;
        }

        private string ShutdownSignalFile()
        {
            return Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "\\shutdown";
        }

        private bool ShutdownHasBeenSignalled()
        {
            if (File.Exists(ShutdownSignalFile()))
            {
                NLogger.Instance.Info("Shutdown signal received-> {0} ", ShutdownSignalFile());
                NLogger.Instance.Info("Shutdown signal received-> {0} ", ShutdownSignalFile());
                return true;
            }
            return false;
        }

        private bool _isProcessing;

        private void mainTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (_isProcessing) return;
            _isProcessing = true;

            // Check database for new items to fetch
            try
            {
                //check for shutdown flag.
                if (ShutdownHasBeenSignalled())
                {
                    if (_adHocQueueController.GlobalListOfOutstandingAdHocRequests.Count == 0)
                    {
                        NLogger.Instance.Info("No pending adhoc requests...Deleting signal file and shutting down process");
                        File.Delete(ShutdownSignalFile());
                        Application.Exit();
                    }
                    else
                    {
                        NLogger.Instance.Info("{0} pending adhoc request(s)... cannot shutdown. Will retry as long as shutdown signal exists.");
                        return;
                    }
                }

                RaiseMessageEvent(string.Format("Checking at {0}", DateTime.Now.ToString("HH:mm:ss")));

                // All items from DB

                ProcessDataRequests(Db.GetTickerItemsToProcess("EXEC GetBloombergDataRequestItems"), false);

                if (DateTime.Now > _setStatusTime)
                {
                    ApplicationStatus.SetStatus(Application.ProductName, "OK", "", 1);
                    _setStatusTime = DateTime.Now.AddSeconds(59);
                }
                else
                {
                    RaiseMessageEvent(string.Format("Next Status set {0}", _setStatusTime.ToString("dd MMM yy HH:mm:ss")));
                }
            }
            catch (SqlException ex)
            {
                NLogger.Instance.Error("Sql server error: Exiting program. Error: {0}", ex.ToString());
                Application.Exit();
            }
            catch (Exception ex)
            {
                NLogger.Instance.Fatal(ex.ToString(), ex);
                RaiseMessageEvent(string.Format("Error: {0}", ex));
                Static.EmailException("BB Field Value Retriever Error", ex.ToString(), ex);
            }
            finally
            {
                _isProcessing = false;
            }
        }

        private void ProcessDataRequests(IEnumerable<RequestItem> requestItemsAll, bool tryReuseMessagesOnRemoteQueue)
        {
            if (!requestItemsAll.Any()) return;

            foreach (var item in requestItemsAll)
                NLogger.Instance.Info("Dequeued message - ID: {0} bbticker: {1} fields: {2}", item.ID, item.BBTicker, item.BBFieldList);

            //refresh routing table.
            var oldRules = _routingRules;
            _routingRules = Db.GetRoutingRules();
            if (!oldRules.SequenceEqual(_routingRules))
                NLogger.Instance.Info("Routing rule table has changed...changes picked up ...");

            //split into separate lists - one for datawarehouse, one for legacy api, one for bloomberg ad hoc
            var requestItemsForBloombergApi = new List<RequestItem>();
            var requestItemsForBloombergAdHoc = new List<RequestItem>();
            var requestItemsForDatawarehouse = new List<RequestItem>();

            foreach (var item in requestItemsAll)
            {
                var ds = GetFirstMatchingDataSource(item);
                if (ds == "Warehouse")
                    requestItemsForDatawarehouse.Add(item);
                else if (ds == "BLAPI")
                    requestItemsForBloombergApi.Add(item);
                else if (ds == "AdHoc")
                    requestItemsForBloombergAdHoc.Add(item);
                else
                    NLogger.Instance.Info("No matching rule found in routing table for userid {0}", item.UserId);
            }

            //process api
            if (requestItemsForBloombergApi.Count != 0)
            {
                var tickers = string.Join(",", requestItemsForBloombergApi.Select(x => x.BBTicker).ToArray());
                RaiseMessageEvent(string.Format("Calling Bloomberg API for {0} tickers ({1})", requestItemsForBloombergApi.Count, tickers));
                ApiController.ProcessDataRequests(requestItemsForBloombergApi);
            }

            //process warehouse
            if (requestItemsForDatawarehouse.Count != 0)
            {
                var tickers = string.Join(",", requestItemsForDatawarehouse.Select(x => x.BBTicker).ToArray());
                RaiseMessageEvent(string.Format("Calling Warehouse for {0} tickers ({1})", requestItemsForDatawarehouse.Count, tickers));
                _datawarehouseController.ProcessDataRequests(requestItemsForDatawarehouse);
            }

            //check for returned errors
            var apiSecondChance = requestItemsForDatawarehouse.FindAll(x => x.Errors.StartsWith("Ticker not found"));
            if (apiSecondChance.Count > 0)
            {
                var tickers = string.Join(",", apiSecondChance.Select(x => x.OriginalInputTicker).ToArray());
                RaiseMessageEvent(string.Format("Trying Bloomberg API for tickers which could not be found in warehouse... {0} tickers ({1})", apiSecondChance.Count, tickers));
                ApiController.ProcessDataRequests(apiSecondChance);
            }

            //go to bloomberg adhoc controller - async
            if (requestItemsForBloombergAdHoc.Count > 0)
            {
                if (tryReuseMessagesOnRemoteQueue)
                {
                    IEnumerable<BloombergAdHocRequestData> remoteMessages = _adHocQueueController.GetMessagesOnRemoteQueueSinceThisTime(requestItemsForBloombergAdHoc.Min(x => x.InsertedWhen));
                    List<int> dsToDelete = new List<int>();
                    foreach (var staleLocal in requestItemsForBloombergAdHoc)
                    {
                        var newAdHocRequest = new BloombergAdHocRequestData(staleLocal);

                        var matchedRemoteMessage = _adHocQueueController.GetMatchingRemoteMessage(newAdHocRequest, remoteMessages);

                        //cant find it in the list of remote messages.

                        if (matchedRemoteMessage == null)
                        {
                            NLogger.Instance.Info("No match for stale: {0} / {1}  - will resubmit to remote queue.", staleLocal.BBTicker, staleLocal.BBFieldList);
                        }
                        else
                        //found. add it to the list of bloombergAdHocRequests for monitoring
                        {
                            newAdHocRequest.bloombergAdHocId = matchedRemoteMessage.bloombergAdHocId;
                            _adHocQueueController.GlobalListOfOutstandingAdHocRequests.TryAdd(newAdHocRequest.OriginalRequestItem.ID, newAdHocRequest);
                            dsToDelete.Add(newAdHocRequest.OriginalRequestItem.ID);
                            NLogger.Instance.Info("Match found for stale: {0} / {1} -> ID>{2}", staleLocal.BBTicker, staleLocal.BBFieldList, newAdHocRequest.bloombergAdHocId);
                        }
                    }

                    foreach (var id in dsToDelete) requestItemsForBloombergAdHoc.RemoveAll(x => x.ID == id);
                }
                //console logging only
                var tickers = string.Join(",", requestItemsForBloombergAdHoc.Select(x => x.BBTicker).ToArray()); RaiseMessageEvent(string.Format("Calling Ad Hoc for {0} tickers ({1})", requestItemsForBloombergAdHoc.Count, tickers));

                _adHocQueueController.SubmitDataRequestsAsync(requestItemsForBloombergAdHoc);
            }
        }

        /// <summary>
        /// Determines whether this is to be serviced by the Bloomberg API or the cached datawarehouse
        /// </summary>
        /// <returns></returns>
        public bool IsForLegacyBloombergApi(RequestItem thisRequestItem)
        {
            return IsForThisDataSource(thisRequestItem, "BLAPI");
        }

        public bool IsForWarehouse(RequestItem thisRequestItem)
        {
            return IsForThisDataSource(thisRequestItem, "Warehouse");
        }

        public string GetFirstMatchingDataSource(RequestItem thisRequestItem)
        {
            foreach (var rule in _routingRules)
            {
                if (
                    (rule.UserIdMatchRegex == null || new Regex(rule.UserIdMatchRegex).IsMatch(thisRequestItem.UserId)) &&
                    (rule.FieldListMatchRegex == null || new Regex(rule.FieldListMatchRegex).IsMatch(thisRequestItem.BBFieldList))
                    )
                    return rule.Datasource;
            }

            return "None";
        }

        private bool IsForThisDataSource(RequestItem thisRequestItem, string thisDatasource)
        {
            foreach (var rule in _routingRules)
            {
                if (rule.Datasource == thisDatasource)
                {
                    if (
                        (rule.UserIdMatchRegex == null || new Regex(rule.UserIdMatchRegex).IsMatch(thisRequestItem.UserId)) &&
                        (rule.FieldListMatchRegex == null || new Regex(rule.FieldListMatchRegex).IsMatch(thisRequestItem.BBFieldList))
                        )
                        return true;
                }
            }

            return false;
        }

        public bool IsForBloombergAdHoc(RequestItem thisRequestItem)
        {
            return IsForThisDataSource(thisRequestItem, "AdHoc");
        }
    }
}