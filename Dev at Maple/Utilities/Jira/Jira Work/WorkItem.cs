using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jira_Connection {
    class WorkItem {
        public DateTime WorkDay { get; set; }
        public double Hours { get; set; }
        public string BacklogCode { get; set; }
        public string Summary { get; set; }
    }
}
