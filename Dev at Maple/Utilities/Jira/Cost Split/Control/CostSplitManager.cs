using Cost_Split.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Maple;
using System.Data;

namespace Cost_Split.Controller {
    class CostSplitManager {

        //public DataTable WorkItems;
        public List<WorkItem> WorkItems;

        public void FetchWorkItems(DateTime FromDate, DateTime ToDate, string Employee){

            DataSet ret = Database.GetWorkSummary(FromDate, ToDate, Employee);

            WorkItems = new List<WorkItem>();

            foreach (DataRow row in ret.Tables[0].Rows) {
                WorkItem item = new WorkItem();
                item.BusinessOwner = row["BusinessOwner"].ToString();
                item.CreditTrading = row["CreditTrading"].ToString() == "True" ? "Y" : "";
                item.Employee = row["Employee"].ToString();
                item.Hours = (double)row["Hours"];
                item.Issue = row["Issue"].ToString();
                item.IssueSummary = row["IssueSummary"].ToString();
                item.LondonTrading = row["LondonTrading"].ToString() == "True" ? "Y" : "";
                item.StockLoan = row["StockLoan"].ToString() == "True" ? "Y" : "";
                item.WorkDate = row["WorkDate"].ToString();
                item.WorkDescription = row["Work Description"].ToString();

                WorkItems.Add(item);
            }

            // Set the values for the bool columns to Y and blank

            
        }

        public void AddWorkItemsToCache(DateTime workSince) {
            JiraClass jira = new JiraClass();
            //DateTime workSince = Database.FetchLatestUpdate();
            List<WorkItem> items;

            if (workSince == DateTime.Parse("1 Jul 14")) {
                items = jira.GetWorkItems();
            } else {
                items = jira.GetWorkItems(workSince);
            }

            // Remove any previous work items from the cache
            Database.RemoveWorkItemsSince(workSince);

            // Add in a place holder so that we don't keep retrieving Issues that we have already covered but
            // aren't yet in the WorkItems table because they have no work logged against them.
            Database.AddInWorkItemPlaceHolder();

            foreach (WorkItem item in items) {
                Database.UpdateWorkItem(item);

            }


        }
    }
}
