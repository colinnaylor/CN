using System.ServiceProcess;

namespace SwiftImporterService
{
    class Program
    {

        static void Main(string[] args)
        {
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new SwiftImportService()
            };
            ServiceBase.Run(ServicesToRun);
        }
    }

}
