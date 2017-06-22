using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BOSS_OptionValueManager
{
    class Utilities
    {

        /// <summary>
        /// return the wingding character corresponding to 1, 2, 3, 4...!
        /// </summary>
        /// <param name="Source"></param>
        /// <returns></returns>
        public static string GetSourceCharacter(InputSourceData.InputSource Source)
        {
            switch (Source)
            {

                case BOSS_OptionValueManager.InputSourceData.InputSource.Override:
                    return "Œ";     //1               
                case BOSS_OptionValueManager.InputSourceData.InputSource.Bloomberg:
                    return "";     //2            
                case BOSS_OptionValueManager.InputSourceData.InputSource.BOSS:
                    return "Ž";     //3            
                case BOSS_OptionValueManager.InputSourceData.InputSource.Missing:
                    return "";     //4               
                default:
                    return "";     //4               
            }
        }

        /// <summary>
        /// return the wingding character corresponding to 1, 2, 3, 4...!
        /// </summary>
        /// <param name="Source"></param>
        /// <returns></returns>
        public static System.Drawing.Color GetSourceColour(InputSourceData.InputSource Source)
        {
            switch (Source)
            {
                case BOSS_OptionValueManager.InputSourceData.InputSource.Override:
                    return System.Drawing.Color.Blue;
                case BOSS_OptionValueManager.InputSourceData.InputSource.Bloomberg:
                    return System.Drawing.Color.Green;
                case BOSS_OptionValueManager.InputSourceData.InputSource.BOSS:
                    return System.Drawing.Color.DarkOrange;
                case BOSS_OptionValueManager.InputSourceData.InputSource.Missing:
                    return System.Drawing.Color.Red;
                default:
                    return System.Drawing.Color.Red;
            }
        }

        public static System.Drawing.Color GetStatusColour(bool OK)
        {
            return OK ? System.Drawing.Color.DarkOrange : System.Drawing.Color.Red;         
        }

        /// <summary>
        /// If the user has opened the application in the morning - they are probably doing the pricing for the previous day.        
        /// Otherwise they are doing the pricing for today
        /// </summary>
        /// <returns></returns>
        public static DateTime GetDefaultValuationDate()
        {
            if (DateTime.Now.Hour >= 12)
            {
                return DateTime.Now.Date;
            }
            else
            {
                return GetLastWeekDay();
            }
        }


        /// <summary>
        /// get the last weekday
        /// </summary>
        /// <returns></returns>
        public static DateTime GetLastWeekDay()
        {
            if (DateTime.Now.DayOfWeek == DayOfWeek.Monday)
            {
                return DateTime.Now.Date.AddDays(-3);
            }
            else
            {
                return DateTime.Now.Date.AddDays(-1);
            }
        }
    }
}
