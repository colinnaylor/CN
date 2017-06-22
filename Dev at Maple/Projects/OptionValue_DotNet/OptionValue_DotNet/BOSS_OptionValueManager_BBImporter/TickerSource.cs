using BBGShared;
using Maple;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OTCOptionValuation_BBImporter
{
    internal class TickerSource
    {
        string sourceName;
        Data.DataLayer.GetTickerDataDel getTickerData;
        Data.DataLayer.SaveTickerDataDel saveTickerData;
        bool useHistoricalRequests;
        private DateTime valuationdate;

        /// <summary>
        /// pass in the methods you want to handle loading/saving the tickers
        /// and specify whether a Bloomberg historical request should be used (i.e when we are capturing the data the next day)
        /// </summary>
        /// <param name="GetTickerData"></param>
        /// <param name="SaveTickerData"></param>
        public TickerSource(string SourceName, Data.DataLayer.GetTickerDataDel GetTickerData, Data.DataLayer.SaveTickerDataDel SaveTickerData, bool UseHistoricalRequests)
        {
            sourceName = SourceName;
            getTickerData = GetTickerData;
            saveTickerData = SaveTickerData;
            useHistoricalRequests = UseHistoricalRequests;
        }

        /// <summary>
        /// notify user of progress
        /// </summary>
        public event StatusUpdateDel StatusChanged;
        private void UpdateStatus(string status)
        {
            NLog.LogManager.GetCurrentClassLogger().Info(status);
            if (StatusChanged != null)
                StatusChanged(string.Format("{0}:{1}", sourceName, status));
        }

        /// <summary>
        /// notify users when the data has been populated and saved
        /// </summary>
        public event EventHandler ProcessComplete;
        private void CompleteProcess()
        {
            if (ProcessComplete != null)
                ProcessComplete(this, null);
        }

        /// <summary>
        /// get the tickers/flds from db, update flds in BB, save back to DB
        /// </summary>
        /// <param name="ValuationDate"></param>
        public void StartProcess(DateTime ValuationDate)
        {
            valuationdate = ValuationDate;

            try
            {
                UpdateStatus("Getting tickers/fields from database");
                List<BloombergDataInstrument> ins = Data.DataLayer.GetTickers(valuationdate, getTickerData(valuationdate));
                UpdateStatus(string.Format("Got {0} tickers/fields from database", ins.Count));

                if (ins.Count == 0)
                {
                    UpdateStatus("No instruments to retrieve");
                    CompleteProcess();
                }
                else
                {
                    UpdateStatus("Getting field data from Bloomberg");
                    GetBBData(ins, valuationdate);
                }
            }
            catch (Exception ex)
            {
                NLog.LogManager.GetCurrentClassLogger().Error(ex.ToString());
#if DEBUG
                Notifier.SendEmail("colin@mpuk.com","Error in OptionValueManager", Notifier.GetExceptionMessagesHTML(ex), true);
#else
                Notifier.Notify(ex);
#endif
                // Need to complete the process as the BB class will not have done it if there was an error
                CompleteProcess();
            }

        }

        /// <summary>
        /// Create the BB connection and populate field data
        /// </summary>
        /// <param name="Instruments"></param>
        private void GetBBData(List<BloombergDataInstrument> Instruments, DateTime valuationDate)
        {

            // Update instruments depending on what date we need the data for
            BloombergDataInstrument.eRequestType requestType = BloombergDataInstrument.eRequestType.Historical;
            if (valuationdate == DateTime.Today.Date || useHistoricalRequests == false)
            {
                requestType = BloombergDataInstrument.eRequestType.Reference;
            }

            foreach (BloombergDataInstrument ins in Instruments)
            {
                ins.RequestType = requestType;
                if (requestType == BloombergDataInstrument.eRequestType.Historical)
                {
                    ins.DateFrom = valuationdate;
                }
            }

            int reference = BERG_from_C_sharp.RequestBBdataVertically(Instruments, "OptionValueManager");


            SaveTickerDataAndNotifyComplete(Instruments, reference);

        }

        /// <summary>
        /// Save TickerData And Notify Complete
        /// </summary>
        /// <param name="Instruments"></param>
        private void SaveTickerDataAndNotifyComplete(List<BloombergDataInstrument> Instruments, int Reference)
        {
            UpdateStatus("Saving field data to database");
            saveTickerData(Instruments, Reference);
            UpdateStatus(string.Format("Saved {0} fields to database", Instruments.Count));
            CompleteProcess();
        }

    }
}
