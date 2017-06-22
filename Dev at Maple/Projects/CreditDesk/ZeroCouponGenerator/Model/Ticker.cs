using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZeroCouponGenerator.Model {
    class Instrument {
        public Instrument() {
            Fields = new List<string>();
        }
        public string Ticker { get; set; }
        public List<string> Fields { get; set; }
        public string Tag { get; set; }
        public int ID { get; set; }

    }
}
