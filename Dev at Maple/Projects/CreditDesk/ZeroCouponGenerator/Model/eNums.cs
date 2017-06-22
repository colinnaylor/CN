using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZeroCouponGenerator {
    public enum DayCountConvention{
        eActual_365,
        eActual_360,
        e30_360,   // SIA
        e30E_360,  // ISMA
        eActual_Actual,
        e30E1_360,
        eActualNL_365
    }

    public enum NextWorkingDayConvention{
        eNexWorkDay,
        eNextWorkDayUnlessNewMonth
    }

    static class eNumConvert{
        static public NextWorkingDayConvention GetNextWorkingDayEnum(string data){
            switch(data){
                case "0":
                    return NextWorkingDayConvention.eNexWorkDay;
                    break;
                case "1":
                    return NextWorkingDayConvention.eNextWorkDayUnlessNewMonth;
                    break;
                default:
                    throw new Exception("Next working day value is not valid.");
            }
        }

        static public DayCountConvention GetDayCountEnum(string data){
            switch(data){
                case "0":
                    return DayCountConvention.eActual_365;
                    break;
                case "1":
                    return DayCountConvention.eActual_360;
                    break;
                case "2":
                    return DayCountConvention.e30_360;
                    break;
                case "3":
                    return DayCountConvention.e30E_360;
                    break;
                case "4":
                    return DayCountConvention.eActual_Actual;
                    break;
                case "5":
                    return DayCountConvention.e30E1_360;
                    break;
                case "6":
                    return DayCountConvention.eActualNL_365;
                    break;
                default:
                    throw new Exception("Day count value is not valid.");
            }
        }
    }
}
