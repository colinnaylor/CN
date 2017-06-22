using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Maple;
using Atlassian.Jira;
using System.IO;

namespace SupportRequests {
    enum JiraSRstatus { None, Open, InProgress, Closed, Paused, TransferredToMsukIT}

    class Program {

        const string FILENAME = "SRs.dat";
        static string itemsToWrite = "";
        static bool logOn = false;
        static bool debug = false;

        static void Main(string[] args) {
            Properties.Settings setting = Properties.Settings.Default;

            if(args.Length > 0)
            {
                Dictionary<string, bool> values = new Dictionary<string, bool>();
                foreach(string arg in args) { values.Add(arg.ToLower(), true); }
                
                // Set args that are there
                if (values.ContainsKey("debug")) debug = true;
                if (values.ContainsKey("log")) logOn = true;

            }

            try {
                Log("============ Starting at {0}".Args(DateTime.Now.ToString("HH:mm:ss")));

                string jUserID = Maple.Security.Decrypt(Properties.Settings.Default.JiraUser);
                string jPassword = Maple.Security.Decrypt(Properties.Settings.Default.JiraPw);
                string appName = "";

                appName = Environment.GetCommandLineArgs()[0];
                Console.WriteLine("");
                Console.WriteLine(" {0}. Checking support requests.".Args(appName));
                Console.WriteLine("");

                //string jqlString = "(project = \"Help MSUK\" AND resolution = Unresolved AND status in (Open, \"In Progress\", \"Transferred To IT MSUK\") AND \"Support Team\" = Dev)";
                string jqlString = "(project = \"Help MSUK\" AND resolution = Unresolved AND status in (Open, \"In Progress\", \"Transferred To IT MSUK\") AND \"Support Team\" = Dev)";

                Dictionary<string,LocalIssue> reportedItems = ReadLastReportItems();

                List<Issue> jiraIssues = GetJiraIssues(jqlString, jUserID, jPassword);
                Log("Issues found = {0}".Args(jiraIssues.Count));

                string toEmail = "";
                itemsToWrite = "";
                foreach (Issue iss in jiraIssues) {

                    toEmail += ProcessItem(iss, reportedItems);

                }

                if (toEmail != "") {
                    toEmail = WrapInTable(toEmail);

                    Notifier.SendEmail(EmailTarget(), "Outstanding SRs", toEmail, true);

                    WriteReportedItems();
                }
            } catch (Exception ex) {
                Notifier.SendEmail(EmailTarget(), "Support Request Notifier Problem", "Error:\r\n\r\n{0}\r\n\r\n{1}".Args(ex.Message, ex.StackTrace));
            }
        }

        private static string EmailTarget()
        {
            string to = Properties.Settings.Default.EmailTarget;

            if (debug) to = "colin.naylor@mpuk.com";

            return to;
        }

        private static List<Issue> GetJiraIssues(string jqlString, string jUserID, string jPassword)
        {
            List<Issue> ret = new List<Issue>();

            Jira jiraConn = new Jira("https://jira.de.maplebank.eu/", jUserID, jPassword);

            int retries = Properties.Settings.Default.JiraRetries;
            while (retries > 0)
            {
                try
                {
                    ret = jiraConn.GetIssuesFromJql(jqlString, 999).ToList();
                    retries = 0;
                }
                catch (Exception ex)
                {
                    Log("Exception in GetJiraIssues. {0}".Args(ex.Message));
                    retries--;
                }
            }

            return ret;
        }

        internal static void Log(string data)
        {
            if (logOn)
            {
                Logger.Log(data);
            }
        }

        private static string ProcessItem(Issue iss, Dictionary<string, LocalIssue> reportedItems) {
            string ret = "";

            Properties.Settings setting = Properties.Settings.Default;

            string key = iss.Key.ToString();

            // Has someone picked it up?
            if (iss.Assignee == null) {
                LocalIssue liss;

                // Did it exist last time we checked?
                if (reportedItems.ContainsKey(key)) {
                    liss = reportedItems[key];

                    double minutesSinceEmail = (DateTime.Now - liss.LastEmailed).TotalMinutes;
                    switch (iss.Priority.ToString()) {
                        case "1": // Blocker
                            if (minutesSinceEmail > setting.BlockerMinuteLimit) {
                                liss.Report = true;
                            }
                            break;
                        case "2": // Critical
                            if (minutesSinceEmail > setting.CriticalMinuteLimit) {
                                liss.Report = true;
                            }
                            break;
                        case "3": // Major
                            if (minutesSinceEmail > setting.MajorMinuteLimit) {
                                liss.Report = true;
                            }
                            break;
                        default:
                            if (minutesSinceEmail > setting.MinuteLimit) {
                                liss.Report = true;
                            }
                            break;
                    }
                } else {
                    // not reported before
                    Log("New issue found {0}".Args(iss.Key));

                    liss = new LocalIssue();
                    liss.Key = iss.Key.ToString();
                    liss.Summary = iss.Summary;
                    liss.Status = int.Parse(iss.Status.ToString());
                    liss.Priority = int.Parse(iss.Priority.ToString());
                    liss.AssignedTo = iss.Assignee == null ? "Unassigned" : iss.Assignee;
                    liss.Report = true;
                }

                if (liss.Report) {
                    Log("Need to report issue {0}".Args(liss.Key));
                    ret = "<tr><td><a href=\"https://jira.de.maplebank.eu/browse/{0}\">{0}</a></td><td>{1}</td><td>{2}</td><td>{3}</td><td>{4}</td></tr>".Args(
                        liss.Key, 
                        PriorityName(liss.Priority), 
                        StatusName(liss.Status), 
                        liss.AssignedTo,
                        liss.Summary);

                    // Lines are split into Key|Priority|Status|LastEmailed|Assignee|Summary
                    itemsToWrite += "{0}|{1}|{2}|{3}|{4}|{5}\r\n".Args(
                        liss.Key, 
                        liss.Priority, 
                        liss.Status, 
                        DateTime.Now.ToString("dd MMM yyyy HH:mm:ss"),
                        liss.AssignedTo,
                        liss.Summary);
                }
            }

            return ret;
        }

        private static string WrapInTable(string toEmail) {
            string ret = "<html><body><table border=\"1\">";
            ret += "<tr><th>SR</th><th>Priority</th><th>Status</th><th>Assigned</th><th>Summary</th></tr>";
            ret += "{0}</table></body></html>".Args(
                toEmail);

            // Add colour
            ret = ret.Replace("Blocker", "<b><Font color=\"Red\">Blocker");
            ret = ret.Replace("Critical", "<b><Font color=\"Red\">Critical");
            ret = ret.Replace("Major", "<Font color=\"Red\">Major");

            return ret;
        }

        private static void WriteReportedItems() {
            File.WriteAllText(FILENAME, itemsToWrite);
        }

        private static string StatusName(int StatusID) {
            string ret = "Unknown";

            switch (StatusID) {
                case 1:
                    ret = "Open";
                    break;
                case 2:
                    ret = "In Progress";
                    break;
                case 3:
                    ret = "Closed";
                    break;
                case 4:
                    ret = "Paused";
                    break;
                case 5:
                    ret = "Transferred to IT MSUK";
                    break;
            }

            return ret;
        }

        private static string PriorityName(int PriorityID)
        {
            string ret = "Unknown";

            switch (PriorityID)
            {
                case 1:
                    ret = "Blocker";
                    break;
                case 2:
                    ret = "Critical";
                    break;
                case 3:
                    ret = "Major";
                    break;
                case 4:
                    ret = "Minor";
                    break;
                case 5:
                    ret = "Trivial";
                    break;
                case 6:
                    ret = "Medium";
                    break;
                default:
                    ret = "Unknown";
                    break;
            }

            return ret;
        }

        private static Dictionary<string, LocalIssue> ReadLastReportItems() {
            Dictionary<string, LocalIssue> ret = new Dictionary<string, LocalIssue>();
            
            string fileName = FILENAME;
            if(debug) fileName = @"S:\dev\ApplicationServer\SupportRequestNotify\SRs.dat";

            if (File.Exists(fileName)) {
                foreach (string line in File.ReadAllLines(fileName)) {
                    // Lines are split into Key|Priority|Status|LastEmailed|Assignee|Summary
                    string[] fields = line.Split(new Char[] { '|' });

                    LocalIssue issue = new LocalIssue();
                    issue.Key = fields[0];
                    issue.Priority = int.Parse(fields[1]);
                    issue.Status = int.Parse(fields[2]);

                    DateTime le;
                    if(DateTime.TryParse(fields[3], out le))
                    {
                        issue.LastEmailed = le;
                    }
                    else
                    {
                        Log("Failed to parse DateTime {0}".Args(fields[3]));
                        issue.LastEmailed = DateTime.Now;
                    }
                    issue.AssignedTo = fields[4];
                    issue.Summary = fields[5];

                    ret.Add(issue.Key, issue);
                }

            }

            return ret;
        }
    }
}
