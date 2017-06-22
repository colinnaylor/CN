using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using BBfieldValueRetriever.Control;
using NUnit.Framework;
using Shared;

namespace BBFieldValueRetrieverTests
{
    [TestFixture]
    public class LegacyTests
    {
        /// <summary>
        /// Ignore for now, needs to run on a machine with Bloomberg installed.
        /// </summary>
        [Test, Ignore("Legacy untested tests")]
        public void TestForReference()
        {
            List<BloombergDataInstrument> bbdis = new List<BloombergDataInstrument>();
            BloombergDataInstrument bbdi = new BloombergDataInstrument();

            bbdi.RequestType = BloombergDataInstrument.eRequestType.Reference;
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

            bbd.InstrumentCompleteChanged += bbd_InstrumentCompleteChanged;
            bbd.PercentCompleteChanged += bbd_PercentCompleteChanged;
            bbd.ProcessCompleted += bbd_ProcessCompleted;
            bbd.StatusChanged += bbd_StatusChanged;
            bbd.GetBloombergData(bbdis);
        }

        /// <summary>
        /// Ignore for now, needs to run on a machine with Bloomberg installed.
        /// </summary>
        [Test, Ignore("Legacy untested tests")]
        public void TestForHistory()
        {
            List<BloombergDataInstrument> bbdis = new List<BloombergDataInstrument>();
            BloombergDataInstrument bbdi = new BloombergDataInstrument();

            bbdi.RequestType = BloombergDataInstrument.eRequestType.Historic;
            bbdi.ID = 0;
            bbdi.Ticker = "VOD LN Equity";
            bbdi.SecurityType = "Equity";
            bbdi.Type = BloombergDataInstrumentType.Security;

            bbdi.BBFields = new Dictionary<string, BloombergDataInstrumentField>();
            bbdi.BBFields.Add("PX_LAST", new BloombergDataInstrumentField("PX_LAST"));

            bbdi.DateFrom = DateTime.Today.AddDays(-12);
            bbdi.DateTo = DateTime.Today.AddDays(-5);

            bbdis.Add(bbdi);

            BloombergData bbd = new BloombergData(new StringCollection(), "colin@mpuk.com", "testing Bloomberg v3 api", ";");
            bbd.InstrumentCompleteChanged += bbd_InstrumentCompleteChanged;
            bbd.PercentCompleteChanged += bbd_PercentCompleteChanged;
            bbd.ProcessCompleted += bbd_ProcessCompleted;
            bbd.StatusChanged += bbd_StatusChanged;
            bbd.GetBloombergData(bbdis);
        }

        [Test, Ignore("Legacy untested tests")]
        public void TestForReference2()
        {
            List<BloombergDataInstrument> bbdis = new List<BloombergDataInstrument>();
            BloombergDataInstrument bbdi = new BloombergDataInstrument();

            bbdi.RequestType = BloombergDataInstrument.eRequestType.Reference;
            bbdi.ID = 0;
            //            bbdi.Ticker = "IT0004840788 Govt";
            bbdi.Ticker = "VOD LN Equity";
            //bbdi.Ticker = "BBG002626686 BUID";
            //bbdi.SecurityType = "Equity";
            bbdi.Type = BloombergDataInstrumentType.Security;
            bbdi.BBFields = new Dictionary<string, BloombergDataInstrumentField>();
            bbdi.BBFields.Add("PX_LAST", new BloombergDataInstrumentField("PX_LAST"));
            bbdi.BBFields.Add("NAME", new BloombergDataInstrumentField("NAME"));
            bbdis.Add(bbdi);

            new BloombergApiController(new BergController()).GetBbgData(bbdis);

            foreach (var item in bbdis)
            {
                foreach (var field in item.BBFields.Values)
                {
                    Console.WriteLine(field.Name + ": " + field.Value);
                }
            }
        }

        private static void bbd_StatusChanged(string status)
        {
            Console.WriteLine(status);
        }

        private static void bbd_PercentCompleteChanged(int percentComplete)
        {
            Console.WriteLine("Completed " + percentComplete + "%");
        }

        private static void bbd_InstrumentCompleteChanged(BloombergDataInstrument instr)
        {
            string ticker = instr.Ticker;
            string value = string.Empty;
            string error = string.Empty;
            Console.WriteLine("BloombergDataInstrument completed " + ticker);
            foreach (string key in instr.BBFields.Keys)
            {
                if (instr.BBFields[key].Value == null)
                {
                    value = "[NULL]";
                }
                else
                {
                    value = instr.BBFields[key].Value.ToString();
                }
                error = instr.BBFields[key].Error;
                Console.WriteLine("Ticker - {0} : Key - {1} : Value - {2} : Error - {3}", ticker, key, value, error);
            }
        }

        private void bbd_ProcessCompleted(List<BloombergDataInstrument> instruments)
        {
            Console.WriteLine("Process completed");
        }
    }
}