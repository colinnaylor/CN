using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SqlCompareTool {
    class Program {
        static void Main(string[] args) {
            // Must have one arg
            if (args.Length == 0) {
                Syntax();
                return;
            }

            string workingFile = args[0];
            if (workingFile.ToLower() == "oldnew") {
                // This is a specific case for comparing old.sql and new.sql in the C:\temp folder
                // Shell out to compare
                string cmdFile = @"C:\Program Files (x86)\Compare It!\wincmp3.exe";
                string param = "\"C:\\Temp\\old.sql\" \"C:\\Temp\\new.sql\"";
                Process pro = new Process();
                pro.StartInfo = new ProcessStartInfo(cmdFile, param);
                pro.StartInfo.WorkingDirectory = Path.GetDirectoryName(cmdFile);
                pro.Start();

                while (!pro.HasExited) {
                    Thread.Sleep(1000);
                }
                if (pro.ExitCode == 0) {
                    Console.WriteLine("Done.");
                } else {
                    Console.WriteLine("Exit code = " + pro.ExitCode.ToString());
                }

            } else {
                string originalFile = @"C:\Temp\" + Path.GetFileName(args[0]) + ".Orig";

                if (args.Length > 1 && args[1].ToLower() == "copy") {
                    // Copy file over Original to reset 
                    try {
                        File.Copy(workingFile, originalFile, true);

                        Console.WriteLine(workingFile);
                        Console.WriteLine(" has been copied to ");
                        Console.WriteLine(originalFile);
                    } catch (Exception ex) {
                        Console.WriteLine("Error: " + ex.Message);
                    }
                } else {
                    // Shell out to compare
                    string cmdFile = @"C:\Program Files (x86)\Compare It!\wincmp3.exe";
                    string param = "\"" + originalFile + "\" \"" + workingFile + "\"";
                    Process pro = new Process();
                    pro.StartInfo = new ProcessStartInfo(cmdFile, param);
                    pro.StartInfo.WorkingDirectory = Path.GetDirectoryName(cmdFile);
                    pro.Start();

                    while (!pro.HasExited) {
                        Thread.Sleep(1000);
                    }
                    if (pro.ExitCode == 0) {
                        Console.WriteLine("Done.");
                    } else {
                        Console.WriteLine("Exit code = " + pro.ExitCode.ToString());
                    }

                }
            }
        }

        static void Syntax() {
            Console.WriteLine("Syntax:");
            Console.WriteLine(Environment.GetCommandLineArgs()[0] + " [path of working file]");
            Console.WriteLine("");
        }
        
    }
}
