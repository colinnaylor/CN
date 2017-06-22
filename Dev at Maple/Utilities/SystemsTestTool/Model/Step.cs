using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SystemsTestTool.Model {
    internal enum StepType {Sql, Cmd }

    class Step {
        public int No { get; set; }
        public string Name { get; set; }
        public StepType Type { get; set; }
        public bool Admin { get; set; }
        public string Command { get; set; }

        public override string ToString() {
            return string.Format("{0}. {1}",No,Name);
        }
    }
}
