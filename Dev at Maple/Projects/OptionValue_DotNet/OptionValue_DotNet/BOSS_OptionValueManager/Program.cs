using Maple;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace BOSS_OptionValueManager
{
    static class Program
    {
        /// <summary>
        /// Open the screen or run the valuation unattended
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            // Capture all unhandled non-thread exceptions.
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
            // Capture all unhandled thread exceptions.
            Application.ThreadException += new System.Threading.ThreadExceptionEventHandler(Application_ThreadException);

            if (args.Length == 0){
                //no arguments - open the form and allow the user to select a date
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new GUI.ValuationScreen());
            }else if (args[0] == "/u"){
                //otherwise run the import for the previous business day
                DateTime day = Utilities.GetDefaultValuationDate();
                Logger.Log("Auto running option valuation for " + day.ToString("dd MMM yy"));

                ValuationManager vm = new ValuationManager(day);
                vm.RunWholeProcess();
                Logger.Log("Auto run complete");
            } else if (args[0].ToLower() == "test") {
                Test();
            }

        }

        static void Test() {
            OptionValue32_CS.Pricer pricer = new OptionValue32_CS.Pricer();

            double ret;
            double strikePrice, underlyingPrice, underlyingVol, yearsToMaturity,riskFreeRate;
            short optionType, optionStyle;
            string dividends;

            strikePrice = 0.99;
            underlyingPrice = 0.9;
            underlyingVol = 0.1;
            yearsToMaturity = 0.25;
            riskFreeRate = 0.04;
            optionType = 0;
            optionStyle = 0;
            dividends = "0.15/0.03";

            ret = pricer.OptionValue(strikePrice, underlyingPrice, underlyingVol, yearsToMaturity, riskFreeRate, optionType,
                optionStyle, dividends);

            MessageBox.Show("Result of test was " + ret.ToString());
        }

        static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            // Set a message to notify that we are shutting down
            Exception exception = new Exception("Unhandled exception occured, application exiting.", e.Exception);

            // Notifying exceptions via email also stores to the db so no need to specify that here
            Notifier.Notify(MapleInteractionDestination, Notifier.SeverityLevel.Fatal, exception);

            // Unhandled exception - close application.
            Environment.Exit(-1);
        }



        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            string message = "";

            // check if there is an exception.  For some reason occasionally there is no exception object.
            if ((e.ExceptionObject as Exception) != null)
                message = "Unhandled exception occured, application exiting.";
            else
                message = "An unknown un-handled Exception occured, application exiting.";

            Exception exception = new Exception(message, e.ExceptionObject as Exception);

            // Notifying exceptions via email also stores to the db so no need to specify that here
            Notifier.Notify(MapleInteractionDestination, Notifier.SeverityLevel.Fatal, exception);

            // Unhandled exception - close application.
            Environment.Exit(-1);
        }

        static Notifier.NotifyDestination MapleInteractionDestination
        {
            get
            {
#if DEBUG
                return Notifier.NotifyDestination.MessageBox;
#else
               return Notifier.NotifyDestination.Email;
#endif
            }
        }
    }
}
