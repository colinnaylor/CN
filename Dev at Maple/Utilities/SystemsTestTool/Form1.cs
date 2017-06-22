using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SystemsTestTool.Model;
using System.Diagnostics;
using System.Security.Principal;
using System.Security.Permissions;
using Maple;
using System.Threading;

namespace SystemsTestTool {
    public partial class Form1 : Form {
        string[] args;
        //string adminApp;

        public Form1(string[] Args) {
            InitializeComponent();

            args = Args;
        }

        const string SCRIPT_FILE = "LiveScript.txt";
        const string SCRIPT_FILE_DEV = "DevScript.txt";
        const string SCRIPT_FILE_DR = "DrScript.txt";

        private string GetScriptFileFullPath()
        {
            string ret = Path.GetDirectoryName(Application.ExecutablePath) + "\\Files\\";

            if (LiveTestRadiobutton.Checked)
            {
                ret += SCRIPT_FILE;
            }else if (DevTestRadiobutton.Checked)
            {
                ret += SCRIPT_FILE_DEV;
            }else if (DrTestRadiobutton.Checked)
            {
                ret += SCRIPT_FILE_DR;
            }
            return ret;
        }

        private void Form1_Load(object sender, EventArgs e) {
            string ver = Application.ProductVersion.Substring(0, Application.ProductVersion.LastIndexOf("."));
            this.Text = Application.ProductName + " v" + ver;

            try {
                DisplayInitialText();

                // Check that the script file is there
                //string scriptFile = DevTestOnlyCheckBox.Checked ? Program.FILE_PATH + "Files\\" + SCRIPT_FILE_DEV : Program.FILE_PATH + "Files\\" + SCRIPT_FILE;
                //string scriptFile = DevTestOnlyCheckBox.Checked ? Path.GetDirectoryName(Application.ExecutablePath) + "\\Files\\" + SCRIPT_FILE_DEV : Path.GetDirectoryName(Application.ExecutablePath) + "\\Files\\" + SCRIPT_FILE;
                string scriptFile = GetScriptFileFullPath();
                if (!File.Exists(scriptFile)) {
                    Log(string.Format("Script file not found at {0}", scriptFile), Color.Crimson);
                }

                string shouldBe, currentUser;
                if (!ValidRunAccount(out shouldBe, out currentUser)) {
                    return;
                }

                // Copy all files in the subfolder Files to the C: drive so that we can set the working folder
                // without having to bother about mapping the S: drive under the admin context
                CopyFilesToC();

                // Spawn off admin app
                //adminApp = Program.FILE_PATH + @"AdminApp\Admin process app.exe";
                CommandLine.ClearFiles();

                this.FormClosing += Form1_FormClosing;

                //if (File.Exists(adminApp)) {
                //    Process proc = new Process();
                //    proc.StartInfo = new ProcessStartInfo(adminApp, Program.FILE_PATH);
                //    proc.StartInfo.UseShellExecute = true;

                //    proc.Start();
                //} else {
                //    Log("Admin application not found [{0}]".Args(adminApp), Color.Crimson);
                //}

                Thread.Sleep(1000);
                this.TopMost = true;
                Thread.Sleep(1000);
                this.TopMost = false;
            } catch (Exception ex) {
                Log("An error occurred on startup. {0}".Args(ex.Message), Color.Crimson);
            }
        }

        void Form1_FormClosing(object sender, FormClosingEventArgs e) {
            CommandLine.WriteAdminCommand("exit");
        }

        private void DisplayInitialText() {
            Font fnt = richTextBox1.Font;

            richTextBox1.Text = "This tool is for testing general system availability.\r\n" +
                "For more specific tests, a standalone test suite should be created.\r\n";
            richTextBox1.SelectionColor = Color.Black;

        }

        private void Log(string Data, Color Colour, bool LogOnly = false) {
            if (InvokeRequired) {
                Invoke(new Action<string,Color,bool>(Log), Data, Colour, LogOnly);
                return;
            }

            string timeStamp = DateTime.Now.ToString("HH:mm:ss");

            if (!LogOnly)
            {
                richTextBox1.SelectionStart = richTextBox1.Text.Length;
                richTextBox1.SelectionColor = Colour;
                richTextBox1.SelectedText = timeStamp + "  " + Data + "\r\n";
                richTextBox1.SelectionStart = richTextBox1.Text.Length;
                richTextBox1.ScrollToCaret();
            }
            Maple.Logger.Log(Data);
        }

        private bool CheckAdmin()
        {
            // To force the app to run with Admin privileges, see the entry in the app.manifest file
            bool ret = false;

            System.AppDomain.CurrentDomain.SetPrincipalPolicy(PrincipalPolicy.WindowsPrincipal);
            WindowsIdentity curIdentity = WindowsIdentity.GetCurrent();
            WindowsPrincipal myPrincipal = new WindowsPrincipal(curIdentity);

            if(myPrincipal.IsInRole(WindowsBuiltInRole.Administrator)){
                ret = true;
            }

            return ret;
        }

        private bool ValidRunAccount(out string ShouldBe, out string currentUser) {
            bool ret = false;
            currentUser = "";
            ShouldBe = Properties.Settings.Default.ValidRunAccount;
            try {
                currentUser = System.Security.Principal.WindowsIdentity.GetCurrent().Name;

                if ("mpuk\\colin".Contains(currentUser.ToLower())) {
                    // Ok, it's a Dev
                    if (Program.userOverride) {
                        ShouldBe = currentUser;
                    } else if (MessageBox.Show("As it's you, do you want to run with your context?", "Dev", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.Yes) {
                        Program.userOverride = true;
                        ShouldBe = currentUser;
                    }
                }

                if (currentUser.ToLower() != ShouldBe.ToLower()) {
                    //not logged in as correct account
                    Log("This process must be run as {0}\r\nCurrent user is: {1}\r\n\r\n".Args(ShouldBe, currentUser), Color.Crimson);
                    ret = false;
                } else {
                    Log("Process is running as {0}\r\n\r\n".Args(ShouldBe, currentUser), Color.DarkBlue);
                    ret = true;
                }
            } catch (Exception ex) {
                Log("Error: " + ex.Message, Color.Crimson);
            }
            return ret;
        }

        BackgroundWorker bw = null;
        bool bwInProgress = false;

        [PrincipalPermission(SecurityAction.Demand, Role = @"BUILTIN\Administrators")]
        private void TestAsAdmin()
        {
            Log("Administrator method test has been run.", Color.DarkBlue);
        }

        //[PrincipalPermission(SecurityAction.Demand, Role = @"BUILTIN\Administrators")]
        private void RunScript(List<Step> steps) {
            TestButton.Enabled = false;
            try {
                Log(string.Format("Running {0} steps...", steps.Count()), Color.Black);

                if (Environment.MachineName.ToLower() == "aphid") {
                    if (StepByStepCheckbox.Checked == false) {
                        if (MessageBox.Show("Should the Step by Step box be checked?", "Check", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.Yes) {
                            StepByStepCheckbox.Checked = true;
                        }
                    }
                }

                bw = new BackgroundWorker();

                bw.WorkerSupportsCancellation = false;
                bw.WorkerReportsProgress = false;

                bw.DoWork += RunSteps;
                bw.RunWorkerCompleted += bw_RunWorkerCompleted;

                Arguments args = new Arguments() { Steps = steps  };

                bwInProgress = true;
                bw.RunWorkerAsync(args);
                do {
                    Application.DoEvents();
                } while (bwInProgress);
                Log("Finished.", Color.Black);
            } finally {
                TestButton.Enabled = true;
            }
        }

        /// <summary>
        /// It is easier to run the app from the S: drive but some of the commands require a change to the Drive
        /// and Folder where the command is to run. Therefore it is easier if we copy the script files to the 
        /// C: drive as the C: drive will always exist even when run as an administrator whereas the S: drive will
        /// not.
        /// </summary>
        private void CopyFilesToC(){
            string from = Path.GetDirectoryName(Application.ExecutablePath) + "\\Files\\";
            string to = Program.FILE_PATH + "Files\\";

            Directory.CreateDirectory(to);

            string[] files = Directory.GetFiles(from);

            foreach (string file in files){
                string ext = file.Substring(file.Length - 4);

                if (!".scc".Contains(ext)){
                    string dest = to + Path.GetFileName(file);

                    FileInfo fi = new FileInfo(dest);
                    if (fi.Exists) {
                        fi.IsReadOnly = false;
                    }
                    File.Copy(file, dest, true);
                }
            }

            from = Path.GetDirectoryName(Application.ExecutablePath) + "\\AdminApp\\";
            to = Program.FILE_PATH + "AdminApp\\";

            Directory.CreateDirectory(to);

            files = Directory.GetFiles(from);

            foreach (string file in files) {
                string ext = file.Substring(file.Length - 4);

                if (!".scc".Contains(ext)) {
                    string dest = to + Path.GetFileName(file);

                    FileInfo fi = new FileInfo(dest);
                    if (fi.Exists) {
                        fi.IsReadOnly = false;
                    }
                    File.Copy(file, dest, true);
                }
            }

            
        }

        void bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
            if (e.Error != null) {
                Log(string.Format("Error: {0}.",e.Error.Message), Color.Crimson);   
            }
            bwInProgress = false;
        }

        private void RunSteps(object sender, DoWorkEventArgs e){
            Arguments args = (Arguments)e.Argument;
            List<Step> steps = args.Steps;

            CommandLine commandLine = new CommandLine();
            Database database = new Database();

            foreach (Step step in steps) {
                string ret = "";
                if (StepByStepCheckbox.Checked){
                    Log("[{0}] {1}".Args(step.Type, step.Command), Color.DarkGray);
                    DialogResult dr = MessageBox.Show("Proceed with [{0}]?".Args(step.Name), "Confirm", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                    switch(dr){
                        case DialogResult.No:
                            continue;
                        case DialogResult.Yes:
                            break;
                        case DialogResult.Cancel:
                            Log("Process Cancelled.", Color.DarkGreen);
                            return;
                    }
                }

                Log(string.Format("{0}. [{1}]", step.No, step.Name), Color.DarkGreen);

                
                switch (step.Type) {
                    case StepType.Cmd:
                        string cmd = step.Command;
                        ret = commandLine.RunCommandLine(ref cmd, step.Admin, StepByStepCheckbox.Checked);
                        Log(cmd, Color.Gray, true);

                        break;
                    case StepType.Sql:
                        ret = database.RunCommand(step.Command);
                        break;
                }


                if (ret == "") {
                    Log("    succeeded.", Color.Black);
                } else {
                    Log("    failed. {0}".Args(ret), Color.Crimson);
                }
            }
        }

        private List<Step> ReadAllSteps(string scriptFile){
            List<Step> steps = null;

            /*  The syntax of the sql file that this app will process is the following:
             * --  at the start of a line means Ignore
             * GO -- The title of the next sectiion -- sql       Indicates the start of the next section and is of Type Sql
             * Exec MyProc etc.
             * */

            //string scriptFile = Program.FILE_PATH + "\\Files\\" + fileName;
            if (!File.Exists(scriptFile)) {
                Log(string.Format("Script file not found at {0}", scriptFile), Color.Crimson);
                return steps;
            }

            string[] lines = File.ReadAllLines(scriptFile);

            steps = new List<Step>();
            int blockNo = 0, lineNo = 0;
            bool firstStepFound = false;
            Step step = null;
            foreach(string line in lines){
                lineNo++;
                string trimLine = line.Trim().ToUpper();
                if (trimLine.StartsWith("--STEP")) {
                    // This is the start of the next section
                    // We expect the line in the form --Step -- Block Title -- sql
                    blockNo++;
                    firstStepFound = true;

                    string[] header = trimLine.Split(new Char[] { '-' }, StringSplitOptions.RemoveEmptyEntries);

                    if (header.Length > 2) {
                        step = new Step();
                        step.No = blockNo;
                        step.Name = header[1].Trim();
                        switch (header[2].ToLower().Trim()) {
                            case "cmd":
                                step.Type = StepType.Cmd;
                                break;
                            case "sql":
                                step.Type = StepType.Sql;
                                break;
                            default:
                                throw new Exception(string.Format("Unhandled step type on Step {0}, line {1}. Don't use hyphens '-' in the Step title.", blockNo, lineNo));
                        }
                        if (header.Length > 3) {
                            if (header[3].ToLower().Trim() == "admin") {
                                step.Admin = true;
                            }
                        }
                        step.Command = "";

                        steps.Add(step);

                    } else {
                        Log(string.Format("Insufficient fields found on Step {0}, line {1}", blockNo, lineNo), Color.Crimson);
                    }
                } else {
                    if (firstStepFound) {
                        // Part of the Sql/Cmd section
                        if (!trimLine.StartsWith("--##")) {
                            step.Command += trimLine + "\r\n";
                        }
                    }
                }
            }

            return steps;
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void TestButton_Click(object sender, EventArgs e){
            try{
                string shouldBe, currentUser;
                if (!ValidRunAccount(out shouldBe, out currentUser)) {
                    return;
                }

                //List<Step> steps = ReadAllSteps(SCRIPT_FILE);
                List<Step> steps = ReadAllSteps(GetScriptFileFullPath());

                RunScript(steps);

            }
            catch (Exception ex)
            {
                Log("Error: " + ex.Message, Color.Crimson);
            }
        }

        private void ViewScriptButton_Click(object sender, EventArgs e){
            try {
                //string fileName = DevTestOnlyCheckBox.Checked ? Program.FILE_PATH + "Files\\" + SCRIPT_FILE_DEV : Program.FILE_PATH + "Files\\" + SCRIPT_FILE;
                //string ting = DevTestOnlyCheckBox.Checked ? Path.GetDirectoryName(Application.ExecutablePath) + "\\Files\\" + SCRIPT_FILE_DEV : Path.GetDirectoryName(Application.ExecutablePath) + "\\Files\\" + SCRIPT_FILE;
                string fileName = GetScriptFileFullPath();

                Process proc = new Process();
                proc.StartInfo = new ProcessStartInfo(fileName);
                proc.StartInfo.UseShellExecute = true;

                proc.Start();
                //            proc.WaitForExit();     // No exit as it should still be open in sql management studio
            }catch(Exception ex){
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private void SqlServerName_TextChanged(object sender, EventArgs e) {
            DisplayInitialText();
        }

    }
}
