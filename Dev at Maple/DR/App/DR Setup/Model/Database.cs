using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Maple;

namespace DR_Setup.Model {
    class Database {
        internal string RunCommand(string Server, string cmd) {
            string ret = "";

            string server = Server;
            if (cmd.Length > 8 && cmd.Substring(0, 9).ToLower() == ":connect ") {
                // This is targeted against a specific server
                server = cmd.Substring(9, cmd.IndexOfAny(new Char[] { '\r', '\n' }) - 9);
                cmd = cmd.Substring(9 + server.Length);
            }
            SQLServer db = new SQLServer(server, "master", "", "");
            try {
                db.Timeout = 300;  // Some processes are long

                // Split commands that are seperated by a GO
                string[] lines = cmd.Split(new Char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                string sql = "";
                foreach (string line in lines){
                    if (line.Trim().ToUpper() == "GO"){
                        ret = db.ExecSql(sql);
                        sql = "";
                        if (ret != "") break;
                    }
                    else{
                        sql += line + "\r\n";
                    }
                }
                if (sql != "")
                {
                    ret = db.ExecSql(sql);
                }

            } catch (Exception ex) {
                ret = "Error: " + ex.Message;
            } finally {
                db.Close();
            }

            return ret;
        }

        internal class DbObject{
            public string Server{get; set;}
            public string Database{get; set;}
            public string ObjectName{get; set;}
            public string ObjectType { get; set; }
            public string LinkedServerName{get; set;}
        }

        internal List<DbObject> GetObjectsUsingLinkedServer(string server, string LinkedServerName) {
            List<DbObject> ret = new List<DbObject>();
            string sql = "SET NOCOUNT ON \r\n" +
                "CREATE TABLE #output(Name varchar(50), DbName varchar(50),ObjectName varchar(500),ObjectType varchar(2),LinkedServerName varchar(50) ) \r\n" +

                "Declare @Sql varchar(2000) ,@BaseSql varchar(2000) ,@Db varchar(50) ,@LinkedServerName varchar(50) \r\n" +

                "SET @LinkedServerName = '" + LinkedServerName + "' \r\n" +

                "SET @BaseSql = ' \r\n" +
                "begin \r\n" +
                "	SELECT DISTINCT @@Servername, db_name(), s.Name + ''.'' + o.name,o.type,''' + @LinkedServerName + '''  \r\n" +
                "	FROM sys.objects o  \r\n" +
                "	JOIN syscomments c on o.object_id = c.id  \r\n" +
                "   join sys.schemas s on s.schema_id = o.schema_id" +
                "	WHERE TEXT LIKE ''%' + @LinkedServerName + '.%''  \r\n" +
                "end' \r\n" +

                "Print @Sql \r\n" +

                "Declare db cursor for \r\n" +
                "Select Name from sys.databases  \r\n" +
                "where name not in ('master','tempdb','model','msdb','SystemInfo') \r\n" +
                "	and name not like '%[_]master' \r\n" +
                "	and name not like '%[_]msdb' \r\n" +

                "Open db \r\n" +
                "Fetch Next from db into @db \r\n" +

                "while @@Fetch_Status = 0 \r\n" +
                "begin \r\n" +
                "	raiserror('Querying the %s database.',10,1,@db) with nowait \r\n" +
                "	begin Try \r\n" +
                "		SET @Sql = 'Use ' + @db + '; \r\n" +
                "		' + @BaseSql \r\n" +
                
                "		INSERT #output \r\n" +
                "		exec(@Sql) \r\n" +
                "	end Try \r\n" +
                "	begin Catch \r\n" +
                "		INSERT #output \r\n" +
                "		SELECT @@Servername, @db, 'Error: ' + error_message(),'', @LinkedServerName \r\n" +
                "	end Catch \r\n" +
		
                "	Fetch Next from db into @db \r\n" +
                "end \r\n" +
                "close db \r\n" +
                "deallocate db \r\n" +

                "SELECT * FROM #output \r\n" +
                "DROP TABLE #output \r\n";

            SQLServer db = new SQLServer(server, "master", "", "");
            try {
                db.Timeout = 900;  // Some processes are long

                SqlDataReader dr = db.FetchDataReader(sql);

                while (dr.Read()) {
                    DbObject ob = new DbObject();

                    ob.Server = dr["Name"].ToString();
                    ob.Database = dr["DbName"].ToString();
                    ob.ObjectName = dr["ObjectName"].ToString();
                    ob.ObjectType = dr["ObjectType"].ToString().Trim().ToUpper();
                    ob.LinkedServerName = dr["LinkedServerName"].ToString();

                    ret.Add(ob);
                }
                dr.Close();

            } finally {
                db.Close();
            }


            return ret;
        }

        internal string GetObjectText(string Server, string Database, string ObjectName) {
            string ret = "";

            string sql = "Use " + Database + ";\r\n";
            sql += "EXEC sp_helptext [" + ObjectName + "]";

            SQLServer db = new SQLServer(Server, Database, "", "");
            try {
                SqlDataReader dr = db.FetchDataReader(sql);

                while(dr.Read()){
                    ret += dr["text"].ToString();
                }
                dr.Close();

            } finally {
                db.Close();
            }

            return ret;
        }

        internal string AlterObject(DbObject Ob, string ObjectText) {
            string ret = "";

            // In order to avoid changing the CREATE keyword to ALTER we grab the permissions, drop the object and recreate it with the
            // same text that was produced by sp_helptext 
            string permissionSql = GetPermissions(Ob);
            string dropSql = "";

            switch (Ob.ObjectType) {
                case "P":
                    dropSql = "DROP PROC [" + Ob.ObjectName + "]";
                    break;
                case "V":
                    dropSql = "DROP VIEW [" + Ob.ObjectName + "]";
                    break;
                case "FN":
                case "IF":
                    dropSql = "DROP FUNCTION [" + Ob.ObjectName + "]";
                    break;
                default:
                    throw new Exception(string.Format("Unhandled object type of {0} in AlterObject method.", Ob.ObjectType));
            }
            // Object name includes the schema so we need to add square brackets around the dot
            dropSql = dropSql.Replace(".", "].[");

            // Maybe a temporary entry, waiting for a decision on this
            bool ok = true;
            if (ObjectText.ToLower().Contains("openquery") && ObjectText.ToLower().Contains("remotedb.dbo.execkondorlive")) {
                ok = false;
            }

            if (ok) {
                SQLServer db = new SQLServer(Ob.Server, Ob.Database, "", "");
                try {
                    string err = db.ExecSql("BEGIN TRAN");
                    if (err == "") {
                        err = db.ExecSql(dropSql);
                    }
                    if (err == "") {
                        err = db.ExecSql(ObjectText);
                    }
                    if (err == "") {
                        err = db.ExecSql("COMMIT TRAN");
                    } else {
                        err += "\r\n" + db.ExecSql("IF @@trancount > 0 RollBack Tran");
                    }
                    if (err != "") {
                        ret += err;
                    } else {
                        if (permissionSql != "") {
                            ret += db.ExecSql(permissionSql);
                        }
                    }
                } finally {
                    db.ExecSql("ROLLBACK TRAN");
                    db.Close();
                }
            }
            return ret;
        }

        private string GetPermissions(DbObject Ob) {
            string ret = "";

            string sql = "SELECT 'GRANT ' + p.permission_name collate latin1_general_cs_as \r\n" +
                " + ' ON [' + s.name + '].[' + o.name + '] TO [' + pr.name + ']' as Line  \r\n" +
                "FROM sys.database_permissions AS p \r\n" +
                "INNER JOIN sys.objects AS o ON p.major_id=o.object_id \r\n" +
                "INNER JOIN sys.schemas AS s ON o.schema_id = s.schema_id \r\n" +
                "INNER JOIN sys.database_principals AS pr ON p.grantee_principal_id=pr.principal_id \r\n" +
                "WHERE o.Name = '" + Ob.ObjectName + "'";

            SQLServer db = new SQLServer(Ob.Server, Ob.Database, "", "");
            try {
                SqlDataReader dr = db.FetchDataReader(sql);

                while (dr.Read()) {
                    ret += dr["Line"].ToString() + "\r\n";
                }
                dr.Close();

            } finally {
                db.Close();
            }

            return ret;
        }
    }
}
