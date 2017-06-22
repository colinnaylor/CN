using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Timers;
using Maple;
using SwiftImporterLib.Model;

namespace SwiftImporterLib.Control
{
    public class SwiftImportManager
    {

        Dictionary<string, bool> filesImported = new Dictionary<string, bool>();

        private DataLayer _dataLayer;

        private IObservable<IList<FileSystemEventArgs>> fileEventStream;

        private Timer mainTimer = new Timer();

        /// <summary>
        /// Hold the list of created files waiting to be processed.
        /// </summary>
        private ConcurrentQueue<string> fileQueue = new ConcurrentQueue<string>();

        public void Init(IEnumerable<KeyValuePair<string, string>> foldersAndMasks)
        {
            NLogger.Instance.Info("Reading ini file information");
            NLogger.Instance.Info("Init file event stream");

            //set up event stream
            fileEventStream = new Utils().GetBufferedFileEventStream(foldersAndMasks);

            //subscribe to it (buffer and distinct). just put it on the queue.
            fileEventStream.Subscribe(
                listArgs =>
                {
                    if (listArgs.Count == 0) return;

                    listArgs.Distinct(new Utils.FileSystemEventArgNameComparer()).ToList().ForEach(
                        listArg =>
                        {
                            NLogger.Instance.Info("Received file system event {0} ", listArg.FullPath);
                            fileQueue.Enqueue(listArg.FullPath);
                        });
                },
                args =>
                {
                    NLogger.Instance.Info("Stopped listening for new files.");
                },
                () =>
                {
                    NLogger.Instance.Info("Stream Error.");
                });

            mainTimer.Elapsed += mainTimerElapsed;
            mainTimer.Interval = 3000;
            mainTimer.Enabled = true;

        }

        private static object mainLock = new object();

        private void mainTimerElapsed(object a, ElapsedEventArgs arg)
        {
            lock (mainLock)
            {
                while (fileQueue.Count > 0)
                {
                    string fileName;
                    if (fileQueue.TryDequeue(out fileName))
                        ImportFile(fileName);
                }
            }
        }

        public SwiftImportManager(DataLayer dataLayer, string[] args)
        {
            _dataLayer = dataLayer;

            // Args should be in name value pairs
            for (int i = 0; i < args.Length; i += 2)
            {
                string name = args[i].ToLower();
                string value = args[i + 1];
                string mask = "*.out";
                if (value.IndexOf("*") > -1)
                {
                    mask = Path.GetFileName(value);
                    value = Path.GetDirectoryName(value);
                }
                switch (name)
                {
                    case "fw":
                        NLogger.Instance.Info("Init file event stream");
                        Init(new List<KeyValuePair<string, string>>() { new KeyValuePair<string, string>(value, mask) });
                        break;
                    default:
                        throw new Exception("Argument name of [{0}] is not supported.".Args(name));
                }
            }
        }

        public SwiftImportManager(DataLayer dataLayer, IEnumerable<string> pollFolders)
        {
            _dataLayer = dataLayer;
            Init(pollFolders.ToList().Select(x => new KeyValuePair<string, string>(Path.GetDirectoryName(x), Path.GetFileName(x))));
        }

        public void ImportFile(string filePathName)
        {
            NLogger.Instance.Info("Importing file {0}", filePathName);

            //if not in dictionary then add it and continue, else ignore and out
            if (filesImported.ContainsKey(filePathName))
                NLogger.Instance.Info("File already exists in service recent memory. (Restart service to flush cache: {0}  ", filePathName);
            else
            {
                filesImported.Add(filePathName, false);

                string fileName = Path.GetFileName(filePathName);
                NLogger.Instance.Info("New file found: {0}  ".Args(fileName));

                try
                {
                    Utils.WaitReady(filePathName); // wait until the file is ready

                    // mark file as being worked on
                    string workingFile = filePathName + ".working";
                    File.Move(filePathName, workingFile);

                    SwiftFile swiftFile = new SwiftFile(workingFile);

                    _dataLayer.SaveSwiftMessages(swiftFile); // and finally distribute the messages

                    // Rename file to .archive
                    string archiveFile = workingFile.Replace(".working", ".archive");
                    File.Move(workingFile, archiveFile);

                    //  NLogger.Instance.Info("  { 0}.".Args(result));
                    filesImported[filePathName] = true;
                }
                catch (Exception ex)
                {
                    NLogger.Instance.Error(ex);
                    throw;
                }
            }

        }

    }
}
