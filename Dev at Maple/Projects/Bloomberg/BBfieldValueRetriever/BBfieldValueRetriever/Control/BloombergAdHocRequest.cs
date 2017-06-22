using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using BBfieldValueRetriever.Model;
using Maple;
namespace BBfieldValueRetriever.Control
{
    public class BloombergAdHocRequestTest : BloombergAdHocController
    {
        public BloombergAdHocRequestTest(BergController mainController)
            : base(mainController)
        {
            //other stuff here
        }

        protected override string GetReturnData(BloombergAdHocRequestData adHocRequestData)
        {
            return "GBp||London|LN|LN||GB00B1S49Q91|B1S49Q9||Domino''s Pizza Group PLC|DOMINO''S PIZZA GROUP PLC||DOM LN||17";
        }

        public override int SubmitDataRequestsAsync(IEnumerable<RequestItem> adHocRequests)
        {
            foreach (var adHocRequest in adHocRequests)
                GlobalListOfOutstandingAdHocRequests.TryAdd(adHocRequest.ID, new BloombergAdHocRequestData(adHocRequest));
            return 1;
        }
    }

    /// <summary>
    /// Manages a request to Toronto's ad hoc request table
    /// </summary>
    public class BloombergAdHocController : BloombergDatasourceController
    {
        public ConcurrentDictionary<int, BloombergAdHocRequestData> GlobalListOfOutstandingAdHocRequests = new ConcurrentDictionary<int, BloombergAdHocRequestData>();
        private Timer _remoteAdHocQueueTimer = new Timer();

        public void StartTimer()
        {
            _remoteAdHocQueueTimer.Start();
        }

        public void StopTimer()
        {
            _remoteAdHocQueueTimer.Stop();
        }

        public BloombergAdHocController(BergController mainController)
        {
            Datasource = "AdHoc";
            this.MainController = mainController;
            Db = mainController.Db;
            _remoteAdHocQueueTimer.Interval = 5000;
            _remoteAdHocQueueTimer.Elapsed += timerCheckRemoteQueue_Elapsed;
        }

        public virtual int SubmitDataRequestsAsync(IEnumerable<RequestItem> adHocRequests)
        {
            //get remote queue ID marker.
            var idMarker = Utils.DbController.GetScalar<int>("select max(bloombergadhocid) from HELIUM.[BloombergDataLicense].[dbo].BloombergAdHoc;") + 1;

            Parallel.ForEach(adHocRequests, new ParallelOptions { MaxDegreeOfParallelism = 10 }, x =>
            {
                var newRemoteRequest = new BloombergAdHocRequestData(x) { bloombergAdHocId = idMarker };
                var sql = String.Format("INSERT INTO [HELIUM].[BloombergDataLicense].[dbo].[BloombergAdHoc] ([FieldsToPull], [SecurityIdentifier],lastupdatedatetime,lastupdateuser)VALUES ( '{0}', '{1}',getdate(),'Berg: {2}' );", newRemoteRequest.OriginalRequestItem.BBFieldList + ",DL_ASSET_CLASS", newRemoteRequest.MappedFriendlyTicker(), newRemoteRequest.OriginalRequestItem.UserId);
                Utils.DbController.ExecuteNonQuery(sql);
                GlobalListOfOutstandingAdHocRequests.TryAdd(newRemoteRequest.OriginalRequestItem.ID, newRemoteRequest);
                //log
                NLogger.Instance.Info("{0} Ad hoc data request sent for request ID {1} for {2}", Process.GetCurrentProcess().Id, x.ID, x.BBTicker);
            });

            return idMarker;
        }

        /// <summary>
        /// Check for return data. Keep timer enabled if not ready.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        private void timerCheckRemoteQueue_Elapsed(object source, ElapsedEventArgs e)
        {
            NLogger.Instance.Info("Checking for data... waiting for {0} ticker(s) ... ... ({1})", GlobalListOfOutstandingAdHocRequests.Count, string.Join(",", GlobalListOfOutstandingAdHocRequests.Values.Select(x => x.OriginalRequestItem.BBTicker)));
            if (GlobalListOfOutstandingAdHocRequests.Count == 0) return;

            //get all done requests
            var completedRemoteMessages = GetCompletedMessagesSinceId(GlobalListOfOutstandingAdHocRequests.Min(x => x.Value.bloombergAdHocId));

            var idsToDelete = new List<int>();
            foreach (var liveRequest in GlobalListOfOutstandingAdHocRequests)
            {
                var matchedRemoteMessage = GetMatchingRemoteMessage(liveRequest.Value, completedRemoteMessages);
                if (matchedRemoteMessage != null && matchedRemoteMessage.ReturnData != null)
                {
                    liveRequest.Value.ReturnData = matchedRemoteMessage.ReturnData;
                    //remove from bloomberg controller's master list (concurrent dictionary of bloomberg ad hoc requests)
                    BloombergAdHocRequestDataReadyHandler(liveRequest.Value);
                    idsToDelete.Add(liveRequest.Key);
                }
            }

            foreach (var id in idsToDelete)
            {
                BloombergAdHocRequestData throwaway;
                GlobalListOfOutstandingAdHocRequests.TryRemove(id, out throwaway);
            }
        }

        private void CostReportActual(List<RequestItem> requestItems)
        {
            foreach (RequestItem ri in requestItems)
            {
                foreach (DateTime key in ri.Data.Keys.ToList())
                {
                    var categories = new List<string>(); //dont need to repeat price categories
                    foreach (var field in Static.SplitWithStringDelimeters(ri.BBFieldList, ',', '[', ']'))
                    {
                        var thisCategory = MainController.FieldPricingCategories.ContainsKey(field.Trim()) ? MainController.FieldPricingCategories[field.Trim()] : "Unknown";

                        if (!categories.Contains(thisCategory))
                        {
                            categories.Add(thisCategory);
                            try
                            {
                                var logentry = string.Format("INSERT INTO CostReportActual (RetrieveTime,Datasource,Ticker,dl_asset_class,FieldPriceCategory) VALUES ('{0:yyyy-MMM-dd HH:mm:ss}','AdHoc','{1}','{2}','{3}');",
                                DateTime.Now,
                                ri.OriginalInputTicker,
                                ri.TickerDownloadAssetClass,
                                thisCategory);

                                NLogger.Instance.Info(logentry);
                                Utils.DbController.ExecuteNonQuery(logentry);
                            }
                            catch (Exception ex)
                            {
                                NLogger.Instance.Error("Error in writing to table CostReportActual {0}", ex.ToString());
                            }
                        }
                    }
                }
            }
        }

        private void BloombergAdHocRequestDataReadyHandler(BloombergAdHocRequestData returnedReq)
        {
            NLogger.Instance.Info("{0} Ad hoc data received for request ID {1} for {2} at {3}.", Process.GetCurrentProcess().Id,
                        returnedReq.OriginalRequestItem.ID,
                        returnedReq.OriginalRequestItem.BBTicker,
                        DateTime.Now.ToString("HH:mm"));

            //call db.savevalues with the original Request Item
            var valueArray = returnedReq.ReturnData.EndsWith("|") ?
                returnedReq.ReturnData.Substring(0, returnedReq.ReturnData.Length - 1).Split('|') :
                returnedReq.ReturnData.Split('|');

            //fill asset class to last value;
            returnedReq.OriginalRequestItem.TickerDownloadAssetClass = valueArray[valueArray.Length - 1];

            //fill errors
            if (valueArray[0].StartsWith("Problem with Request format", StringComparison.OrdinalIgnoreCase)
                ||
                valueArray[0].StartsWith("Error_Code", StringComparison.OrdinalIgnoreCase)
            )
                returnedReq.OriginalRequestItem.Errors += string.Format("Toronto AdHoc Service/Bloomberg AdHoc Data License Webservice error: {0}", valueArray[0]);
            else
            {
                //error log NA NS or blanks.
                var fieldList = returnedReq.OriginalRequestItem.BBFieldList.Split(',');
                for (int i = 0; i < fieldList.Length; i++)
                {
                    var returnedValue = valueArray[i];
                    var key = fieldList[i];

                    //Clean values
                    valueArray[i] = Static.CleanValueReturnedFromBloomberg(returnedReq.OriginalRequestItem.BBTicker, key, valueArray[i]);

                    //dont write N.A.
                    if (returnedValue.Equals("N.A.") || returnedValue.Equals("N.S.") || returnedValue.Trim().Equals(string.Empty))
                    {
                        returnedReq.OriginalRequestItem.Errors += string.Format("[{0}|returned {1}]", key, returnedValue.Trim().Equals(string.Empty) ? "blank string" : returnedValue);
                        valueArray[i] = null;
                    }
                }
                returnedReq.OriginalRequestItem.Data.Add(DateTime.Now, valueArray);
            }
            Db.SaveValues(new List<RequestItem> { returnedReq.OriginalRequestItem });

            CostReportUserAttribution(new List<RequestItem> { returnedReq.OriginalRequestItem });
        }

        public override void CostReportUserAttribution(List<RequestItem> requestItems)
        {
            base.CostReportUserAttribution(requestItems);
            CostReportActual(requestItems);
        }

        /// <summary>
        /// Check for return data.
        /// </summary>
        /// <returns></returns>
        protected virtual string GetReturnData(BloombergAdHocRequestData adHocRequestData)
        {
            var sql = string.Format("select top 1 returndata from [HELIUM].[BloombergDataLicense].[dbo].[BloombergAdHoc] where returndata is not null and bloombergadhocid >= {0} and fieldstopull='{1},DL_ASSET_CLASS' and securityidentifier='{2}' order by bloombergAdHocId desc", adHocRequestData.bloombergAdHocId, adHocRequestData.OriginalRequestItem.BBFieldList, adHocRequestData.MappedFriendlyTicker());
            return Utils.DbController.GetScalar<string>(sql);
        }

        protected virtual IEnumerable<BloombergAdHocRequestData> GetCompletedMessagesSinceId(int adHocQueueRequestId)
        {
            var sql = string.Format("select securityidentifier, fieldstopull, returndata from [HELIUM].[BloombergDataLicense].[dbo].[BloombergAdHoc] where returndata is not null and bloombergadhocid >= {0} and status=2", adHocQueueRequestId);
            return Utils.DbController.GetObjects<BloombergAdHocRequestData>(sql);
        }

        public BloombergAdHocRequestData GetMatchingRemoteMessage(BloombergAdHocRequestData localMessage, IEnumerable<BloombergAdHocRequestData> remoteMessages)
        {
            var matches = remoteMessages.Where(remote => remote.SecurityIdentifier == localMessage.MappedFriendlyTicker() && remote.FieldsToPull == string.Format("{0},DL_ASSET_CLASS", localMessage.OriginalRequestItem.BBFieldList));
            if (matches.Count() == 0) return null;
            return matches.First(x => x.bloombergAdHocId == matches.Max(y => y.bloombergAdHocId));
        }

        public IEnumerable<BloombergAdHocRequestData> GetMessagesOnRemoteQueueSinceThisTime(DateTime gmtTime)
        {
            var utc = gmtTime.ToUniversalTime();

            DateTime est = TimeZoneInfo.ConvertTime(gmtTime,
                                    TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time")
                  );

            StringBuilder sql = new StringBuilder();
            sql.Append("SELECT bloombergadhocid, \n");
            sql.Append("       securityidentifier , fieldstopull \n");
            sql.Append("FROM   [HELIUM].[BloombergDataLicense].[dbo].[BloombergAdHoc] \n");
            sql.Append("WHERE  \n");
            sql.Append("        ( ( lastupdateuser LIKE 'berg%' \n");
            sql.AppendFormat("               AND lastupdatedatetime >= '{0:ddMMMyyyy HH:mm:ss}' ) ", gmtTime);
            sql.Append("              OR ( lastupdateuser NOT LIKE 'berg%' \n");
            sql.AppendFormat("                   AND lastupdatedatetime >= '{0:ddMMMyyyy HH:mm:ss}' ) ) ", est);
            sql.Append("ORDER  BY bloombergadhocid DESC");

            var list = Utils.DbController.GetObjects<BloombergAdHocRequestData>(sql.ToString());

            Parallel.ForEach(list, x => NLogger.Instance.Info("Remote messages since {0} London time-> ID:{1} Ticker/Fields:{2}/{3}", gmtTime, x.bloombergAdHocId, x.SecurityIdentifier, x.FieldsToPull));

            return list;
        }
    }
}