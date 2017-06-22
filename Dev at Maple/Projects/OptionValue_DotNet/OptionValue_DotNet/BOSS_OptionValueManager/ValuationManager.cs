using Maple;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BOSS_OptionValueManager
{
    /// <summary>
    /// Manage the various steps in the process - updating status as we go
    /// </summary>
    public class ValuationManager
    {
        DateTime valuationDate;
        public DateTime ValuationDate
        {
            get { return valuationDate; }            
        }
        List<BOSSOption> options;

//      private ValuationManager() { ;}
        public ValuationManager(DateTime ValuationDate)
        {
            valuationDate = ValuationDate;
        }

        public List<BOSSOption> Options
        {
            get { return options; }            
        }

        public event StatusUpdateDel StatusChanged;
        private void UpdateStatus(string status)
        {
            Notifier.Notify(Notifier.NotifyDestination.File, Notifier.SeverityLevel.None, status);

            if (StatusChanged != null)
                StatusChanged(string.Format(status));
        }

        public void Load()
        {
            Data.DataLayer dl = new Data.DataLayer();
            options = dl.GetBOSSOptionsToPrice(valuationDate);
            UpdateStatus(String.Format("Loaded {0} options from BOSS", options.Count));
            
            foreach (BOSSOption o in options)
            {
                o.CalculateOptionValue(valuationDate);
            }
            UpdateStatus(String.Format("Valued {0} options from BOSS", options.Count));
        }

        public void Save()
        {
            Data.DataLayer dl = new Data.DataLayer();            
            dl.DeleteOptionPrices(valuationDate);

            foreach (BOSSOption o in options)
            {
                dl.SaveOptionPrice(o, valuationDate);
            } 

            UpdateStatus(String.Format("Saved {0} option valuations", options.Count));
        }

        public void Export()
        {
            Data.DataLayer dl = new Data.DataLayer();
            dl.SaveOptionPricesToBOSS(valuationDate);

            UpdateStatus("Valid option prices have been saved to BOSS");
        }

        public void RunWholeProcess(){

            try {
                Load();
                Save();
                Export();
            } catch (Exception ex) {
                Logger.Log("Error in RunWholeProcess method. [{0}]".Args(ex.Message));
                Maple.Notifier.Notify(ex);
            }
        }
        
    }
}
