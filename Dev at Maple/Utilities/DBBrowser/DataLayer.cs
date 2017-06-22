
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Reflection;
using System.Text;
using DBBrowser.Properties;
using Maple;
using Maple.Database;

namespace DBBrowser
{
    public class DataLayer
    {
        internal DataLayer()
        {
        }

        private static DataSet GetData(string server, string database, string sql)
        {
            var db = new SQLServer(server, database, "", "")
            {
                Timeout = 0,
                NTauthentication = true
            };
            // no timeout

            DataSet ds;
            try
            {
                SqlCommand c = new SqlCommand(sql) { CommandType = CommandType.Text };
                ds = db.FetchData(c);
            }
            catch (Exception e)
            {
                Exception e2 = new Exception(sql, e);
                throw e2;
            }
            finally
            {
                db.Close();
            }
            return ds;
        }

        public static List<DBObject> GetDbObjectList()
        {

            //list of all objects acrosss servers and databases
            List<DBObject> ret = new List<DBObject>();

            //list of servers to look at... 
            StringCollection servers = Settings.Default.ServerList;

            //connect to the tempdb db on each server (as we should have perms on this!) to get db list
            foreach (string server in servers)
            {
                //each database
                foreach (var database in new DatabaseController(server, "tempdb").GetList<string>("select name from sys.databases where name not in ('master','tempdb','model','msdb')"))
                {
                    //basic objects
                    //if boss2000, get the last access time as well.
                    if (server.EndsWith("MINKY", StringComparison.OrdinalIgnoreCase) && database.Equals("BOSS2000", StringComparison.OrdinalIgnoreCase))
                        ret.AddRange(new DatabaseController(server, database).GetObjects<DBObject>(string.Format("select '{0}' as [Server], '{1}' as [Database],  Type, NAME, 'last exec: ' + convert(varchar(max),lastaccess ,23) as ExtendedInfo from sys.objects o left join usagestat s on s.objectid = o.object_id where type in ('U','P','FN','V','IF','TF')", server, database)));
                    else
                        ret.AddRange(new DatabaseController(server, database).GetObjects<DBObject>(string.Format("select '{0}' as [Server], '{1}' as [Database],  Type, NAME from sys.objects  where type in ('U','P','FN','V','IF','TF')", server, database)));

                    //more details- columns
                    ret.AddRange(new DatabaseController(server, database).GetObjects<DBObject>(string.Format("SELECT '{0}' as [Server], '{1}' as [Database], 'C' as [Type], TABLE_NAME as ExtendedInfo, COLUMN_NAME as Name FROM INFORMATION_SCHEMA.COLUMNS", server, database)));
                }

                //add sql agent jobs
                StringBuilder sql = new StringBuilder();
                sql.Append("SELECT j.NAME + ' / ' + s.step_name AS NAME, \n");
                sql.Append("       s.command ,\n");
                sql.AppendFormat(" '{0}' as Server, ", server);
                sql.Append(" 'J' as Type ");
                sql.Append("FROM   msdb.dbo.sysjobs j \n");
                sql.Append("       INNER JOIN msdb.dbo.sysjobsteps s \n");
                sql.Append("               ON s.job_id = j.job_id \n");

                ret.AddRange(new DatabaseController(server, "tempdb").GetObjects<DBObject>(sql.ToString()));
            }

            //ssrs objects
            var sqlFile = new Uri(Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase) +
                "\\sql\\GetDBObjectListFromReportingServer.sql");
            var sqlString = File.ReadAllText(sqlFile.AbsolutePath);

            foreach (var dbObject in new DatabaseController(Settings.Default.SSRSConnectionString).GetObjects<DBObject>(sqlString))
            {
                dbObject.Server = "(SSRS) " + Settings.Default.SSRSConnectionString.Split(';')[0].Split('=')[1];
                dbObject.Database = Settings.Default.SSRSConnectionString.Split(';')[1].Split('=')[1];
                ret.Add(dbObject);
            }

            return ret;
        }

        public static List<DBObject> GetDbObjectExtendedList(string searchTerm)
        {
            //list of all objects acrosss servers and databases
            List<DBObject> ret = new List<DBObject>();

            //list of servers to look at... 
            StringCollection servers = Settings.Default.ServerList;

            //connect to the tempdb db on each server (as we should have perms on this!) to get db list
            foreach (string server in servers)
            {
                foreach (DataRow drdbname in GetData(server, "tempdb", "select name from sys.databases where name not in ('master','tempdb','model','msdb')").Tables[0].Rows)
                {
                    string database = drdbname["NAME"].ToString();

                    if (server.EndsWith("MINKY", StringComparison.OrdinalIgnoreCase) && database.Equals("BOSS2000", StringComparison.OrdinalIgnoreCase))
                        ret.AddRange(new DatabaseController(server, database).GetObjects<DBObject>(string.Format("select distinct '{0}' as [Server], '{1}' as [Database], sc.name as Owner, o.TYPE,o.NAME, 'last exec: ' + convert(varchar(max),lastaccess ,23) as ExtendedInfo from sys.objects o join sys.schemas sc on o.schema_id = sc.schema_id join syscomments c on o.object_id=c.id left join usagestat s on s.objectid = o.object_id where text like '%{2}%'", server, database, searchTerm)));
                    else
                        ret.AddRange(new DatabaseController(server, database).GetObjects<DBObject>(string.Format("select distinct '{0}' as [Server], '{1}' as [Database], s.name as Owner, o.TYPE,o.NAME from sys.objects o join sys.schemas s on o.schema_id = s.schema_id join syscomments c on o.object_id=c.id where text like '%{2}%'", server, database, searchTerm)));
                }


                //add sql agent jobs
                StringBuilder sql = new StringBuilder();

                sql.Append("SELECT j.NAME + ' / ' + s.step_name AS NAME, \n");
                sql.Append("       s.command \n");
                sql.Append("FROM   msdb.dbo.sysjobs j \n");
                sql.Append("       INNER JOIN msdb.dbo.sysjobsteps s \n");
                sql.Append("               ON s.job_id = j.job_id \n");
                sql.AppendFormat("WHERE  command LIKE '%{0}%' \n", searchTerm);
                sql.AppendFormat("        OR step_name LIKE '%{0}%' \n", searchTerm);
                sql.AppendFormat("        OR NAME LIKE '%{0}%'", searchTerm);

                foreach (DataRow drobjectname in GetData(server, "tempdb", sql.ToString()).Tables[0].Rows)
                {
                    ret.Add(new DBObject
                    {
                        Server = server,
                        Database = "",
                        Name = drobjectname["NAME"].ToString(),
                        Type = "J"
                    });
                }



            }

            //ssrs objects
            var sqlFile = new Uri(Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase) +
                    "\\sql\\GetDBObjectListFromReportingServerDeepSearch.sql");

            var sqlString = string.Format(File.ReadAllText(sqlFile.AbsolutePath), searchTerm);
            using (var sqlConnection = new SqlConnection(Settings.Default.SSRSConnectionString))
            {
                using (var sqlCommand = new SqlCommand(sqlString, sqlConnection))
                {
                    sqlConnection.Open();
                    using (var reader = sqlCommand.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ret.Add(new DBObject
                            {
                                Server = "(SSRS) " + Settings.Default.SSRSConnectionString.Split(';')[0].Split('=')[1],
                                Database = Settings.Default.SSRSConnectionString.Split(';')[1].Split('=')[1],
                                Name = reader["NAME"].ToString(),
                                Type = reader["TypeDescription"].ToString()

                            });

                        }
                    }
                }
            }


            return ret;
        }

        public static string GetDbObjectDefinition(DBObject o)
        {
            StringBuilder sb = new StringBuilder();
            DataSet ds;

            if (o.Type == "J")
            {
                var jobname = o.Name.Split(new[] { " / " }, StringSplitOptions.None)[0];
                var jobstep = o.Name.Split(new[] { " / " }, StringSplitOptions.None)[1];
                StringBuilder sql = new StringBuilder();
                sql.Append("SELECT \n");
                sql.Append("       s.command \n");
                sql.Append("FROM   msdb.dbo.sysjobs j \n");
                sql.Append("       INNER JOIN msdb.dbo.sysjobsteps s \n");
                sql.Append("               ON s.job_id = j.job_id \n");
                sql.AppendFormat("WHERE name = '{0}' \n", jobname);
                sql.AppendFormat("and step_name = '{0}' \n", jobstep);

                return GetData(o.Server, o.Database, sql.ToString()).Tables[0].Rows[0][0].ToString();
            }
            if (o.Server.ToLower().Contains("ssrs"))
            {

                var sqlFile = new Uri(Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase) +
                                      "\\sql\\GetDBObjectDefinitionFromReportingServer.sql");

                var sql = string.Format(File.ReadAllText(sqlFile.AbsolutePath), o.Name);
                using (var sqlConnection = new SqlConnection(Settings.Default.SSRSConnectionString))
                {
                    using (var sqlCommand = new SqlCommand(sql, sqlConnection))
                    {
                        sqlConnection.Open();
                        return (string)sqlCommand.ExecuteScalar();
                    }
                }
            }

            try
            {
                ds = GetData(o.Server, o.Database, string.Format("exec sp_helptext [{0}.{1}]", o.Owner, o.Name));
            }
            catch (Exception)
            {

                return "No text available for this object type";
            }

            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                if (dr["Text"].ToString().Length > 0 && dr["Text"].ToString() != "\r\n")
                    sb.Append(dr["Text"]);
            }

            return sb.ToString();
        }

    }
}
