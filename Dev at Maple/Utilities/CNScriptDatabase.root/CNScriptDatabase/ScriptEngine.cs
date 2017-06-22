using System;
using Microsoft.SqlServer.Management.Smo;
using Microsoft.SqlServer.Management.Common;
using Microsoft.VisualStudio.SourceSafe.Interop;
using System.Threading;
using System.Collections.Specialized;
using System.IO;
using System.Linq;

//for directory delete for read-only files
using System.Management;
using System.Collections.Generic;
using System.Data.SqlClient;
using Microsoft.SqlServer.Management.Smo.Agent;
using Maple;

namespace Microsoft.Samples.SqlServer
{
	public delegate void ScriptingHandler(bool scriptOnly);
    public delegate void ScriptingHandlerPrg(string text,int val);
    public delegate void ScriptingProgressTopValue(string caption,int index);

	/// <summary>
	/// Summary description for ScriptEngine.
	/// </summary>
	public class ScriptEngine
	{
		public event ScriptingHandler ScriptDone;
        //public event ScriptingHandlerPrg ScriptProgress;
        //public event ScriptingProgressTopValue ScriptProgressTopVal;

		// member variables
		Server      	m_sqlserver;
		string			m_dbName;
		VSSDatabaseClass	m_vssDatabase;
		string			m_vssRootPath;
		VSSItem			m_vssRoot;
		string			m_workingFolder;

        private string filename;

		public static	Transfer s_transfer = new Transfer();
        System.Windows.Forms.ToolStripLabel dname;
        System.Windows.Forms.ToolStripLabel oname;

#region Enums
        enum Db_Types
        {
            Defaults, Synonym, Rules, SPs, Tables, UserDefinedAggregates, UserDefinedDataTypes,
            UserDefinedFunctions, UserDefinedTypes, Users, Views, Assembly, Triggers, SqlJobs
        };
#endregion
       
		// constructor
        public ScriptEngine(Server sqlserver, // sql server
            dbClass currentDB,
            // root in VSS under which items are created
            System.Windows.Forms.ToolStripLabel databasename,
            System.Windows.Forms.ToolStripLabel objectname,
            string localScriptPath
            )
		{
            string vssRootPath = currentDB.VssProjectParent;

            m_vssDatabase = new VSSDatabaseClass();
            m_vssDatabase.Open(currentDB.VssIniFile, currentDB.VssLogin, currentDB.VssPw);
            
            m_sqlserver = sqlserver;
			m_vssRootPath = vssRootPath;
			m_vssRoot = m_vssDatabase.get_VSSItem(vssRootPath, false);

            m_workingFolder = localScriptPath;
			m_workingFolder += "\\DBScriptManager";
			System.IO.Directory.CreateDirectory(m_workingFolder);
            dname = databasename; oname = objectname;
		}

		public string DatabaseName 
		{
			get { return m_dbName; }
			set { m_dbName = value; }
		}

        private string serverName = "";
        ServerConnection serverConn = null;

		public void Script(ServerConnection ServerConn, bool scriptOnly)
		{
			try
			{
                serverName = ServerConn.ServerInstance;
                serverConn = ServerConn;

				if (m_dbName == null)
					return; // there is no DB to script
                foreach (Database db in m_sqlserver.Databases) {

                    if (m_dbName.ToLower() == db.Name.ToLower()) {
                        string dest = m_workingFolder + "\\" + m_sqlserver.Name + "\\" + db.Name;
                        if (Directory.Exists(dest)) {
                            DeleteDirectory(dest);
                        }

                        System.IO.Directory.CreateDirectory(m_workingFolder);

                        if (scriptOnly == false) {
                            CheckoutFromSourcesafe(db, dest);
                        }

                        ScriptDatabase(db, dest);

                        if (errorsOccurred == false) {
                            if (scriptOnly == false) {
                                AddToSourcesafe(db, dest);

                                // The sourcesafe bit above removes each file but we still need to kill the folder.
                                DeleteDirectory(m_workingFolder);
                            }
                        }
                        break;
                    }
                }
                if (errorsOccurred) {
                    string logFile = Maple.Logger.GetLogFilePathAndName(true);

                    Funcs.EmailReport("DB script error",
                        string.Format("See the log file for more info\r\n\r\n {0}", logFile));
                    ApplicationStatus.SetStatus(string.Format("Scripting of {0}.{1}.", serverName, m_dbName), "Error", "An error occurred", 1450);
                } else {
                    ApplicationStatus.SetStatus(string.Format("Scripting of {0}.{1}.", serverName, m_dbName), "OK", "", 1450);
                }
            } 
			catch (Exception ex){ //end up here in case of abort
                throw ex;
			}
            if (ScriptDone != null) {
                ScriptDone(scriptOnly);
            }
		}
        private  void DeleteDirectory(string targetDirectory)
        {
            try {
                // Ensure no read only files have been left
                string[] files = Directory.GetFiles(targetDirectory, "*.*", SearchOption.AllDirectories);
                foreach (string file in files) {
                    File.SetAttributes(file, FileAttributes.Normal);
                }

                // Now it should be safe to delete the Folder
                Directory.Delete(targetDirectory, true);
                
            } catch(Exception ex) {
                // We need to start with a completely empty folder
                Logger.Log(string.Format("Unable to remove the {0} folder.\r\n{1}", targetDirectory, ex.Message));
                throw;
            }
        }

		private void ScriptDatabase(Database db, string dest){
			try{
                System.IO.Directory.CreateDirectory(dest);

				s_transfer.DropDestinationObjectsFirst = true;
				s_transfer.CopySchema = true;
				s_transfer.CopyAllObjects = false; // first stop all objects
                ReadandWriteObjectsFromDatabasetoFile(db, dest);
                dname.Text = "Scripting Complete"; oname.Text = "";

			}catch(Exception ex){
                m_vssDatabase.Close();
                m_vssDatabase = null;
                dname.Text = "Error: " + ex.Message;
                errorsOccurred = true;
                Logger.Log("Error: " + ex.Message);
                Logger.Log("Stack: " + ex.StackTrace);

            }
        }

        string[] preExistingSourcesafeEntries;

        private void CheckoutFromSourcesafe(Database db, string dest) {
            try {
                dname.Text = "Check out from Sourcesafe. Objects = " + objectCount.ToString(); oname.Text = "";
                string vssPath = m_vssRootPath;
                VSSItem VssDbItem = GetVssItem(vssPath, VSSItemType.VSSITEM_PROJECT, serverName, db.Name);
                VssDbItem.LocalSpec = dest;

                VssDbItem.Checkout("DBScriptManager Automatic Checkout",
                    dest,
                    (int)(VSSFlags.VSSFLAG_GETYES | VSSFlags.VSSFLAG_RECURSYES));

                preExistingSourcesafeEntries = Directory.GetFiles(dest, "*.sql", SearchOption.AllDirectories); 
                dname.Text = "Sourcesafe check out complete."; oname.Text = "";
            } catch (Exception ex) {
                dname.Text = "Error in AddToSourcesafe method. [" + ex.Message + "]"; oname.Text = "";
                m_vssDatabase.Close();
                m_vssDatabase = null;
                errorsOccurred = true;
                Logger.Log("AddToSourcesafe Error\r\n" + ex.StackTrace);
                throw;
            }
        }

        private void AddToSourcesafe(Database db, string dest) {
            try {
                dname.Text = "Check out from Sourcesafe. Objects = " + objectCount.ToString(); oname.Text = "";
                string vssPath = m_vssRootPath;
                VSSItem VssDbItem = GetVssItem(vssPath, VSSItemType.VSSITEM_PROJECT, serverName, db.Name);
                VssDbItem.LocalSpec = dest;
              
                dname.Text = "Checking in to Sourcesafe. Objects = " + objectCount.ToString(); oname.Text = "";
                VssDbItem.Checkin("DBScriptManager Automatic Checkin",
                    dest,
					(int)(VSSFlags.VSSFLAG_DELTAYES| VSSFlags.VSSFLAG_RECURSYES|
					VSSFlags.VSSFLAG_DELYES));


                dname.Text = "Undoing checkouts. Objects = " + objectCount.ToString(); oname.Text = "";
                VssDbItem.UndoCheckout(dest,
                    (int)(VSSFlags.VSSFLAG_GETNO | VSSFlags.VSSFLAG_DELYES |
                    VSSFlags.VSSFLAG_RECURSYES));

                //  Now ensure that all the files that were originally retrieved from Sourcesafe have been removed
                foreach (string file in preExistingSourcesafeEntries) {
                    File.Delete(file);
                }

                dname.Text = "Adding to Sourcesafe. Objects = " + objectCount.ToString(); oname.Text = "";
                VssDbItem.Add(dest, "Created by DBScriptManager",
                    (int)(VSSFlags.VSSFLAG_DELYES | VSSFlags.VSSFLAG_RECURSYES));

                dname.Text = "Sourcesafe check in complete."; oname.Text = "";

            } catch (Exception ex) {
                dname.Text = "Error in AddToSourcesafe method. [" + ex.Message + "]"; oname.Text = "";
                m_vssDatabase.Close();
                m_vssDatabase = null;
                errorsOccurred = true;
                Logger.Log("AddToSourcesafe Error\r\n" + ex.StackTrace);
                throw;
			}
		}

	  
		// try to get the path and if not create it
		private VSSItem GetVssItem(string path, VSSItemType type,string ParentName, string dbname)
		{
			VSSItem item = null;
			try {
				item = m_vssDatabase.get_VSSItem(path + ParentName + "/" + dbname, false);
			} catch(Exception ex1){
                Console.Out.WriteLine(ex1.Message);
				try {
                    // See if the parent exists
                    try {
                        item = m_vssDatabase.get_VSSItem(path + ParentName, false);
                    } catch {
                        m_vssRoot.NewSubproject(ParentName, "Created by ScriptManager");
                    }

                    if (type == VSSItemType.VSSITEM_PROJECT) {
                        item = m_vssRoot.NewSubproject(ParentName + "/" + dbname, "Created by ScriptManager");
                    }
				} catch(Exception ex2) {
                    Console.Out.WriteLine("Caught in GetVssItem method. " + ex2.Message);
				}
			}
			return item;
		}

        int objectCount;
        bool errorsOccurred = false;

        public void ReadandWriteObjectsFromDatabasetoFile(Database db, string dest){
            objectCount = 0;

            for (Db_Types dt = Db_Types.Defaults; dt <= Db_Types.SqlJobs; dt++)
            {
                string workingFolder = dest + @"\" + dt.ToString() + @"\";
                if (dt.ToString() == "SPs") {
                    workingFolder = dest + @"\Stored Procedures\";
                } else if (dt.ToString() == "Triggers") {
                    workingFolder = dest + @"\DDL Triggers\";
                }

                System.IO.Directory.CreateDirectory(workingFolder);
                try {
                    switch (dt) {
                        case Db_Types.Defaults:
                            dname.Text = db.Name + ":  Defaults";
                            LoadDefaults(db, workingFolder);
                            break;
                        case Db_Types.Synonym:
                            dname.Text = db.Name + ":  Synonym";
                            LoadSynonym(db, workingFolder);
                            break;
                        case Db_Types.SPs:
                            dname.Text = db.Name + ":  Stored Procedures";
                            LoadSPs(db, workingFolder);
                            break;
                        case Db_Types.UserDefinedAggregates:
                            dname.Text = db.Name + ":  UserDefinedAggregates";
                            LoadUserDefinedAggregates(db, workingFolder);
                            break;
                        case Db_Types.UserDefinedFunctions:
                            dname.Text = db.Name + ":  UserDefinedFunctions";
                            LoadUserDefinedFunctions(db, workingFolder);
                            break;
                        case Db_Types.UserDefinedTypes:
                            dname.Text = db.Name + ":  UserDefinedTypes";
                            LoadUserDefinedTypes(db, workingFolder);
                            break;
                        case Db_Types.UserDefinedDataTypes:
                            dname.Text = db.Name + ":  UserDefinedDataTypes";
                            LoadUserDefinedDataTypes(db, workingFolder);
                            break;
                        case Db_Types.Tables:
                            dname.Text = db.Name + ":  Tables";
                            LoadTables(db, workingFolder);
                            break;
                        case Db_Types.Triggers:
                            dname.Text = db.Name + ":  DDL_Triggers";
                            LoadDdlTriggers(db, workingFolder);
                            break;
                        case Db_Types.Rules:
                            dname.Text = db.Name + ":  Rules";
                            LoadRules(db, workingFolder);
                            break;
                        case Db_Types.Views:
                            dname.Text = db.Name + ":  Views";
                            LoadViews(db, workingFolder);
                            break;
                        case Db_Types.Users:
                            dname.Text = db.Name + ":  Users";
                            LoadUsers(db, workingFolder);
                            break;
                        case Db_Types.Assembly:
                            dname.Text = db.Name + ":  Assembly";
                            LoadSqlAssembly(db, workingFolder);
                            break;
                        case Db_Types.SqlJobs:
                            dname.Text = db.Name + ":  Jobs";
                            LoadSqlJobs(db, workingFolder);
                            break;
                        default:
                            throw new Exception("DB Type not handled [" + dt.ToString() + "]");
                    }
                } catch (Exception ex) {
                    Logger.Log("ERROR in ReadAndWriteObjectsFromDatabaseToFile. " + ex.Message);
                    throw ex;
                }
            }
        }

        private void LoadSqlJobs(Database db, string workingFolder) {
            Logger.Log("SqlJobs for " + db.Name + " on " + serverName);
            if (db.Name.ToLower() == "msdb") {
                try{
                    Server svr = new Server(serverConn);

                    Logger.Log("Job Total = " + svr.JobServer.Jobs.Count.ToString());
                    foreach (Job job in svr.JobServer.Jobs) {
                        filename = ReplaceInvalidChars(job.Name);
                        oname.Text = "Processing SQL Job... " + filename;

                        FileStream file = CreateFileStream(workingFolder + filename + ".sql");
                        StreamWriter sw = new StreamWriter(file);
                        StringCollection sc = job.Script();
                        WriteToStream(sc, sw);

                        sw.Close();
                        file.Close();
                        objectCount++;
                    }
                } catch (Exception ex) {
                    Logger.Log("ERROR " + ex.Message);
                    throw ex;
                }
            }
        }

        private string ReplaceInvalidChars(string p) {
            string ret = p.Replace(":","");
            ret = ret.Replace(";", "");
            ret = ret.Replace("\\", "");
            ret = ret.Replace("/", "");
            ret = ret.Replace(",", "");

            return ret;
        }
        
       
        #region LoadSmoObjects

        private void LoadSqlAssembly(Database db, string workingFolder)
        {
            foreach (SqlAssembly assy in db.Assemblies)
            {
                filename = assy.Name;
                oname.Text = "Processing Assemblies... " + filename;
                FileStream file = CreateFileStream(workingFolder + filename + ".sql");
                StreamWriter sw = new StreamWriter(file);
                StringCollection sc = assy.Script();
                WriteToStream(sc, sw);

                sw.Close();
                file.Close();
                objectCount++;
            }
        }

        private void LoadUsers(Database db, string workingFolder)
        {
            foreach (User user in db.Users)
            {
                Console.Out.WriteLine("User- " + user.Name);
                filename = user.Name;
                filename = filename.Replace("\\", "");
                oname.Text = "Processing Users... " + filename;

                FileStream file = CreateFileStream(workingFolder + filename + ".sql");

                StreamWriter sw = new StreamWriter(file);
                StringCollection sc = user.Script();
                WriteToStream(sc, sw);

                sw.Close();
                file.Close();
                objectCount++;
            }
        }

        private FileStream CreateFileStream(string Filename) {
            // Files will already exist if they are already in Sourcesafe
            FileStream file = new FileStream(Filename, FileMode.Create, FileAccess.Write);

            return file;
        }

        private void LoadViews(Database db, string workingFolder)
        {
            try{
                List<string> views = GetNonSystemObjectNames(serverName, db.Name, eObjectTypes.view);

                foreach(string name in views){
                    Console.Out.WriteLine("View- " + name);
                    int pos = name.IndexOf(".");
                    string nameOnly = name.Substring(pos + 1);
                    string schema = name.Substring(0, pos);
                    
                    View view = db.Views[nameOnly];
                    if (view == null) {
                        // find it. I couldn't find out what the key should be when the object has a schema other than dbo
                        System.Collections.IEnumerator i = db.Views.GetEnumerator();

                        while (i.MoveNext()) {
                            View spo = (View)i.Current;
                            if (spo.Name == nameOnly && spo.Schema == schema) {
                                //Console.Out.WriteLine("Found");
                                view = spo;
                                break;
                            }
                        }
                    }
                    
                    filename = view.Schema + "." + view.Name;
                    filename = RemoveInvalidChars(filename);

                    oname.Text = "Processing Views... " + filename;
                    
                    FileStream file = CreateFileStream(workingFolder + filename + ".sql");
                    StreamWriter sw = new StreamWriter(file);
                    ScriptingOptions so = new ScriptingOptions();
                    so.IncludeIfNotExists = true;
                    StringCollection sc = view.Script(so);
                    foreach (string s in sc)
                        sw.WriteLine(s);
                    sw.Close();
                    file.Close();
                    objectCount++;
                }
            } catch (Exception ex) {
                throw ex;
            }

        }

        private string RemoveInvalidChars(string filename) {
            string ret = filename;
            
            foreach(char c in Path.GetInvalidFileNameChars()){
                ret = ret.Replace(c.ToString(),"");
            }
            // Sourcesafe doesn't like percents
            ret = ret.Replace("%", "pct");

            return ret;
        }

        private void LoadRules(Database db, string workingFolder)
        {
            foreach (Rule rule in db.Rules)
            {
                filename = rule.Schema + "." + rule.Name;
                oname.Text = "Processing Rules... " + filename;
                FileStream file = CreateFileStream(workingFolder + filename + ".sql");
                StreamWriter sw = new StreamWriter(file);
                StringCollection sc = rule.Script();
                WriteToStream(sc, sw);

                sw.Close();
                file.Close();
                objectCount++;
            }
        }

        private void LoadDdlTriggers(Database db, string workingFolder)
        {
            foreach (DdlTriggerBase trigger in db.Triggers){
                filename = trigger.Name;
                oname.Text = "Processing DDL Triggers... " + filename;
                FileStream file = CreateFileStream(workingFolder + filename + ".sql");
                StreamWriter sw = new StreamWriter(file);
                StringCollection sc = trigger.Script();
                WriteToStream(sc, sw);

                sw.Close();
                file.Close();
                objectCount++;
            }
        }

        private void LoadTables(Database db, string workingFolder)
        {
            try{
                List<string> tables = GetNonSystemObjectNames(serverName, db.Name, eObjectTypes.table);

                foreach(string name in tables){
                    Console.Out.WriteLine("Table- " + name);
                    int pos = name.IndexOf(".");
                    string nameOnly = name.Substring(pos + 1);
                    string schema = name.Substring(0, pos);

                    Table table = db.Tables[nameOnly];
                    if (table == null || table.Schema != schema) {
                        // find it. I couldn't find out what the key should be when the object has a schema other than dbo
                        System.Collections.IEnumerator i = db.Tables.GetEnumerator();

                        while (i.MoveNext()) {
                            Table spo = (Table)i.Current;
                            if (spo.Name == nameOnly && spo.Schema == schema) {
                                //Console.Out.WriteLine("Found");
                                table = spo;
                                break;
                            }
                        }
                    }

                    filename = table.Schema + "." + table.Name;
                    oname.Text = "Processing Tables... " + filename;

                    // For schema names that are Windows logins, we must remove the backslash
                    filename = filename.Replace("\\", "_");

                    // For one single file    FileStream file = new FileStream(workingFolder + "TriggersAll" + ".sql", FileMode.Append, FileAccess.Write);
                    // I've used the Create and not CreateNew as the file may already exist.
                    FileStream file = CreateFileStream(workingFolder + filename + ".sql");
                    StreamWriter sw = new StreamWriter(file);

                    // Options tell the script engine what to do
                    ScriptingOptions so = new ScriptingOptions();
                    so.ClusteredIndexes = true;
                    so.NonClusteredIndexes = true;
                    so.Indexes = true;
                    so.IncludeIfNotExists = false;
                    
                    StringCollection sc = table.Script(so);
                    
                    WriteToStream(sc, sw);

                    sw.WriteLine("GO\r\n");
                    if (table.Triggers.Count > 0) {
                        foreach (Trigger trig in table.Triggers) {
                            sc = trig.Script();
                            WriteToStream(sc, sw);
                        }
                        sw.WriteLine("GO\r\n");
                    }

                    sw.Close();
                    file.Close();
                    objectCount++;
                }
            } catch (Exception ex) {
                throw ex;
            }
        }

        private void LoadUserDefinedDataTypes(Database db, string workingFolder)
        {
            foreach (UserDefinedDataType udt in db.UserDefinedDataTypes)
            {
                filename = udt.Schema + "." + udt.Name;
                oname.Text = "Processing UDDTs... " + filename;
                FileStream file = CreateFileStream(workingFolder + filename + ".sql");
                StreamWriter sw = new StreamWriter(file);
                StringCollection sc = udt.Script();
                WriteToStream(sc, sw);

                sw.Close();
                file.Close();
                objectCount++;
            }
        }

        private void LoadUserDefinedTypes(Database db, string workingFolder)
        {
            foreach (UserDefinedType udt in db.UserDefinedTypes)
            {

                filename = udt.Schema + "." + udt.Name;
                oname.Text = "Processing UDTs... " + filename;
                FileStream file = CreateFileStream(workingFolder + filename + ".sql");
                StreamWriter sw = new StreamWriter(file);
                StringCollection sc = udt.Script();
                WriteToStream(sc, sw);

                sw.Close();
                file.Close();
                objectCount++;
            }
        }

        private void LoadUserDefinedFunctions(Database db, string workingFolder)
        {
            List<string> UDFs = GetNonSystemObjectNames(serverName, db.Name, eObjectTypes.function);

            foreach (string name in UDFs){
                int pos = name.IndexOf(".");
                string nameOnly = name.Substring(pos + 1);
                string schema = name.Substring(0, pos);
                UserDefinedFunction udf = db.UserDefinedFunctions[nameOnly];
                if (udf == null) {
                    // find it. I couldn't find out what the key should be when the object has a schema other than dbo
                    System.Collections.IEnumerator i = db.UserDefinedFunctions.GetEnumerator();

                    while (i.MoveNext()) {
                        UserDefinedFunction spo = (UserDefinedFunction)i.Current;
                        if (spo.Name == nameOnly && spo.Schema == schema) {
                            //Console.Out.WriteLine("Found");
                            udf = spo;
                            break;
                        }
                    }
                }

                filename = udf.Schema + "." + udf.Name;
                oname.Text = "Processing UDFs... " + filename;

                FileStream file = CreateFileStream(workingFolder + filename + ".sql");
                StreamWriter sw = new StreamWriter(file);
                StringCollection sc = udf.Script();
                WriteToStream(sc, sw);

                sw.Close();
                file.Close();
                objectCount++;
            }
        }

        private void LoadUserDefinedAggregates(Database db, string workingFolder)
        {
            foreach (UserDefinedAggregate uda in db.UserDefinedAggregates)
            {
                filename = uda.Schema + "." + uda.Name;
                oname.Text = "Processing UDAs... " + filename;
                FileStream file = CreateFileStream(workingFolder + filename + ".sql");
                StreamWriter sw = new StreamWriter(file);
                StringCollection sc = uda.Script();
                WriteToStream(sc, sw);

                sw.Close();
                file.Close();
                objectCount++;
            }
        }

        private enum eObjectTypes {procedure, view, table, function};

        private List<string> GetNonSystemObjectNames(string Server, string Database, eObjectTypes obType) {
            List<string> ret = new List<string>();

            SQLServer comm = new SQLServer(Server, Database, "", "");
            try
            {
                string sql = "SELECT Schema_Name(schema_id), name FROM sys.objects WHERE Type = '";
                switch (obType)
                {
                    case eObjectTypes.procedure:
                        sql += "P'";
                        break;
                    case eObjectTypes.table:
                        sql += "U'";
                        break;
                    case eObjectTypes.view:
                        sql += "V'";
                        break;
                    case eObjectTypes.function:
                        sql += "FN'";
                        break;
                }
                sql += " and is_ms_shipped = 0";
                SqlDataReader dr = comm.FetchDataReader(sql);
                while (dr.Read())
                {
                    ret.Add(dr[0] + "." + dr[1]);
                }

                return ret;}
            finally{
                comm.Close();
            }
        }
        private void LoadSPs(Database db, string workingFolder){
            try {
                List<string> sps = GetNonSystemObjectNames(serverName, db.Name, eObjectTypes.procedure);

                foreach (string name in sps) {
                    Console.Out.WriteLine("Proc- " + name);
                    int pos = name.IndexOf(".");
                    string nameOnly = name.Substring(pos + 1);
                    string schema = name.Substring(0, pos);
                    StoredProcedure sp = db.StoredProcedures[nameOnly];
                    if (sp == null) {
                        // find it. I couldn't find out what the key should be when the object has a schema other than dbo
                        System.Collections.IEnumerator i = db.StoredProcedures.GetEnumerator();
                        
                        while (i.MoveNext()) {
                            StoredProcedure spo = (StoredProcedure)i.Current;
                            if (spo.Name == nameOnly && spo.Schema == schema) {
                                //Console.Out.WriteLine("Found");
                                sp = spo;
                                break;
                            }
                        }
                    }
                    filename = sp.Schema + "." + sp.Name;
                    oname.Text = "Processing Stored Procedures... " + filename;

                    FileStream file = CreateFileStream(workingFolder + filename + ".sql");
                    StreamWriter sw = new StreamWriter(file);

                    StringCollection sc = sp.Script();
                    WriteToStream(sc, sw);

                    sw.Close();
                    file.Close();
                    objectCount++;
                }
            } catch (Exception ex) {
                throw ex;
            }
        }

        private void WriteToStream(StringCollection sc, StreamWriter sw) {
            foreach (string line in sc) {
                string togo = line;
                if (togo.EndsWith("\r\n")) {
                    togo = togo.Substring(0, togo.Length - 2);
                } 
                if (togo.StartsWith("\r\n")) {
                    togo = togo.Substring(2);
                }
                sw.WriteLine(line);
                sw.WriteLine("GO");
            }
        }

        private void LoadSynonym(Database db, string workingFolder)
        {
            foreach (Synonym synonym in db.Synonyms)
            {
                filename = synonym.Schema + "." + synonym.Name;
                oname.Text = "Processing Synonyms... " + filename;
                FileStream file = CreateFileStream(workingFolder + filename + ".sql");
                StreamWriter sw = new StreamWriter(file);
                StringCollection sc = synonym.Script();
                WriteToStream(sc, sw);

                sw.Close();
                file.Close();
                objectCount++;
            }
        }

        private void LoadDefaults(Database db, string workingFolder)
        {
            foreach (Default defaults in db.Defaults)
            {
                filename = defaults.Schema + "." + defaults.Name;
                oname.Text = "Processing Defaults... " + filename;
                FileStream file = CreateFileStream(workingFolder + filename + ".sql");
                StreamWriter sw = new StreamWriter(file);
                StringCollection sc = defaults.Script();
                WriteToStream(sc, sw);

                sw.Close();
                file.Close();
                objectCount++;
            }
        }

        private void LoadRoles(Database db, string workingFolder)
        {
            foreach (ApplicationRole approle in db.Roles)
            {
                filename = approle.Name;
                oname.Text = "Processing Roles... " + filename;
                FileStream file = CreateFileStream(workingFolder + filename + ".sql");
                StreamWriter sw = new StreamWriter(file);
                StringCollection sc = approle.Script();
                WriteToStream(sc, sw);

                sw.Close();
                file.Close();
                objectCount++;
            }
        }
        private void LoadAppRoles(Database db, string workingFolder)
        {
            foreach (ApplicationRole approle in db.ApplicationRoles)
            {
                filename = approle.Name;
                oname.Text = "Processing App Roles... " + filename;
                FileStream file = CreateFileStream(workingFolder + filename + ".sql");
                StreamWriter sw = new StreamWriter(file);
                StringCollection sc = approle.Script();
                WriteToStream(sc, sw);

                sw.Close();
                file.Close();
                objectCount++;
            }
        }

        private void LoadDatabaseRoles(Database db, string workingFolder)
        {
            foreach (DatabaseRole dbrole in db.Roles)
            {
                filename = dbrole.Name;
                oname.Text = "Processing DB Roles... " + filename;
                FileStream file = CreateFileStream(workingFolder + filename + ".sql");
                StreamWriter sw = new StreamWriter(file);
                StringCollection sc = dbrole.Script();
                WriteToStream(sc, sw);

                sw.Close();
                file.Close();
                objectCount++;
            }
        }
        #endregion
	}
}
