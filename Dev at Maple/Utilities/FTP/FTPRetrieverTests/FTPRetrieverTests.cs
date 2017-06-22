using System.IO;
using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using FTP_Retriever;
using NUnit.Framework;
using System.Net.FtpClient;

namespace FTPRetrieverTests
{
    [TestFixture]

    public class FTPRetrieverTests
    {
        public static string AssemblyDirectory
        {
            get
            {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }

        [Test]
        public void GetFileFromBarclaysOldSiteUsingFTP()
        {
            //   string ftpSite; string userName; string pw; int portNumber;

            //Database.FetchLoginInfo("Barclays", out ftpSite, out userName, out pw, out portNumber);

            //delete files from local first.

            var file1 = "mapleb_PM28C5-1_S_287054_20150209.dat"; var file2 = "mapleb_PM28C5-1_S_287054_20150210.dat";
            File.Delete(file1); File.Delete(file2);

            Console.WriteLine(Path.Combine(AssemblyDirectory, file2));

            Maple.FTP.FTP.Download(
                string.Format("ftp://ftp.efg.barcap.com/in/{0}", file1),
                "maplecft",
                "mmaboss14",
            Path.Combine(AssemblyDirectory, file1), true);

            Maple.FTP.FTP.Download(
                string.Format("ftp://ftp.efg.barcap.com/in/{0}", file2),
                "maplecft",
                "mmaboss14",
                Path.Combine(AssemblyDirectory, file2), true);

            Assert.IsTrue(File.Exists(file1)); Assert.IsTrue(File.Exists(file2));
        }

        [Test]
        public void GetFileFromBarclaysNewSiteUsingFTP()
        {
            var file1 = "mapleb_PM28C5-1_S_287054_20150209.dat"; var file2 = "mapleb_PM28C5-1_S_287054_20150210.dat";
            File.Delete(file1); File.Delete(file2);

            Maple.FTP.FTP.Download(
                string.Format("ftp://ftp.prime.barcap.com/in/{0}", file1),
                "maplecft",
                "mmaboss14",
             Path.Combine(AssemblyDirectory, file1), true, true);  //need to set to passive mode for new barclays server - prob firewall rules setup differently.
            Maple.FTP.FTP.Download(
                string.Format("ftp://ftp.prime.barcap.com/in/{0}", file2),
                "maplecft",
                "mmaboss14",
              Path.Combine(AssemblyDirectory, file2), true, true);  //need to set to passive mode for new barclays server - prob firewall rules setup differently.

            Assert.IsTrue(File.Exists(file1)); Assert.IsTrue(File.Exists(file2));
        }
        [Test]
        public void GetFileFromBarclaysNewSiteUsingSFTP()
        {

            var file1 = "mapleb_PM28C5-1_S_287054_20150209.dat"; var file2 = "mapleb_PM28C5-1_S_287054_20150210.dat";
            File.Delete(file1); File.Delete(file2);

            string output;
            Maple.FTP.SFTP sftp = new Maple.FTP.SFTP();

            sftp.SFtpGet(new List<string>() { file1, file2 },
            "ftp.prime.barcap.com",
            "in", "maplecft", "mmaboss14", out output, 2222);

            Assert.IsTrue(File.Exists(file1)); Assert.IsTrue(File.Exists(file2));

        }

        /// <summary>
        /// http://www.schiffhauer.com/downloading-a-file-from-ftp-with-system-net-ftpclient/
        /// </summary>
        [Test]
        public void GetFileFromBarclaysNewSiteUsingFTPUsingFTPClient()
        {
            var file1 = "mapleb_PM28C5-1_S_287054_20150209.dat"; var file2 = "mapleb_PM28C5-1_S_287054_20150210.dat";
            File.Delete(file1); File.Delete(file2);


            using (var ftpClient = new FtpClient())
            {
                ftpClient.Host = "ftp.prime.barcap.com";
                ftpClient.Credentials = new NetworkCredential("maplecft", "mmaboss14");
                var path = "/in/";
                var destinationDirectory = AssemblyDirectory;

                ftpClient.Connect();

                // List all files with a .txt extension
                foreach (var ftpListItem in
                    ftpClient.GetListing(path, FtpListOption.Modify | FtpListOption.Size)
                        .Where(ftpListItem => Path.GetFileName(ftpListItem.Name).Equals(file1) || Path.GetFileName(ftpListItem.Name).Equals(file2)))
                {
                    var destinationPath = string.Format(@"{0}\{1}", destinationDirectory, ftpListItem.Name);

                    using (var ftpStream = ftpClient.OpenRead(ftpListItem.FullName))
                    using (var fileStream = File.Create(destinationPath, (int)ftpStream.Length))
                    {
                        var buffer = new byte[8 * 1024];
                        int count;
                        while ((count = ftpStream.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            fileStream.Write(buffer, 0, count);
                        }

                        // In this example, we're deleting the file after downloading it.
                        //ftpClient.DeleteFile(ftpListItem.FullName);
                    }
                }
            }

            Assert.IsTrue(File.Exists(file1)); Assert.IsTrue(File.Exists(file2));



        }

    }

}
