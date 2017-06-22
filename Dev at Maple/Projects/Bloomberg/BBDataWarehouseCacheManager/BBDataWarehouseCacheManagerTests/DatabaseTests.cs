using System;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using BBDataWarehouseCacheManager;
using BBDataWarehouseCacheManager.Controllers;
using NUnit.Framework;

namespace BBDataWarehouseCacheManagerTests
{
    [TestFixture]
    public class DatabaseTests
    {
        [Test]
        public void ExecuteReaderTest()
        {
            var t = Utils.DbController.GetList<string>("select top 100 bbticker from bloombergdatarequestitem;");
        }
        [Test]
        public void TestConnectionToTorontoViaLinkedServers()
        {
            var sql = new StringBuilder();
            sql.Append(File.ReadAllText(@"sql\\TestDownloadDataFromToronto.sql"));
            sql.Replace("{Position_Date}", "28Aug2014");


            using (var sqlConnection = new SqlConnection(
            ConfigurationManager.ConnectionStrings["BloombergConnectionString"].ToString()
            ))
            {
                sqlConnection.Open();
                using (var sqlCommand = new SqlCommand(sql.ToString(), sqlConnection))
                {
                    var reader = sqlCommand.ExecuteNonQuery();
                }
            }
        }

        [Test, Ignore("Data is not static, need to rework this test")]
        public void CanTellWhenTorontoDataIsReady()
        {

            Assert.IsTrue(new TorontoViewManager().DataIsReadyInViewForDate("ve_bloombergpersecuritypull", new DateTime(2014, 12, 15)));
            Assert.IsFalse(new DatawarehouseManager().HasDataInWarehouseTableForDate("torviewbloombergpersecuritypull", new DateTime(2014, 12, 16)));
            Assert.IsFalse(new DatawarehouseManager().HasDataInWarehouseTableForDate("torviewbloombergpersecuritypull", new DateTime(2014, 12, 14)));
            Assert.IsTrue(new DatawarehouseManager().HasDataInWarehouseTableForDate("torviewbloombergpersecuritypull", new DateTime(2014, 12, 15)));

            Assert.IsTrue(new DatawarehouseManager().DataAvailableInAllPricingViews(new DateTime(2014, 12, 15)));
            Assert.IsFalse(new DatawarehouseManager().DataAvailableInAllPricingViews(new DateTime(2014, 12, 14)));
            Assert.IsFalse(new DatawarehouseManager().DataAvailableInAllPricingViews(new DateTime(2014, 12, 16)));

        }
    }
}
