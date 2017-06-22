using System.Collections.Generic;
using System.IO;
using System.Linq;
using Maple.Database;
using NUnit.Framework;
using SwiftImporterLib;
using SwiftImporterLib.Control;
using SwiftImporterLib.Model;
using SwiftImporterService.Properties;

namespace SwiftImporterServiceTests
{
    [TestFixture]
    public class MainTester
    {
        [Test]
        public void SavingMessages()
        {
            //setup

            if (!Directory.Exists(@"C:\temp\Swift_Files_Prod\")) Directory.CreateDirectory(@"C:\temp\Swift_Files_Prod\");
            var mgr = new SwiftImportManager(new DataLayer(new DatabaseController("Data Source=devMINKY;Initial Catalog=Reconciliation;Integrated Security=True")), new List<string>() { @"C:\temp\Swift_Files_Prod\*.out" });

            //drop a file into the folder 

            //File.Copy("Testfiles\\M300_20160114.114822.inc", @"C:\temp\Swift_Files_Prod\Testfiles\M300_20160114.114822.inc", true);
            File.Delete(@"Testfiles\M300_20160114.114822.inc.working");
            File.Delete(@"Testfiles\M300_20160114.114822.inc.archive");

            File.Delete(@"Testfiles\MT535.00591822.out.working");
            File.Delete(@"Testfiles\MT535.00591822.out.archive");

            File.Delete(@"Testfiles\MT940.00322918.out.working");
            File.Delete(@"Testfiles\MT940.00322918.out.archive");

            File.Delete(@"Testfiles\MT950.00523025.out.working");
            File.Delete(@"Testfiles\MT950.00523025.out.archive");

            //explicitly call the import
            mgr.ImportFile("Testfiles\\M300_20160114.114822.inc");

            mgr.ImportFile("Testfiles\\MT535.00591822.out");
            mgr.ImportFile("Testfiles\\MT940.00322918.out");
            mgr.ImportFile("Testfiles\\MT950.00523025.out");


        }

        [Test]
        public void DeserialiseDifferentSwiftTypes()
        {
            SwiftFile swiftFile = new SwiftFile("Testfiles\\MT535.00591822.out");
            foreach (SwiftMessage message in swiftFile.Messages)
                Assert.AreEqual(MessageType.MT535, message.Type);

            swiftFile = new SwiftFile("Testfiles\\MT940.00322918.out");
            foreach (SwiftMessage message in swiftFile.Messages)
                Assert.AreEqual(MessageType.MT940, message.Type);

            swiftFile = new SwiftFile("Testfiles\\MT950.00523025.out");
            foreach (SwiftMessage message in swiftFile.Messages)
                Assert.AreEqual(MessageType.MT950, message.Type);

        }

        [Test]
        public void PollFolders()
        {
            var t = Settings.Default.PollFolders.Cast<string>().ToList().Select(x => new KeyValuePair<string, string>(Path.GetDirectoryName(x), Path.GetFileName(x)));

            var mgr = new SwiftImportManager(new DataLayer(new DatabaseController(Settings.Default.ConnectionString)), Settings.Default.PollFolders.Cast<string>());

            //new AutoResetEvent(false).WaitOne();

        }
    }
}
