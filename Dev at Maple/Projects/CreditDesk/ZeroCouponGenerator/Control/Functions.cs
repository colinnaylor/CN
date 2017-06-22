using ZeroCouponGenerator.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Data.SqlClient;


namespace ZeroCouponGenerator
{
    class Functions
    {
        internal static DateTime GetWorkingDayDate(int workingDays, DateTime Start, Dictionary<DateTime, DateTime> Holidays, NextWorkingDayConvention nd)
        {
            int moveDay;
            int startMonth;
            int days = 0;
            DateTime workDay;


            // Adjust for preceeding working days
            if (workingDays < 0)
            {
                // We are moving backwards, just like H. G. Wells
                moveDay = -1;
            }
            else
            {
                moveDay = 1;
            }
            workDay = Start;
            startMonth = Start.Month;

            while (days != workingDays)
            {
                workDay = workDay.AddDays(moveDay);

                // Is it a weekend
                if (workDay.DayOfWeek != DayOfWeek.Saturday && workDay.DayOfWeek != DayOfWeek.Sunday)
                {
                    // Is it a Holiday
                    if (!Holidays.ContainsKey(workDay))
                    {
                        // It is a working day. Increment our counter */
                        days += moveDay;
                    }
                }
            }

            if (nd == NextWorkingDayConvention.eNextWorkDayUnlessNewMonth)
            {
                // Check to see if it has gone to the next month
                if (workDay.Month != startMonth)
                {
                    // It did, oh dear, we didn't want that to happen !
                    // We need the last business day of the month we were in
                    workDay = GetWorkingDayDate(-1, DateTime.Parse("1 " + workDay.ToString("MMM yyyy")), Holidays, nd);
                }
            }

            return workDay;
        }

        internal static Dictionary<DateTime, DateTime> GetHolidays(string country)
        {
            var ret = new Dictionary<DateTime, DateTime>();
            using (var conn = new SqlConnection(Settings.Default.MMAConnectionString))
            {

                using (var command = new SqlCommand(
                    string.Format("select holiday from tblholiday where HolidayCentre = '{0}'", country), conn))
                {

                    conn.Open();

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ret.Add(reader.GetDateTime(0), reader.GetDateTime(0));
                        }
                    }
                }
            }
            return ret;
        }


        internal static int DaysBetween(DateTime StartDate, DateTime EndDate, DayCountConvention dayCountConvention)
        {
            int ret = 0;
            int day1, day2, months, years;

            switch (dayCountConvention)
            {
                case DayCountConvention.eActual_360:
                case DayCountConvention.eActual_365:
                case DayCountConvention.eActual_Actual:
                case DayCountConvention.eActualNL_365:
                    ret = (int)EndDate.Subtract(StartDate).TotalDays;
                    break;
                default:
                    // Gets the No of days between when the convention is something over 360
                    years = (EndDate.Year - StartDate.Year) * 360;
                    months = (EndDate.Month - StartDate.Month) * 30;
                    day1 = StartDate.Day;
                    day2 = EndDate.Day;
                    if (day1 == 31)
                    {
                        day1 = 30;
                    }

                    switch (dayCountConvention)
                    {
                        case DayCountConvention.e30_360:
                            if (day2 == 31 && day1 == 30) { day2 = 30; }
                            break;
                        case DayCountConvention.e30E_360:
                            if (day2 == 31) { day2 = 30; }

                            break;
                    }

                    ret = years + months + (day2 - day1);
                    break;
            }

            return ret;

        }

        internal static int DaysInYear(DayCountConvention dayCount)
        {
            int ret = 0;
            switch (dayCount)
            {
                case DayCountConvention.e30_360:
                case DayCountConvention.e30E_360:
                case DayCountConvention.e30E1_360:
                case DayCountConvention.eActual_360:
                    ret = 360;
                    break;
                case DayCountConvention.eActual_365:
                case DayCountConvention.eActual_Actual:
                case DayCountConvention.eActualNL_365:
                    ret = 365;
                    break;
            }
            return ret;
        }

        internal static double AveragePrice(double bid, double ask)
        {
            if (bid == 0)
            {
                return ask;
            }
            else if (ask == 0)
            {
                return bid;
            }
            else
            {
                return (bid + ask) / 2.0;
            }
        }

        /// <summary>
        /// Finds a polynomial curve that best fits the point curve passed in
        /// </summary>
        /// <param name="points">The rate and term point curve to match the polynomail curve to</param>
        /// <param name="polyOrder"></param>
        /// <returns></returns>
        internal static double[,] GetPolynomialCurve(double[,] points, int degrees)
        {
            MathUtils mu = new MathUtils();

            mu.PrintMatrix(points, "points");

            double[,] polyCurve = new double[points.GetUpperBound(0) + 1, points.GetUpperBound(1) + 1];

            int numPoints = points.GetUpperBound(0) + 1;
            double[,] mat1 = new double[numPoints, degrees + 1];
            double[,] mat2;
            double[,] matMult = new double[degrees + 1, degrees + 1];
            double[,] ratesMult = new double[degrees + 1, 1];
            double[,] rates = new double[numPoints, 1];

            try
            {
                // Create matrix 1 by increasing the term by each power of degree
                for (int counter = 0; counter < numPoints; counter++)
                {
                    // Set vectorToProcess with the rates
                    rates[counter, 0] = points[counter, 0];
                    // set the first colum of the matrixToProcess with 1s (term to the power of zero)
                    mat1[counter, 0] = 1;

                    for (int counter2 = 0; counter2 < degrees; counter2++)
                    {
                        mat1[counter, counter2 + 1] = Math.Pow(points[counter, 1], (double)counter2 + 1);
                    }
                }

                // mat2 is the same as mat1 but transposed
                mat2 = Tranpose(mat1);

                mu.PrintMatrix(mat1, "mat1");
                mu.PrintMatrix(mat2, "mat2");

                // Multipy matrix 1 by the transposed matrix 2
                mu.MultiplyMatrix(mat1, mat2, out matMult);

                mu.PrintMatrix(matMult, "matMult");

                //mu.PrintMatrix(rates, "rates");
                // Now multiply the rates by the transposed matrix 2
                mu.MultiplyMatrix(rates, mat2, out ratesMult);
                mu.PrintMatrix(ratesMult, "ratesMult");

                // Inverse the multiplied matrix
                double[,] results = mu.Inverse(matMult);
                //mu.PrintMatrix(results, "results");

                // And finally multiply the multiplied rates by the inversed multiplied matrix
                mu.MultiplyMatrix(ratesMult, results, out polyCurve);
                //mu.PrintMatrix(polyCurve, "polyCurve");

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return polyCurve;
        }

        private static double[,] Tranpose(double[,] matrix)
        {
            int rows = matrix.GetUpperBound(0) + 1;
            int cols = matrix.GetUpperBound(1) + 1;
            double[,] ret = new double[cols, rows];

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    ret[j, i] = matrix[i, j];
                }
            }

            return ret;
        }

        internal static double[,] FitToPolynomialCurve(double[] terms, double[,] polyCurve)
        {
            double[,] ret = new double[terms.Length, 2];
            int degree = polyCurve.GetUpperBound(0);

            for (int p = 0; p < terms.Length; p++)
            {
                double val = polyCurve[0, 0];
                //Console.WriteLine(val.ToString());

                double t = terms[p];
                for (int pwr = 1; pwr < degree + 1; pwr++)
                {
                    Console.WriteLine(polyCurve[pwr, 0] * Math.Pow(terms[p], pwr));
                    val += polyCurve[pwr, 0] * Math.Pow(terms[p], pwr);
                }

                ret[p, 0] = terms[p];
                ret[p, 1] = val;
            }

            return ret;
        }

        internal static double GetRateFromCurve(int Term, List<Rate> curveData)
        {
            double ret = 0;
            int index = 0;
            Rate a, b; // before and after

            if (curveData.Count == 0)
            {
                // do nothing, ret is already zero
            }
            else if (curveData.Count == 1 || Term == 0)
            {
                ret = curveData[0].SpotRate;
            }
            else if (Term > curveData[curveData.Count - 1].Term)
            {
                ret = curveData[curveData.Count - 1].SpotRate;
            }
            else
            {
                for (int i = 1; i < curveData.Count; i++)
                {
                    index = i;
                    if (curveData[i].Term > Term)
                    {
                        break;
                    }
                }

                a = curveData[index];
                b = curveData[index - 1];
                if (b.Term == Term)
                {
                    ret = b.SpotRate;
                }
                else
                {
                    int daysAfter = Term - b.Term;
                    int daysBetween = a.Term - b.Term;

                    ret = b.SpotRate + ((a.SpotRate - b.SpotRate) * daysAfter / daysBetween);
                }
            }

            return ret;
        }

        internal static double GetDiscountFactor(int Term, List<Rate> Rates, DayCountConvention dayCount)
        {
            double ret = 0;

            double daysInYr = DaysInYear(dayCount);
            double discountRate = GetRateFromCurve(Term, Rates);

            ret = GetDiscountFactorFromRate(discountRate, Term, daysInYr);

            return ret;
        }

        private static double GetDiscountFactorFromRate(double Rate, int Term, double DaysInYear)
        {
            double yearFrac = Term / DaysInYear;
            return 1 / Math.Pow(1 + (Rate / 100), yearFrac);
        }

        internal static double GetRateFromDiscountFactor(double DiscountFactor, int Term, double DaysInYear)
        {
            double yearFrac = Term / DaysInYear;
            return (1 / Math.Pow(DiscountFactor, (1 / yearFrac))) - 1;
        }

        internal static TermStructure GetTermStructure(DateTime Maturity, DateTime StartDate, DateTime FirstCouponDate, double CouponRate,
            DayCountConvention dayCount, NextWorkingDayConvention nextDay, int Freq, Dictionary<DateTime, DateTime> Holidays,
            bool FixedCashPayments)
        {

            TermStructure ts = CalcCurveFromMaturity(Maturity, StartDate, CouponRate, nextDay, Freq, Holidays, false);
            ts.DayCount = dayCount;

            // Calc daycount depending on accrual rule
            int yearDays = DaysInYear(dayCount);

            int point;
            TermPoint tp;
            TermPoint ltp;
            for (point = 1; point < ts.PointCount; point++)
            {
                tp = ts.Points[point];
                ltp = ts.Points[point - 1];

                tp.Term = DaysBetween(ltp.PointDate, tp.PointDate, dayCount);

                if (FixedCashPayments)
                {
                    int lastYear = (int)tp.PointDate.Subtract(tp.PointDate.AddYears(-1)).TotalDays;
                    if (tp.Term <= yearDays || lastYear == -366)
                    {
                        tp.EffectiveRate = tp.Rate / Freq * yearDays / tp.Term;
                    }
                    else
                    {
                        tp.EffectiveRate = tp.Rate;
                    }
                }
                else
                {
                    tp.EffectiveRate = tp.Rate;
                }
            }

            point = 0;
            while (point > -1 && ts.PointCount > 1)
            {
                tp = ts.Points[point];
                if (tp.PointDate < StartDate)
                {
                    tp.PointDate = StartDate;
                    tp = ts.Points[point + 1];
                    tp.Term = DaysBetween(StartDate, tp.PointDate, dayCount);
                }
                else if (tp.PointDate > StartDate && tp.PointDate < FirstCouponDate && point != 0)
                {
                    ts.Points.Remove(tp);

                    if (point < ts.PointCount && point > 0)
                    {
                        tp = ts.Points[point];
                        ltp = ts.Points[point - 1];
                        tp.Term = DaysBetween(ltp.PointDate, tp.PointDate, dayCount);
                    }

                    point--;
                }
                else if (tp.PointDate >= FirstCouponDate)
                {
                    point = -2; // get out
                }
                point++;
            }

            int totalTerm = 0;
            for (point = 1; point < ts.PointCount; point++)
            {
                tp = ts.Points[point];
                ltp = ts.Points[point - 1];

                // BUT for actual / actual and in a leap year some more calculations need to be done.
                // e.g.  Period  2 jan 96 to 1 jan 97 = (365/366 + 1/365)  NOT including the last day but including the first day
                if (dayCount == DayCountConvention.eActual_Actual && FixedCashPayments == false
                    && ltp.PointDate.Year != tp.PointDate.Year)
                {
                    tp.YearFraction =
                        ltp.PointDate.Subtract(DateTime.Parse("1 Jan " + ltp.PointDate.AddYears(1).Year)).TotalDays
                        /
                        DateTime.Parse("1 Jan " + ltp.PointDate.AddYears(1).Year).Subtract(DateTime.Parse("1 Jan " + ltp.PointDate.Year)).TotalDays
                        +
                        tp.PointDate.Subtract(DateTime.Parse("1 Jan " + tp.PointDate.Year)).TotalDays
                        /
                        DateTime.Parse("1 Jan " + tp.PointDate.AddYears(1).Year).Subtract(DateTime.Parse("1 Jan " + tp.PointDate.Year)).TotalDays;
                }
                else
                {
                    if (tp.Rate > 0 && FixedCashPayments)
                    {
                        tp.YearFraction = tp.EffectiveRate * tp.Term / yearDays / tp.Rate;
                    }
                    else
                    {
                        tp.YearFraction = (double)tp.Term / (double)yearDays;
                    }
                }

                tp.Amount = tp.Rate * tp.YearFraction;

                // Reset daycounts to be cummulative
                totalTerm += tp.Term;
                tp.Term = totalTerm;
            }

            return ts;
        }

        private static TermStructure CalcCurveFromMaturity(DateTime Maturity, DateTime StartDate, double CouponRate,
            NextWorkingDayConvention nextDay, int Freq, Dictionary<DateTime, DateTime> Holidays,
            bool FixedCashPayments)
        {

            List<TermPoint> points = new List<TermPoint>();

            DateTime pointDate = Maturity;
            bool lastPoint = true;

            while (pointDate > StartDate)
            {
                if (lastPoint)
                {
                    // Only do the last point once, decrement the date next time around
                    lastPoint = false;
                }
                else
                {
                    pointDate = pointDate.AddMonths(-(12 / Freq));
                }

                TermPoint point = new TermPoint();
                point.Rate = CouponRate;
                point.PointDate = pointDate;
                int month = point.PointDate.Month;

                point.PayDate = GetWorkingDayDate(1, pointDate.AddDays(-1), Holidays, nextDay);

                if (!FixedCashPayments) { point.PointDate = point.PayDate; }

                points.Add(point);
            };

            TermStructure ts = new TermStructure();
            for (int i = points.Count - 1; i >= 0; i--)
            {
                ts.Points.Add(points[i]);
            }

            return ts;
        }

        internal static double PresentValue(DateTime ValueDate, double SpreadOverLibor, TermStructure ts, InputData inputData, Dictionary<DateTime, DateTime> Holidays, bool FixedCashPayments)
        {
            double ret = 0;

            // LastPaymentDate should be the date from which we wish to start accrueing interest.
            // This is either the lastcoupon payment (for fixed cash payments) or the Startdate if that is later and not Fixed Cash Payments
            // The curve passed in (Rates) would have either the lastcouponpayment or the startdate as the preceding point
            DateTime lastPayDate = DateTime.MinValue;
            DayCountConvention dayC = ts.DayCount;

            foreach (TermPoint point in ts.Points)
            {
                lastPayDate = point.PointDate;
                if (point.PointDate >= ValueDate)
                {
                    break;
                }
            }

            for (int p = 1; p < ts.PointCount; p++)
            {
                DateTime nextPaymentDate = ts.Points[p].PointDate;
                DateTime nextPayDate = ts.Points[p].PayDate;
                int daysToDiscount = DaysBetween(ValueDate, nextPayDate, dayC);

                if (daysToDiscount > 0)
                {
                    double fixingRate = ts.Points[p].Rate;

                    double discountFactor = GetDiscountFactor(daysToDiscount, inputData.Rates, dayC);
                    int daysToAccrue = DaysBetween(lastPayDate, nextPaymentDate, dayC);

                    double paymentValue = AccruedForDaysInPeriod(ts.Points[p - 1].PointDate, ts.Points[p].PointDate, ts, fixingRate + SpreadOverLibor, FixedCashPayments);

                    ret += paymentValue * discountFactor;
                }
                lastPayDate = nextPayDate;
            }

            return ret;
        }

        internal static double AccruedForDaysInPeriod(DateTime AccrueFrom, DateTime AccrueTo, TermStructure ts, double Rate, bool FixedCashPayments)
        {
            double ret = 0;
            TermPoint periodStart = ts.Points[0];
            TermPoint periodEnd = null;

            if (AccrueTo > ts.Points[0].PointDate && AccrueFrom < ts.Points[ts.PointCount - 1].PointDate)
            {
                // Find the applicable period
                foreach (TermPoint point in ts.Points)
                {
                    if (point.PointDate <= AccrueFrom)
                    {
                        periodStart = point;
                    }
                    else if (point.PointDate >= AccrueTo)
                    {
                        periodEnd = point;
                        break;
                    }
                }
                // If the caller has asked for Accrued to a point past the end of the term structure it is not valid
                // and the PeriodEnd variable will be equal to zero
                if (periodEnd == null)
                {
                    throw new Exception(string.Format("AccrueTo data [{0}] is past the end of the curve [{1}].", AccrueTo.ToString("dd MMM yy"), ts.Points[ts.PointCount - 1].PointDate.ToString("dd MMM yy")));
                }

                int days = DaysBetween(AccrueFrom, AccrueTo, ts.DayCount);
                int periodLength = DaysBetween(periodStart.PointDate, periodEnd.PointDate, ts.DayCount);
                int freq = (int)Math.Round((double)365 / periodLength, 0);

                // We need to get the rate of the point to which we are accrueing up to
                // A rate quoted within the Curve is the portion of rate for that period. So the rate according to the curve for a
                // semi annual bond paying 5% would be 2.5%
                if (Rate == 0)
                {
                    Rate = periodEnd.Amount;
                }

                if (FixedCashPayments)
                {
                    if (periodLength > 366)
                    {
                        // it's longer than a year.
                        // We need to work out the accrued as if a coupon "was" paid each year
                        // This will take account of when a Feb 29 occurrs

                        // Split the accrued to before and after an imaginary paydate being one year before the real pay date
                        DateTime couponPoint2 = periodEnd.PointDate;
                        do
                        {
                            DateTime couponPoint = couponPoint2.AddYears(-1);
                            periodLength = DaysBetween(couponPoint, couponPoint2, ts.DayCount);

                            if (couponPoint < AccrueTo)
                            {
                                if (couponPoint2 < AccrueTo)
                                {
                                    // We have done the part up to AccrueTo
                                    if (AccrueFrom < couponPoint)
                                    {
                                        days = DaysBetween(couponPoint, couponPoint2, ts.DayCount);
                                    }
                                    else
                                    {
                                        days = DaysBetween(AccrueFrom, couponPoint, ts.DayCount);
                                    }
                                }
                                else
                                {
                                    // Work out accrued from the CouponPoint to the AccrueTo date
                                    if (AccrueFrom < couponPoint)
                                    {
                                        days = DaysBetween(couponPoint, AccrueTo, ts.DayCount);
                                    }
                                    else
                                    {
                                        days = DaysBetween(AccrueFrom, AccrueTo, ts.DayCount);
                                    }
                                }
                                ret += (Rate * days / periodLength);
                            }
                            couponPoint2 = couponPoint;
                        } while (couponPoint2 < AccrueFrom);
                    }
                    else
                    {
                        if (periodLength != 0)
                        {
                            ret += (Rate / freq) * (days / periodLength);
                        }
                    }
                }
                else
                {
                    switch (ts.DayCount)
                    {
                        case DayCountConvention.e30_360:
                        case DayCountConvention.e30E_360:
                        case DayCountConvention.e30E1_360:
                        case DayCountConvention.eActual_360:
                            ret = Rate * days / 360;
                            break;
                        case DayCountConvention.eActual_365:
                        case DayCountConvention.eActualNL_365:
                            ret = Rate * days / 365;
                            break;
                        case DayCountConvention.eActual_Actual:
                            // Check for leap years
                            int year1 = (int)DateTime.Parse("1 Jan " + AccrueTo.AddYears(1).Year).Subtract(DateTime.Parse("1 Jan " + AccrueFrom.Year)).TotalDays;

                            if (year1 == 731)
                            { // it goes into two years one of which is a leap year
                                // A bit different for this
                                // This assumes that the period is <= 1 year, ie, cannot span more than 2 adjacent years

                                // Days = startpoint up to the first of next year
                                days = (int)DateTime.Parse("1 Jan " + periodEnd.PointDate.Year).Subtract(periodStart.PointDate).TotalDays;
                                // Days2 = the first of the next year to the end point
                                int days2 = (int)periodEnd.PointDate.Subtract(DateTime.Parse("1 Jan " + periodEnd.PointDate.Year)).TotalDays;

                                // Number of days in each seperate period which falls in each year
                                year1 = (int)DateTime.Parse("1 Jan " + periodEnd.PointDate.Year).Subtract(DateTime.Parse("1 Jan " + periodStart.PointDate.Year)).TotalDays;
                                int year2 = (int)DateTime.Parse("1 Jan " + periodEnd.PointDate.Year + 1).Subtract(DateTime.Parse("1 Jan " + periodEnd.PointDate.Year)).TotalDays;

                                ret = (((Rate / year1) * days)) + (((Rate / year2) * days2));
                            }
                            else
                            {
                                ret = (Rate * days) / year1;
                            }
                            break;
                    }
                }
            }

            return ret;
        }
    }
}
