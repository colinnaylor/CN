namespace BBfieldValueRetriever.Control
{
    public class TorontoViewController
    {
        /// <summary>
        /// Bloomberg calendar TE is missing.
        /// </summary>
        /// <param name="bloombergCalendarCode"></param>
        /// <returns></returns>
        public string MapBloombergCalendarCodeToTorontoHolidayCode(string bloombergCalendarCode)
        {
            if (bloombergCalendarCode.Equals("EN")) return "LnS";
            if (bloombergCalendarCode.Equals("US")) return "CME";
            if (bloombergCalendarCode.Equals("GE")) return "MaT";
            if (bloombergCalendarCode.Equals("TE")) return "Tgt"; 
            
            if (bloombergCalendarCode.Equals("FR")) return "PaB";
            if (bloombergCalendarCode.Equals("IT")) return "MiB";
            if (bloombergCalendarCode.Equals("NE")) return "AmB";
            if (bloombergCalendarCode.Equals("SW")) return "StB";
            
            

            return bloombergCalendarCode;
        }
        public string MapBloombergCalendarCodeToTorontoHolidayDescription(string bloombergCalendarCode)
        {
            if (bloombergCalendarCode.Equals("EN")) return "United Kingdom";
            if (bloombergCalendarCode.Equals("US")) return "United States";
            if (bloombergCalendarCode.Equals("GE")) return "Germany";
            if (bloombergCalendarCode.Equals("FR")) return "France";
            if (bloombergCalendarCode.Equals("IT")) return "Italy";
            if (bloombergCalendarCode.Equals("NE")) return "Netherlands";
            if (bloombergCalendarCode.Equals("SW")) return "Sweden";
            if (bloombergCalendarCode.Equals("TE")) return "Europe (TARGET)";
            return bloombergCalendarCode;
        }
    }
}