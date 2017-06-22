using System;
using BBDataWarehouseCacheManager.Controllers;
using NUnit.Framework;

namespace BBDataWarehouseCacheManagerTests
{
    [TestFixture]
    public class DataIntegrityChecks
    {
        [Test, Ignore("Takes too long")]
        public void CheckDataByConsumer()
        {
            var mgr = new DataIntegrityController();
            Console.WriteLine(mgr.CheckDataIntegrityByConsumer("OptionValueManager", DateTime.Parse("13apr15"), 0.04));

            Console.WriteLine(mgr.CheckDataIntegrityByConsumer("InsertUpdateSecurityPricesFromBERG_EOD", DateTime.Parse("30mar15"), 0.1));
            //Console.WriteLine(mgr.CheckDataIntegrityByConsumer("RefRates", DateTime.Parse("30mar15")));

            //Console.WriteLine(mgr.CheckDataIntegrityByConsumer("IndexPrice", DateTime.Parse("30mar15")));
            //Console.WriteLine(mgr.CheckDataIntegrityByConsumer("UpdateStockAndIndices", DateTime.Parse("30mar15")));
            //Console.WriteLine(mgr.CheckDataIntegrityByConsumer("UpdateStockAndIndices2", DateTime.Parse("30mar15")));

        }

        [Test, Ignore("Takes too long")]
        public void CheckDataHistoricalInsertUpdateSecurityPricesFromBERG_EOD()
        {
            var mgr = new DataIntegrityController();

            for (DateTime thisDate = DateTime.Parse("1mar15"); thisDate < DateTime.Parse("1apr15"); thisDate = thisDate.AddDays(1))
            {
                Console.WriteLine(mgr.CheckDataIntegrityByConsumer("InsertUpdateSecurityPricesFromBERG_EOD", thisDate, 0.1));
            }


        }

        [Test, Ignore("Takes too long")]
        public void CheckLocalCachedTorontoViewIntegrity()
        {

            new DataIntegrityController().CheckDataIntegrityForLocalCacheOfTorontoViews(DateTime.Parse("29May2015"), 0.15);
        }


    }
}
