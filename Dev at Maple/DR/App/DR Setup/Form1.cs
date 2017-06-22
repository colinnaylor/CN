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
using DR_Setup.Model;
using System.Diagnostics;
using System.Security.Principal;
using System.Security.Permissions;
using Maple;
using System.Threading;

namespace DR_Setup {
    public partial class Form1 : Form {
        string[] args;
        string adminApp;

        public Form1(string[] Args) {
            InitializeComponent();

            args = Args;
        }

        const string SERVER_SCRIPT_FILE = "DR.sql";
        const string CLOSE_SCRIPT_FILE = "DRclose.sql";
        const string WORKSTATION_SCRIPT_FILE = "DRworkstation.sql";
        const string SERVER_SCRIPT_FILE_NAME = "Server Script";
        const string CLOSE_SCRIPT_FILE_NAME = "Close DR Script";
        const string WORKSTATION_SCRIPT_FILE_NAME = "Workstation Script";

        System.Windows.Forms.Timer timer1 = null;

        private void Form1_Load(object sender, EventArgs e) {
            string ver = Application.ProductVersion.Substring(0, Application.ProductVersion.LastIndexOf("."));
            this.Text = Application.ProductName + " v" + ver;

            // Note: This app and the Admin app must be set to run as 64 bit. 
            // If the app runs as a 32 bit app then Windows will very kindly modify the registry update from Software\Microsoft
            // to Software\Wow6432Node\Microsoft so we need to run as a 64 bit app so that we have full control.

            try {
                DisplayInitialText();

                // Check that the script file is there
                string scriptFile = AppLocation("Files\\") + SERVER_SCRIPT_FILE;
                if (!File.Exists(scriptFile)) {
                    Log(string.Format("Server Script file not found at {0}", scriptFile), Color.Crimson);
                }
                scriptFile = AppLocation("Files\\") + CLOSE_SCRIPT_FILE;
                if (!File.Exists(scriptFile)) {
                    Log(string.Format("Close Script file not found at {0}", scriptFile), Color.Crimson);
                }
                scriptFile = AppLocation("Files\\") + WORKSTATION_SCRIPT_FILE;
                if (!File.Exists(scriptFile))
                {
                    Log(string.Format("Workstation Script file not found at {0}", scriptFile), Color.Crimson);
                }
                
                string shouldBe, currentUser;
                if (!ValidRunAccount(out shouldBe, out currentUser)) {
                    return;
                }

                ViewScriptCombo.Items.Add(SERVER_SCRIPT_FILE_NAME);
                ViewScriptCombo.Items.Add(CLOSE_SCRIPT_FILE_NAME);
                ViewScriptCombo.Items.Add(WORKSTATION_SCRIPT_FILE_NAME);
                ViewScriptCombo.SelectedIndex = 0;

                timer1 = new System.Windows.Forms.Timer();
                timer1.Tick += new EventHandler(timer1_Tick);
                timer1.Interval = 3000;
                if (args.Length > 0 && args[0].ToLower() == "workstation") {
                    timer1.Start();
                }

                // Copy all files in the subfolder Files to the C: drive so that we can set the working folder
                // without having to bother about mapping the S: drive under the admin context
                CopyFilesToC();

                // Spawn off admin app
                adminApp = Program.FILE_PATH + @"AdminApp\Admin process app.exe";
                CommandLine.ClearFiles();

                this.FormClosing += Form1_FormClosing;

                if (File.Exists(adminApp)) {
                    Process proc = new Process();
                    proc.StartInfo = new ProcessStartInfo(adminApp, Program.FILE_PATH);
                    proc.StartInfo.UseShellExecute = true;

                    proc.Start();
                } else {
                    Log("Admin application not found [{0}]".Args(adminApp), Color.Crimson);
                }
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

            richTextBox1.Text = "";
            richTextBox1.SelectionColor = Color.Black;

            string usr = Properties.Settings.Default.ValidRunAccount;
            richTextBox1.Text = "";
            richTextBox1.SelectionFont = new Font(fnt.FontFamily,  14, FontStyle.Bold);
            richTextBox1.SelectedText = "Instructions\r\n";
            richTextBox1.SelectionFont = new Font(fnt.FontFamily, 12);
            richTextBox1.SelectedText = "Ensure textbox at the top of this window reflects the correct server name.\r\n" +
                "First, run the Server setup just once, from any workstation, logged in as {0}.\r\n".Args(usr) +
                "Run the Workstation setup on every workstation, logged in as {0}.\r\n".Args(usr) +
                "The Close DR process should be run ONLY at the end of the DR test.\r\n";
            richTextBox1.SelectionStart = richTextBox1.Text.Length;

            richTextBox1.SelectionFont = new Font(fnt.FontFamily, 12);
            richTextBox1.SelectedText = "\r\nDR SQL Server is set to ";
            richTextBox1.SelectionFont = new Font(fnt.FontFamily, 12, FontStyle.Bold);
            richTextBox1.SelectedText = SqlServerName.Text;
            richTextBox1.SelectionFont = new Font(fnt.FontFamily, 12);
            richTextBox1.SelectedText = ". Make sure this is correct before proceeding.\r\n";
            richTextBox1.SelectionFont = fnt;
            richTextBox1.SelectedText = "\r\n";
        }

        void  timer1_Tick(object sender, EventArgs e){
            timer1.Stop();

 	        WorkstationButton_Click(null,null);

            MessageBox.Show("Workstation Setup is complete.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);

            this.Close();
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

        private void SetupButton_Click(object sender, EventArgs e) {
            try{
                string shouldBe, currentUser;

                if (ValidRunAccount(out shouldBe, out currentUser)) {
                    Log("Running as {0}".Args(currentUser), Color.Black);

                    List<Step> steps = ReadAllSteps(SERVER_SCRIPT_FILE);
                    string server = SqlServerName.Text;
                    string fileServer = FileServerTextbox.Text;

                    RunScript(steps, server, fileServer);
                }

            }catch(Exception ex){
                Log("Error: " + ex.Message,Color.Crimson);
            }
        }

        private bool ValidRunAccount(out string ShouldBe, out string currentUser) {
            bool ret = false;
            currentUser = "";
            ShouldBe = Properties.Settings.Default.ValidRunAccount;
            try {
                currentUser = System.Security.Principal.WindowsIdentity.GetCurrent().Name;

                if ("mpuk\\colin".Contains(currentUser.ToLower())) {
                    // Ok, it's a Dev
                    if (MessageBox.Show("As it's you, do you want to run with your context?", "Dev", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.Yes) {
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
        private void RunScript(List<Step> steps, string server, string fileServer) {
            WorkstationButton.Enabled = false;
            SetupButton.Enabled = false;
            CloseButton.Enabled = false;
            try {
                Log(string.Format("Running {0} steps...", steps.Count()), Color.Black);

                if (Environment.MachineName.ToLower() == "aphid") {
                    if (StepByStepCheckbox.Checked == false) {
                        if (MessageBox.Show("Should the Debug box be checked?", "Check", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.Yes) {
                            StepByStepCheckbox.Checked = true;
                        }
                    }
                }

                bw = new BackgroundWorker();

                bw.WorkerSupportsCancellation = false;
                bw.WorkerReportsProgress = false;

                bw.DoWork += RunSteps;
                bw.RunWorkerCompleted += bw_RunWorkerCompleted;

                Arguments args = new Arguments() { Steps = steps, Server = server, FileServer = fileServer  };

                bwInProgress = true;
                bw.RunWorkerAsync(args);
                do {
                    Application.DoEvents();
                } while (bwInProgress);
                Log("Finished.", Color.Black);
            } finally {
                WorkstationButton.Enabled = true;
                SetupButton.Enabled = true;
                CloseButton.Enabled = true;
            }
        }

        /// <summary>
        /// It is easier to run the app from the S: drive but some of the commands require a change to the Drive
        /// and Folder where the command is to run. Therefore it is easier if we copy the script files to the 
        /// C: drive as the C: drive will always exist even when run as an administrator whereas the S: drive will
        /// not. Plus it's easier to tell the AdminApp a location on the C: drive to monitor without fussing about what the full url of the S: drive is
        /// </summary>
        private void CopyFilesToC(){

            CopyFolderToC("Files", "", ".sql");
            CopyFolderToC("AdminApp", "", "");
            CopyFolderToC("Desktop", "publish", "");

        }

        private void CopyFolderToC(string folder, string prefixExclude, string extensionExclude)
        {
            string from = AppLocation("{0}\\".Args(folder));
            string to = Program.FILE_PATH + "{0}\\".Args(folder);

            Directory.CreateDirectory(to);

            string[] files = Directory.GetFiles(from);

            foreach (string file in files)
            {
                bool exclude = false;
                string filename = Path.GetFileName(file).ToLower();
                string ext = Path.GetExtension(file).ToLower();
                if (prefixExclude.Length > 0 && filename.StartsWith(prefixExclude))
                {
                    exclude = true;
                }
                if(extensionExclude.Length > 0 && extensionExclude.ToLower().Contains(ext))
                {
                    exclude = true;
                }

                if (!exclude)
                {
                    string dest = to + Path.GetFileName(file);

                    FileInfo fi = new FileInfo(dest);
                    if (fi.Exists)
                    {
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
            string server = args.Server;
            string fileServer = args.FileServer;

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

                step.Command = step.Command.Replace("%SqlServerName%", server);
                step.Command = step.Command.Replace("%FileServerName%", fileServer);
                
                switch (step.Type) {
                    case StepType.Cmd:
                        string cmd = step.Command;
                        ret = commandLine.RunCommandLine(ref cmd, step.Admin, StepByStepCheckbox.Checked);
                        Log(cmd, Color.Gray, true);

                        break;
                    case StepType.Sql:
                        ret = database.RunCommand(server, step.Command);
                        break;
                }


                if (ret == "") {
                    Log("    succeeded.", Color.Black);
                } else if (step.Command.ToLower().Contains("removelinkedservercalls ")) {
                    // For some steps, we want to report what has happened but still continue
                    Log("    failed. {0}".Args(ret), Color.Crimson);
                    Log("    This step should be re-run once the problem is fixed.", Color.Blue);
                } else {
                    Log("    failed. {0}".Args(ret), Color.Crimson);
                    Log("Process stopped.", Color.DarkGreen);
                    break;
                }
            }
        }

        private void CloseButton_Click(object sender, EventArgs e) {
            try {
                string shouldBe, currentUser;
                if (!ValidRunAccount(out shouldBe, out currentUser)) {
                    return;
                }

                if (MessageBox.Show("Are you sure you wish to run the CLOSE script now? It should only be run at the END of the DR test.",
                    "Are you sure?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.Yes) {
                    List<Step> steps = ReadAllSteps(CLOSE_SCRIPT_FILE);
                    string server = SqlServerName.Text;
                    string fileServer = FileServerTextbox.Text;

                    RunScript(steps, server, fileServer);
                }
            } catch (Exception ex) {
                Log("Error: " + ex.Message, Color.Crimson);
            }
        }

        private List<Step> ReadAllSteps(string fileName){
            List<Step> steps = null;

            /*  The syntax of the sql file that this app will process is the following:
             * --  at the start of a line means Ignore
             * GO -- The title of the next sectiion -- sql       Indicates the start of the next section and is of Type Sql
             * Exec MyProc etc.
             * */

            string scriptFile = AppLocation("Files\\") + fileName;
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
                if (line.Trim().ToUpper().StartsWith("--STEP")) {
                    // This is the start of the next section
                    // We expect the line in the form GO  -- Block Title -- sql
                    blockNo++;
                    firstStepFound = true;

                    string[] header = line.Split(new Char[] { '-' }, StringSplitOptions.RemoveEmptyEntries);

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
                        if (!line.Trim().StartsWith("--##")) {
                            step.Command += line + "\r\n";
                        }
                    }
                }
            }

            return steps;
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void WorkstationButton_Click(object sender, EventArgs e){
            try{
                string shouldBe, currentUser;
                if (!ValidRunAccount(out shouldBe, out currentUser)) {
                    return;
                }

                List<Step> steps = ReadAllSteps(WORKSTATION_SCRIPT_FILE);
                string server = SqlServerName.Text;
                string fileServer = FileServerTextbox.Text;

                RunScript(steps, server, fileServer);

                SetupButton.Enabled = true;
            }
            catch (Exception ex)
            {
                Log("Error: " + ex.Message, Color.Crimson);
            }
        }

        private string AppLocation(string SubFolder)
        {
            string path = Path.GetDirectoryName(Application.ExecutablePath);
            string ret = Path.Combine(path, SubFolder);

            return ret;
        }

        private void ViewScriptButton_Click(object sender, EventArgs e){
            try {
                string fileName = AppLocation("Files\\");
                switch (ViewScriptCombo.SelectedItem.ToString()) {
                    case SERVER_SCRIPT_FILE_NAME:
                        fileName += SERVER_SCRIPT_FILE;
                        break;
                    case CLOSE_SCRIPT_FILE_NAME:
                        fileName += CLOSE_SCRIPT_FILE;
                        break;
                    case WORKSTATION_SCRIPT_FILE_NAME:
                        fileName += WORKSTATION_SCRIPT_FILE;
                        break;
                }

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

        private void ServerTestButton_Click(object sender, EventArgs e) {

            RunScript(new List<Step>() { new Step() { 
                Type = StepType.Sql, 
                No = 1, 
                Name = string.Format("{0} SQL Server connection test.",SqlServerName.Text), 
                Command = "SELECT @@Servername" } }, SqlServerName.Text, FileServerTextbox.Text);

        }

    }
}
