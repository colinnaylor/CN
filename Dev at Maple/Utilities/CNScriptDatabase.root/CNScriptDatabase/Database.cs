using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Samples.SqlServer {
    public class dbClass {
        public string Server { get; set; }
        public string Database { get; set; }
        public string VssIniFile { get; set; }
        public string VssLogin { get; set; }
        public string VssPw { get; set; }
        public string VssProjectParent { get; set; }
    }
}
