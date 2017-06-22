using System;
using System.Globalization;

namespace BBfieldValueRetriever.Model
{
    public class CalendarNonSettlementDateRequest
    {
        /// <summary>
        /// Constructor taking in a bbfieldlist string like
        /// CALENDAR_NON_SETTLEMENT_DATES[CALENDAR_START_DATE,20140213,CALENDAR_END_DATE,20180403,SETTLEMENT_CALENDAR_CODE,EN]
        /// </summary>
        /// <param name="FieldList"></param>
        public CalendarNonSettlementDateRequest(string FieldList)
        {
            var data = FieldList.Split('[', ',', ']');

            for (int i = 0; i < data.Length; i++)
            {
                if (data[i] == "CALENDAR_START_DATE") CalendarStartDate = DateTime.ParseExact(data[i + 1], "yyyyMMdd", CultureInfo.InvariantCulture);
                if (data[i] == "CALENDAR_END_DATE") CalendarEndDate = DateTime.ParseExact(data[i + 1], "yyyyMMdd", CultureInfo.InvariantCulture);
                if (data[i] == "SETTLEMENT_CALENDAR_CODE") SettlementCalendarCode = data[i + 1];
            }
        }

        public CalendarNonSettlementDateRequest()
        { }

        public DateTime CalendarStartDate { get; set; }

        public DateTime CalendarEndDate { get; set; }

        public string SettlementCalendarCode { get; set; }
    }
}