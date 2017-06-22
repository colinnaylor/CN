using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Cost_Split.Model {
    class WorkItem {
        public int ID { get; set; }
        public string BusinessOwner{ get; set; }
        public string WorkDate{ get; set; }
        public string Employee{ get; set; }
        public double Hours{ get; set; }
        public string Issue{ get; set; }
        public string IssueSummary{ get; set; }
        public string WorkDescription{ get; set; }
        public string LondonTrading{ get; set; }
        public string StockLoan { get; set; }
        public string CreditTrading{ get; set; }
        public DateTime UpdateTime { get; set; }
    }
}
