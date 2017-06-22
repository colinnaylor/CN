using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;

namespace Microsoft.Samples.SqlServer {
    class Logger {
        public static void Log(string data) {
            Maple.Logger.Log(data);
        }

        public static void LogError(string Note, Exception ex) {
            Log("ERROR " + Note);
            Log(ex.Message);
            Log(ex.StackTrace);
            while (ex.InnerException != null) {
                ex = ex.InnerException;
                Log(ex.Message);
                Log(ex.StackTrace);
            }
            string body = Maple.Notifier.GetExceptionMessages(ex);
            body += "\r\n\r\nSee log files for stack trace.\r\n\r\n";
            body += string.Format("This email has been sent from the {0} application.",
                Application.ProductName);
            Funcs.EmailReport("Script Database Error", body);
        }
    }
}
