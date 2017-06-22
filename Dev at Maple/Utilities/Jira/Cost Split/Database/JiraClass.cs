using Atlassian.Jira;
using Cost_Split.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Maple;

namespace Cost_Split {
    class JiraClass {
        private Jira jiraConn = null;

        // Install-Package Atlassian.SDK -Version 2.2.0
        // https://www.nuget.org/packages/Atlassian.SDK/2.2.0

        private void InitialiseJira() {
            string jUserID = "colinn";
            string jPassword = "Friday13!";

            jiraConn = new Jira("https://jira.de.maplebank.eu/", jUserID, jPassword);

        }

        internal List<WorkItem> GetWorkItems(DateTime workSince) {
            List<WorkItem> ret = new List<WorkItem>();

            InitialiseJira();

            //workSince = DateTime.Parse("1 Aug 15");
            string jqlString = "(project = \"IT MSUK\" OR project = \"Help MSUK\") and updated >= \"{0}\"".Args(workSince.ToString("yyyy/MM/dd HH:mm"));
            //jqlString = "(issuekey=MSUKIT-447)";

            List<Issue> jiraIssues = jiraConn.GetIssuesFromJql(jqlString,1000).ToList();

            foreach (Issue issue in jiraIssues) {
                List<WorkItem> items = ReadWorkItems(issue);

                ret.AddRange(items);
            }

            return ret;
        }

        private List<WorkItem> ReadWorkItems(Issue issue) {
            List<WorkItem> ret = new List<WorkItem>();

            string businessOwner = "n/a";
            try {
                foreach (CustomField field in issue.CustomFields) {
                    if (field.Id.ToLower() == Properties.Settings.Default.BusinessOwnerTag) { // 14602
                        // Business Owner
                        businessOwner = field.Values[0];
                        break;
                    }
                }
                
            } catch {
                businessOwner = "n/a";
            }

            string[] costCentres = new string[0];
            try {
                costCentres = issue.CustomFields["Cost Centre"].Values;
            } catch {
                // leave empty
            }
            string creditTrading = IsCostCentre(costCentres, "Credit Trading") ? "Y" : "";
            string londonTrading = IsCostCentre(costCentres, "London Trading") ? "Y" : "";
            string stockLoan = IsCostCentre(costCentres, "Stock Loan") ? "Y" : "";

            foreach (Worklog work in issue.GetWorklogs()) {
                string worker = work.Author;
                double hours = work.TimeSpentInSeconds;

                WorkItem item = new WorkItem();
                item.ID = int.Parse(work.Id);
                item.BusinessOwner = businessOwner;
                item.CreditTrading = creditTrading;
                item.Employee = work.Author;
                item.Hours = work.TimeSpentInSeconds / 3600.0;
                item.Issue = issue.Key.ToString();
                item.IssueSummary = issue.Summary;
                item.LondonTrading = londonTrading;
                item.StockLoan = stockLoan;
                if (work.StartDate == null) {
                    item.WorkDate = "n/a";
                } else {
                    item.WorkDate = ((DateTime)work.StartDate).ToString("dd MMM yyyy");
                }
                item.WorkDescription = work.Comment.Replace("\n","").Replace("\r","");
                if (issue.Updated == null) {
                    item.UpdateTime = DateTime.Parse("1 Jan 2000");
                } else {
                    item.UpdateTime = (DateTime)issue.Updated;
                }

                ret.Add(item);
            }

            return ret;
        }

        private bool IsCostCentre(string[] costCentres, string CostCentre) {
            bool ret = false;

            foreach (string centre in costCentres) {
                if (centre.ToLower() == CostCentre.ToLower()) {
                    ret = true;
                    break;
                }
            }

            return ret;
        }


        internal List<WorkItem> GetWorkItems() {
            List<WorkItem> ret = new List<WorkItem>();
            DateTime day = DateTime.Parse("1 Jul 14");
            int jump = 15;

            InitialiseJira();

            while(day <= DateTime.Now){
                string jqlString = "(project = \"IT MSUK\" OR project = \"Help MSUK\") and updated >= \"{0}\"".Args(day.ToString("yyyy/MM/dd"));
                jqlString += " and updated < \"{0}\"".Args(day.AddDays(jump).ToString("yyyy/MM/dd"));

                List<Issue> jiraIssues = jiraConn.GetIssuesFromJql(jqlString, 1000).ToList();

                foreach (Issue issue in jiraIssues) {
                    List<WorkItem> items = ReadWorkItems(issue);

                    ret.AddRange(items);
                }

                day = day.AddDays(jump);
            };

            return ret;
        }
    }
}
