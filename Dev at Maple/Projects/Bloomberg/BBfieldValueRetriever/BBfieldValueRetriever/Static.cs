using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using BBfieldValueRetriever.Properties;
using Maple;
using Maple.Database;

namespace BBfieldValueRetriever
{
    public class Utils
    {
        public static DatabaseController DbController = new DatabaseController(new SQLServer(Settings.Default.DSN).ConnectionString);


    }

    public class Static
    {
        internal static void Email(string subject, string body)
        {
            subject = string.Format("{0} ({1} Instance)", subject, Settings.Default.EnvName);
            Notifier.DUOC(subject, body);
        }

        internal static void EmailException(string subject, string body, Exception ex)
        {
            string detail = Notifier.GetExceptionMessagesHTML(ex);
            detail += "\r\n\r\n" + ex.StackTrace;

            Email(subject, body + "\r\n\r\n" + detail);
        }

        public static string CleanValueReturnedFromBloomberg(string ticker, string fieldName, string input)
        {
            string ret = null;
            if (fieldName.Equals("LAST_UPDATE") || fieldName.Equals("MATURITY")
                || fieldName.EndsWith("_DT", StringComparison.OrdinalIgnoreCase))
            {
                DateTime outDate;
                if (DateTime.TryParseExact(input, "M/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out outDate))
                    ret = outDate.ToString("yyyyMMdd HH:mm:ss");
            }

            if (ret != null)
            {
                NLogger.Instance.Info("Datawarehouse value cleaned from: {0} to: {1} ({2} / {3})", input, ret, ticker, fieldName);
                return ret;
            }
            return input;
        }

        public static string[] SplitWithStringDelimeters(string inputString, char delimeter, char stringOpenDelimeter, char stringCloseDelimeter)
        {
            if (inputString.Contains(stringOpenDelimeter))
            {
                var originalStrings = new List<string>();
                var temp = inputString.Split(stringOpenDelimeter, stringCloseDelimeter);
                for (int i = 0; i < temp.Length - 1; i++)
                {
                    var originalString = string.Format("{0}{1}{2}", stringOpenDelimeter, temp[i + 1], stringCloseDelimeter);
                    var escapedString = originalString.Replace(delimeter, '|');
                    inputString = inputString.Replace(originalString, escapedString);
                    originalStrings.Add(originalString);
                }

                var tokens = inputString.Split(delimeter);

                for (int i = 0; i < tokens.Length; i++)
                {
                    foreach (var originalString in originalStrings)
                    {
                        var escapedString = originalString.Replace(delimeter, '|');
                        tokens[i] = tokens[i].Replace(escapedString, originalString);
                    }
                }
                return tokens;
            }
            return inputString.Split(delimeter);
        }
    }
}