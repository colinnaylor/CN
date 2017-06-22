using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using BBfieldValueRetriever;
using BBfieldValueRetriever.Control;
using BBfieldValueRetriever.Model;
using BBFieldValueRetrieverTests.Properties;
using Maple;
using NUnit.Framework;
using Shared;

namespace BBFieldValueRetrieverTests
{
    [TestFixture]
    public class LongRunningTests
    {
        [Test]
        public void GetDataWarehouseRequestItemsFromLiveAndProcessThemInDev()
        {
            //retrieve bdis
            var sqlFile = @"sqls\GetBloombergDataRequestItems.sql";
            var controller = new BergController();
            var sql = File.ReadAllText(sqlFile);
            var requestItems = new Database().GetTickerItemsToProcess(sql);

            //split into separate lists - one for datawarehouse, one for legacy api, one for bloomberg ad hoc
            var requestItemsForBloombergApi = requestItems.FindAll(x => controller.IsForLegacyBloombergApi(x));
            var requestItemsForBloombergAdHoc = requestItems.FindAll(x => controller.IsForBloombergAdHoc(x));
            var requestItemsForDatawarehouse = requestItems.FindAll(x => !controller.IsForLegacyBloombergApi(x) && !controller.IsForBloombergAdHoc(x));

            Assert.AreEqual(requestItems.Count, requestItemsForDatawarehouse.Count + requestItemsForBloombergApi.Count + requestItemsForBloombergAdHoc.Count);

            //get errors

            new TestBloombergDatawarehouse().ProcessDataRequests(requestItemsForDatawarehouse);
        }

        [Test]
        public void RequestItemFromAdHocPull()
        {
            BloombergAdHocController controller = new BloombergAdHocController(new BergController());

            //req.DataReady += this.BloombergAdHocRequestDataReadyHandler;
            controller.SubmitDataRequestsAsync(new List<RequestItem>
            {
                new RequestItem
                {
                    BBTicker = "B3WJHK4 SEDOL1",OriginalInputTicker = "B3WJHK4 SEDOL1",
                    BBFieldList =
                        "NAME,SECURITY_NAME,SECURITY_DES,CRNCY,EXCH_CODE,TICKER,TICKER_AND_EXCH_CODE,ISSUER,ID_ISIN,ID_SEDOL1,ID_SEDOL2,REDEMP_VAL,ID_CUSIP,OPT_CONT_SIZE_REAL,PAR_AMT,OPT_STRIKE_PX,MATURITY,OPT_PUT_CALL,OPT_EXER_TYP,OPT_UNDL_TICKER,CALLABLE,PUTABLE,START_ACC_DT,FUT_NOTL_MTY,CPN,CPN_TYP,DAY_CNT_DES,CPN_FREQ,CV_CNVS_RATIO,ADR_SH_PER_ADR,EQY_PRIM_EXCH,EQY_PRIM_EXCH_SHRT,FLOATER,FUT_CONT_SIZE,FUT_LAST_TRADE_DT,FUTURES_CATEGORY,PX_QUOTE_LOT_SIZE,WRT_EXPIRE_DT,VOLATILITY_30D"
                }
            });

            Assert.AreEqual(1, controller.GlobalListOfOutstandingAdHocRequests.Count);

            controller.StartTimer();
            //this is bad but its for a test only.
            while (true)
            {
                if (controller.GlobalListOfOutstandingAdHocRequests.Count == 0) break;
                Thread.Sleep(3000);
            }
            Console.WriteLine("Completed.");
        }

        [Test]
        public void EnsureCostReportingIsOkFromAdHocPull()
        {
            var controller = new BloombergAdHocRequestTest(new BergController());
            controller.SubmitDataRequestsAsync(new List<RequestItem> {new RequestItem
            {
                BBTicker = "B3WJHK4 SEDOL1",
                BBFieldList = "CRNCY,CV_CNVS_RATIO,EQY_PRIM_EXCH,EQY_PRIM_EXCH_SHRT,EXCH_CODE,ID_CUSIP,ID_ISIN,ID_SEDOL1,ID_SEDOL2,ISSUER,NAME,REDEMP_VAL,TICKER_AND_EXCH_CODE,ADR_SH_PER_ADR"
            }});

            controller.StartTimer();

            Thread.Sleep(20000);
        }

        [Test]
        public void EnsureThatGetObjectsWorksWithEnumTypes()
        {
            var request = Utils.DbController.GetFirstObject<RequestItem>("select top 1 * from bloombergdatarequestitem where requesttype = 'reference'");
            Assert.AreEqual(BloombergDataInstrument.eRequestType.Reference, request.RequestType);

            request = Utils.DbController.GetFirstObject<RequestItem>("select top 1 * from bloombergdatarequestitem where requesttype = 'intradaybar'");
            Assert.AreEqual(BloombergDataInstrument.eRequestType.IntraDayBar, request.RequestType);

            request = Utils.DbController.GetFirstObject<RequestItem>("select top 1 * from bloombergdatarequestitem where requesttype = 'historic'");
            Assert.AreEqual(BloombergDataInstrument.eRequestType.Historic, request.RequestType);

            //test for case insensitivity
            request = Utils.DbController.GetFirstObject<RequestItem>("select top 1 'histoRic' as requesttype from bloombergdatarequestitem where requesttype = 'historic'");
            Assert.AreEqual(BloombergDataInstrument.eRequestType.Historic, request.RequestType);

            request = Utils.DbController.GetFirstObject<RequestItem>("select top 1 'hiSToRic' as requesttype from bloombergdatarequestitem where requesttype = 'historic'");
            Assert.AreEqual(BloombergDataInstrument.eRequestType.Historic, request.RequestType);
        }
    }

    [TestFixture]
    public class QueueTests
    {
        /// <summary>
        /// Puts a request for bloomberg data on the queue (table) and retrieves it
        /// </summary>
        [Test]
        public void QueueAndDequeueRequestItem()
        {
            decimal ret = QueueRequestItem();

            //retrieve it
            var requestItems = new Database().GetTickerItemsToProcess("EXEC GetBloombergDataRequestItems");

            //check that we got it back
            Assert.AreEqual(requestItems.FindAll(x => x.ID == ret).Count, 1);
            Assert.AreEqual(requestItems.First(x => x.ID == ret).BBFieldList, "PX_LAST,NAME,PX_BID");
            Assert.AreEqual(requestItems.First(x => x.ID == ret).BBTicker, "VOD LN Equity");
        }

        private static decimal QueueRequestItem()
        {
            //queue a request
            var db = new SQLServer(Settings.Default.DSN);
            var sql = new StringBuilder();
            sql.Append("delete BloombergDataRequestItem where userid = 'AutomatedUnitTest';");
            sql.Append("INSERT BloombergDataRequestItem(RequestType,BBTicker,BBFieldList,UserID,TargetServer) ");
            sql.Append(string.Format("SELECT 'Reference','VOD LN Equity','PX_LAST,NAME,PX_BID','AutomatedUnitTest','{0}'; select @@identity;", Environment.MachineName));

            return Utils.DbController.GetScalar<decimal>(sql.ToString());
        }

        [Test]
        public void ComposeBloombergRequest()
        {
            decimal ret = QueueRequestItem();
            //retrieve it
            var requestItems = new Database().GetTickerItemsToProcess("EXEC GetBloombergDataRequestItems");
            var bb = new BloombergApiController(new BergController()).ComposeBBobjects(requestItems);
            Assert.AreEqual(3, bb[0].BBFields.Count);
            Assert.AreEqual(3, new BloombergApiController(new BergController()).CalcHits(bb));
        }

        [Test]
        public void MakeSureErrorListsWorkAsExpected()
        {
            var mgr = new TestBloombergDatawarehouse();
            Assert.IsTrue(mgr.NaToSecurityErrors.Contains("2021151 UV SEDOL1|PX_BID"));
            Assert.IsTrue(mgr.NaToSecurityErrors.Contains("2021151 UV SEDOL1|PX_ASK"));
            Assert.IsTrue(mgr.NaToSecurityErrors.Contains("2021151 UV SEDOL1|PX_YEST_CLOSE"));
        }

        [Test]
        public void ProcessRequestForEquityIndexWeights()
        {
            //retrieve bdis
            var sql = @"select top 1 * from BloombergDatarequestitem where bbfieldlist like '%mwei%'  and bbticker = 'ibex index' and insertedwhen > getdate() - 5";

            var requestItems = new Database().GetTickerItemsToProcess(sql);
            var controller = new BergController();
            //split into separate lists - one for datawarehouse, one for legacy api, one for bloomberg ad hoc
            var requestItemsForBloombergApi = requestItems.FindAll(x => controller.IsForLegacyBloombergApi(x));
            var requestItemsForBloombergAdHoc = requestItems.FindAll(x => controller.IsForBloombergAdHoc(x));
            var requestItemsForDatawarehouse = requestItems.FindAll(x => !controller.IsForLegacyBloombergApi(x) && !controller.IsForBloombergAdHoc(x));

            Assert.AreEqual(requestItems.Count, requestItemsForDatawarehouse.Count + requestItemsForBloombergApi.Count + requestItemsForBloombergAdHoc.Count);
            Assert.AreEqual(1, requestItemsForDatawarehouse.Count);

            //get errors
            new TestBloombergDatawarehouse().ProcessDataRequests(requestItemsForDatawarehouse);
        }

        [Test]
        public void RetrieveFromDatawarehouse()
        {
            var t = new BloombergDatawarehouseController(new BergController()).GetDataFromDatawarehouse(new RequestItem { OriginalInputTicker = "0963590 VX SEDOL1" });
            t = new BloombergDatawarehouseController(new BergController()).GetDataFromDatawarehouse(new RequestItem { OriginalInputTicker = " SZU GR Equity" });

            t = new BloombergDatawarehouseController(new BergController()).GetDataFromDatawarehouse(new RequestItem { OriginalInputTicker = " ARL GR  Equity" });
            Assert.AreEqual(true, Convert.ToBoolean(t.FOUND_CREDITRISK));
            Assert.AreEqual(true, Convert.ToBoolean(t.FOUND_DESCRIPTION));
            Assert.AreEqual(true, Convert.ToBoolean(t.FOUND_NNAP));

            Assert.AreEqual(false, Convert.ToBoolean(t.FOUND_FIXEDINCOME));
            Assert.AreEqual(false, Convert.ToBoolean(t.FOUND_NAP));

            //    Assert.AreEqual(true, t.TickerFoundInPerSecurityPull);
            Assert.AreEqual(true, t.TickerFoundInBackOfficeFiles);

            t = new BloombergDatawarehouseController(new BergController()).GetDataFromDatawarehouse(new RequestItem { OriginalInputTicker = "n/a Equity" });
            Assert.AreEqual(false, t.TickerFoundInPerSecurityPull);
            Assert.AreEqual(false, t.TickerFoundInBackOfficeFiles);
        }

        [Test]
        public void TickerIsNotFoundInBloomberg()
        {
            var naToSecurityErrors = new List<string>();
            var naToProductErrors = new List<string>();
            var tickersNotFound = new List<string>();
            GetErrorRequestsFromLive(naToSecurityErrors, naToProductErrors, tickersNotFound);
            Assert.IsTrue(tickersNotFound.Contains("CA57632XAW35 CORP"));
        }

        [Test]
        public void RequestItemRoutingTests()
        {
            var controller = new BergController();

            var requestItem = new RequestItem
            {
                BBFieldList = "PX_LAST",
                RequestType = BloombergDataInstrument.eRequestType.IntraDayBar,
                UserId = "IntradayPrices"
            };
            Assert.IsTrue(controller.IsForLegacyBloombergApi(requestItem));

            requestItem = new RequestItem
            {
                BBFieldList = "PX_LAST",
                RequestType = BloombergDataInstrument.eRequestType.IntraDayTick,
                UserId = "IntradayFx"
            };
            Assert.IsTrue(controller.IsForLegacyBloombergApi(requestItem));

            requestItem = new RequestItem
            {
                BBFieldList = "PX_LAST",
                RequestType = BloombergDataInstrument.eRequestType.Reference,
                UserId = ""
            };
            Assert.IsFalse(controller.IsForLegacyBloombergApi(requestItem));

            requestItem = new RequestItem
            {
                BBFieldList = "PX_LAST,OPT_DELTA_SOMETHING",
                RequestType = BloombergDataInstrument.eRequestType.Reference,
                UserId = ""
            };
            Assert.IsFalse(controller.IsForLegacyBloombergApi(requestItem));

            requestItem = new RequestItem
            {
                BBFieldList = "SW_VAL_PREMIUM....",
                RequestType = BloombergDataInstrument.eRequestType.Reference,
                UserId = ""
            };
            Assert.IsFalse(controller.IsForLegacyBloombergApi(requestItem));

            requestItem = new RequestItem
            {
                BBFieldList = "nSW_VAL_PREMIUM....",
                RequestType = BloombergDataInstrument.eRequestType.Reference,
                UserId = ""
            };
            Assert.IsFalse(controller.IsForLegacyBloombergApi(requestItem));

            requestItem = new RequestItem
            {
                BBFieldList = "RTG_....",
                RequestType = BloombergDataInstrument.eRequestType.Reference,
                UserId = ""
            };
            Assert.IsFalse(controller.IsForLegacyBloombergApi(requestItem));

            requestItem = new RequestItem
            {
                BBFieldList = "RTG....",
                RequestType = BloombergDataInstrument.eRequestType.Reference,
                UserId = ""
            };
            Assert.IsFalse(controller.IsForLegacyBloombergApi(requestItem));

            requestItem = new RequestItem
            {
                BBFieldList = "kjhjkh jkhCALENDAR_NON_SETTLEMENT_DATEskjhjkh",
                RequestType = BloombergDataInstrument.eRequestType.Reference,
                UserId = ""
            };
            Assert.IsFalse(controller.IsForLegacyBloombergApi(requestItem));

            requestItem = new RequestItem
            {
                BBFieldList = "kjhjkh jkhCALENDAR_NON_SETTLEMENT_DATESkjhjkh",
                RequestType = BloombergDataInstrument.eRequestType.Reference,
                UserId = ""
            };
            Assert.IsFalse(controller.IsForLegacyBloombergApi(requestItem));

            //adhoc test
            requestItem = new RequestItem
            {
                BBFieldList = "kjhjkh jkhCALENDAR_NON_SETTLEMENT_DATESkjhjkh",
                RequestType = BloombergDataInstrument.eRequestType.Reference,
                UserId = "StaticDataImport"
            };
            Assert.IsTrue(controller.IsForBloombergAdHoc(requestItem));
        }

        public string GetBloombergProduct(string ticker)
        {
            if (ticker.ToUpper().Trim().EndsWith("SEDOL1")) return "EQUITY"; //this is almost entirely true
            if (ticker.ToUpper().Trim().EndsWith("EQUITY")) return "EQUITY";
            if (ticker.ToUpper().Trim().EndsWith("INDEX")) return "INDEX";
            if (ticker.ToUpper().Trim().EndsWith("COMDTY")) return "COMDTY";
            if (ticker.ToUpper().Trim().EndsWith("CURNCY")) return "CURNCY";
            if (ticker.ToUpper().Trim().EndsWith("CORP")) return "CORP";
            if (ticker.ToUpper().Trim().EndsWith("GOVT")) return "GOVT";
            return "UNKNOWN";
        }

        /// <summary>
        /// this goes to prod, but it is retrieving records only!
        /// </summary>
        /// <param name="naToSecurityErrors"></param>
        /// <param name="naToProductErrors"></param>
        /// <param name="tickersNotFound"></param>

        public void GetErrorRequestsFromLive(List<string> naToSecurityErrors, List<string> naToProductErrors, List<string> tickersNotFound)
        {
            //var NAToSecurityErrors = new List<string>();
            //var NAToProductErrors = new List<string>();

            StringBuilder var1 = new StringBuilder();
            var1.Append("select distinct bbticker , error from BloombergDataRequestItem where error is not null");

            using (var sqlConn = new SqlConnection("Server=minky;database=bloomberg;Trusted_Connection=yes;"))
            {
                using (var sqlCommand = new SqlCommand(var1.ToString(), sqlConn))
                {
                    sqlConn.Open();
                    using (var reader = sqlCommand.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            foreach (var item in reader[1].ToString().Split(new[] { "[", "]" }, StringSplitOptions.RemoveEmptyEntries))
                            {
                                var ticker = reader[0];
                                var fieldName = item.Split('|')[0].Trim().ToUpper();
                                var error = item.Contains("|") ? item.Split('|')[1] : "";
                                if (error.Contains("not applicable to product"))
                                {
                                    if (GetBloombergProduct(ticker.ToString()) != "UNKNOWN")
                                    {
                                        if (!naToProductErrors.Contains(GetBloombergProduct(ticker.ToString()) + "|" + fieldName))
                                            naToProductErrors.Add(GetBloombergProduct(ticker.ToString()) + "|" + fieldName);
                                    }
                                }
                                if (item.Contains("not applicable to security") || item.Contains("N.A."))
                                    naToSecurityErrors.Add(ticker + "|" + fieldName);
                            }
                        }
                    }
                }
            }

            //
            //var tickersNotFound = new List<string>();
            var1.Clear();
            var1.Append("select distinct bbticker from BloombergDataRequestItem where error like '%Unknown/Invalid security%' ");
            using (var sqlConn = new SqlConnection("Server=minky;database=bloomberg;Trusted_Connection=yes;"))
            {
                using (var sqlCommand = new SqlCommand(var1.ToString(), sqlConn))
                {
                    sqlConn.Open();
                    using (var reader = sqlCommand.ExecuteReader())
                    {
                        while (reader.Read())
                            tickersNotFound.Add(reader[0].ToString());
                    }
                }
            }
        }

        [Test]
        public void AutoresetTest()
        {
            new BloombergApiControllerStub().GetBbgData(null);
        }

        [Test]
        public void PeekLocalQueueForStaleMessages()
        {
            //clear recent messages on queue from this machine

            //peek stale messages
            var controller = new BergController();
            controller.OnResurrect();
        }
    }

    [TestFixture]
    public class BergContinuesProcessingStaleMessagesOnResurrection
    {
        [TestFixtureSetUp]
        public void Setup()
        {
            //local test copy of remote q
            StringBuilder varname1 = new StringBuilder();
            varname1.Append("IF ( EXISTS (SELECT * \n");
            varname1.Append("             FROM   INFORMATION_SCHEMA.TABLES \n");
            varname1.Append("             WHERE  TABLE_NAME = 'BloombergAdHocTest') ) \n");
            varname1.Append("  BEGIN \n");
            varname1.Append("      DROP TABLE [BloombergAdHocTest] \n");
            varname1.Append("  END");
            Utils.DbController.ExecuteNonQuery(varname1.ToString());

            StringBuilder varname11 = new StringBuilder();
            varname11.Append("CREATE TABLE [dbo].[BloombergAdHocTest] \n");
            varname11.Append("  ( \n");
            varname11.Append("     [BloombergAdHocId]   [INT] NOT NULL, \n");
            varname11.Append("     [FieldsToPull]       [VARCHAR](8000) NULL, \n");
            varname11.Append("     [SecurityIdentifier] [VARCHAR](50) NULL, \n");
            varname11.Append("     [ReturnData]         [VARCHAR](8000) NULL, \n");
            varname11.Append("     [LastUpdateDatetime] [DATETIME] NULL, \n");
            varname11.Append("     [LastUpdateUser]     [VARCHAR](50) NULL, \n");
            varname11.Append("     [Status]             [TINYINT] NULL, \n");
            varname11.Append("     [BloombergRequestId] [VARCHAR](50) NULL \n");
            varname11.Append("  ) \n");
            varname11.Append("ON [PRIMARY]");
            Utils.DbController.ExecuteNonQuery(varname11.ToString());

            // remote messages - completed, in progress
            StringBuilder varname12 = new StringBuilder();
            varname12.Append("INSERT INTO [dbo].[BloombergAdHocTest] \n");
            varname12.Append("           ([BloombergAdHocId] \n");
            varname12.Append("           ,[FieldsToPull] \n");
            varname12.Append("           ,[SecurityIdentifier] \n");
            varname12.Append("           ,[ReturnData] \n");
            varname12.Append("           ,[LastUpdateDatetime] \n");
            varname12.Append("           ,[LastUpdateUser] \n");
            varname12.Append("           ,[Status] \n");
            varname12.Append("           ,[BloombergRequestId]) \n");
            varname12.Append("     VALUES \n");
            varname12.Append("           (263239 \n");
            varname12.Append("           ,'PX_ASK,PX_BID,LAST_TRADEABLE_DT,DL_ASSET_CLASS' \n");
            varname12.Append("           ,'EDM7 COMB Comdty' \n");
            varname12.Append("           ,'99.830000|99.825000|03/13/2017|18|' \n");
            varname12.Append("           ,dateadd(MINUTE,-15,dateadd(hour,-5,getdate())) \n");
            varname12.Append("           ,'' \n");
            varname12.Append("           ,2 \n");
            varname12.Append("           ,'6d66eca6-c37f-4332-bd9a-e46918e4bcb1')");

            varname12.Append("INSERT INTO [dbo].[BloombergAdHocTest] \n");
            varname12.Append("           ([BloombergAdHocId] \n");
            varname12.Append("           ,[FieldsToPull] \n");
            varname12.Append("           ,[SecurityIdentifier] \n");
            varname12.Append("           ,[ReturnData] \n");
            varname12.Append("           ,[LastUpdateDatetime] \n");
            varname12.Append("           ,[LastUpdateUser] \n");
            varname12.Append("           ,[Status] \n");
            varname12.Append("           ,[BloombergRequestId]) \n");
            varname12.Append("     VALUES \n");
            varname12.Append("           (263239 \n");
            varname12.Append("           ,'PX_ASK,PX_BID,LAST_TRADEABLE_DT,DL_ASSET_CLASS' \n");
            varname12.Append("           ,'EDM8 COMB Comdty' \n");
            varname12.Append("           ,'99.830000|99.825000|03/13/2017|18|' \n");
            varname12.Append("           ,dateadd(MINUTE,-15,dateadd(hour,-5,getdate())) \n");
            varname12.Append("           ,'' \n");
            varname12.Append("           ,1 \n");
            varname12.Append("           ,'6d66eca6-c37f-4332-bd9a-e46918e4bcb1')");

            Utils.DbController.ExecuteNonQuery(varname12.ToString());

            //local stale messages 3 messages - they will be foudn to be done , in progress and lost
            Utils.DbController.ExecuteNonQuery("delete BloombergDataRequestItem where processedby=host_name() and insertedwhen > getdate()-5");

            StringBuilder sb = new StringBuilder();
            sb.Append("INSERT INTO [dbo].[BloombergDataRequestItem] \n");
            sb.Append("           ([RequestType] \n");
            sb.Append("           ,[BBTicker] \n");
            sb.Append("           ,[BBFieldList] \n");
            sb.Append("           ,[DateFrom] \n");
            sb.Append("           ,[DateTo] \n");
            sb.Append("           ,[Periodicity] \n");
            sb.Append("           ,[InsertedWhen] \n");
            sb.Append("           ,[ProcessedWhen] \n");
            sb.Append("           ,[Processed] \n");
            sb.Append("           ,[Error] \n");
            sb.Append("           ,[UserID] \n");
            sb.Append("           ,[Reference] \n");
            sb.Append("           ,[ProcessedBy] \n");
            sb.Append("           ,[RequestSetID] \n");
            sb.Append("           ,[TargetServer]) \n");
            sb.Append("     VALUES \n");
            sb.Append("           ('Reference' \n");
            sb.Append("           ,'EDM7 COMB Comdty' \n");
            sb.Append("           ,'PX_ASK,PX_BID,LAST_TRADEABLE_DT' \n");
            sb.Append("           ,null \n");
            sb.Append("           ,null \n");
            sb.Append("           ,null \n");
            sb.Append("           ,dateadd(minute,-30,getdate()) \n");
            sb.Append("           ,null \n");
            sb.Append("           ,1 \n");
            sb.Append("           ,null \n");
            sb.Append("           ,'ZeroCouponGenerator - done' \n");
            sb.Append("           ,1234 \n");
            sb.Append("           ,HOST_NAME() \n");
            sb.Append("           ,null \n");
            sb.Append("           ,null)");

            sb.Append("INSERT INTO [dbo].[BloombergDataRequestItem] \n");
            sb.Append("           ([RequestType] \n");
            sb.Append("           ,[BBTicker] \n");
            sb.Append("           ,[BBFieldList] \n");
            sb.Append("           ,[DateFrom] \n");
            sb.Append("           ,[DateTo] \n");
            sb.Append("           ,[Periodicity] \n");
            sb.Append("           ,[InsertedWhen] \n");
            sb.Append("           ,[ProcessedWhen] \n");
            sb.Append("           ,[Processed] \n");
            sb.Append("           ,[Error] \n");
            sb.Append("           ,[UserID] \n");
            sb.Append("           ,[Reference] \n");
            sb.Append("           ,[ProcessedBy] \n");
            sb.Append("           ,[RequestSetID] \n");
            sb.Append("           ,[TargetServer]) \n");
            sb.Append("     VALUES \n");
            sb.Append("           ('Reference' \n");
            sb.Append("           ,'EDM8 COMB Comdty' \n");
            sb.Append("           ,'PX_ASK,PX_BID,LAST_TRADEABLE_DT' \n");
            sb.Append("           ,null \n");
            sb.Append("           ,null \n");
            sb.Append("           ,null \n");
            sb.Append("           ,dateadd(minute,-30,getdate()) \n");
            sb.Append("           ,null \n");
            sb.Append("           ,1 \n");
            sb.Append("           ,null \n");
            sb.Append("           ,'ZeroCouponGenerator - in progress' \n");
            sb.Append("           ,1234 \n");
            sb.Append("           ,HOST_NAME() \n");
            sb.Append("           ,null \n");
            sb.Append("           ,null)");

            sb.Append("INSERT INTO [dbo].[BloombergDataRequestItem] \n");
            sb.Append("           ([RequestType] \n");
            sb.Append("           ,[BBTicker] \n");
            sb.Append("           ,[BBFieldList] \n");
            sb.Append("           ,[DateFrom] \n");
            sb.Append("           ,[DateTo] \n");
            sb.Append("           ,[Periodicity] \n");
            sb.Append("           ,[InsertedWhen] \n");
            sb.Append("           ,[ProcessedWhen] \n");
            sb.Append("           ,[Processed] \n");
            sb.Append("           ,[Error] \n");
            sb.Append("           ,[UserID] \n");
            sb.Append("           ,[Reference] \n");
            sb.Append("           ,[ProcessedBy] \n");
            sb.Append("           ,[RequestSetID] \n");
            sb.Append("           ,[TargetServer]) \n");
            sb.Append("     VALUES \n");
            sb.Append("           ('Reference' \n");
            sb.Append("           ,'EDM9 COMB Comdty' \n");
            sb.Append("           ,'PX_ASK,PX_BID,LAST_TRADEABLE_DT' \n");
            sb.Append("           ,null \n");
            sb.Append("           ,null \n");
            sb.Append("           ,null \n");
            sb.Append("           ,dateadd(minute,-30,getdate()) \n");
            sb.Append("           ,null \n");
            sb.Append("           ,1 \n");
            sb.Append("           ,null \n");
            sb.Append("           ,'ZeroCouponGenerator - lost' \n");
            sb.Append("           ,1234 \n");
            sb.Append("           ,HOST_NAME() \n");
            sb.Append("           ,null \n");
            sb.Append("           ,null)");

            Utils.DbController.ExecuteNonQuery(sb.ToString());

            //peek stale messages
            var controller = new BergController();
            var localStales = controller.PeekStaleMessagesFromLocalQueue();
            Assert.AreEqual(3, localStales.Count());
        }
    }
}