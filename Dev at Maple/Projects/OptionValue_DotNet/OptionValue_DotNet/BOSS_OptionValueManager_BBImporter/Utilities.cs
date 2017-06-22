using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OTCOptionValuation_BBImporter {
	class Utilities {
		/// <summary>
		/// If this is being run after 4 PM it is for current day othersise, previous day.
		/// TODO: Need to make this an application setting
		/// </summary>
		/// <returns></returns>
		public static DateTime GetDefaultValuationDate() {
			if (DateTime.Now.Hour > 16) {
				return DateTime.Now.Date;
			} else {
				return GetLastWeekDay();
			}
		}


		/// <summary>
		/// get the last weekday
		/// </summary>
		/// <returns></returns>
		public static DateTime GetLastWeekDay() {
			if (DateTime.Now.DayOfWeek == DayOfWeek.Monday) {
				return DateTime.Now.Date.AddDays(-3);
			} else {
				return DateTime.Now.Date.AddDays(-1);
			}
		}
	}
}
