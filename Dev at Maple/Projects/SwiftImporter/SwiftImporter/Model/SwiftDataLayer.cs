using System;
using Maple;
using SwiftImporterUI.Properties;

namespace SwiftImporterUI.Model
{
    internal class SwiftDataLayer : IDisposable
    {
        SwiftDataDataContext swiftData;

        internal SwiftDataLayer()
        {
            Connect();
        }

      

        internal void SaveSwiftStatement(SwiftStatement statement)
        {
            if (!IsStatementInDb(statement))
            {            
                swiftData.SwiftStatements.InsertOnSubmit(statement);
                swiftData.SubmitChanges();
            }
        }

        internal bool IsStatementInDb(SwiftStatement statement)
        {            
            return swiftData.StatementExists(statement);

        }

        #region Connection stuff

        private void Connect()
        {

            SQLServer db = null;
            string connStr;
            try
            {
                db = new SQLServer(Dsn);
                connStr = db.ConnectionString;
            }
            finally
            {
                if (db != null)
                    db.Close();
                db = null;
            }

            swiftData = new SwiftDataDataContext(connStr);
            swiftData.CommandTimeout = 0; //10 mins
        }

        public static string Dsn
        {
            get
            {
                return
#if DEBUG
    #if UAT
                Properties.Settings.Default.DSN_TEST_UAT;
    #else
                Settings.Default.DSN_TEST;
    #endif
#else
            Properties.Settings.Default.DSN_LIVE;
#endif
            }
        }

        private void Disconnect()
        {
            swiftData.Dispose();
            swiftData = null;
        }

        #endregion


        public void Dispose()
        {
            Disconnect();
        }
    }
}
