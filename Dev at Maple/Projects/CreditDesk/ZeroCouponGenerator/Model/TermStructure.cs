using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZeroCouponGenerator {
    class TermPoint {
        public DateTime PointDate;
        public DateTime PayDate;
        public double Rate;
        public double EffectiveRate;
        public int Term;
        public double YearFraction;
        public double Amount;

        public override string ToString() {
            return Term + ". " + PointDate.ToString("dd MMM yy") + ". " + Rate.ToString("0.000000");
        }
    }

    class TermStructure {
        public List<TermPoint> Points;
        public DayCountConvention DayCount;

        public int PointCount {
            get { return Points.Count; }
        }

        public TermStructure() {
            Points = new List<TermPoint>();
        }

        public void PrintStructure() {
            foreach (TermPoint point in Points) {
                string data = point.PointDate.ToString("dd MMM yy") + "\t";
                data += point.PayDate.ToString("dd MMM yy") + "\t";
                data += point.Rate.ToString("0.000000") + "\t";
                data += point.EffectiveRate.ToString("0.000000") + "\t";
                data += point.Amount.ToString("0.000000") + "\t";
                data += point.Term + "\t";
                data += point.YearFraction.ToString("0.000000");

                Console.Out.WriteLine(data);
            }
        }
    }
}
