using ZeroCouponGenerator.Properties;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace ZeroCouponGenerator
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Utils.Log("Started");
            try
            {

                if (args.Length != 0)
                {

                    if (args[0] == "BBG")
                    {
                        try
                        {
                            //get latest bbg data only
                            Database db = new Database();
                            db.GetTickerRecordSet();
                        }
                        catch (Exception ex)
                        {
                            Utils.Log(ex.Message);
                            Utils.Log(ex.StackTrace);
                        }
                    }
                    else if (args[0] == "CALC")
                    {
                        Manager man = new Manager();
                        if (args.Length > 1)
                        {
                            //run only curve based on specific id for a particular date
                            man.Action(int.Parse(args[1]));
                        }
                        else
                        {
                            man.Action();

                            //append task to import discount factors into boss.
                            Utils.Log("calling SP spRCR_DataImport...");
                            using (var conn = new SqlConnection(Settings.Default.BossConnectionString))
                            {
                                using (var command = new SqlCommand())
                                {
                                    command.CommandType = System.Data.CommandType.StoredProcedure;
                                    command.CommandText = "spRCR_DataImport";
                                    command.Parameters.Add(new SqlParameter("@DateAt", DateTime.Now.Date));
                                    command.Connection = conn;
                                    conn.Open();
                                    command.ExecuteNonQuery();
                                }
                            }
                            Utils.Log("Finished calling SP spRCR_DataImport.");

                        }
                    }
                }
                else
                {
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    Application.Run(new Form1());
                }
            }
            catch (Exception ex)
            {
                Utils.Log(string.Format("Fatal error: {0}", ex.ToString()));
            }

            Utils.Log("Complete");
        }
    }
}
