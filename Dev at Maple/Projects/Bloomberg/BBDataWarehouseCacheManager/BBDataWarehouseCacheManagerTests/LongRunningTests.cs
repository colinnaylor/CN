using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using BBDataWarehouseCacheManager.Controllers;
using BBDataWarehouseCacheManager.Models;
using NUnit.Framework;

namespace BBDataWarehouseCacheManagerTests
{
    [TestFixture]
    public class LongRunningTests
    {

        [Test]
        public void CategoriseBergTickersIntoTorontoAssetTypes()
        {

            new DatawarehouseManager().CategoriseBergTickersIntoTorontoAssetTypes(new DateTime(2014, 10, 29));




        }

        class RequestItemMini
        {
            public string bbticker { get; set; }
            public string FieldList { get; set; }
            public string RequestType { get; set; }
            public string UserId { get; set; }
            public bool IsForLegacyBloombergAPI()
            {

                if (RequestType.StartsWith("Intra"))
                {

                    return true;
                }

                if ((FieldList.ToUpper().Contains("OPT_DELTA")) ||
                    (FieldList.ToUpper().Contains("BETA_ADJ_OVER")) ||
                    (FieldList.ToUpper().Contains("CALENDAR_NON_SETTLEMENT_DATES")) ||
                    (FieldList.ToUpper().StartsWith("SW_")) ||
                    (FieldList.ToUpper().StartsWith("RTG_")) ||
                    (FieldList.ToUpper().StartsWith("BB_COMP")))
                {

                    return true;
                }

                return false;
            }

            public bool IsForBloombergAdHoc()
            {

                if (UserId.Equals("StaticDataImport", StringComparison.OrdinalIgnoreCase))
                {

                    return true;
                }

                if (UserId.Equals("InsertUpdateCurrencyRatesFromBERG", StringComparison.OrdinalIgnoreCase))
                {

                    return true;
                }

                if (UserId.Equals("InsertUpdateSecurityPricesFromBERG", StringComparison.OrdinalIgnoreCase)
                    && FieldList.ToUpper().Contains("PX_LAST"))
                {

                    return true;
                }


                return false;
            }


        }
        [Test, Ignore("Takes too long to run")]
        public void GetDataWarehouseRequestItemsForEquitiesOnlyFromLiveAndProcessThemInDevAndCostThem()
        {

            var myList = new List<RequestItemMini>();
            //override and go to prod.
            var prodDb = @"Data Source=minky;Initial Catalog=bloomberg;Integrated Security=SSPI";
            using (SqlConnection connection = new SqlConnection(prodDb))
            {
                connection.Open();
                //                using (SqlCommand command = new SqlCommand("select distinct bbticker,bbfieldlist,requesttype,UserId from BloombergDataRequestItem where insertedwhen between '1sep14' and '30sep14'", connection))
                using (SqlCommand command = new SqlCommand("select distinct bbticker,bbfieldlist,requesttype,UserId from BloombergDataRequestItem", connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            myList.Add(new RequestItemMini()
                            {
                                bbticker = reader[0].ToString(),
                                FieldList = reader[1].ToString(),
                                RequestType = reader[2].ToString(),
                                UserId = reader[3].ToString()
                            });


                        }
                    }
                }
            }

            foreach (var item in myList)
            {

                //if (new BergTicker(item.bbticker).TorPullType == "E")
                //{
                string source;
                if (!item.IsForLegacyBloombergAPI())
                //if (!item.IsForLegacyBloombergAPI() && !item.IsForBloombergAdHoc())
                {
                    var t = new BergTicker(item.bbticker);
                    foreach (var field in item.FieldList.Split(','))
                    {
                        source = item.IsForBloombergAdHoc() ? "adhoc," : "warehouse,";

                        Console.WriteLine(source + item.bbticker + "," + field + "," + t.TorPullType + "," + item.UserId);
                    }
                }
                //}
            }

        }


    }

}
