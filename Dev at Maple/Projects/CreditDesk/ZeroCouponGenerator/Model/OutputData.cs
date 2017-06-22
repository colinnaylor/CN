using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZeroCouponGenerator {
    class OutputPoint {
        public OutputPoint(DateTime Date, int Term, double Rate) {
            date = Date;
            term = Term;
            rate = Rate;
        }

        private DateTime date;
        public DateTime Date {
            get { return date; }
        }

        private int term;
        public int Term {
            get { return term; }
        }

        private double rate;
        public double Rate {
            get { return rate; }
        }
    }

    class OutputData {
        public List<OutputPoint> Rates;
    }
}
