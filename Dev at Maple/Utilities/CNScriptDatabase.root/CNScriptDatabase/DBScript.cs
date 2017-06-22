#region Using directives

using System;
using System.Collections.Generic;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Specialized;
using System.Text;

// SMO namespaces
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using Microsoft.SqlServer;
using System.Data.SqlClient;
//VSS
using Microsoft.VisualStudio.SourceSafe.Interop;
using System.Threading;
using System.IO;
using System.Runtime.Remoting.Messaging;
using Maple;

#endregion

namespace Microsoft.Samples.SqlServer
{
    public partial class CNDBScript : Form
    {
        private Server SqlServerSelection;
        private List<dbClass> dbList = new List<dbClass>();
        private dbClass currentDB = null;
        private ScriptEngine m_scriptEngine = null;
        //private Thread m_scriptEngineThread = null;
        private ServerConnection ServerConn;
        private string tempPath = "";
        private bool exitWhenFinished = false;

        public CNDBScript(string[] args)
        {
            bool run = true;
            InitializeComponent();
            DBListView.Columns.Add("Select Databases to script", DBListView.Width - 5, HorizontalAlignment.Left);
#if DEBUG
            if(MessageBox.Show("auto run?","Debug",MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.No){
                args = new string[0];
            }
#endif

            if (args.Length > 0) {
                // Should be the name of the file to process
                if (args.Length == 1) {
                    string file = args[0];
                    if (File.Exists(file)) {
                        exitWhenFinished = true;

                        // Read file in
                        string[] lines = File.ReadAllLines(file);
                        dbList = new List<dbClass>();

                        foreach (string line in lines) {
                            if (line.Trim() != "" && !line.StartsWith("#")) {
                                if (line.Trim().ToLower().Substring(0, 8) == "temppath") {
                                    tempPath = line.Substring(line.IndexOf("=") + 1).Trim();
                                    if (!tempPath.EndsWith("\\")) tempPath += "\\";
                                } else {
                                    string[] fields = line.Split(new char[] { ',' });
                                    if (fields.Length == 5) {
                                        for (int i = 0; i < fields.Length; i++) {
                                            fields[i] = fields[i].Trim();
                                        }
                                        if (!fields[0].EndsWith("\\")) fields[0] += "\\";
                                        if (!fields[3].EndsWith("/")) fields[3] += "/";

                                        fields[0] += "srcsafe.ini";

                                        // Work through all database on that server
                                        List<string> databases = GetDatabaseNames(fields[4]);

                                        foreach (string database in databases) {
                                            dbClass db = new dbClass();
                                            db.VssIniFile = fields[0];
                                            db.VssLogin = fields[1];
                                            db.VssPw = fields[2];
                                            if (db.VssPw.Length > 1) {
                                                db.VssPw = db.VssPw.Substring(1);
                                            }
                                            db.VssProjectParent = fields[3];
                                            db.Server = fields[4];
                                            db.Database = database;

                                            dbList.Add(db);
                                        }
                                    } else {
                                        Log("Bad line found in control file. " + line);
                                    }
                                }
                            }
                        }
                        if (dbList.Count > 0) {
                            timer1.Interval = 1000;
                            timer1.Enabled = true;
                        }
                    } else {
                        run = false;
                    }
                }
            }
            if(!run){
                Environment.Exit(1);
            }
        }

        private List<string> GetDatabaseNames(string Server) {
            List<string> ret = new List<string>();

            SQLServer comm = new SQLServer(Server, "master", "", "");
            try {
                string sql = "select name from sys.databases where name != 'tempdb'";

                SqlDataReader dr = comm.FetchDataReader(sql);
                while (dr.Read()) {
                    ret.Add(dr[0].ToString());
                }

                return ret;
            } finally {
                comm.Close();
            }
        }

        private void Log(string data) {
            Logger.Log(data);

            try {
                DoLogOutput del = new DoLogOutput(SetLogText);
                this.Invoke(del, data);
            } catch {
                // Ignore if we can't update the textbox
            }
        }

        private void SetLogText(string data) {
            Output.SelectionStart = Output.Text.Length;
            Output.SelectedText = DateTime.Now.ToString("HH:mm:ss") + "  " + data + "\r\n";
            Output.SelectionStart = Output.Text.Length;
            Output.ScrollToCaret();
        }

        private void DBScript_Load(object sender, EventArgs e) {
            string ver = Application.ProductVersion.Substring(0, Application.ProductVersion.LastIndexOf("."));
            this.Text = Application.ProductName + " v" + ver;
            Log("Application start version " + ver);


            ScriptGroupBox.Enabled = true;
            ServerNamesComboBox.Text = Properties.Settings.Default.LastConnectedServer;
            toolStripStatusLabel1.Text = "";
            toolStripStatusLabel2.Text = "";
            toolStripStatusLabel3.Text = "DB = none";
            VSSProjTxt.Text = Properties.Settings.Default.SourceSafeProject;

            int lc = dbList.Count;

            if (!TestForVSS()) {
                Log("Please ensure that Sourcesafe has been installed on this machine.");
                string pc = Environment.MachineName;
                Maple.ApplicationStatus.SetStatus(Application.ProductName, "Error", "Sourcesafe has not been installed on " + pc, 10);
                if (dbList.Count == 0) {
                    // Was run with auto option
                    MessageBox.Show("Please ensure that Sourcesafe has been installed on this machine.", Application.ProductName);
                }
                this.Close();
            }
        }

        private bool TestForVSS() {
            bool ret = true;
            try {
                VSSDatabaseClass m_vssDB = new VSSDatabaseClass();
                Logger.Log("Creation of VSSDatabaseClass was successful.");
            } catch(Exception ex) {
                Logger.LogError("TestForVSS", ex);
                ret = false;
            }
            return ret;
        }

        private void ShowDatabases(bool selectDefault)
        {
            // Show the current databases on the server
            try
            {
                if (!DBListView.InvokeRequired) {
                    // Clear control
                    DBListView.Items.Clear();

                    // Limit the properties returned to just those that we use
                    this.SqlServerSelection.SetDefaultInitFields(typeof(Database),
                   "CreateDate", "IsSystemObject", "CompatibilityLevel");
                    this.SqlServerSelection.SetDefaultInitFields(typeof(Table),
                        "CreateDate", "IsSystemObject");
                    this.SqlServerSelection.SetDefaultInitFields(typeof(Microsoft.SqlServer.Management.Smo.View),
                        "CreateDate", "IsSystemObject");
                    this.SqlServerSelection.SetDefaultInitFields(typeof(StoredProcedure),
                        "CreateDate", "IsSystemObject");
                    this.SqlServerSelection.SetDefaultInitFields(typeof(UserDefinedFunction),
                        "CreateDate", "IsSystemObject");
                    this.SqlServerSelection.SetDefaultInitFields(typeof(Column), true);

                    // Add database objects to combobox; the default ToString 
                    // will display the database name
                    foreach (Database db in SqlServerSelection.Databases) {
                        if ( (db.IsSystemObject == false || db.Name.ToLower() == "msdb") && db.IsAccessible == true) {
                            DBListView.Items.Add(db.Name);
                        }
                    }
                    DBListView.Enabled = true;
                }                        
            }
            catch (SmoException ex)
            {
                Logger.LogError("ShowDatabase method",ex);
            }
        }

        private void ScriptDBBtn_Click(object sender, EventArgs e)
        {

            if (ScriptDBBtn.Text == "Begin"){
                dbList.Clear();

                foreach (ListViewItem lvi in DBListView.Items) {
                    if (lvi.Checked) { // selected database
                        dbClass db = new dbClass();
                        db.Server = ServerNamesComboBox.Text;
                        db.Database = lvi.Text;
                        db.VssIniFile = VSSIniPath.Text;
                        db.VssLogin = VSSLoginTxt.Text;
                        db.VssPw = VSSPwdTxt.Text;
                        db.VssProjectParent = VSSProjTxt.Text;
                        db.Server = SqlServerSelection.Name;

                        dbList.Add(db);
                    }
                }

                if (dbList.Count > 0) {
                    scriptOnly = ScriptOnlyCheckbox.Checked;
                    tempPath = localScriptPath.Text;

                    string ret = ScriptDBs();
                }
            } else {
                isCancelled = true;
                //backgroundWorker1.CancelAsync();
            }
        }

        /// <summary>
        /// Script the database and check in to Sourcesafe if required
        /// </summary>
        /// <param name="VSSiniPath"></param>
        /// <param name="VSSlogin"></param>
        /// <param name="VSSpw"></param>
        /// <param name="VSSproject"></param>
        /// <param name="Server"></param>
        /// <returns></returns>
        private string ScriptDBs() {
            string ret = "";

            ScriptDBBtn.Text = "Stop";
            // The call to ScriptAndCheckIn keeps calling itself until all dbs in dbList are done.
            ScriptAndCheckIn();

            return ret;
        }

        private bool scriptOnly = false;

        delegate void DoScriptWork();
        delegate void DoSetButtonText(string data);
        delegate void DoSetComboText(string data);
        delegate void DoLogOutput(string data);

        public void ScriptAndCheckIn()
        {
            DoScriptWork worker = new DoScriptWork(backgroundWorker1_DoWork);
            worker.BeginInvoke(new AsyncCallback(backgroundWorker1_RunWorkerCompleted), null);

            System.Windows.Forms.Application.DoEvents();
        }

        static bool isCancelled = false;

        private void backgroundWorker1_DoWork()
        {
            Thread.CurrentThread.Name = "Script Thread";

            try {
                while (dbList.Count > 0) {
                    currentDB = dbList[0];

                    Log("-----");
                    Log(currentDB.Database + " on " + currentDB.Server + " to " + currentDB.VssProjectParent + " in " + currentDB.VssIniFile);
                    try {
                        // Force a reconnection so that any recent changes are taken into account
                        if (Connect(currentDB.Server)) {

                            m_scriptEngine = new ScriptEngine(SqlServerSelection,
                                                        currentDB, toolStripStatusLabel1, toolStripStatusLabel2, localScriptPath.Text);
                            //m_scriptEngine.ScriptDone += new ScriptingHandler(ScriptAndCheckIn);
                        }
                    } catch (Exception ex) {
                        Log(ex.Message);
                        Log(ex.StackTrace);
                        throw;
                    }
                    dbList.RemoveAt(0);

                    if (m_scriptEngine != null) {
                        m_scriptEngine.DatabaseName = currentDB.Database;
                        toolStripStatusLabel3.Text = "DB = " + currentDB.Database;

                        // Start the time-consuming operation.
                        Log("Commencing the script of " + currentDB.Database + " on " + currentDB.Server);

                        m_scriptEngine.Script(ServerConn, scriptOnly);
                    } else {
                        Funcs.EmailReport("Database Scriptor Report", currentDB.Database + " on " + currentDB.Server + " has not been scripted.");
                    }
                }

                Maple.ApplicationStatus.SetStatus(Application.ProductName, "OK", "Finished", 1445);
            } catch (Exception ex) {
                Logger.LogError("DoWork", ex);
                Maple.ApplicationStatus.SetStatus(Application.ProductName, "Error", ex.Message, 10);
            }
        }

        // This event handler demonstrates how to interpret 
        // the outcome of the asynchronous operation implemented
        // in the DoWork event handler.
        private void backgroundWorker1_RunWorkerCompleted(IAsyncResult res) {
            AsyncResult result = (AsyncResult)res;
            Console.Out.WriteLine("Thread " + Thread.CurrentThread.ManagedThreadId.ToString());

            DoScriptWork del = (DoScriptWork)result.AsyncDelegate;
            try {
                DoSetButtonText d = new DoSetButtonText(SetButtonText);

                del.EndInvoke(res);

                if (isCancelled) {
                    // The user canceled the operation.
                    Log("Operation was canceled");
                    // The operation completed normally.
                    this.Invoke(d, ScriptDBBtn, "Begin");
                } else {
                    // The operation completed normally.
                    // All done
                    Log("Completed");
                    this.Invoke(d, "Begin");

                    if (exitWhenFinished) {
                        Environment.Exit(0);
                    }
                }
            } catch (Exception ex) {
                Log("Error in backgroundWorker1_RunWorkerCompleted. " + ex.Message);

                ScriptDBBtn.Text = "Begin";
            }
        }

        private void SetButtonText(string Data) {
            ScriptDBBtn.Text = Data;
        }

        private void SetComboText(string Data) {
            ServerNamesComboBox.Text = Data;
        }

        private void ConnectCommandButton_Click(object sender, EventArgs e) {
            Connect(ServerNamesComboBox.Text);
        }

        private bool Connect(string Server){

            Log("Attempting connection to " + Server);
            try
            {
                DoSetComboText d = new DoSetComboText(SetComboText);
                this.Invoke(d, Server);

                // Recreate connection
                ServerConn = new ServerConnection();

                // Fill in necessary information
                ServerConn.ServerInstance = Server;

                // Setup capture and execute to be able to display script
                ServerConn.SqlExecutionModes = SqlExecutionModes.ExecuteAndCaptureSql;

                ServerConn.LoginSecure = false;
                ServerConn.Login = "BackupOperator";
                ServerConn.Password = "BackupOp7[]";

                // Go ahead and connect
                ServerConn.Connect();

                if (ServerConn.SqlConnectionObject.State == ConnectionState.Open){
                    Log("Connected");
                    SqlServerSelection = new Server(ServerConn);
                    // Must be admin on the target machine
                    if (!SqlServerSelection.Roles.Contains("sysadmin")) {
                        Funcs.EmailReport("Database Scripter Problem", "The user " + ServerConn.Login + " is not a member of the SysAdmin role on " + ServerConn.ServerInstance
                            + "<br />The login context must be a member of the SysAdmin role.");
                        return false;
                    }

                    if (SqlServerSelection != null) {
                        //this.Text = SqlServerSelection.Name;

                        // Refresh database list
                        ShowDatabases(true);

                        Properties.Settings.Default.LastConnectedServer = ServerConn.ServerInstance;
                    }
                }else{
                    Log("Failed connection to " + Server);
                    this.Close();
                }
            }catch (ConnectionFailureException ex){
                Logger.LogError("", ex);
            }
            catch (SmoException ex){
                Logger.LogError("SMO", ex);
            }
            return true;
        }

        private void GetServerList()
        {
            DataTable dt;

            // Set local server as default
            dt = SmoApplication.EnumAvailableSqlServers(false);
            if (dt.Rows.Count > 0)
            {
                // Load server names into combo box
                foreach (DataRow dr in dt.Rows)
                {
                    ServerNamesComboBox.Items.Add(dr["Name"]);
                }

                // Default to this machine 
                ServerNamesComboBox.SelectedIndex
                    = ServerNamesComboBox.FindStringExact(
                    System.Environment.MachineName);

                // If this machine is not a SQL server 
                // then select the first server in the list
                if (ServerNamesComboBox.SelectedIndex < 0)
                {
                    ServerNamesComboBox.SelectedIndex = 0;
                }
            }
            else
            {
                Logger.LogError("", new Exception("No servers found"));
            }
        }

        private void OnSqlInfoMessage(object sender,
            System.Data.SqlClient.SqlInfoMessageEventArgs args)
        {
            foreach (SqlError err in args.Errors)
            {
                Logger.LogError("OnSqlInfoMessage", new Exception(err.Message));
            }
        }

        private void OnServerMessage(object sender,
            ServerMessageEventArgs args)
        {
            SqlError err = args.Error;

            Logger.LogError("OnServerMessage", new Exception(err.Message));
        }

        private void OnStateChange(object sender, StateChangeEventArgs args)
        {
            if (this.IsDisposed == false)
            {
                Log(string.Format(System.Globalization.CultureInfo.InvariantCulture,
                    "Original state {0}. Current state {1}",
                    args.OriginalState.ToString(), args.CurrentState.ToString()));
            }
        }

        private void OnStatementExecuted(object sender,
            StatementEventArgs args)
        {
            Log(args.SqlStatement);
        }

        private void GetServer_Click(object sender, EventArgs e)
        {
            GetServerList();
        }

        private void button1_Click(object sender, EventArgs e) {

        }

        private void DBListView_ItemChecked(object sender, ItemCheckedEventArgs e) {
            int i = 0;
            foreach(ListViewItem item in DBListView.Items){
                if(item.Checked){
                    i++;
                }
            }
            SelectedCountLabel.Text = "DBs selected " + i.ToString();
        }

        private void label6_Click(object sender, EventArgs e) {

        }

        private void label1_Click(object sender, EventArgs e) {

        }

        private void label9_Click(object sender, EventArgs e) {

        }

        private void label8_Click(object sender, EventArgs e) {

        }

        private void DBListView_SelectedIndexChanged(object sender, EventArgs e) {

        }

        private void timer1_Tick(object sender, EventArgs e) {
            timer1.Enabled = false;
            if (dbList.Count > 0) {
                ScriptGroupBox.Enabled = false;
                Log("Starting auto scripting");
                ScriptDBs();
            }
        }

         
    }
}