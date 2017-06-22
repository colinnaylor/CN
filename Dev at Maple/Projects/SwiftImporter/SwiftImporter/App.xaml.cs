using System;
using System.Windows;
using System.Windows.Threading;
using Maple;

namespace SwiftImporterUI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            DispatcherUnhandledException += App_DispatcherUnhandledException;
        }

        void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            // Set a message to notify that we are shutting down
            Exception exception = new Exception("Unhandled exception occured, application exiting.", e.Exception);

            Notifier.NotifyDestination dest =

#if DEBUG
 Notifier.NotifyDestination.MessageBox;
#else
            Notifier.NotifyDestination.Email | Notifier.NotifyDestination.File;
#endif

            // Notifying exceptions via email also stores to the db so no need to specify that here
            Notifier.Notify(dest, Notifier.SeverityLevel.Fatal, exception);

            // Return exit code  
            this.Shutdown(-1);

            // Prevent .net framework's default unhandled exception processing 
            e.Handled = true;
        }
    }
}
