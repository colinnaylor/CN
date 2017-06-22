using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SupportRequests {
    class LocalIssue {
        public string Key { get; set; }
        public string Summary { get; set; }
        public int Priority { get; set; }
        public int Status { get; set; }
        public string AssignedTo { get; set; }
        public DateTime LastEmailed { get; set; }

        public bool Report { get; set; }
    }
}
