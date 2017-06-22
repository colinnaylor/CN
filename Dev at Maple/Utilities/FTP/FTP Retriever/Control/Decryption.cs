using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.Packaging;
using System.IO.Compression;
using System.Diagnostics;
using FTP_Retriever.Properties;

namespace FTP_Retriever {
    class Decryption {
        internal static string PGP(string folder, string fileName) {
            string ret = fileName;

            return ret;
        }

        internal static string ZIP(string folder, string fileName, string domain = "") {

            string fileExtension = "";

            if (fileName.Contains("/"))
            {
                fileExtension = Path.GetDirectoryName(fileName);
            }

            string file = "\"" + folder + fileName + "\"";
            string cmd;
            string pw = "";

            if (domain != "")
            {
                pw = Settings.Default.LSESTAGING;    
            }
            
            if (pw == "")
            {
                cmd = string.Concat("winrar x ", file, " ", "\"", folder, fileExtension, "/", "\"");
            }
            else
            {
                cmd = string.Concat("winrar x -p", pw, " ", file, " ", "\"" , folder, fileExtension, "/", "\"");
            }

            ProcessStartInfo processInfo;
            Process process;

            processInfo = new ProcessStartInfo("cmd.exe", "/c" + cmd);
            processInfo.CreateNoWindow = true;
            processInfo.UseShellExecute = false;
            processInfo.RedirectStandardError = true;
            processInfo.RedirectStandardOutput = true;

            process = Process.Start(processInfo);
            process.WaitForExit(10000);  

            File.Delete(folder + fileName);
            string [] unzip = Directory.GetFiles(folder + fileExtension);
            string ret = unzip[0];  //assuming one file in directory
            return ret;
        }

        internal static string DES(string folder, string fileName) {
            string ret = fileName;


            return ret;
        }
    }
}
