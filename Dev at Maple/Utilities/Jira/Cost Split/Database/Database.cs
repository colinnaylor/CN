using Maple;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cost_Split {
    class Database {
        private static SQLServerConnection conn;

        private static void GetDbConn() {
            conn = new SQLServerConnection(Properties.Settings.Default.DSN);
        }

        internal static DateTime FetchLatestUpdate() {
            GetDbConn();
            string sql = "Select isnull(max(UpdateTime),'1 Jul 14') MaxTime From WorkItem";
            return DateTime.Parse(SQLServer.GetValue(conn, sql).ToString());
        }

        internal static void UpdateWorkItem(Model.WorkItem item) {
            GetDbConn();
            string sql = "EXEC UpdateWorkItem {0},'{1}','{2}','{3}',{4},'{5}',".Args(item.ID, item.BusinessOwner, item.WorkDate,
                item.Employee, item.Hours, item.Issue);
            sql += "'{0}','{1}',{2},{3},{4},'{5}'".Args(item.IssueSummary.Replace("'","''"),
                item.WorkDescription.Replace("'", "''"), 
                item.LondonTrading == "Y" ? 1 : 0,
                item.StockLoan == "Y" ? 1 : 0,
                item.CreditTrading == "Y" ? 1 : 0,
                item.UpdateTime.ToString("yyyyMMdd HH:mm:ss"));

            SQLServer.ExecSql(conn, sql);

        }

        internal static void AddInWorkItemPlaceHolder() {
            GetDbConn();
            string sql = "EXEC UpdateWorkItem {0},'{1}','{2}','{3}',{4},'{5}',".Args(0, "n/a", DateTime.Now.ToString("yyyyMMdd"),
                "n/a", 0, "LastUpdateTime");
            sql += "'{0}','{1}',{2},{3},{4},'{5}'".Args("Last update placeholder",
                "",
                0,
                0,
                0,
                DateTime.Now.AddMinutes(-5.0).ToString("yyyyMMdd HH:mm:ss"));

            SQLServer.ExecSql(conn, sql);

        }

        internal static System.Data.DataSet GetWorkSummary(DateTime FromDate, DateTime ToDate, string Employee) {
            GetDbConn();
            string sql;

            if (Employee.Trim() == "") {
                sql = "EXEC GetWorkSummary '{0}','{1}', null".Args(FromDate.ToString("yyyyMMdd"), ToDate.ToString("yyyyMMdd"));
            } else {
                sql = "EXEC GetWorkSummary '{0}','{1}','{2}'".Args(FromDate.ToString("yyyyMMdd"), ToDate.ToString("yyyyMMdd"), Employee);
            }

            System.Data.DataSet ret = SQLServer.FetchDataSet(conn, sql);

            return ret;
        }


        internal static string ServerDbconnection() {
            GetDbConn();
            string ret = SQLServer.GetValue(conn, "select @@servername + '.' + db_name()").ToString();
            return ret;
        }

        internal static void RemoveWorkItemsSince(DateTime workSince) {
            GetDbConn();
            string sql = "Delete WorkItem Where WorkDate >= '{0}'".Args(workSince.ToString("yyyyMMdd"));
            SQLServer.ExecSql(conn,sql);

        }
    }
}
