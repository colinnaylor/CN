using Maple;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Samples.SqlServer {
    class Funcs {
        static DateTime lastSent = DateTime.MinValue;

        internal static void EmailReport(string subject, string body) {
            string html = "<html><p>" + body + "</p></thml>";

            if (DateTime.Now.Subtract(lastSent).TotalMinutes > 60) {
#if DEBUG
                Notifier.Notify(Notifier.NotifyDestination.Email, Notifier.SeverityLevel.High, html, "colin.naylor@mpuk.com", subject);
#else
                Notifier.Notify(Notifier.NotifyDestination.Email, Notifier.SeverityLevel.High, html, "colin.naylor@mpuk.com", subject);
//                Notifier.Notify(Notifier.NotifyDestination.Email, Notifier.SeverityLevel.High, html, "DUOC@mpuk.com", subject);
#endif
                lastSent = DateTime.Now;
            }
        }
    }
}
