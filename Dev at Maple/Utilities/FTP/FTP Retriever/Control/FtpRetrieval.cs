using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FTP_Retriever.Properties;
using System.Data.SqlClient;
using System.IO;
using System.Windows.Forms;
using Maple.PubSubClient;
using System.Threading;
using System.Net;

namespace FTP_Retriever
{

    #region EventArgs and Delegate
    /// <summary>
    /// A custom EventArgs class to hold any extra data that we require
    /// </summary>
    public class MessageEventArgs : EventArgs
    {
        // Extra variables
        public string Message;

        // Constructor
        public MessageEventArgs(string message)
        {
            Message = message;
        }
    }

    /// <summary>
    /// A delegate with the event signature
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void MessageEventDelegate(object sender, MessageEventArgs e);
    #endregion

    class FtpRetrieval
    {
        System.Timers.Timer timer1 = new System.Timers.Timer();

        public event MessageEventDelegate MessageEvent;
        public event MessageEventDelegate StatusEvent;

        Maple.PubSubClient.Client pubsubClient;

        /// <summary>
        /// Start monitoring and retrieving ftp files as required
        /// </summary>
        public void Start()
        {
            timer1.Interval = 500; // initial startup time
            timer1.Elapsed += new System.Timers.ElapsedEventHandler(timer1_Elapsed);
            timer1.Start();

            // We can check for items to retrieve periodically or by special request via the pubsub
            pubsubClient = new Maple.PubSubClient.Client();
            pubsubClient.statusChange += new Maple.PubSubClient.StateChangeDelegate(pubsubClient_statusChange);
            pubsubClient.tick += new EventHandler(pubsubClient_tick);
            pubsubClient.connect(Settings.Default.PubsubServer);

        }

        void pubsubClient_statusChange(Maple.PubSubClient.eStatus status, string feedback)
        {
            RaiseMessageEvent(string.Format("{0} Pubsub status = {1}", Settings.Default.PubsubServer, status.ToString()));

            if (status == eStatus.eConnected)
            {
                // Force key to exist
                pubsubClient.publish("\\ftpfetch\\", "Starting", eCallType.eSynchronous);

                pubsubClient.subscribe("\\ftpfetch\\", 5000);
            }
            else if (status == eStatus.eSessionProblem)
            {
                // Can be caused because we are debugging an app whilst still connected to the pubsub
                pubsubClient.disconnect();
                pubsubClient.connect(Settings.Default.PubsubServer);
            }
        }

        void pubsubClient_tick(object sender, EventArgs e)
        {
            // A request from the pubsub
            string key = string.Empty;
            object value = null;
            long id = -1;
            while (pubsubClient.tickQueueCount() > 0)
            {
                try
                {
                    pubsubClient.getTick(out key, out value, out id);
                    RaiseMessageEvent(string.Format("Received PubSub message [{0}] [{1}]", key, value));

                    switch (key)
                    {
                        case "\\ftpfetch\\":
                            string[] values = value.ToString().Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                            int ftpID = int.Parse(values[0]);
                            DateTime fileDate = DateTime.Parse(values[1]);
                            string filename = values[2].ToString();

                            // Set data in db to show that we require a fetch for this date
                            Database.SetItemForRetrieval(ftpID, fileDate, filename);

                            // Wait for any current activity to subside
                            while (timer1.Enabled == false)
                            {
                                Thread.Sleep(500);
                            }

                            // then force a read
                            timeToCheck = DateTime.Now;
                            timer1.Stop();
                            timer1.Interval = 50;
                            timer1.Start();
                            break;
                        case "\\ftpList\\":
                            break;
                    }
                }
                catch (Exception ex)
                {
                    if (ex.Message == "No elements in the TickQueue.")
                    {
                        return;
                    }
                    else
                    {
                        Funcs.ReportProblem(ex);
                    }
                }
            }

        }

        DateTime timeToCheck = DateTime.Now;

        void timer1_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            timer1.Stop();

            // Set the timer to the normal running interval as opposed to the startup interval
            if (timer1.Interval != Settings.Default.TimerInterval)
            {
                timer1.Interval = Settings.Default.TimerInterval;
            }
            if (DateTime.Now >= timeToCheck)
            {
                Action();
            }

            timer1.Start();
        }

        Dictionary<int, FtpDetail> FtpDetails = new Dictionary<int, FtpDetail>();

        public void Action()
        {
            try
            {
#if !DEBUG
                Maple.ApplicationStatus.SetStatus(Application.ProductName, "OK", "", Settings.Default.MinutesBetweenChecks);
#endif

                RaiseMessageEvent("Reading FTP details.");
                RaiseStatusEvent("processing...");
                ReadFtpDetails();

                RaiseMessageEvent(string.Format("Processing {0} FTP items.", FtpDetails.Count));
                foreach (FtpDetail detail in FtpDetails.Values)
                {
                    RaiseMessageEvent(string.Format("[{0}] retrieve items = {1}.", detail.Name, detail.RetrieveItems.Count));
                    foreach (RetrieveItem item in detail.RetrieveItems.Values)
                    {
                        // Fetch this item
                        if (item.ParseOnly)
                        {
                            RaiseMessageEvent(string.Format("Parsing existing item for {0}", detail.Name));
                        }
                        else if (item.FileName.ToLower() == "getfilelist")
                        {
                            RaiseMessageEvent(string.Format("Fetching file list for {0}", detail.Name));
                        }
                        else
                        {
                            RaiseMessageEvent(string.Format("Fetching item for {0}", detail.Name));
                        }
                        item.StartTime = DateTime.Now;

                        string ret = FetchFileAndLoadData(detail, item);

                        double ms = DateTime.Now.Subtract(item.StartTime).TotalMilliseconds;
                        if (item.ParseOnly)
                        {
                            ms = -1; // Don't record parse times, only ftp fetch times
                        }

                        if (ret == "")
                        {
                            Database.UpdateRetrieveItem(item.ID, "Success", "", ms);
                            RaiseMessageEvent("Success.");
                        }
                        else
                        {
                            Database.UpdateRetrieveItem(item.ID, "Failure", ret, ms);
                            RaiseMessageEvent(string.Format("Failure. {0}", ret));
                        }
                    }
                }
                RaiseMessageEvent("Processing complete.");

                // And after we've done the important stuff we have some time to update the Schedules
                UpdateScheduleItems();
            }
            catch (Exception ex)
            {
                RaiseStatusEvent("Error");
                RaiseMessageEvent("Error. " + ex.Message);
                Funcs.ReportProblem(ex);

            }
            finally
            {
                timeToCheck = DateTime.Now.AddMinutes(Settings.Default.MinutesBetweenChecks);
                RaiseStatusEvent(string.Format("Next check at {0}", timeToCheck.ToString("HH:mm:ss")));
            }
        }

        private void UpdateScheduleItems()
        {
            // The updating of the Scheduled items is done in C# so that we have greater flexibility over 
            // the formatting of the required file names.
            List<ScheduledItem> newItems = Database.FetchNewScheduledItems();

            foreach (ScheduledItem item in newItems)
            {
                string fileName = AdjustFTPfilenameFromMask(item.FileNameMask, item.FileDate);

                Database.InsertRetrieveItem(item.DetailID, fileName, item.FileDate, item.FetchTime, false, 1);
            }

        }

        private string AdjustFTPfilenameFromMask(string fileNameMask, DateTime reconciliationDate)
        {
            int start = fileNameMask.IndexOf("[%");
            int end = fileNameMask.IndexOf("%]");
            if (start > -1 && end > -1 && end > start)
            {
                string mask = fileNameMask.Substring(start, end - start + 2);
                string formatString = mask.Substring(2, mask.Length - 4);
                string newValue = reconciliationDate.ToString(formatString);

                fileNameMask = fileNameMask.Replace(mask, newValue);
            }

            return fileNameMask;
        }


        private string FetchFileAndLoadData(FtpDetail detail, RetrieveItem item)
        {
            string ret = "";

            try
            {
                if (item.FileName.ToLower() == "getfilelist")
                {
                    FetchFileList(detail, item);
                }
                else
                {
                    // Retrieve a file
                    string LocalFile;
                    if (item.ParseOnly)
                    {
                        LocalFile = FindFileLocation(detail, item) + item.FileName;
                    }
                    else
                    {
                        LocalFile = FetchRemoteFile(detail, item);
                    }

                    DataTable data = ParseFile(detail, item, LocalFile);

                    Database.SaveFileData(item.ID, data, detail.TargetTable);
                }
            }
            catch (Exception ex)
            {
                ret = ex.Message;
            }

            return ret;
        }

        private void FetchFileList(FtpDetail detail, RetrieveItem item)
        {
            string site, userName, pw;

            Dictionary<string, Maple.FTP.FileDetail> files = new Dictionary<string, Maple.FTP.FileDetail>(); ;

            switch (detail.FptType)
            {
                case FtpDetail.eFtpType.ftp:
                    Database.FetchLoginInfo(detail.FtpLookupValue, out site, out userName, out pw);

                    files = Maple.FTP.FTP.GetFtpFileList(site, detail.Folder, userName, pw);
                    break;
                case FtpDetail.eFtpType.sftp:
                    int portNumberOverride;
                    Database.FetchLoginInfo(detail.FtpLookupValue, out site, out userName, out pw, out portNumberOverride);

                    Maple.FTP.SFTP sftp = new Maple.FTP.SFTP();
                    if (portNumberOverride == -1)
                        files = sftp.GetSFtpFileList(site, detail.Folder, userName, pw);
                    else
                        files = sftp.GetSFtpFileList(site, detail.Folder, userName, pw, portNumberOverride);
                    break;
                case FtpDetail.eFtpType.UnixShare:
                    string[] found = Directory.GetFiles(detail.Folder);

                    foreach (string file in found)
                    {
                        Maple.FTP.FileDetail f = new Maple.FTP.FileDetail();
                        FileInfo fi = new FileInfo(file);

                        f.Name = fi.Name;
                        f.Size = (int)fi.Length;
                        f.Timestamp = fi.LastWriteTime;

                        files.Add(f.Name.ToLower(), f);
                    }
                    break;
                case FtpDetail.eFtpType.Https:
                    Database.FetchLoginInfo(detail.FtpLookupValue, out site, out userName, out pw);

                    string domain = Path.GetDirectoryName(userName);
                    userName = Path.GetFileName(userName);

                    WebClient webClient = new WebClient();
                    webClient.Credentials = new NetworkCredential(userName, pw, domain);

                    //create temp folder first
                    Directory.CreateDirectory(detail.Folder + Path.GetDirectoryName(item.FileName));
                    webClient.DownloadFile(site, detail.Folder + item.FileName);
                    //tempFolder + item.FileName
                    //File.Copy(folder + item.FileName, tempFolder + item.FileName);


                    //files = sftp.GetSFtpFileList(site, detail.Folder, userName, pw);
                    break;
            }

            Database.SaveFileList(item.ID, files);
        }

        private string FindFileLocation(FtpDetail detail, RetrieveItem item)
        {
            string ret = Settings.Default.FileStorageRoot;
            Funcs.EndSlash(ref ret, true);
            ret += detail.Name + "\\";
            ret += item.FileDate.ToString("yyyy\\\\MM\\\\");

            return ret;
        }

        private DataTable ParseFile(FtpDetail detail, RetrieveItem item, string LocalFile)
        {
            string[] columns = detail.ColumnsRequired.ToLower().Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            List<int> columnIndexes = new List<int>();

            string data = File.ReadAllText(LocalFile);
            string[] lines = data.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            bool avoidHeaderRow = false;
            string delim = detail.Delimiter;
            char[] delims = delim.ToCharArray();

            if (detail.ColumnIdentifierType == FtpDetail.eColumnIdentifierType.Name)
            {
                avoidHeaderRow = true;

                // Find out what data is in what column
                string[] headers = lines[0].Split(delims, StringSplitOptions.RemoveEmptyEntries);
                Dictionary<string, int> headIds = new Dictionary<string, int>();
                int index = 0;
                foreach (string header in headers)
                {
                    headIds.Add(header.ToLower(), index++);
                }

                //  Check that the expected headers are present
                foreach (string column in columns)
                {
                    if (!headIds.ContainsKey(column))
                    {
                        throw new Exception(string.Format("Expected header was not found in the incoming file [{0}].", column));
                    }
                    else
                    {
                        // Replace column names with their index
                        columnIndexes.Add(headIds[column]);
                    }
                }
            }
            else
            { // column indexes supplied
                foreach (string column in columns)
                {
                    columnIndexes.Add(int.Parse(column));
                }
            }

            DataTable outPut = new DataTable();

            // Now we have the column indexes, work through the data
            int rowNo = 1;
            foreach (string line in lines)
            {
                if (avoidHeaderRow)
                {
                    avoidHeaderRow = false;
                }
                else
                {
                    if (line.Trim() != "")
                    {
                        //string[] fields = line.Split(delims, StringSplitOptions.RemoveEmptyEntries);
                        string[] fields = line.Split(delims);
                        // Read as many columns as are available

                        DataRow row = new DataRow(rowNo);

                        int colNo = 0;
                        foreach (int col in columnIndexes)
                        {
                            if (col >= fields.Length)
                            {
                                // not enough columns
                                break;
                            }
                            colNo++;
                            row.AddData(colNo, fields[col]);
                        }

                        // Only add rows with columns
                        if (row.ColumnCount > 0)
                        {
                            outPut.AddRow(row);
                            rowNo++;
                        }
                    }
                }
            }

            return outPut;
        }

        private string FetchRemoteFile(FtpDetail detail, RetrieveItem item)
        {
            string uri = "", userName = "", pw = "", folder = "", ftpStore = "", domain = "";

            ftpStore = FindFileLocation(detail, item);

            string ext = Path.GetExtension(item.FileName);
            folder = detail.Folder;

            // Temp area to store files before we move them
            string tempFolder = Path.GetTempPath();

            switch (detail.FptType)
            {
                case FtpDetail.eFtpType.ftp:
                    if (!folder.EndsWith("/")) { folder += "/"; }
                    Database.FetchLoginInfo(detail.FtpLookupValue, out uri, out userName, out pw);
                    uri = "ftp://" + uri + "/" + folder + item.FileName;
                    Maple.FTP.FTP.Download(uri, userName, pw, tempFolder + item.FileName, true);
                    break;

                case FtpDetail.eFtpType.sftp:
                    if (!folder.EndsWith("/")) { folder += "/"; }
                    int portNumberOverride;
                    Database.FetchLoginInfo(detail.FtpLookupValue, out uri, out userName, out pw, out portNumberOverride);
                    Maple.FTP.SFTP ftp = new Maple.FTP.SFTP();
                    string sessionOutput = string.Empty;
                    List<string> files = new List<string>();
                    files.Add(item.FileName);
                    if (portNumberOverride == -1)
                        ftp.SFtpGet(files, uri, detail.Folder, userName, pw, out sessionOutput);
                    else
                        ftp.SFtpGet(files, uri, detail.Folder, userName, pw, out sessionOutput, portNumberOverride);
                    File.Move(Path.GetDirectoryName(Application.ExecutablePath) + "\\" + files[0], tempFolder + files[0]);
                    break;

                case FtpDetail.eFtpType.UnixShare:
                    if (!folder.EndsWith("\\")) { folder += "\\"; }
                    File.Delete(tempFolder + item.FileName);
                    File.Copy(folder + item.FileName, tempFolder + item.FileName);
                    break;

                case FtpDetail.eFtpType.Https:
                    if (!folder.EndsWith("/")) { folder += "/"; }
                    Database.FetchLoginInfo(detail.FtpLookupValue, out uri, out userName, out pw);
                    uri = uri + "/" + item.FileName;

                    domain = Path.GetDirectoryName(userName);
                    userName = Path.GetFileName(userName);

                    WebClient webClient = new WebClient();
                    webClient.Credentials = new NetworkCredential(userName, pw, domain);

                    //create temp folder first
                    Directory.CreateDirectory(tempFolder + Path.GetDirectoryName(item.FileName));
                    webClient.DownloadFile(uri, tempFolder + item.FileName);

                    break;

            }

            string toReturn = "";

            // Check that file is there
            if (File.Exists(tempFolder + item.FileName))
            {
                string newFileName = item.FileName;
                string lastdir = "";
                int number;

                if (detail.PGP)
                {
                    item.FileName = Decryption.PGP(tempFolder, item.FileName);
                }
                if (detail.ZIP)
                {
                    lastdir = Path.GetDirectoryName(item.FileName);
                    if (int.TryParse(lastdir, out number))
                    {
                        //directory includes date
                        Directory.CreateDirectory(ftpStore + lastdir);
                    }
                    newFileName = Decryption.ZIP(tempFolder, item.FileName, domain);
                    ext = Path.GetExtension(newFileName);


                }
                if (detail.DES)
                {
                    item.FileName = Decryption.DES(tempFolder, item.FileName);
                }

                // Move to permanent location
                Directory.CreateDirectory(ftpStore);

                //int count = 1;
                newFileName = Path.GetFileNameWithoutExtension(newFileName);

                string add = "";
                // This part finds a new filename if it already exists
                //while (File.Exists(ftpStore + newFileName + add + ext)) {
                //    add = string.Format(" ({0})", count++);
                //}

                if (lastdir != "")
                {
                    string newFileLocation = lastdir + "/" + newFileName + ext;
                    File.Copy(tempFolder + newFileLocation, ftpStore + newFileLocation, true);
                    toReturn = ftpStore + newFileLocation;
                    File.Delete(tempFolder + newFileLocation);
                }
                else
                {
                    File.Copy(tempFolder + item.FileName, ftpStore + newFileName + add + ext, true);
                    toReturn = ftpStore + newFileName + add + ext;
                    File.Delete(tempFolder + item.FileName);
                }

            }

            return toReturn;
        }

        /// <summary>
        /// Read in the details of the Ftp requirements and inserts/updates the classes in the FtpDetails collection
        /// </summary>
        private void ReadFtpDetails()
        {
            string ret = Database.FetchFtpDetail(ref FtpDetails);

            ret = Database.FetchRetrieveItems(FtpDetails);

        }

        #region Raising Events
        /// <summary>
        /// Use a method to raise the event after checking for subscribers
        /// </summary>
        /// <param name="message"></param>
        private void RaiseMessageEvent(string message)
        {
            // Check to make sure something has subscribed to the delegate
            if (MessageEvent != null)
            {
                MessageEvent(this, new MessageEventArgs(message));
            }
        }

        private void RaiseStatusEvent(string status)
        {
            // Check to make sure something has subscribed to the delegate
            if (StatusEvent != null)
            {
                StatusEvent(this, new MessageEventArgs(status));
            }
        }
        #endregion
    }
}
