using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Timers;
using System.Windows.Input;
using Microsoft.Practices.Prism.Commands;
using SwiftImporterUI.Model;
using SwiftImporterUI.Properties;

namespace SwiftImporterUI
{
    public class MainViewModel : INotifyPropertyChanged
    {
        SwiftImportManager swiftManager;
        public MainViewModel()
        {
            Statements = new List<SwiftStatement>();
            swiftManager = new SwiftImportManager();
            swiftManager.StatusChanged += swiftManager_StatusChanged;
            swiftManager.Initialise();  // have to init after subscribing to events above so that we can recieve the status updates + errors straight away.

            FilePath = swiftManager.FileMonitorPath;
            MonitoringPath = swiftManager.FileMonitorPath;
            IsMonitoring = true;
            KeepStatusInMemory = false;

            // setup bgworker
            bgWorker = new BackgroundWorker();
            bgWorker.DoWork += bgWorker_DoWork;
            bgWorker.ProgressChanged += bgWorker_ProgressChanged;
            bgWorker.RunWorkerCompleted += bgWorker_RunWorkerCompleted;
            bgWorker.WorkerSupportsCancellation = true;
            bgWorker.WorkerReportsProgress = true;

#if !DEBUG
            // force a run on startup in case stuff has come in whilst it was not running
            UpdateStatus("Running on startup in case files were missed whilst down...");
            RunBgWorker(bgworkerType.ImportAndSaveAll);
#endif

            // setup timer
            timer = new Timer(1000);
            timer.Elapsed += timer_Elapsed;

            IsTimerScheduled = true;
        }

        Timer timer;
        bool hasTimerAlreadyRun;

        void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                timer.Stop();
                var timeToRun = Settings.Default.ScheduledRunTime;
                if (DateTime.Now.TimeOfDay > timeToRun &&
                    DateTime.Now.TimeOfDay < timeToRun.Add(TimeSpan.FromMinutes(15)))
                {
                    if (hasTimerAlreadyRun)
                        return;
                    UpdateStatus("Scheduled timer started...");
                    RunBgWorker(bgworkerType.ImportAndSaveAll);
                    hasTimerAlreadyRun = true;

                }
                else
                    hasTimerAlreadyRun = false;


            }
            catch (Exception ex)
            {
                swiftManager.ReportError(ex);
                throw ex;
            }
            finally
            {
                timer.Start();
            }
        }



        void swiftManager_ProgressChanged(int percentage)
        {
            bgWorker.ReportProgress(percentage);
        }

        void swiftManager_StatusChanged(string status)
        {
            UpdateStatus(status);
        }


        void UpdateStatus(string status)
        {
            if (KeepStatusInMemory)
                Status = DateTime.Now.ToString("dd-MMM-yy HH:mm:ss") + " - " + status + "\r\n" + Status;
        }


        #region properties

        bool keepStatusInMemory;
        public bool KeepStatusInMemory
        {
            get { return keepStatusInMemory; }
            set { keepStatusInMemory = value; OnPropertyChanged("KeepStatusInMemory"); }
        }

        bool isMonitoring = true;
        public bool IsMonitoring
        {
            get { return isMonitoring; }
            set
            {
                isMonitoring = value;
                swiftManager.WatchForFiles = value;
                OnPropertyChanged("IsMonitoring");
            }
        }


        public bool IsTimerScheduled
        {
            get { return timer.Enabled; }
            set
            {
                UpdateStatus("Timer is " + (value ? "ON" : "OFF"));
                timer.Enabled = value;
                OnPropertyChanged("IsTimerScheduled");
            }
        }

        private int progressPercentage;
        public int ProgressPercentage
        {
            get { return progressPercentage; }
            set { progressPercentage = value; OnPropertyChanged("ProgressPercentage"); }
        }

        private string status;
        public string Status
        {
            get { return status; }
            set { status = value; OnPropertyChanged("Status"); }
        }


        List<SwiftStatement> statements;
        public List<SwiftStatement> Statements
        {
            get { return statements; }
            set { statements = value; OnPropertyChanged("Statements"); }
        }

        string filePath;
        public string FilePath
        {
            get { return filePath; }
            set { filePath = value; OnPropertyChanged("FilePath"); }
        }

        string monitoringPath;
        public string MonitoringPath
        {
            get { return monitoringPath; }
            set
            {
                // path has changed so before we continue, make sure monitoring is turned off until the user explicitly turns it on
                IsMonitoring = false;

                monitoringPath = value;
                swiftManager.FileMonitorPath = value;
                OnPropertyChanged("MonitoringPath");
            }
        }

        #endregion

        private void ImportSwiftFile()
        {
            Statements = new List<SwiftStatement>();
            try
            {
                Statements = swiftManager.ImportSwiftFile(FilePath);
            }
            catch (Exception ex)
            {
                swiftManager.ReportError(ex);
            }
        }

        private void ImportAllSwiftFiles()
        {
            Statements = new List<SwiftStatement>();

            RunBgWorker(bgworkerType.ImportAll);
        }



        public event ProgressChangedEventHandler ProgressChanged
        {
            add { bgWorker.ProgressChanged += value; }
            remove { bgWorker.ProgressChanged -= value; }
        }

        #region BgWorker methods

        BackgroundWorker bgWorker = new BackgroundWorker();

        void bgWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            UpdateProgress(e.ProgressPercentage);

            // throw new NotImplementedException();
        }

        void UpdateProgress(int percentage)
        {
            ProgressPercentage = percentage;
        }

        enum bgworkerType
        {
            ImportAll,
            SaveAll,
            ImportAndSaveAll
        }

        void bgWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                // look out for progress updates
                swiftManager.ProgressChanged += swiftManager_ProgressChanged;


                var workertype = (bgworkerType)e.Argument;
                switch (workertype)
                {
                    case bgworkerType.ImportAll:
                        //Statements = swiftManager.ImportAllSwiftFiles(FilePath, bgWorker);
                        Statements = swiftManager.ImportAllSwiftFiles(FilePath);
                        break;
                    case bgworkerType.SaveAll:
                        swiftManager.SaveSwiftStatements(Statements);
                        break;
                    case bgworkerType.ImportAndSaveAll:
                        Statements = swiftManager.ImportAllSwiftFiles(FilePath);
                        swiftManager.SaveSwiftStatements(Statements);
                        break;
                    default:
                        throw new NotSupportedException("Bgworker type not setup");
                }

                // remove progress updates once done as we don't want the swift importer manager updating the ui, only update when manually run from the ui
                swiftManager.ProgressChanged -= swiftManager_ProgressChanged;


            }
            catch (Exception ex)
            {
                swiftManager.ReportError(ex);
            }
        }

        private void RunBgWorker(bgworkerType workertype)
        {
            if (!bgWorker.IsBusy)
                bgWorker.RunWorkerAsync(workertype);
        }

        void bgWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null) // if an unhandled error occured on the background thread we need to know
            {
                Status = e.Error.Message;
                throw e.Error;
            }
        }

        #endregion

        private void CancelBgWorker()
        {
            //bgWorker.CancelAsync();
            swiftManager.Cancel();
        }

        private void SaveStatements()
        {
            RunBgWorker(bgworkerType.SaveAll);
            //swiftManager.SaveSwiftStatements(Statements);
        }


        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }


        #region commands

        private DelegateCommand saveStatementsCommand;
        public ICommand SaveStatementsCommand
        {
            get
            {
                if (saveStatementsCommand == null)
                    saveStatementsCommand = new DelegateCommand(SaveStatements, SaveCanExecute);
                return saveStatementsCommand;
            }
        }

        public bool SaveCanExecute()
        {
            return Statements != null && Statements.Count > 0 && !bgWorker.IsBusy;
        }

        private DelegateCommand importFileCommand;
        public ICommand ImportFileCommand
        {
            get
            {
                if (importFileCommand == null)
                    importFileCommand = new DelegateCommand(ImportSwiftFile, ImportCanExecute);
                return importFileCommand;
            }
        }

        public bool ImportCanExecute()
        {
            return !ImportAllCanExecute() && !bgWorker.IsBusy;
        }

        private DelegateCommand importAllFilesCommand;
        public ICommand ImportAllFilesCommand
        {
            get
            {
                if (importAllFilesCommand == null)
                    importAllFilesCommand = new DelegateCommand(ImportAllSwiftFiles, ImportAllCanExecute);
                return importAllFilesCommand;
            }
        }

        private DelegateCommand cancelImportCommand;
        public ICommand CancelImportCommand
        {
            get
            {
                if (cancelImportCommand == null)
                    cancelImportCommand = new DelegateCommand(CancelBgWorker, CancelCanExecute);
                return cancelImportCommand;
            }
        }

        private DelegateCommand cancelCommand;
        public ICommand CancelCommand
        {
            get
            {
                if (cancelCommand == null)
                    cancelCommand = new DelegateCommand(CancelBgWorker, CancelCanExecute);
                return cancelCommand;
            }
        }

        public bool CancelCanExecute()
        {
            return bgWorker.IsBusy;
        }

        public bool ImportAllCanExecute()
        {
            return FilePath.Trim().EndsWith(@"\") && !CancelCanExecute();
        }

        #endregion

    }
}
