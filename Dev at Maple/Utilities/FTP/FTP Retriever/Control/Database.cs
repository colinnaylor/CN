using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Maple;
using FTP_Retriever.Properties;
using System.Data.SqlClient;
using System.Data;
using System.Xml;
namespace FTP_Retriever
{
    public class Database
    {
        internal static string FetchFtpDetail(ref Dictionary<int, FtpDetail> details)
        {
            string ret = "";
#if DEBUG
            SQLServer db = new SQLServer(Settings.Default.Test_FtpDSN);
#else
            SQLServer db = new SQLServer(Settings.Default.FtpDSN);
#endif

            string sql = "SELECT * FROM Detail";

            try
            {
                SqlDataReader dr = db.FetchDataReader(sql);

                while (dr.Read())
                {
                    int id = (int)dr["ID"];
                    string name = dr["Name"].ToString();

                    FtpDetail detail;
                    bool add = false;
                    if (details.ContainsKey(id))
                    {
                        detail = details[id];
                    }
                    else
                    {
                        detail = new FtpDetail(id, name);
                        add = true;
                    }

                    if (dr["ColIdentType"].ToString().ToLower() == "index")
                    {
                        detail.ColumnIdentifierType = FtpDetail.eColumnIdentifierType.Index;
                    }
                    else if (dr["ColIdentType"].ToString().ToLower() == "name")
                    {
                        detail.ColumnIdentifierType = FtpDetail.eColumnIdentifierType.Name;
                    }
                    detail.ColumnsRequired = dr["ColumnsRequired"].ToString();
                    detail.Delimiter = dr["Delimiter"].ToString();
                    detail.Folder = dr["Folder"].ToString();

                    string shareType = dr["TransferType"].ToString().ToLower();
                    switch (shareType)
                    {
                        case "ftp":
                            detail.FptType = FtpDetail.eFtpType.ftp;
                            break;
                        case "sftp":
                            detail.FptType = FtpDetail.eFtpType.sftp;
                            break;
                        case "unixshare":
                            detail.FptType = FtpDetail.eFtpType.UnixShare;
                            break;
                        case "https":
                            detail.FptType = FtpDetail.eFtpType.Https;
                            break;
                        default:
                            throw new Exception(string.Format("Bad share type read from database [{0}]", shareType));
                    }

                    detail.FtpLookupValue = dr["FtpLookupName"].ToString();
                    detail.TargetColumnNames = dr["TargetColumnNames"].ToString();
                    detail.TargetColumnTypes = dr["TargetColumnTypes"].ToString();
                    detail.TargetTable = dr["TargetTable"].ToString();
                    detail.ViewName = dr["ViewName"].ToString().Trim();

                    detail.PGP = dr["pgp"].ToString() == "True";
                    detail.ZIP = dr["zip"].ToString() == "True";
                    detail.DES = dr["des"].ToString() == "True";

                    object viewCreated = dr["ViewCreated"];
                    if (detail.ViewName != "" && viewCreated.ToString() == "")
                    {
                        CreateViewForFtpData(detail);
                    }


                    if (add)
                    {
                        details.Add(detail.ID, detail);
                    }
                }
                dr.Close();
            }
            finally
            {
                db.Close();
            }

            return ret;
        }

        private static void CreateViewForFtpData(FtpDetail detail)
        {
            string[] columns = detail.TargetColumnNames.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            List<string> columnTypes = detail.TargetColumnTypes.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries).ToList<string>();

            if (columns.Length != columnTypes.Count)
            {
                throw new Exception("Target column names and target column types must have the same count.");
            }

            string sql = string.Format("IF exists(select * from sys.objects where name = '{0}' and type = 'v') DROP VIEW {0};\r\n",
                detail.ViewName);
            ExecSql(sql, string.Format("Failed to drop existing view for {0} ftp item.", detail.Name));

            sql = string.Format("CREATE VIEW [{0}]\r\nAS\r\n\r\n", detail.ViewName);
            sql += "SELECT d.ID, r.ID RetrieveID, r.FileDate, dt.Row, ";

            int index = 0;
            foreach (string column in columns)
            {
                string type = columnTypes[index];
                if (type.Length > 6 && type.ToLower().Substring(0, 7) == "varchar"
                    || type.Length > 7 && type.ToLower().Substring(0, 8) == "datetime")
                {
                    // Leave as string
                    // As we are uncertain of the format that the dates may come in as we leave the dates as strings
                    sql += string.Format("c{0} as '{1}'", index + 1, column);
                }
                else
                {
                    // convert to new type
                    sql += string.Format("convert({0},c{1}) as '{2}'", columnTypes[index], index + 1, column);
                }
                index++;
                sql += ",\r\n";
            }
            sql = sql.Remove(sql.Length - 3); // take off trailing comma and newline
            sql += string.Format("\r\nFROM Detail d \r\n"
                + "JOIN Retrieve r ON r.DetailID = d.ID \r\n"
                + "JOIN {0} dt ON dt.RetrieveID = r.ID \r\n"
                + "WHERE d.Name = '{1}'", detail.TargetTable, detail.Name);

            ExecSql(sql, string.Format("Failed to create view for {0} ftp item.", detail.Name));

            sql = string.Format("UPDATE Detail SET ViewCreated = '{0}' WHERE ID = {1}", DateTime.Now.ToString("yyyyMMdd HH:mm:ss"), detail.ID);
            ExecSql(sql, string.Format("Failed to update Detail table with view creation time for {0} ftp item.", detail.Name));

        }

        private static void ExecSql(string sql, string failMessage)
        {
            SqlCommand cmd = new SqlCommand(sql);
            cmd.CommandType = CommandType.Text;

            ExecSql(cmd, failMessage);

        }

        private static void ExecSql(SqlCommand cmd, string failMessage)
        {
#if DEBUG
            SQLServer db = new SQLServer(Settings.Default.Test_FtpDSN);
#else
            SQLServer db = new SQLServer(Settings.Default.FtpDSN);
#endif
            try
            {
                string ret = db.ExecSql(cmd);

                if (ret != "")
                {
                    throw new Exception(failMessage + " " + ret + "\r\n\r\n" + cmd.CommandText);
                }

            }
            finally
            {
                db.Close();
            }
        }

        internal static string FetchRetrieveItems(Dictionary<int, FtpDetail> FtpDetails)
        {
            string ret = "";
            string sql = "SELECT * FROM Retrieve WHERE State = 'New' AND TimeToRetrieve < GetDate() ORDER BY TimeToRetrieve";

            foreach (FtpDetail d in FtpDetails.Values)
            {
                // Create the collection again so that we discard any unwanted, processed or old items
                d.RetrieveItems = new Dictionary<int, RetrieveItem>();
            }

#if DEBUG
            SQLServer db = new SQLServer(Settings.Default.Test_FtpDSN);
#else
            SQLServer db = new SQLServer(Settings.Default.FtpDSN);
#endif
            try
            {
                SqlDataReader dr = db.FetchDataReader(sql);

                while (dr.Read())
                {
                    int detailID = (int)dr["DetailID"];
                    if (FtpDetails.ContainsKey(detailID))
                    {
                        FtpDetail parent = FtpDetails[detailID];

                        int retrieveID = (int)dr["ID"];
                        string fileName = dr["Filename"].ToString();
                        DateTime fileDate = (DateTime)dr["FileDate"];
                        DateTime retrieveTime = (DateTime)dr["TimeToRetrieve"];
                        int attempt = int.Parse(dr["Attempt"].ToString());
                        bool parseOnly = (bool)dr["ParseOnly"];

                        RetrieveItem item = new RetrieveItem(parent, retrieveID, fileName, fileDate, retrieveTime);
                        item.ParseOnly = parseOnly;
                        item.Attempt = attempt;

                        parent.RetrieveItems.Add(retrieveID, item);
                    }
                    else
                    {
                        Funcs.ReportProblem(string.Format("FTP Detail ID [{0}] not found in collection.", detailID));
                    }

                }
                dr.Close();
            }
            finally
            {
                db.Close();
            }

            return ret;
        }

        internal static void SaveFileData(int RetrieveID, DataTable data, string targetTable)
        {
            string ret = "";

            int maxColumnCount = 0;

            var xmldoc = new XmlDocument();
            xmldoc.AppendChild(xmldoc.CreateElement("DataValues"));
            for (int r = 1; r <= data.RowCount; r++)
            {

                DataRow row = data.GetRow(r);
                if (row.ColumnCount > maxColumnCount) { maxColumnCount = row.ColumnCount; }

                var newRow = xmldoc.CreateElement("DataValue");
                XmlAttribute attr = xmldoc.CreateAttribute("Row");
                attr.Value = r.ToString();
                newRow.Attributes.Append(attr);

                for (int c = 1; c <= row.ColumnCount; c++)
                {
                    string col = "c" + c.ToString();

                    attr = xmldoc.CreateAttribute(col);
                    attr.Value = row.GetData(c);
                    newRow.Attributes.Append(attr);
                }
                xmldoc.DocumentElement.AppendChild(newRow);
            }

            SqlCommand cmd = new SqlCommand("DataUpdateInsert");
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add(new SqlParameter("@RetrieveID", SqlDbType.Int));
            cmd.Parameters.Add(new SqlParameter("@XML", SqlDbType.Xml));
            cmd.Parameters.Add(new SqlParameter("@ColumnCount", SqlDbType.Int));
            cmd.Parameters.Add(new SqlParameter("@TargetTable", SqlDbType.VarChar));

            cmd.Parameters["@RetrieveID"].Value = RetrieveID;
            cmd.Parameters["@XML"].Value = xmldoc.InnerXml;
            cmd.Parameters["@ColumnCount"].Value = maxColumnCount;
            cmd.Parameters["@TargetTable"].Value = targetTable;

            ExecSql(cmd, "Error when writing to database.\r\n" + ret);

        }

        public static void FetchLoginInfo(string LookupValue, out string ftpSite, out string userName, out string pw)
        {
            ftpSite = "";
            userName = "";
            pw = "";

            SQLServer db = new SQLServer(Settings.Default.FtpDetailServer, Settings.Default.FtpDetailDatabase, "", "");
            try
            {
                string sql = string.Format("SELECT Address, Username, Password FROM FTPsetting Where Location = '{0}'", LookupValue);

                SqlDataReader dr = db.FetchDataReader(sql);
                if (dr.Read())
                {
                    ftpSite = dr["Address"].ToString();
                    userName = dr["Username"].ToString();
                    pw = dr["Password"].ToString();
                }
                dr.Close();
            }
            finally
            {
                db.Close();
            }

            Maple.Logger.Log(string.Format("Fetched login details ({0}) for {1}", ftpSite, LookupValue));
        }

        public static void FetchLoginInfo(string LookupValue, out string ftpSite, out string userName, out string pw, out int portNumber)
        {
            FetchLoginInfo(LookupValue, out  ftpSite, out  userName, out pw);

            //if address ends with :number - set that number to be the port number and remove that string from the address.
            portNumber = -1;
            var ftpSiteParts = ftpSite.Split(':');
            if (ftpSiteParts.Length >= 2)
            {
                int portNumberOverride;
                if (int.TryParse(ftpSiteParts[ftpSiteParts.Length - 1], out portNumberOverride))
                {
                    portNumber = portNumberOverride;
                    ftpSite = ftpSite.Replace(":" + portNumber, "");
                }
            }
        }

        internal static void InsertRetrieveItem(int DetailID, string FileName, DateTime FileDate, DateTime FetchTime, bool ParseOnly, int Attempt)
        {

            string sql = string.Format("InsertRetrieveItem {0},'{1}','{2}','{3}',{4},{5}",
                DetailID, FileName, FileDate.ToString("yyyyMMdd"), FetchTime.ToString("yyyyMMdd HH:mm:ss"), ParseOnly, Attempt);

            ExecSql(sql, string.Format("Failed to insert new Retrieve item to database. DetailID {0}. Filename={1}", DetailID, FileName));
        }

        internal static void UpdateRetrieveItem(int RetrieveID, string State, string Note, double MillisecondsTaken)
        {
            int ms = (int)MillisecondsTaken;

            string sql = string.Format("UpdateRetrieveItem {0}, '{1}','{2}',", RetrieveID, State, Note.Replace("'", "''"));

            if (ms == -1)
            {
                sql += "null";
            }
            else
            {
                sql += ms.ToString();
            }

            ExecSql(sql, "Failed to update RetrieveItem. ");
        }

        internal static void SetItemForRetrieval(int ftpID, DateTime fileDate, string FileName)
        {
            ExecSql(string.Format("SetItemForRetrieval {0}, '{1}', '{2}'", ftpID, fileDate.ToString("yyyyMMdd"), FileName),
                string.Format("Failed to set item for retrieval. Detail ID {0}, File date {1}", ftpID, fileDate));
        }

        internal static void SaveFileList(int RetrieveID, Dictionary<string, Maple.FTP.FileDetail> files)
        {

            string xml = string.Format("UpdateFileList {0},'<DataValues>", RetrieveID);

            foreach (Maple.FTP.FileDetail file in files.Values)
            {
                xml += string.Format("<DataValue FileName=\"{0}\" FileTime=\"{1}\" FileSize=\"{2}\"/>",
                    file.Name, file.Timestamp.ToString("yyyyMMdd HH:mm:ss"), file.Size);
            }
            xml += "</DataValues>'";

            SqlCommand cmd = new SqlCommand("UpdateFileList");
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add(new SqlParameter("@RetrieveID", SqlDbType.Int));
            cmd.Parameters.Add(new SqlParameter("@XML", SqlDbType.Xml));

            string debug = xml.ToString();
            cmd.Parameters["@RetrieveID"].Value = RetrieveID;
            cmd.Parameters["@XML"].Value = xml.ToString();

            ExecSql(xml, string.Format("Failed to insert file list for RetrieveID {0}", RetrieveID));
        }

        internal static List<ScheduledItem> FetchNewScheduledItems()
        {
            List<ScheduledItem> ret = new List<ScheduledItem>();
#if DEBUG
            SQLServer db = new SQLServer(Settings.Default.Test_FtpDSN);
#else
            SQLServer db = new SQLServer(Settings.Default.FtpDSN);
#endif
            try
            {
                SqlDataReader dr = db.FetchDataReader("GetScheduledItems");

                while (dr.Read())
                {
                    ScheduledItem item = new ScheduledItem();

                    item.DetailID = (int)dr["DetailID"];
                    item.FileDate = DateTime.Parse(dr["FileDate"].ToString());
                    item.FileNameMask = dr["FileNameMask"].ToString();
                    item.FetchTime = DateTime.Parse(dr["FetchTime"].ToString());

                    ret.Add(item);
                }
                dr.Close();
            }
            finally
            {
                db.Close();
            }

            return ret;
        }
    }
}
