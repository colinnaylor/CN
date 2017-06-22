using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Maple;

namespace OTCOptionValuation_BBImporter
{
    /// <summary>
    /// run all of the bb data imports
    /// </summary>
    public class ImportManager
    {
        Queue<TickerSource> sources = new Queue<TickerSource>();
        DateTime valuationDate;

        public void RunSources(DateTime ValuationDate, bool RunVolatility, bool RunRates, bool RunDividends, bool MissingVolatilitiesOnly)
        {
            valuationDate = ValuationDate;
            Data.DataLayer dl = new Data.DataLayer();

            //create the three sources and start...

            //Capture Date = Valuation Date     use non-historical VOLATILITY_XXD   (NB: will only be correct after market close)
            //Capture Date = Valuation Date + 1 use historical VOLATILITY_XXD       
            if (RunVolatility)
            {
                if (MissingVolatilitiesOnly)
                    AddSource(new TickerSource("volatility", dl.GetTickerData_Volatility_MissingOnly, dl.SaveTickerData_Volatility, ValuationDate.Date != DateTime.Now.Date));
                else
                    AddSource(new TickerSource("volatility", dl.GetTickerData_Volatility, dl.SaveTickerData_Volatility, ValuationDate.Date != DateTime.Now.Date));
            }

            //Capture Date = Valuation Date     use non-historical PX_LAST
            //Capture Date = Valuation Date + 1 use non-historical PX_LAST          (MUST BE BEFORE 11 AM, PX_LAST IS UPDATED AFTER THIS)
            if (RunRates)
                AddSource(new TickerSource("rate", dl.GetTickerData_Rate, dl.SaveTickerData_Rate, ValuationDate.Date != DateTime.Now.Date));

            //always use non-historical
            if (RunDividends)
                AddSource(new TickerSource("dividend", dl.GetTickerData_Dividend, dl.SaveTickerData_Dividend, false));

            StartNextProcess();
        }

        private void AddSource(TickerSource source)
        {
            source.StatusChanged += new StatusUpdateDel(source_StatusChanged);
            source.ProcessComplete += new EventHandler(source_ProcessComplete);
            sources.Enqueue(source);
        }


        private void StartNextProcess()
        {
            if (sources.Count != 0)
            {
                UpdateStatus(string.Format("Before Dequeue. Count is {0}.", sources.Count));
                sources.Dequeue().StartProcess(valuationDate);
                UpdateStatus(string.Format("After Dequeue. Count is {0}.", sources.Count));
            }
            else
            {
                UpdateStatus("Import complete");
                CompleteProcess();
            }
        }

        void source_StatusChanged(string status)
        {
            UpdateStatus(status);
        }

        void source_ProcessComplete(object sender, EventArgs e)
        {
            StartNextProcess();
        }

        /// <summary>
        /// notify user of progress
        /// </summary>
        public event StatusUpdateDel StatusChanged;
        private void UpdateStatus(string status)
        {
            Notifier.Notify(Notifier.NotifyDestination.File, Notifier.SeverityLevel.None, status);
            NLog.LogManager.GetCurrentClassLogger().Info(status);

            if (StatusChanged != null)
                StatusChanged(string.Format(status));
        }

        /// <summary>
        /// notify that all of the imports have been complete
        /// </summary>
        public event EventHandler ProcessComplete;
        private void CompleteProcess()
        {
            if (ProcessComplete != null)
                ProcessComplete(this, null);
        }
    }
}
