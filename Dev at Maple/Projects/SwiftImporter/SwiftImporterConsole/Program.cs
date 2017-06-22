using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Maple;
using Maple.Database;
using SwiftImporterConsole.Properties;
using SwiftImporterLib;
using SwiftImporterLib.Control;

namespace SwiftImporterConsole
{
    class Program
    {

        static string applicationName = "";

        static void Main(string[] args)
        {
            Version version = Assembly.GetExecutingAssembly().GetName().Version;
            string ver = "{0}.{1}.{2}".Args(version.Major, version.Minor, version.Build);

            applicationName = Path.GetFileNameWithoutExtension(Environment.GetCommandLineArgs()[0]);
            NLogger.Instance.Info("{0} starting @ {1}.  Version {2}".Args(applicationName, DateTime.Now.ToString("yyyyMMdd HH:mm:ss"), ver), true);
            NLogger.Instance.Info("Config = {0}".Args(Settings.Default.ConfigName), true, ConsoleColor.Yellow);
            NLogger.Instance.Info("-------------------------------------------", true);

            try
            {
                SwiftImportManager im;
                if (args.Length == 0)
                {
                    im = new SwiftImportManager(new DataLayer(new DatabaseController(Settings.Default.ConnectionString)), Settings.Default.PollFolders.Cast<string>());

                }
                else
                {
                    im = new SwiftImportManager(new DataLayer(new DatabaseController(Settings.Default.ConnectionString)), args);
                }
                new AutoResetEvent(false).WaitOne();

            }
            catch (Exception ex)
            {
                NLogger.Instance.Info("", true);
                NLogger.Instance.Info("Error: {0}".Args(ex.Message), true, ConsoleColor.Red);
                NLogger.Instance.Info("", true);
                WriteSyntax();
            }

            NLogger.Instance.Info("{0} terminating @ {1}".Args(applicationName, DateTime.Now.ToString("yyyyMMdd HH:mm:ss")), true);
        }


        private static void WriteSyntax()
        {
            Console.WriteLine("Syntax is:");
            Console.WriteLine("");
            Console.WriteLine("{0} FW \"C:\\temp\\MyFileWatchFolder\\*.out\"".Args(applicationName));
            Console.WriteLine("");
            Console.WriteLine("  \"C:\\temp\\MyFileWatchFolder\\\" being the folder to watch.");
            Console.WriteLine("  *.out                        being the file mask to watch for.");
            Console.WriteLine("");
            Console.WriteLine("  FW = File Watcher");
            Console.WriteLine("");
        }

    }

}
