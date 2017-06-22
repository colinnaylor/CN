using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Threading;

namespace OTCOptionValuation_BBImporter
{
    static class Program
    {
        private static EventWaitHandle s_Signal;

        /// <summary>
        /// If a '/u' argument is passed in the import will run for the previous business day.
        /// Otherwise the screen will be displayed allowing the user to enter a valuation date
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            NLog.LogManager.GetCurrentClassLogger().Info("Start"); 
            if (args.Length == 0)
            {
                //no arguments - open the form and allow the user to select a date
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new GUI.ImportScreen());
            }
            else
            {
                if (s_Signal == null)
                    s_Signal = new EventWaitHandle(false, EventResetMode.ManualReset);

                //read in import configuration
                Arguments CommandLine = new Arguments(args);
                bool RunVolatility, RunRates, RunDividends, MissingVolatilitiesOnly;
                RunVolatility = (CommandLine["v"] != null);
                RunRates = (CommandLine["r"] != null);
                RunDividends = (CommandLine["d"] != null);
                MissingVolatilitiesOnly = (CommandLine["m"] != null);

                //otherwise run the import for the previous/current business day
                ImportManager im = new ImportManager();

                im.ProcessComplete += new EventHandler(im_ProcessComplete);
                im.RunSources(Utilities.GetDefaultValuationDate(), RunVolatility, RunRates, RunDividends, MissingVolatilitiesOnly);

                //block the thread until the sources have been priced
                //TODO: really need to use the bloomberg synchronous request to avoid this...
                s_Signal.WaitOne();
            }
        }

        static void im_ProcessComplete(object sender, EventArgs e)
        {
            //unblock the thread and allow the program to exit
            if (s_Signal != null)
                s_Signal.Set();

        }
    }
}
