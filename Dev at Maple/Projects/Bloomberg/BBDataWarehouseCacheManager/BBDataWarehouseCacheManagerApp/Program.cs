using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using BBDataWarehouseCacheManager;
using BBDataWarehouseCacheManager.Controllers;

namespace BBDataWarehouseCacheManagerApp
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Utils.Logger.Info("Starting process");
            Utils.Logger.Info("Database connection string: {0} ", ConfigurationManager.ConnectionStrings["BloombergConnectionString"].ToString());


            try
            {
                var mgr = new DatawarehouseManager();
                Utils.SetLoggerClassName(args[0]);

                if (args[0].ToUpper() == "PM")
                {
                    var T = DateTime.Now.Date;

                    if (args.Length == 2)
                    {
                        T = DateTime.Parse(args[1]);
                    }

                    Utils.Logger.Info("Running EOD for: {0}", T.ToString(CultureInfo.InvariantCulture));

                    //fill with tickers from T positions
                    Utils.Logger.Info(string.Format("{0}: GetLiveTickersUsingBergConsumerSqls", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));
                    var tickers = mgr.GetLiveTickersUsingBergConsumerSqls(T);

                    Utils.Logger.Info(string.Format("{0}: InsertTickersIntoDatawarehouse", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));
                    mgr.InsertTickersIntoDatawarehouse(tickers, T);

                    //recreate datawarehouse - categorise tor long types for all tickers in datawarehouse
                    Utils.Logger.Info(string.Format("{0}: CategoriseBergTickersIntoTorontoAssetTypes", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));
                    mgr.CategoriseBergTickersIntoTorontoAssetTypes(T);

                    //upload tickers to toronto
                    Utils.Logger.Info(string.Format("{0}: UploadTickersToToronto", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));
                    Utils.DbController.ExecuteNonQuery(File.ReadAllText("Sql\\UploadTickersToToronto.sql").Replace("{Position_Date}", T.ToString("ddMMMyyyy")));
                }
                else if (args[0].ToUpper() == "AM")
                {
                    var tminus1 = new DateUtils().PreviousWorkDay(DateTime.Now.Date);

                    //download from tor for T-1
                    Utils.Logger.Info(string.Format("{0}: DownloadDataFromToronto", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));
                    Utils.DbController.ExecuteNonQuery(File.ReadAllText("Sql\\DownloadDataFromToronto.sql").Replace("{Position_Date}", tminus1.ToString("ddMMMyyyy")));

                    //enrich datawarehouse entries for T-1
                    Utils.Logger.Info(string.Format("{0}: EnrichDatawarehouse", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));
                    Utils.DbController.ExecuteNonQuery(File.ReadAllText("sql\\EnrichDatawarehouse.sql").Replace("{Position_Date}", tminus1.ToString("ddMMMyyyy")));

                    //ready for use!
                }
                else if (args[0].ToUpper() == "ENRICH")
                {
                    var tminus1 = new DateUtils().PreviousWorkDay(DateTime.Now.Date);
                    if (args.Length == 2)
                    {
                        tminus1 = DateTime.Parse(args[1]);
                    }
                    var dw = new DatawarehouseManager();
                    if (dw.HasBeenEnrichedForDate(tminus1))
                        Utils.Logger.Info("EnrichDatawarehouse - doing nothing - already enriched: {0:ddMMMyyyy}", tminus1);
                    else
                    {
                        if (dw.DataAvailableInAllPricingViews(tminus1))
                        {
                            Utils.Logger.Info("EnrichDatawarehouse - starting: {0:ddMMMyyyy}", tminus1);
                            Utils.DbController.ExecuteNonQuery(File.ReadAllText("sql\\EnrichDatawarehouse.sql").Replace("{Position_Date}", tminus1.ToString("ddMMMyyyy")));
                            Utils.Logger.Info("EnrichDatawarehouse - completed: {0:ddMMMyyyy}", tminus1);

                            Utils.Logger.Info("Calculating usage costs - writing to table CostReportActual - starting: {0:ddMMMyyyy}", tminus1);
                            mgr.RunCostReportActual(tminus1);
                            Utils.Logger.Info("Calculating usage costs - writing to table CostReportActual - completed: {0:ddMMMyyyy}", tminus1);

                            CheckData(tminus1);
                        }
                        else
                            Utils.Logger.Info("EnrichDatawarehouse - doing nothing - not all required data is ready: {0:ddMMMyyyy}", tminus1);
                    }
                }
                else if (args[0].ToUpper() == "COSTS")
                {
                    var tminus1 = new DateUtils().PreviousWorkDay(DateTime.Now.Date);
                    if (args.Length == 2)
                    {
                        tminus1 = DateTime.Parse(args[1]);
                    }
                    mgr.RunCostReportActual(tminus1);
                }
                else if (args[0].ToUpper() == "DOWNLOAD")
                {
                    var tminus1 = new DateUtils().PreviousWorkDay(DateTime.Now.Date);
                    if (args.Length == 2)
                    {
                        tminus1 = DateTime.Parse(args[1]);
                    }

                    //download from tor for T-1
                    var dw = new DatawarehouseManager(); var toronto = new TorontoViewManager();

                    foreach (var torView in toronto.ListOfTorontoViews())
                    {
                        Utils.Logger.Info("Checking for work to be done on {0} for date {1:ddMMMyyyy}", torView, tminus1);
                        if (!dw.HasDataInWarehouseTableForDate(torView.Replace("ve_", "TorView"), tminus1)
                            && toronto.DataIsReadyInViewForDate(torView, tminus1))
                        {
                            Utils.Logger.Info("DOWNLOAD starting ... {0} for date {1:ddMMMyyyy}", torView, tminus1);
                            Utils.DbController.ExecuteNonQuery(
                                File.ReadAllText(string.Format("Sql\\DownloadDataFromToronto.{0}.sql", torView.Replace("ve_", "TorView")))
                                .Replace("{Position_Date}", tminus1.ToString("ddMMMyyyy")));
                            Utils.Logger.Info("DOWNLOAD completed ... {0} for date {1:ddMMMyyyy}", torView, tminus1);
                        }
                        else
                            Utils.Logger.Info("Doing nothing - already have data or data not yet available ... {0} for date {1:ddMMMyyyy}", torView, tminus1);
                    }
                }
                else if (args[0].ToUpper() == "GETTICKERS")
                {
                    var T = DateTime.Now.Date;
                    if (args.Length == 2)
                    {
                        T = DateTime.Parse(args[1]);
                    }
                    //fill with tickers from T positions
                    Utils.Logger.Info(string.Format("{0}: GetLiveTickersUsingBergConsumerSqls", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));
                    var tickers = mgr.GetLiveTickersUsingBergConsumerSqls(T);

                    Utils.Logger.Info(string.Format("{0}: InsertTickersIntoDatawarehouse", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));
                    mgr.InsertTickersIntoDatawarehouse(tickers, T);
                }
                else if (args[0].ToUpper() == "UPLOAD")
                {
                    var T = DateTime.Now.Date;

                    //upload tickers to toronto
                    Utils.Logger.Info(string.Format("{0}: UploadTickersToToronto", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));
                    Utils.DbController.ExecuteNonQuery(File.ReadAllText("Sql\\UploadTickersToToronto.sql").Replace("{Position_Date}", T.ToString("ddMMMyyyy")));
                }
                else if (args[0].ToUpper() == "CATEGORISE")
                {
                    var T = DateTime.Now.Date;
                    if (args.Length == 2)
                    {
                        T = DateTime.Parse(args[1]);
                    }
                    //recreate datawarehouse - categorise tor long types for all tickers in datawarehouse
                    Utils.Logger.Info(string.Format("{0}: CategoriseBergTickersIntoTorontoAssetTypes", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));
                    mgr.CategoriseBergTickersIntoTorontoAssetTypes(T);
                }
                else if (args[0].ToUpper() == "CHECK")
                {
                    var tminus1 = new DateUtils().PreviousWorkDay(DateTime.Now.Date);
                    if (args.Length == 2)
                        tminus1 = DateTime.Parse(args[1]);

                    CheckData(tminus1);
                }
            }
            catch (Exception ex)
            {
                Utils.Logger.Error("Stopping process with error: {0}", ex.ToString());
            }
            finally
            {
                Utils.Logger.Info("Process end.");
            }
        }

        static private void CheckData(DateTime effectiveDate)
        {
            var checker = new DataIntegrityController();

            checker.CheckDataIntegrityForEntireWarehouse(effectiveDate, 0.15);

            var thresholds = new Dictionary<string, double>
            {
                {"InsertUpdateSecurityPricesFromBERG_EOD", 0.15},
                {"OptionValueManager", 0.05},
                {"BloombergGetFloatingRateBond", 0.1},
                {"IndexPrice", 0.1},
                {"RefRates", 0.1},
                {"GetBossDelta", 0.1},
                {"SSRADR", 0.1},
                {"SSRBond", 0.1},
                {"SSRBossSwap", 0.1},
                {"SSRCountryList", 0.1},
                {"SSRCountryListUnd", 0.8},
                {"SSRFuture", 0.1},
                {"SSRISIN", 0.1},
                {"SSRISINUnd", 0.1},
                {"SSRIndexConst", 0.1},
                {"SSRIndexConst2", 0.1},
                {"SSRSector", 0.1},
                {"SSRStock", 0.1},
                {"SSRStockUnd", 0.1}
            };

            foreach (var item in thresholds)
                checker.CheckDataIntegrityByConsumer(item.Key, effectiveDate, item.Value);

            //    checker.CheckDataIntegrityForLocalCacheOfTorontoViews(effectiveDate, 0.15);

        }
    }
}