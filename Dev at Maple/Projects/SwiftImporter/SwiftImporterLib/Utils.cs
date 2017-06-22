using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using Maple;

namespace SwiftImporterLib
{
    public class Utils
    {

        /// <summary> 
        /// Waits until a file can be opened with write permission 
        /// </summary> 
        public static void WaitReady(string fileName)
        {
            DateTime waitStartTime = DateTime.Now;
            while (true)
            {
                try
                {
                    using (Stream stream = File.Open(fileName, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
                    {
                        if (stream != null)
                        {
                            Trace.WriteLine(string.Format("Output file {0} ready.", fileName));
                            break;
                        }
                    }
                }
                catch (FileNotFoundException ex)
                {
                    NLogger.Instance.Info("Output file {0} not yet ready ({1})", fileName, ex.Message);
                }
                catch (IOException ex)
                {
                    NLogger.Instance.Info("Output file {0} not yet ready ({1})", fileName, ex.Message);
                }
                catch (Exception e)
                {
                    NLogger.Instance.Error(e);
                    throw;
                }
                Thread.Sleep(500);

                if (waitStartTime < DateTime.Now.AddMinutes(-15))
                    throw new Exception("File was not accessible within the time allocated.  Check whats up with the file:  " + fileName);
            }
        }

        public IObservable<FileSystemEventArgs> GetFileEventStream(string folderName, string pattern)
        {
            //create FSW
            var fileSystemWatcher = new FileSystemWatcher(folderName, pattern)
            {
                EnableRaisingEvents = true,
                IncludeSubdirectories = false
            };

            //create the streams 
            var createdFiles = Observable.FromEventPattern<FileSystemEventHandler, FileSystemEventArgs>(
                h => fileSystemWatcher.Created += h,
                h => fileSystemWatcher.Created -= h)
                .Select(x => x.EventArgs);

            var changedFiles = Observable.FromEventPattern<FileSystemEventHandler, FileSystemEventArgs>(
                h => fileSystemWatcher.Changed += h,
                h => fileSystemWatcher.Changed -= h)
                .Select(x => x.EventArgs);

            var renamedFiles = Observable.FromEventPattern<RenamedEventHandler, FileSystemEventArgs>(
                h => fileSystemWatcher.Renamed += h,
                h => fileSystemWatcher.Renamed -= h)
                .Select(x => x.EventArgs);

            NLogger.Instance.Info("Stream created {0} {1}", folderName, pattern);
            return changedFiles.Concat(createdFiles).Concat(renamedFiles);

        }

        public IObservable<IList<FileSystemEventArgs>> GetBufferedFileEventStream(IEnumerable<KeyValuePair<string, string>> folderAndMask)
        {
            IObservable<FileSystemEventArgs> stream = Observable.Empty<FileSystemEventArgs>();
            ;
            folderAndMask.ToList().ForEach(x =>
            {
                stream = stream.Merge(GetFileEventStream(x.Key, x.Value));
            });
            return stream.Buffer(TimeSpan.FromSeconds(3));
        }

        public class FileSystemEventArgNameComparer : IEqualityComparer<FileSystemEventArgs>
        {
            public bool Equals(FileSystemEventArgs x, FileSystemEventArgs y)
            {
                return x.FullPath == y.FullPath;

            }

            public int GetHashCode(FileSystemEventArgs obj)
            {
                return obj.FullPath.GetHashCode();

            }
        }

    }
}
