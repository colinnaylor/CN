using System.ComponentModel;
using System.Configuration.Install;
using System.ServiceProcess;

namespace SwiftImporterService
{
    [RunInstaller(true)]
    public class WindowsServiceInstaller : Installer
    {
        /// <summary>
        /// Public Constructor for WindowsServiceInstaller.
        /// - Put all of your Initialization code here.
        /// </summary>
        public WindowsServiceInstaller()
        {
            ServiceProcessInstaller serviceProcessInstaller =
                               new ServiceProcessInstaller();
            ServiceInstaller serviceInstaller = new ServiceInstaller();

            //# Service Account Information
            serviceProcessInstaller.Account = ServiceAccount.User;  //ServiceAccount.LocalSystem;
            serviceProcessInstaller.Username = "mpuk\\colin";
            serviceProcessInstaller.Password = null;

            //# Service Information
            serviceInstaller.DisplayName = "Maple Swift Importer";
            serviceInstaller.Description = "Monitors folder locations for Swift files and imports them when they are created.";
            serviceInstaller.StartType = ServiceStartMode.Automatic;

            serviceInstaller.ServiceName = "Swift Importer";

            this.Installers.Add(serviceProcessInstaller);
            this.Installers.Add(serviceInstaller);
        }
    }
}
