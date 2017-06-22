using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZeroCouponGenerator {
    class ZeroCouponCalc {
        public List<OutputPoint> Output = null;

        public string Calc(InputData inputData) {
            string ret = "";

            SetupLiborData(inputData);
            inputData.PrintRates(inputData.LiborData);
            SetupFutureData(inputData);
            inputData.PrintRates(inputData.FutureData);
            SetupSwapData(inputData);
            inputData.PrintRates(inputData.SwapData);

            FutureBootStrap(inputData);

            // Add Libors and Futures to the Rates collection
            inputData.Rates = new List<Rate>();
            inputData.Rates.AddRange(inputData.LiborData);
            inputData.Rates.AddRange(inputData.FutureData);

            // Get the Libors and Futures in order
            //inputData.Rates = SortByTerm(inputData);
            inputData.Rates = SortByTermDate(inputData.Rates);

            DateTime curveSplitDate = DateTime.MinValue, curveFitEndDate = DateTime.MinValue;
            // We now need to obtain the latest Libor date that is before the earliest future date [curveSplitDate]
            if (inputData.FutureData.Count > 0) {
                foreach (Rate libor in inputData.LiborData) {
                    if (libor.TermDate <= inputData.FutureData[0].TermDate) {
                        curveSplitDate = libor.TermDate;
                    }else{
                        break;
                    }
                }
            }

            curveFitEndDate = inputData.FutureData[inputData.FutureData.Count - 1].TermDate.AddMonths(6);

            if (inputData.SwapData[0].TermDate.AddMonths(6).CompareTo(curveFitEndDate) == 1) {
                curveFitEndDate = inputData.SwapData[0].TermDate.AddMonths(6);
            }

            FitCurveBetweenDates(inputData.Rates, inputData.StartDate, inputData.Rates[inputData.Rates.Count-1].TermDate.AddDays(1) );
            inputData.PrintRates(inputData.Rates);

            BootstrapSwapRates(inputData);

            for (int i = 0; i <= 4; i++) {
                FitCurveBetweenDates(inputData.Rates, curveSplitDate.AddMonths(i * 3), curveFitEndDate.AddMonths(i*3));
                inputData.PrintRates(inputData.Rates);
            }

            inputData.Rates = SortByTerm(inputData.Rates);

            // Remove duped terms
            for (int i = 1; i < inputData.Rates.Count; i++) {
                if (inputData.Rates[i].Term == inputData.Rates[i - 1].Term) {
                    inputData.Rates.RemoveAt(i);
                }
            }

            // Move to output class
            Output = new List<OutputPoint>();
            foreach(Rate rate in inputData.Rates){
                Output.Add(new OutputPoint(rate.TermDate, rate.Term, rate.SpotRate));
            }
                
            return ret;
        }

        private void BootstrapSwapRates(InputData inputData) {

            // prev rate starts off as the most recent Future rate
            Rate prev = inputData.FutureData[inputData.FutureData.Count - 1];

            foreach (Rate rate in inputData.SwapData) {
                double fixedCoupon = rate.SwapRate / 100;
                
                DateTime lastDate = rate.TermDate.AddMonths(-12 / inputData.SwapFixedPaymentFrequency);
                
                TermStructure ts = Functions.GetTermStructure(lastDate, rate.StartDate, rate.StartDate, fixedCoupon,
                    inputData.SwapFixedDayCountConvention, inputData.NextWorkingDay, inputData.SwapFixedPaymentFrequency, inputData.Holidays, false);

                ts.PrintStructure();
                double fixedCash = Functions.PresentValue(rate.StartDate, 0, ts, inputData, inputData.Holidays, false);

                ts = Functions.GetTermStructure(rate.TermDate, lastDate, lastDate, fixedCoupon, 
                    inputData.SwapFixedDayCountConvention, inputData.NextWorkingDay, inputData.SwapFixedPaymentFrequency, inputData.Holidays, false);
                ts.PrintStructure();

                double accrued = 1 + Functions.AccruedForDaysInPeriod(lastDate, rate.TermDate, ts, fixedCoupon, false);
                double discountFactor = 0;

                accrued = accrued > 0 ? discountFactor = (1-fixedCash) / accrued : 1 - fixedCash;

                // Adjust rates to curve basis
                if (inputData.DayCountConvention != inputData.SwapFixedDayCountConvention) {
                    rate.Term = Functions.DaysBetween(rate.StartDate, rate.TermDate, inputData.DayCountConvention);
                }

                rate.SpotRate = 100 * Functions.GetRateFromDiscountFactor(discountFactor, rate.Term, Functions.DaysInYear(inputData.DayCountConvention));
                inputData.Rates.Add(rate);
            }
        }

        private void FitCurveBetweenDates(List<Rate> Rates, DateTime StartDate, DateTime EndDate) {
            // An ordered set of rates should be passed in here
            List<Rate> toFit = new List<Rate>();
            foreach (Rate rate in Rates) {
                if (rate.TermDate >= StartDate && rate.TermDate < EndDate) {
                    toFit.Add(rate);
                }
            }
            int count = toFit.Count;

            if (count > 2) {
                double[,] curvePoints = new double[count, 2];
                for (int i = 0; i < count; i++) {
                    curvePoints[i, 0] = toFit[i].SpotRate;
                    curvePoints[i, 1] = toFit[i].Term;
                }

                // This calculates a trinomial curve
                double[,] polyCurve = Functions.GetPolynomialCurve(curvePoints, 3);
                double[] terms = new double[count];
                for (int i = 0; i < count; i++) { terms[i] = curvePoints[i, 1]; }

                double[,] newPoints = Functions.FitToPolynomialCurve(terms, polyCurve);

                // Now modify the spot rates from the poly curve
                for (int i = 0; i < count; i++) {
                    toFit[i].SpotRate = newPoints[i, 1];
                }
            }
        }

        private void BuildCurveBetweenDates(DateTime StartDate, DateTime TermDate) {

            int LiborIndex, FutureIndex, SwapIndex;

        }

        private List<Rate> SortByTerm(List<Rate> Rates) {
            List<Rate> ret = new List<Rate>();

            ret.AddRange(Rates);

            bool done;  // bubble sort
            do{
                done = true;
                for (int p = 0; p < ret.Count-1; p++) {
                    if (ret[p].Term > ret[p + 1].Term) {
                        Rate temp = ret[p + 1];
                        ret.Remove(temp);
                        ret.Insert(p, temp);
                        done = false;
                    }
                }
            }while(!done);

            return ret;
        }

        private List<Rate> SortByTermDate(List<Rate> Rates) {
            List<Rate> ret = new List<Rate>();

            ret.AddRange(Rates);

            bool done;  // bubble sort
            do {
                done = true;
                for (int p = 0; p < ret.Count - 1; p++) {
                    if (ret[p].TermDate > ret[p + 1].TermDate) {
                        Rate temp = ret[p + 1];
                        ret.Remove(temp);
                        ret.Insert(p, temp);
                        done = false;
                    }
                }
            } while (!done);

            return ret;
        }

        private void FutureBootStrap(InputData inputData) {
            const int forwardDiscountTerm = 90;
            Rate prevRate = null;
            Rate lastRate = null;

            foreach (Rate rate in inputData.FutureData) {
                if (prevRate == null) {
                    rate.SpotRate = Functions.GetRateFromCurve(rate.Term, inputData.LiborData);
                } else {
                    if (100 - prevRate.Price < 100) {
                        double forwardDiscountFactor = 1 / (1 + (((100 - prevRate.Price) / 100) * forwardDiscountTerm / rate.DaysInYear));

                        double discountToPrevFuture = Functions.GetDiscountFactor(prevRate.Term, inputData.LiborData, inputData.LiborDayCountConvention);

                        double rawRate = 100 * Functions.GetRateFromDiscountFactor(discountToPrevFuture * forwardDiscountFactor, rate.Term, rate.DaysInYear);
                        if (rawRate > 0) {
                            // Adjust rates to curve basis
                            if (inputData.DayCountConvention != inputData.FutureDayCountConvention) {
                                int diy = Functions.DaysInYear(inputData.DayCountConvention);
                                rate.Term = Functions.DaysBetween(rate.StartDate, rate.TermDate, inputData.DayCountConvention);
                                rate.SpotRate = 100 * Functions.GetRateFromDiscountFactor(discountToPrevFuture * forwardDiscountFactor, rate.Term, diy);
                            } else {
                                rate.SpotRate = rawRate;
                            }
                        }
                    }
                }

                prevRate = rate;
                if (rate.SpotRate > 0) { lastRate = rate; }
            }
        }

        private void SetupLiborData(InputData inputData) {

            foreach (Rate rate in inputData.LiborData) {
                rate.StartDate = inputData.StartDate;

                switch(rate.TermCode){
                    case "TN":
                        rate.StartDate = Functions.GetWorkingDayDate(1,rate.StartDate, inputData.Holidays, inputData.NextWorkingDay);
                        rate.TermDate = Functions.GetWorkingDayDate(1,rate.StartDate, inputData.Holidays, inputData.NextWorkingDay);;
                        break;
                    case "ON":
                        rate.TermDate = Functions.GetWorkingDayDate(1, rate.StartDate, inputData.Holidays, inputData.NextWorkingDay);
                        break;
                    case "SW":
                        rate.StartDate = Functions.GetWorkingDayDate(inputData.SettleDaysForLibors, rate.StartDate, inputData.Holidays, inputData.NextWorkingDay);
                        rate.TermDate = rate.StartDate.AddDays(7);
                        break;
                    default:
                        rate.StartDate = Functions.GetWorkingDayDate(inputData.SettleDaysForLibors, rate.StartDate, inputData.Holidays, inputData.NextWorkingDay);
                        int count = int.Parse(rate.TermCode.Substring(0,1));

                        switch(rate.TermCode.Substring(1,1)){
                            case "W":
                                rate.TermDate = rate.StartDate.AddDays(7 * count).AddDays(-1);
                                break;
                            case "M":
                                rate.TermDate = rate.StartDate.AddMonths(count).AddDays(-1);
                                break;
                            case "Y":
                                rate.TermDate = rate.StartDate.AddYears(count).AddDays(-1);
                                break;
                            default:
                                throw new Exception(string.Format("Rate TermCode not handled. [{0}]", rate.TermCode));
                        }
                        // Move back one day then go to the next working day
                        rate.TermDate = Functions.GetWorkingDayDate(1, rate.TermDate, inputData.Holidays, inputData.NextWorkingDay);
                        break;
                }

                // Note that in the original it initially uses DaysBetween but then in the Libor Bootstrap it replace
                // the term with Maturity - Startdate
                //rate.Term = Functions.DaysBetween(rate.StartDate, rate.TermDate, DayCountConvention.e30E1_360);
                rate.Term = (int)rate.TermDate.Subtract(rate.StartDate).TotalDays;

                rate.SpotRate = Functions.AveragePrice(rate.Bid, rate.Ask);

                rate.DaysInYear = Functions.DaysInYear(inputData.DayCountConvention);

                rate.YearFrac = (double)rate.Term / (double)rate.DaysInYear;

                rate.DiscountFactor = 1 / (1+ ((rate.SpotRate / 100) * rate.YearFrac));

                //double df = 0.999994722250077130;
                //double a = (1 / rate.YearFrac);
                //double b = Math.Pow(rate.DiscountFactor, a);
                //double test = 100 * ((1 / (b)) - 1);
                //rate.SpotRate = 100 * ((1 / (b)) - 1);

                rate.SpotRate = 100 * ((1 / (Math.Pow(rate.DiscountFactor, (1 / rate.YearFrac)))) - 1);

            }
        }

        private void SetupFutureData(InputData inputData) {
            List<Rate> unwanted = new List<Rate>();

            foreach (Rate rate in inputData.FutureData) {
                rate.DaysInYear = Functions.DaysInYear(inputData.FutureDayCountConvention);

                rate.StartDate = inputData.StartDate;
                rate.TermDate = rate.Expiry;
                rate.Term = Functions.DaysBetween(rate.StartDate, rate.TermDate,inputData.FutureDayCountConvention);

                if (rate.TermDate.Subtract(rate.StartDate).TotalDays > inputData.MaxFutureTermDays) {
                    // We don't want this Future to be included. Note that the code doesn't use the daycount here
                    rate.NotWanted = true;
                    unwanted.Add(rate);
                }

                // Only months of 3,6,9,12 are required
                if (rate.TermDate.Month % 3 != 0) {
                    rate.NotWanted = true;
                    unwanted.Add(rate);
                }
                rate.Price = Functions.AveragePrice(rate.Bid, rate.Ask);
            }

            foreach (Rate rate in unwanted) {
                inputData.FutureData.Remove(rate);
            }

        }

        private void SetupSwapData(InputData inputData) {

            foreach (Rate rate in inputData.SwapData) {
                rate.StartDate = Functions.GetWorkingDayDate(1, inputData.StartDate.AddDays(inputData.SettleDaysForSwaps - 1),
                    inputData.Holidays, inputData.NextWorkingDay);
                string t = rate.TermCode;
                t = t.Substring(0, t.IndexOf("Y"));
                int mth = int.Parse(t);

                rate.TermDate = rate.StartDate.AddMonths(mth*12);
                rate.Term = (int)rate.TermDate.Subtract(rate.StartDate).TotalDays;

                rate.RawSwapRate = Functions.AveragePrice(rate.Bid, rate.Ask);

            }

            // Now we fit the swap rates that we have into the best fitting polynomial curve.
            double[,] curvePoints = new double[inputData.SwapData.Count, 2];
            int row = 0;
            foreach (Rate rate in inputData.SwapData) {
                curvePoints[row, 0] = rate.RawSwapRate;
                curvePoints[row++, 1] = rate.Term;
            }
            // This calculates a hexic curve
            double[,] polyCurve = Functions.GetPolynomialCurve(curvePoints, 6);

            if (inputData.SwapFixedPaymentFrequency == 1) {
                AddInExtraSwaps(inputData);
            }

            double[] terms = new double[inputData.SwapData.Count];
            row = 0;
            foreach (Rate swap in inputData.SwapData) { terms[row++] = swap.Term; }

            double[,] newPoints = Functions.FitToPolynomialCurve(terms, polyCurve);
            // Now place the new swap rates into our input data
            row = 0;
            foreach (Rate rate in inputData.SwapData) {rate.SwapRate = newPoints[row++, 1]; }

            inputData.SwapData = SortByTerm(inputData.SwapData);

        }

        private void AddInExtraSwaps(InputData inputData) {
            List<Rate> newSwaps = new List<Rate>();

            int j = 0;
            foreach (Rate swap in inputData.SwapData) {
                if (j > 0) {
                    Rate newSwap = new Rate();
                    newSwap.StartDate = Functions.GetWorkingDayDate(inputData.SettleDaysForSwaps, inputData.StartDate, inputData.Holidays,
                        inputData.NextWorkingDay);

                    newSwap.TermDate = swap.TermDate.AddMonths(-6);
                    newSwap.Term = (int)newSwap.TermDate.Subtract(newSwap.StartDate).TotalDays;

                    newSwaps.Add(newSwap);
                }
                j = 1;
            }

            inputData.SwapData.AddRange(newSwaps);
        }
    }
}
