using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace ZeroCouponGenerator
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.Text = Application.ProductName + "  v" + Application.ProductVersion.Substring(0, Application.ProductVersion.LastIndexOf("."));

            CurrencyCombo.Items.Add("GBP");
            CurrencyCombo.Items.Add("EUR");
            CurrencyCombo.Items.Add("USD");
            CurrencyCombo.SelectedIndex = 0;

            //Testbutton_Click(null, null);
        }

        DateTime ValueDate = DateTime.MinValue;
        InputData inputData = null;

        private void Testbutton_Click(object sender, EventArgs e)
        {
            try
            {
                ReportBox.Text = "";
                Report("Beginning calc for " + CurrencyCombo.Text);
                ReadTestData(CurrencyCombo.Text);
                Report("Test data read.");
                ReportBox.Refresh();

                inputData.Holidays = Functions.GetHolidays(inputData.HolidayCentre);
                ZeroCouponCalc calc = new ZeroCouponCalc();

                if (inputData.LiborData.Count < 3 || inputData.FutureData.Count < 3 || inputData.SwapData.Count < 3)
                {
                    throw new Exception(string.Format("Not enough rates read in for {0} currency.", CurrencyCombo.Text));
                }

                string ret = calc.Calc(inputData);
                if (ret.Length > 0)
                {
                    Report("Error. Calc method returned:");
                    Report(ret);
                }
                else
                {
                    Report("Calc completed successfully");
                    Report("===========================");
                    foreach (OutputPoint rate in calc.Output)
                    {
                        Report(rate.Date.ToString("dd MMM yy") + "\t" + rate.Term + "\t" + rate.Rate);
                    }
                }


            }
            catch (Exception ex)
            {
                Report("Error:");
                Report(ex.Message);
            }
        }

        private void ReadTestData(string Currency)
        {
            string folder = Application.ExecutablePath.Substring(0, Application.ExecutablePath.LastIndexOf("\\"));
            string file = folder + "\\Data\\RateCaptureTest.txt";
            if (File.Exists(file))
            {

                string[] lines = File.ReadAllLines(file);

                inputData = new InputData();
                inputData.LiborData = new List<Rate>();
                inputData.FutureData = new List<Rate>();
                inputData.SwapData = new List<Rate>();
                string currencySection = "";

                foreach (string line in lines)
                {
                    if (line.Trim().Length > 0)
                    {
                        if (line.Trim().StartsWith("["))
                        {
                            // Start of a currency section
                            currencySection = line.Trim().Substring(1, 3);

                        }
                        else
                        {
                            if (currencySection == Currency)
                            {
                                // If is has an '\' in it, it is one of the settings
                                if (line.IndexOf("\\") > -1)
                                {
                                    string[] setting = line.Split(new Char[] { '\\' });
                                    string name = setting[0].Trim().ToLower();
                                    string value = setting[1].Trim();
                                    Console.WriteLine(name);

                                    switch (name)
                                    {
                                        case "startdate":
                                            inputData.StartDate = DateTime.Parse(value);
                                            break;
                                        case "daycountconvention":
                                            inputData.DayCountConvention = eNumConvert.GetDayCountEnum(value);
                                            break;
                                        case "libordaycountconvention":
                                            inputData.LiborDayCountConvention = eNumConvert.GetDayCountEnum(value);
                                            break;
                                        case "futuredaycountconvention":
                                            inputData.FutureDayCountConvention = eNumConvert.GetDayCountEnum(value);
                                            break;
                                        case "swapfixeddaycountconvention":
                                            inputData.SwapFixedDayCountConvention = eNumConvert.GetDayCountEnum(value);
                                            break;
                                        case "nextworkingday":
                                            inputData.NextWorkingDay = eNumConvert.GetNextWorkingDayEnum(value);
                                            break;
                                        case "holidaycentre":
                                            inputData.HolidayCentre = value;
                                            break;
                                        case "maxfuturetermindays":
                                            inputData.MaxFutureTermDays = int.Parse(value);
                                            break;
                                        case "settledaysforfutures":
                                            inputData.SettleDaysForFutures = int.Parse(value);
                                            break;
                                        case "settledaysforlibor":
                                            inputData.SettleDaysForLibors = int.Parse(value);
                                            break;
                                        case "settledaysforswaps":
                                            inputData.SettleDaysForSwaps = int.Parse(value);
                                            break;
                                        case "swapfloatpaymentfrequency":
                                            inputData.SwapFloatPaymentFrequency = int.Parse(value);
                                            break;
                                        case "swapfixedpaymentfrequency":
                                            inputData.SwapFixedPaymentFrequency = int.Parse(value);
                                            break;
                                        default:
                                            throw new Exception(string.Format("Unexpected setting name [{0}]", name));
                                    }

                                }
                                else
                                {
                                    Rate rec = new Rate();
                                    string[] fields = line.Split(new Char[] { ',' });

                                    rec.SecType = fields[0];
                                    rec.TermCode = fields[1];
                                    rec.Bid = double.Parse(fields[2]);
                                    rec.Ask = double.Parse(fields[3]);
                                    DateTime day;
                                    if (DateTime.TryParse(fields[5], out day))
                                    {
                                        rec.Expiry = day;
                                    }

                                    switch (rec.SecType.ToLower())
                                    {
                                        case "libor":
                                            inputData.LiborData.Add(rec);
                                            break;
                                        case "future":
                                            inputData.FutureData.Add(rec);
                                            break;
                                        case "swap":
                                            inputData.SwapData.Add(rec);
                                            break;
                                        default:
                                            throw new Exception("Sec Type not handled in ReadTestData.");
                                    }
                                }
                            }
                        }
                    }
                }

            }
            else
            {
                throw new Exception("File does not exist. " + file);
            }
        }

        private void Report(string data)
        {
            ReportBox.SelectionStart = ReportBox.Text.Length;
            ReportBox.SelectedText = data + "\r\n";
            ReportBox.SelectionStart = ReportBox.Text.Length;
        }

    }
}
