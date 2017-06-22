using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using System.Threading;
using Maple;
using Maple.Database;
using SwiftImporterLib;
using SwiftImporterLib.Control;
using SwiftImporterService.Properties;

namespace SwiftImporterService
{
    partial class SwiftImportService : ServiceBase
    {
        public SwiftImportService()
        {
            InitializeComponent();

            ServiceName = "Swift Importer";
            EventLog.Log = "Application";

            // These Flags set whether or not to handle that specific
            //  type of event. Set to true if you need it, false otherwise.
            CanHandlePowerEvent = true;
            CanHandleSessionChangeEvent = true;
            CanPauseAndContinue = true;
            CanShutdown = true;
            CanStop = true;
        }

        string applicationName = "";
        SwiftImportManager im;

        private void StartWork()
        {
            Action action = StartImportManager;
            action.BeginInvoke(null, null);
            //StartImportManager();
        }
        private void StopWork()
        {
            im = null;
        }

        private void StartImportManager()
        {
            im = new SwiftImportManager(new DataLayer(new DatabaseController(Settings.Default.ConnectionString)), Settings.Default.PollFolders.Cast<string>());
            var reset = new AutoResetEvent(false);
            reset.WaitOne();
        }

        protected override void OnStart(string[] args)
        {
            base.OnStart(args);
            NLogger.Instance.Info("Service has started.");
            Version version = Assembly.GetExecutingAssembly().GetName().Version;
            string ver = "{0}.{1}.{2}".Args(version.Major, version.Minor, version.Build);

            applicationName = Path.GetFileNameWithoutExtension(Environment.GetCommandLineArgs()[0]);
            NLogger.Instance.Info("{0} starting @ {1}.  Version {2}".Args(applicationName, DateTime.Now.ToString("yyyyMMdd HH:mm:ss"), ver), true);
            NLogger.Instance.Info("Config = {0}".Args(Settings.Default.ConfigName), true, ConsoleColor.Yellow);
            NLogger.Instance.Info("-------------------------------------------", true);

            StartWork();
        }

        protected override void OnStop()
        {
            base.OnStop();
            StopWork();

            NLogger.Instance.Info("Service has stopped.");
            NLogger.Instance.Info("{0} terminating @ {1}".Args(applicationName, DateTime.Now.ToString("yyyyMMdd HH:mm:ss")), true);
        }

        protected override void OnPause()
        {
            base.OnPause();
            base.OnStop();
            StopWork();
            NLogger.Instance.Info("Service has paused.");

        }

        protected override void OnContinue()
        {
            base.OnContinue();
            StartWork();
            NLogger.Instance.Info("Service has re-started.");
        }

    }
}
