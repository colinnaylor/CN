﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Maple;

namespace SystemsTestTool.Model {
    class CommandLine {
        const string COMMAND_FILE = "cmd.ctl";
        const string RESULT_FILE = "cmd.res";
        const string TEMP_FILE = "cmd.t0";


        public string RunCommandLine(ref string line, bool Admin, bool debug) {
            string ret = "";

            // Take off any quotes that wrap the command
            if (line.StartsWith("\"")) line = line.Substring(1);
            if (line.EndsWith("\"")) line = line.Substring(0,line.Length -1);
            // Replace doubled quotes with single ones
            line = line.Replace("\"\"", "\"");
            // Remove any carriage returns at the start of the text
            while (line.Substring(0, 1).IndexOfAny(new char[] { '\n', '\r' }) > -1) {
                line = line.Substring(1);
            }

            string action = "";
            if (line.Substring(0, 8).ToLower() == "replace ") { action = "replace"; }
            if (line.Substring(0, 10).ToLower() == "applaunch ") { action = "applaunch"; }

            switch(action){
                case "replace":
                    string[] args = line.Split(new char[] {' '},StringSplitOptions.RemoveEmptyEntries);
                    args[3] = args[3].Replace("\r\n", "");

                    ret = ReplaceContent(args[1], args[2], args[3]);
                    break;
                case "applaunch":
                    // Pass line to a new method which will handle the content on a line by line basis
                    ret = AppLaunch(line);

                    break;
                default:
                    Directory.SetCurrentDirectory(Program.FILE_PATH);

                    // To test for whether we are in Admin mode
                    //line = "whoami /groups /FO list | findstr /c:\"Mandatory Label\"\r\nPause";

                    if (debug) { line += "\r\nPause"; }

                    if (Admin) {
                        // Pass control to the Admin side of this system
                        WriteAdminCommand(line);

                        DateTime timeout = DateTime.Now.AddSeconds(60);
                        bool timedOut = false;
                        while (timedOut == false) {
                            if (DateTime.Now.CompareTo(timeout) != -1) { timedOut = true; }

                            string res = "";
                            if (GetAdminCommandResult(ref res)) {
                                if (res != "") {
                                    ret = res;
                                }
                                break;
                            }
                        }
                    } else {
                        // Save command to a batch file
                        File.WriteAllText("RunThis.cmd", line);

                        Process proc = new Process();
                        proc.StartInfo = new ProcessStartInfo("RunThis.cmd");
                        proc.StartInfo.UseShellExecute = false;

                        //proc.StartInfo.RedirectStandardOutput = true;

                        proc.Start();
                        //string output = proc.StandardOutput.ReadToEnd();
                        proc.WaitForExit();

                        if (proc.ExitCode != 0) {
                            //ret = string.Format("Exit code {0}. {1}", proc.ExitCode.ToString(), output);
                            ret = string.Format("Exit code {0}.", proc.ExitCode.ToString());
                        }
                    }
                    break;
            }

            return ret;
        }

        private string AppLaunch(string line) {
            string ret = "";
            string[] lines = line.Split(new Char[] {'\r','\n'},StringSplitOptions.RemoveEmptyEntries);

            string app = lines[0].Substring(10);
            if(!File.Exists(app)){
                ret = "File did not exist [{0}]".Args(app);
            }else{
                Process proc = new Process();
                proc.StartInfo = new ProcessStartInfo(app);
                proc.StartInfo.UseShellExecute = false;

                try{
                    proc.Start();

                    for(int i = 1; i< lines.Length; i++){
                        string cmd = lines[i].ToLower();

                        if(cmd.StartsWith("wait")){
                            double time = double.Parse(cmd.Substring(4));
                            int seconds = (int)(time * 1000);
                            Thread.Sleep(seconds);
                        }
                        if(cmd.StartsWith("findwindow")){
                            string searchFor = lines[i].Substring(11).ToLower();
                            if(searchFor.StartsWith("\"")) searchFor = searchFor.Substring(1);
                            if(searchFor.EndsWith("\"")) searchFor = searchFor.Remove(searchFor.Length-1);

                            bool found = false;
                            System.Diagnostics.Process[] processes = System.Diagnostics.Process.GetProcesses();
                            foreach(Process pr in processes){
                                string windowName = pr.MainWindowTitle.ToLower();

                                if(windowName.Contains(searchFor)){
                                    found = true;
                                    break;
                                }
                            }
                            if(!found){
                                ret = "A window with the text \"{0}\" was not found.".Args(searchFor);
                            }

                        }

                    }
                }catch(Exception ex){
                    ret = "Error: " + ex.Message;
                }
                
            }
            return ret;
        }

        internal static void WriteAdminCommand(string data) {
            Directory.SetCurrentDirectory(Program.FILE_PATH);

            File.WriteAllText(TEMP_FILE, data);
            File.Delete(COMMAND_FILE);
            File.Move(TEMP_FILE, COMMAND_FILE);
        }

        private bool GetAdminCommandResult(ref string Res) {
            Directory.SetCurrentDirectory(Program.FILE_PATH);

            bool ret = false;
            if (File.Exists(RESULT_FILE)) {
                Thread.Sleep(100);
                Res = File.ReadAllText(RESULT_FILE);
                ret = true;
                File.Delete(RESULT_FILE);
            }
            return ret;
        }

        private string ReplaceContent(string file, string find, string replace) {
            string ret = "";
            int count = 0;

            try {
                // Do work here (or call a method in another class)
                Directory.SetCurrentDirectory(Program.FILE_PATH);

                string data = File.ReadAllText(file);

                int pos = 0;
                while (pos > -1) {
                    pos = data.IndexOf(find, pos, StringComparison.OrdinalIgnoreCase);
                    if (pos > -1) {
                        data = data.Substring(0, pos) + replace + data.Substring(pos + find.Length);
                        count++;
                    }
                }

                File.WriteAllText(file, data);
            }catch(Exception ex){
                ret = ex.Message;
            }

            return ret;
        }

        internal static void ClearFiles() {
            File.Delete(Program.FILE_PATH + "\\" + COMMAND_FILE);
        }
    }
}
