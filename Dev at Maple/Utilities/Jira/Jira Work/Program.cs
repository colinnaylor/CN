using Atlassian.Jira;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Maple;
using System.IO;
using System.Diagnostics;

namespace Jira_Connection {
    static class Program {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args) {
            // Install using:
            // Install-Package Atlassian.SDK -Version 2.2.0
            try {
                string jUserID = "colinn";
                string jPassword = "Friday13!";
                string toFile = "";
                string appName = "";

                appName = Environment.GetCommandLineArgs()[0];

                //DateTime workDay = DateTime.Parse("29 May 15");
                DateTime workSince = DateTime.Now.Date;
                string user = "colinn";
                if (args.Length > 0) {
                    user = args[0].Trim();

                    if (user.Contains("?") || user.Contains("help")) {
                        Console.WriteLine("");
                        Console.WriteLine(" Syntax:");
                        Console.WriteLine(" {0} [User] [Date Since] OutputFile.csv".Args(appName));
                        Console.WriteLine("");

                        return;
                    }
                }
                if (args.Length > 1) {
                    workSince = DateTime.Parse(args[1].Trim());
                }
                if (args.Length > 2 ){
                    toFile = args[2].Trim();
                }

                double totalHours = 0;

                Console.WriteLine("");
                Console.WriteLine(" Work for {0} since {1}".Args(user, workSince.ToString("dd MMM yyyy")));
                if(toFile != ""){
                    Console.WriteLine(" Output to a file name {0}".Args(toFile));
                }

                Jira jiraConn = new Jira("https://jira.de.maplebank.eu/", jUserID, jPassword);

                //string jqlString = "project = MSUKIT AND updated >= \"2015/05/29\"";
                string jqlString = "(project = \"IT MSUK\" OR project = \"Help MSUK\") and updated >= \"{0}\"  Order by \"Start Date\"".Args(workSince.ToString("yyyy/MM/dd"));

                List<Issue> jiraIssues = jiraConn.GetIssuesFromJql(jqlString, 999).ToList();

                List<WorkItem> workItems = new List<WorkItem>();

                foreach (Issue iss in jiraIssues) {
                    foreach (Worklog work in iss.GetWorklogs()) {
                        string worker = work.Author;
                        double hours = work.TimeSpentInSeconds;
                        hours = hours / 3600.0;

                        if (worker == user) {
                            DateTime dayOfWork = DateTime.MinValue;
                            if (work.StartDate.HasValue) {
                                dayOfWork = (DateTime)work.StartDate;
                                dayOfWork = dayOfWork.Date;
                            }
                            if (dayOfWork >= workSince) {
                                WorkItem wi = new WorkItem();

                                wi.WorkDay = dayOfWork;
                                wi.Hours = hours;
                                wi.BacklogCode = iss.Key.ToString();
                                wi.Summary = iss.Summary;
                                workItems.Add(wi);

                                totalHours += wi.Hours;
                            }
                        }
                    }
                }

                string output = "";
                output += " Day,Hours,Backlog,Summary\r\n";
                foreach (WorkItem wi in workItems) {
                    output += " {0},{1},{2},{3}\r\n".Args(wi.WorkDay.ToString("dd MMM yy"), wi.Hours.ToString("0.0"), wi.BacklogCode, wi.Summary);
                }
                output += "\r\n {0} hours.".Args(totalHours);

                if(toFile != ""){
                    File.WriteAllText(@"c:\Temp\Work.csv", output);
                    Process proc = new Process();
                    proc.StartInfo = new ProcessStartInfo(@"c:\Temp\Work.csv");
                    proc.Start();
                    Console.WriteLine("Output as \"c:\\Temp\\Work.csv\"");
                }else{
                    ConsoleColor orig = Console.ForegroundColor;
                    try {

                        Console.ForegroundColor = ConsoleColor.DarkYellow;
                        output = output.Replace("Day,Hours,Backlog,Summary", "Day,,Hours,Backlog,,Summary");
                        output = output.Replace(",", "\t");
                        Console.WriteLine(output);
                    } finally {
                        Console.ForegroundColor = orig;
                    }
                }

            } catch (Exception ex) {
                Console.WriteLine(" Error - {0}".Args(ex.Message));
            }
            //List<Project> projects = jiraConn.GetProjects().ToList();

            //Issue iss = jiraConn.GetIssue("MSUKIT-611");
            //List<Worklog> work = iss.GetWorklogs().ToList();


            //foreach (Worklog w in work) {
            //    Console.WriteLine(w.Author);
            //    Console.WriteLine(w.TimeSpent);
            //}

            //jqlString = PrepareJqlbyDates("2015-05-01", "2015-05-31");
            //jiraIssues = jiraConn.GetIssuesFromFilter("My Backlog").ToList();

            //IEnumerable<Issue> jiraIssues = jiraConn.GetIssuesFromJql(jqlString, 999);

            //foreach(var issue in jiraIssues)
            //{
            //     System.Console.WriteLine(issue.Key.Value +" -- "+ issue.Summary);
            //}

        }
        static string PrepareJqlbyDates(DateTime beginDate)
        {
            //string jqlString = "project = MSUKIT AND resolved >= "+beginDate+" AND resolved <= "+ endDate;
            string jqlString = "project = MSUKIT AND updated >= \"{0}\"".Args(beginDate.ToString("yyyy/MM/dd"));
            
            return jqlString;
        }

        static void AdHoc(string jUserID, string jPassword, DateTime workSince, string user) {
            Jira jiraConn = new Jira("https://jira.de.maplebank.eu/", jUserID, jPassword);

            //string jqlString = "project = MSUKIT AND updated >= \"2015/05/29\"";
            string jqlString = "(project = \"IT MSUK\" OR project = \"Help MSUK\") and issue".Args(workSince.ToString("yyyy/MM/dd"));

            List<Issue> jiraIssues = jiraConn.GetIssuesFromJql(jqlString, 999).ToList();

            List<WorkItem> workItems = new List<WorkItem>();

            foreach (Issue iss in jiraIssues) {
                double totalHours = 0;

                foreach (Worklog work in iss.GetWorklogs()) {
                    string worker = work.Author;
                    double hours = work.TimeSpentInSeconds;
                    hours = hours / 3600.0;

                    if (worker == user) {
                        DateTime dayOfWork = DateTime.MinValue;
                        if (work.StartDate.HasValue) {
                            dayOfWork = (DateTime)work.StartDate;
                            dayOfWork = dayOfWork.Date;
                        }
                        if (dayOfWork >= workSince) {
                            WorkItem wi = new WorkItem();

                            wi.WorkDay = dayOfWork;
                            wi.Hours = hours;
                            wi.BacklogCode = iss.Key.ToString();
                            wi.Summary = iss.Summary;
                            workItems.Add(wi);

                            totalHours += wi.Hours;
                        }
                    }
                }
            }

        }
    }
}
