using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Admin_process_app {
    public partial class Form1 : Form {
        string[] args;

        Timer timer1;
        string monitorFolder;
        const string COMMAND_FILE = "cmd.ctl";
        const string RESULT_FILE = "cmd.res";
        const string TEMP_FILE = "cmd.t1";

        public Form1(string[] Args) {
            InitializeComponent();

            args = Args;

            if (args.Length > 0) {
                monitorFolder = args[0];
            } else {
                monitorFolder = Directory.GetCurrentDirectory();
            }
            WorkingFolderTextBox.Text = monitorFolder;
            Directory.SetCurrentDirectory(monitorFolder);

            timer1 = new Timer();
            timer1.Interval = 500;
            timer1.Tick += timer1_Tick;
            timer1.Start();

            this.Left = this.Left + 1000;

            this.Visible = false;
#if DEBUG
            this.Visible = true;
#endif
        }

        void timer1_Tick(object sender, EventArgs e) {
            timer1.Enabled = false;
            try {
                timeLabel.Text = DateTime.Now.ToString("HH:mm:ss");

                if (File.Exists(COMMAND_FILE)) {
                    Log("Command found");

                    string content = File.ReadAllText(COMMAND_FILE);
                    File.Delete(COMMAND_FILE);
                    Log(content);

                    if (content.Substring(0,4).ToLower() == "exit") {
                        this.Close();
                        return;
                    }

                    File.WriteAllText("RunThis.cmd", content);

                    Process proc = new Process();
                    proc.StartInfo = new ProcessStartInfo("RunThis.cmd");

                    proc.Start();
                    Log("Process started...");

                    proc.WaitForExit();

                    string res = "";
                    if (proc.ExitCode != 0) {
                        res = "Exit code " + proc.ExitCode.ToString();
                    }
                    File.WriteAllText(TEMP_FILE, res);
                    File.Delete(RESULT_FILE);
                    File.Move(TEMP_FILE, RESULT_FILE);
                    Log("Complete");
                }

            } finally {
                timer1.Enabled = true;
            }
        }

        private void Log(string data) {
            LogTextBox.SelectionStart = LogTextBox.Text.Length;
            LogTextBox.SelectedText = data + "\r\n";
            LogTextBox.SelectionStart = LogTextBox.Text.Length;
        }

        private void Form1_Load(object sender, EventArgs e) {

        }
    }
}
