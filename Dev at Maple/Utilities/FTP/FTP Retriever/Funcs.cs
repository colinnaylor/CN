using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Maple;
using System.Windows.Forms;

namespace FTP_Retriever
{
    class Funcs
    {
        internal static void ReportProblem(string Message)
        {
#if DEBUG
            MessageBox.Show(Message, "Error");
#else
            Notifier.Notify(Message);
#endif
        }

        internal static DateTime lastReport = DateTime.MinValue;

        internal static void ReportProblem(Exception ex)
        {
            if (DateTime.Now.Subtract(lastReport).TotalMinutes > Properties.Settings.Default.MinutesBetweenEmails)
            {
                lastReport = DateTime.Now;
#if DEBUG
                MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace, "Error");
#else
                Notifier.Notify(ex);
#endif
            }
        }


        internal static void EndSlash(ref string TextString, bool EndSlashRequired)
        {
            bool exists = TextString.EndsWith("\\");
            if (exists && !EndSlashRequired)
            {
                TextString = TextString.Remove(TextString.Length - 1);
            }
            else if (!exists && EndSlashRequired)
            {
                TextString = TextString + "\\";
            }
        }
    }
}
