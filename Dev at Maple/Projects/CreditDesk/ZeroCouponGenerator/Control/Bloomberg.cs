using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Shared;
using System.Threading;

namespace ZeroCouponGenerator
{
    class Bloomberg
    {
        public void GetBBGData(List<BloombergDataInstrument> bbdis)
        {
            Maple.Logger.Log("Started GetBBGData");
            BloombergData bbd = new BloombergData();

            //bbd.InstrumentCompleteChanged += new BloombergData.InstrumentComplete(bbd_InstrumentCompleteChanged);
            bbd.PercentCompleteChanged += new BloombergData.PercentComplete(bbd_PercentCompleteChanged);
            bbd.ProcessCompleted += new BloombergData.ProcessStatus(bbd_ProcessCompleted);
            bbd.StatusChanged += new BloombergData.StatusUpdate(bbd_StatusChanged);
            bbd.GetBloombergData(bbdis);

            while (!completed) {
                Thread.Sleep(100);
                // wait
            }
            Console.WriteLine("Completed");
            Maple.Logger.Log("Completed GetBBGData");
        }

        static void bbd_PercentCompleteChanged(int percentComplete) {
            Maple.Logger.Log("Completed " + percentComplete.ToString() + "%");
            Console.WriteLine("Completed " + percentComplete.ToString() + "%");
        }

        private  bool completed = false;
         void bbd_ProcessCompleted(List<BloombergDataInstrument> instruments)
        {
            try {
                Maple.Logger.Log("Insert into CurveGenerateData");
                string sql = "INSERT CurveGeneratorData (StampID, TickerID, Ask, Bid, Maturity) VALUES ";
                Console.WriteLine("Process completed");
                //Console.WriteLine(instruments[0].BBFields[0].Value.ToString());
                foreach (BloombergDataInstrument inst in instruments) {
                    sql = sql + "(" + inst.Tag + "," + inst.ID;
                    //Console.WriteLine(inst.ID);
                    //Console.WriteLine(inst.Tag);
                    //Console.WriteLine(inst.Ticker);

                    foreach (BloombergDataInstrumentField val in inst.BBFields.Values) {
                        if (val.Name == "LAST_TRADEABLE_DT") {
                            sql = sql + ",'" + val.Value + "'";
                        } else {
                            sql = sql + "," + (val.Value == null ? 0 : val.Value);
                        }
                    }
                    if (inst.BBFields.Count == 2) {
                        sql = sql + ",null),";
                    } else {
                        sql = sql + "),";
                    }
                }
                sql = sql.Substring(0, sql.Length - 1);
                sql = sql.Replace("-", "");
                Console.WriteLine(sql);

                Maple.Logger.Log(sql);

                Database db = new Database();
                db.ExecSql(sql, "Error inserting bbg data into CurveGeneratorData");

                Maple.Logger.Log("Done insert in CurveGeneratorData");


                completed = true;
            } catch (Exception ex) {
                Maple.Logger.Log(ex.Message);
                Maple.Logger.Log(ex.StackTrace);
            } finally {
                completed = true;
            }
        }

         void bbd_StatusChanged(string status)
        {
            //Use maple log functionality
            Console.WriteLine(status);
        }

    }
}
