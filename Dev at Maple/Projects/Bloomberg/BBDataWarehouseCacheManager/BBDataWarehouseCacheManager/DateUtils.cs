using System;
using System.Configuration;
using Maple.Database;
using NLog;

namespace BBDataWarehouseCacheManager
{
    public class Utils
    {
        public static Logger Logger = new LogFactory().GetLogger("");
        public static DatabaseController DbController = new DatabaseController(ConfigurationManager.ConnectionStrings["BloombergConnectionString"].ToString()) { CommandTimeout = 900 };

        public static void SetLoggerClassName(string name)
        {
            Logger = new LogFactory().GetLogger(name);
        }
    }

    public class DateUtils
    {
        public DateTime PreviousWorkDay(DateTime date)
        {
            date = date.AddDays(-1);
            while (IsWeekend(date))
            {
                date = date.AddDays(-1);
            }

            //while(IsHoliday(date) || IsWeekend(date))

            return date;
        }

        private bool IsWeekend(DateTime date)
        {
            return date.DayOfWeek == DayOfWeek.Saturday ||
                   date.DayOfWeek == DayOfWeek.Sunday;
        }
    }
}