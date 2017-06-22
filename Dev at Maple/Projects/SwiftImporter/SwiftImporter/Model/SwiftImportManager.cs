using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Timers;
using Maple;
using SwiftImporterService;
using SwiftImporterUI.Properties;
using Timer = System.Timers.Timer;

namespace SwiftImporterUI.Model
{
    public class SwiftImportManager
    {
        //CancellationTokenSource cancelSource;
        int statusCheckMins = 2;
        Dictionary<string, bool> fileToImport = new Dictionary<string, bool>();
        public SwiftImportManager()
        {
            ApplicationStatus.SetStatus("SwiftImporter", "OK", "Started", statusCheckMins);
            Timer timer = new Timer(statusCheckMins * 60000);
            timer.Elapsed += timer_Elapsed;
#if !DEBUG
            timer.Start();
#endif
        }

        void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            ApplicationStatus.SetStatus("SwiftImporter", "OK", "Ready and waiting...", statusCheckMins);
        }

        public void Initialise()
        {

            FileMonitorPath =
#if DEBUG
 Settings.Default.SwiftFilesPath_TEST;
#else
            Properties.Settings.Default.SwiftFilesPath;
#endif


        }

        /// <summary>
        /// Reports any files that haven't been processed.  This acts like an independant check that can be run at any point, to check that all files have been processed.
        /// </summary>
        public void ReportAnyUnprocessedFiles()
        {
            int filesUnArchived = CountUnArchivedFiles();
            if (filesUnArchived > 0)
            {
                string message = "There are currently " + filesUnArchived.ToString()
                                    + " unprocessed .out files in the following location:  " + FileMonitorPath;
                message += "\r\n\r\nYou now have 2 choices:\r\n"
                                + "1.  The next scheduled runtime is "
                                + new DateTime(Settings.Default.ScheduledRunTime.Ticks).ToString("hh:mm tt")
                                + ".  At this precise time the swift importer will start and any .out files at that time will be processed.\r\n"
                                + "2.  Alternatively you can go and manually run it."
                                + "\r\n\r\nCheck here for more info:  http://cougar/SiteDirectory/ITDev/MARS%20Wiki/Swift%20Importer.aspx";

                string htmlMessage = message.Replace("\r\n\r\n", "<P>");
                htmlMessage = message.Replace("\r\n", "<BR>");

                UpdateStatus("*WARNING:  " + message);
                SendEmail(htmlMessage, "WARNING - " + filesUnArchived.ToString() + " unprocessed swift files");
            }
        }


        #region FileWatcher stuff

        FileSystemWatcher fileWatcher;
        private void SetupFileWatcher()
        {
            if (fileWatcher == null)
            {
                // Creating a new one seems to take a while so best to only create it when really need to.  
                // This stuff is static.  The file path can be updated seperately if changed.
                ResetFileWatcher();
            }

            InitialiseFileWatcherEvents();
        }


        private void InitialiseFileWatcherEvents()
        {
            fileWatcher.Error += fileWatcher_Error;

            if (WatchForFiles)
            {
                UpdateStatus("File Monitoring on - " + fileWatcher.Path);
                fileWatcher.Created += fileWatcher_Created;
            }
            else
            {
                fileWatcher.Created -= fileWatcher_Created;
                UpdateStatus("File Monitoring off!");
            }
        }

        /// <summary>
        /// Completely resets the file watcher from scratch.  
        /// </summary>
        private void ResetFileWatcher()
        {
            if (fileWatcher != null)
            {
                fileWatcher.Created -= fileWatcher_Created;
                fileWatcher.Error -= fileWatcher_Error;
                fileWatcher = null;
            }

            UpdateStatus("Creating new file watcher...");

            fileWatcher = new FileSystemWatcher();
            fileWatcher.Path = FileMonitorPath;
            fileWatcher.Filter = "*.out";
            fileWatcher.NotifyFilter = NotifyFilters.FileName;
            fileWatcher.IncludeSubdirectories = true;
            fileWatcher.EnableRaisingEvents = true;

            // now setup the rest of the events
            InitialiseFileWatcherEvents();

            UpdateStatus("File watcher setup");
        }

        void fileWatcher_Error(object sender, ErrorEventArgs e)
        {

            var fileEx = e.GetException();

            string errorMessage = "An error has occured with the file watcher:  ";

            try
            {
                // throwing it captures all the other exception info such as TargetSite
                throw new WarningException(errorMessage + fileEx.Message + ".  Will attempt to reconnect and report again if unsuccessfull...", fileEx);
            }
            catch (WarningException ex)
            {
                ReportError(ex);
            }

            fileWatcher = new FileSystemWatcher(); // just to reinit it

            int attempts = 1;
            while (!fileWatcher.EnableRaisingEvents)
            {
                try
                {
                    attempts++;
                    ResetFileWatcher();  // try and set it back up again
                }
                catch (Exception ex)
                {
                    bool weekend = DateTime.Today.DayOfWeek == DayOfWeek.Saturday || DateTime.Today.DayOfWeek == DayOfWeek.Sunday;

                    TimeSpan timeout = weekend ? new TimeSpan(4, 0, 0) : new TimeSpan(0, 1, 0); // on weekends we wait 4 hours, otherwise just a minute
                    try
                    {

                        throw new WarningException(string.Format("{0} {1}.  \r\nA reconnection attempt has failed, will try again in {2}", errorMessage, ex.Message, timeout.ToReadableString()), ex);
                    }
                    catch (WarningException wex)
                    {
                        ReportError(wex);
                        Thread.Sleep(timeout); //Wait for retry after the set time
                    }

                }
            }
        }



        void fileWatcher_Created(object sender, FileSystemEventArgs e)
        {
            //check if file is is dictionary
            //if not in dictionary then add it and continue, else ignore and out
            if (!fileToImport.ContainsKey(e.FullPath))
            {
                fileToImport.Add(e.FullPath, true);
                //Console.WriteLine("Processing " + e.FullPath);

                UpdateStatus("New Swift file found:  " + e.FullPath);

                try
                {
                    SwiftFile.WaitReady(e.FullPath); // wait until the file is ready

                    var statements = ImportSwiftFile(e.FullPath); // now import it

                    SaveSwiftStatements(statements); // and finally save the statement 
                }
                catch (Exception ex)
                {
                    ReportError(ex);
                }
            }

        }

        string fileMonitorPath;
        public string FileMonitorPath
        {
            get
            {
                return fileMonitorPath;
            }
            set
            {
                fileMonitorPath = value;
                if (fileWatcher != null)
                    fileWatcher.Path = value;
            }
        }

        bool watchForFiles;
        public bool WatchForFiles
        {
            get { return watchForFiles; }
            set { watchForFiles = value; SetupFileWatcher(); }
        }

        #endregion


        private int CountUnArchivedFiles()
        {
            DirectoryInfo dir = new DirectoryInfo(FileMonitorPath);

            var allFiles = dir.GetFiles("*.out", SearchOption.AllDirectories);

            return allFiles.Count();
        }

        DateTime lastReportedErrorTime = DateTime.MinValue;
        int errorsReported = 0;
        bool collationEmailSent = false;
        public void ReportError(Exception exception)
        {
            errorsReported++;
            UpdateStatus("*ERROR:  " + exception.Message + "\r\n\r\nStackTrace:  " + exception.StackTrace);

            DateTime anHourAgo = DateTime.Now.AddHours(-1);

            if (errorsReported > 10)  // reported too many already 
            {
                if (lastReportedErrorTime < anHourAgo)  // time to resend errors ?
                {
                    collationEmailSent = false;
                    errorsReported = 0;
                }
                else
                {
                    // send one email to inform of collation
                    if (!collationEmailSent)
                    {
                        Thread.Sleep(5000);
                        SendEmail("Too many errors to report, so ignoring for 1 hour.", //  Please look at the logs for all the errors:  " + logsPath, 
                                   "SwiftImpoter Error Collation Notification...");
                        collationEmailSent = true;
                    }
                    return; // not time to resend yet so just log it 
                }
            }

            lastReportedErrorTime = DateTime.Now;

            SendEmail(exception);
        }

        private void SendEmail(Exception ex)
        {

#if DEBUG
            Notifier.Notify(Notifier.NotifyDestination.MessageBox, ex);
#else
            Notifier.Notify(Notifier.NotifyDestination.Email, Notifier.SeverityLevel.Medium, ex);
#endif
        }

        private void SendEmail(string message, [Optional] string subject)
        {

            message += "<P>Log:  " + Logger.GetLogFilePathAndName(true);
#if DEBUG
            if (string.IsNullOrEmpty(subject))
                Notifier.Notify(Notifier.NotifyDestination.Email, Notifier.SeverityLevel.Medium, message, "hansa@mpuk.com");
            else
                Notifier.Notify(Notifier.NotifyDestination.Email, Notifier.SeverityLevel.Medium, message, "hansa@mpuk.com", subject);
#else
            if (string.IsNullOrEmpty(subject))
                Notifier.Notify(Notifier.NotifyDestination.Email, Notifier.SeverityLevel.Medium, message);
            else
                Notifier.Notify(Notifier.NotifyDestination.Email, Notifier.SeverityLevel.Medium, message, subject);
#endif

        }



        #region Cancellation stuff

        public void Cancel()
        {
            //cancelSource.Cancel(true);
        }

        #endregion

        public List<SwiftStatement> ImportSwiftFile(string filePath)
        {
            List<SwiftStatement> statements = null;
            UpdateStatus("Importing swift file: " + filePath + "...");

            try
            {
                statements = SwiftImporter.ImportFile(filePath);
                UpdateStatus("Statements in file = " + statements.Count.ToString());
                //RemoveStatementsAlreadyProcessed(statements);
            }
            catch (Exception ex)
            {
                throw new Exception("Problem importing file:  " + filePath + "\r\nError:  " + ex.Message);
            }

            UpdateStatus("Swift File Imported!");

            return statements;
        }

        public List<SwiftStatement> ImportAllSwiftFiles(string dirPath)
        {
            var statements = new List<SwiftStatement>();

            DirectoryInfo dir = new DirectoryInfo(dirPath);
            if (!dir.Exists)
                throw new DirectoryNotFoundException("Cannot find the directory: " + dirPath + ".  Please check and reimport files.");

            var allFiles = dir.GetFiles("*.out", SearchOption.AllDirectories);

            int i = 1;

            bool errorOccured = false;

            foreach (var file in allFiles)
            {
                UpdateProgress(i++, allFiles.Count());
                try
                {
                    var singleFileStmnts = ImportSwiftFile(file.FullName);
                    if (singleFileStmnts != null)
                        statements.AddRange(singleFileStmnts);
                }
                catch (Exception ex)
                {
                    errorOccured = true;
                    UpdateStatus(ex.Message + "\r\n" + ex.StackTrace + "\r\n\r\nContinuing...");
                    // just report it and continue
                }
            }

            UpdateStatus("Total statements imported = " + statements.Count.ToString());

            if (errorOccured) throw new Exception("An error occured whilst importing multiple swift files.  Please check the logs for details:  " + Logger.GetLogFilePathAndName(true));

            return statements;
        }

        public void SaveSwiftStatements(List<SwiftStatement> statements)
        {
            UpdateStatus("Saving " + statements.Count.ToString() + " statements...");
            Dictionary<string, bool> filesToArchive = new Dictionary<string, bool>();

            int i = 0;
            int saved = 0;
            bool errorOccured = false;
            using (SwiftDataLayer dataLayer = new SwiftDataLayer())
            {
                foreach (var stmt in statements)
                {
                    if (!filesToArchive.ContainsKey(stmt.FileName))
                    {
                        filesToArchive.Add(stmt.FileName, true); // Default to OK
                    }

                    if (!dataLayer.IsStatementInDb(stmt))
                    {
                        try
                        {
                            dataLayer.SaveSwiftStatement(stmt);
                            saved++;
                        }
                        catch (Exception ex)
                        {
                            errorOccured = true;
                            filesToArchive[stmt.FileName] = false;

                            UpdateStatus(ex.Message + "\r\n" + ex.StackTrace + "\r\n\r\nContinuing...");
                            // just report it and continue
                            SendEmail(new Exception(string.Format("Error saving data for {0}\r\n\r\n", stmt.FileName), ex));
                        }
                    }
                    else
                        UpdateStatus("Statement already saved so ignoring:  FileName-" + stmt.FileName
                                                                          + ", StatmentNo-" + stmt.StatementNumber);
                    i++;

                    UpdateProgress(i, statements.Count);
                }
                // if there was an error, we can just leave it un-archived so it will get picked up again next time
                foreach (string key in filesToArchive.Keys)
                {
                    if (filesToArchive[key])
                    {
                        ArchiveFile(key);
                    }
                }

            }

            UpdateStatus(saved.ToString() + " Statements saved!");
            if (errorOccured) throw new Exception("An error occured whilst saving the statements.  Please check the logs for details:  " + Logger.GetLogFilePathAndName(true));

        }

        private void ArchiveFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                UpdateStatus("Archiving file - " + filePath);

                string newFileName = filePath + ".archive";

                int i = 1;
                while (File.Exists(newFileName))
                {
                    i++;
                    newFileName = filePath + ".archive" + i.ToString();
                }

                File.Move(filePath, newFileName);
                //remove here from dictionary
                if (fileToImport.ContainsKey(filePath))
                {
                    fileToImport.Remove(filePath);
                }
            }
        }


        public delegate void StatusUpdate(string status);
        public event StatusUpdate StatusChanged;

        private void UpdateStatus(string status)
        {
            if (StatusChanged != null)
                StatusChanged(status);

            Logger.Log(status);
        }

        public delegate void ProgressUpdate(int percentage);
        public event ProgressUpdate ProgressChanged;
        private void UpdateProgress(int incrValue, int total)
        {
            try
            {
                var perc = ((double)incrValue / total * 100);
                int percentage = (int)perc;
                if (ProgressChanged != null)
                    ProgressChanged(percentage);
            }
            catch (Exception ex)
            {
                // We shouldn't be worried about the failure of a progress bar being updated
                Logger.Log("Failed to update progress bar. " + ex.Message);
            }
        }
    }

}
