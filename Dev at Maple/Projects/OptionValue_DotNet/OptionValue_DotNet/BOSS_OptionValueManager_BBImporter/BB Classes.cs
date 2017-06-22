using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BBGShared {
    public class BloombergDataInstrumentField {

        /// <summary>
        /// Initializes a new instance of the <see cref="BloombergDataInstrumentField"/> class.
        /// </summary>
        /// <param name="name">The name of the Bloomberg field.</param>
        public BloombergDataInstrumentField(string name) {
            this.Name = name;
        }

        /// <summary>
        /// Gets or sets the name of the Bloomberg field.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }
    }

    public class BloombergDataInstrument {
        public enum eRequestType { NotSet, Reference, Historical, IntraDayBar, IntraDayTick }

        public BloombergDataInstrument() {
            BBFields = new Dictionary<string, BloombergDataInstrumentField>();
        }

        /// <summary>
        /// Gets or sets the ID - usually the primary key, what the developer will use to identify the instrument.
        /// </summary>
        /// <value>
        /// The ID.
        /// </value>
        public int ID { get; set; }
        
        /// <summary>
        /// Gets or sets the ticker - the unique code that Bloomberg uses to identify a security etc.
        /// </summary>
        /// <value>
        /// The ticker.
        /// </value>
        public string Ticker { get; set; }

        /// <summary>
        /// Gets or sets the BB fields - the fields that we are requesting from Bloomberg.
        /// </summary>
        /// <value>
        /// The BB fields.
        /// </value>
        public Dictionary<string, BloombergDataInstrumentField> BBFields { get; set; }

        /// <summary>
        /// The date of an historical request or the start date of an historical date range request
        /// </summary>
        public DateTime DateFrom { get; set; }

        /// <summary>
        /// The type of request that you are asking for
        /// </summary>
        public eRequestType RequestType { get; set; }
    }

    public static class Conversion
    {

        #region internal methods
        /// <summary>
        /// Gets the field value from the BloombergData BBField collection.
        /// </summary>
        /// <param name="instrument">The instrument.</param>
        /// <param name="fieldName">Name of the field.</param>
        /// <returns></returns>
        private static object GetFieldValue(BloombergDataInstrument instrument, string fieldName)
        {
            object fieldValue = null;
            try
            {
                if (instrument.BBFields != null)
                {
                    if (instrument.BBFields.ContainsKey(fieldName))
                    {
                        //fieldValue = instrument.BBFields[fieldName].Value;
                        fieldValue = instrument.BBFields[fieldName];
                    }
                }
            }
            catch
            {
                fieldValue = null;
            }
            return fieldValue;
        }
        #endregion

        #region String
        /// <summary>
        /// Gets the string value from the field value object.
        /// </summary>
        /// <param name="fieldValue">The field value.</param>
        /// <returns></returns>
        public static string GetString(object fieldValue)
        {
            string actualValue = null;
            if (fieldValue != null)
            {
                actualValue = fieldValue.ToString();
            }
            return actualValue;
        }
        /// <summary>
        /// Gets the string value from the BloombergData BBField collection.
        /// </summary>
        /// <param name="instrument">The instrument.</param>
        /// <param name="fieldName">Name of the field.</param>
        /// <returns></returns>
        public static string GetString(BloombergDataInstrument instrument, string fieldName)
        {
            object fieldValue = GetFieldValue(instrument, fieldName);
            return GetString(fieldValue);
        }
        #endregion

        #region Decimal
        /// <summary>
        /// Gets the decimal value from the field value object.
        /// </summary>
        /// <param name="fieldValue">The field value.</param>
        /// <returns></returns>
        public static decimal? GetDecimal(object fieldValue)
        {
            decimal? actualValue = null;
            if (fieldValue != null)
            {
                decimal temp;
                Decimal.TryParse(fieldValue.ToString(), out temp);
                actualValue = (decimal?)temp;
            }
            return actualValue;
        }
        /// <summary>
        /// Gets the decimal value from the BloombergData BBField collection.
        /// </summary>
        /// <param name="instrument">The instrument.</param>
        /// <param name="fieldName">Name of the field.</param>
        /// <returns></returns>
        public static decimal? GetDecimal(BloombergDataInstrument instrument, string fieldName)
        {
            object fieldValue = GetFieldValue(instrument, fieldName);
            return GetDecimal(fieldValue);
        }
        #endregion

        #region DateTime
        /// <summary>
        /// Gets the date time value from the field value object.
        /// </summary>
        /// <param name="fieldValue">The field value.</param>
        /// <returns></returns>
        public static DateTime? GetDateTime(object fieldValue)
        {
            DateTime? actualValue = null;
            if (fieldValue != null)
            {
                DateTime temp;
                if (DateTime.TryParseExact(fieldValue.ToString(), "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AdjustToUniversal, out temp))
                {
                    actualValue = (DateTime?)temp;
                }
            }
            return actualValue;
        }

        /// <summary>
        /// Gets the date time value from the BloombergData BBField collection.
        /// </summary>
        /// <param name="instrument">The instrument.</param>
        /// <param name="fieldName">Name of the field.</param>
        /// <returns></returns>
        public static DateTime? GetDateTime(BloombergDataInstrument instrument, string fieldName)
        {
            object fieldValue = GetFieldValue(instrument, fieldName);
            return GetDateTime(fieldValue);
        }
        #endregion

        #region Long
        /// <summary>
        /// Gets the long value from the field value object.
        /// </summary>
        /// <param name="fieldValue">The field value.</param>
        /// <returns></returns>
        public static long? GetLong(object fieldValue)
        {
            long? actualValue = null;
            if (fieldValue != null)
            {
                long temp;
                long.TryParse(fieldValue.ToString(), out temp);
                actualValue = (long?)temp;
            }
            return actualValue;
        }

        /// <summary>
        /// Gets the long value from the BloombergData BBField collection.
        /// </summary>
        /// <param name="instrument">The instrument.</param>
        /// <param name="fieldName">Name of the field.</param>
        /// <returns></returns>
        public static long? GetLong(BloombergDataInstrument instrument, string fieldName)
        {
            object fieldValue = GetFieldValue(instrument, fieldName);
            return GetLong(fieldValue);
        }
        #endregion

        #region Int
        /// <summary>
        /// Gets the int value from the field value object.
        /// </summary>
        /// <param name="fieldValue">The field value.</param>
        /// <returns></returns>
        public static int? GetInt(object fieldValue)
        {
            int? actualValue = null;
            if (fieldValue != null)
            {
                int temp;
                int.TryParse(fieldValue.ToString(), out temp);
                actualValue = (int?)temp;
            }
            return actualValue;
        }

        /// <summary>
        /// Gets the int value from the BloombergData BBField collection.
        /// </summary>
        /// <param name="instrument">The instrument.</param>
        /// <param name="fieldName">Name of the field.</param>
        /// <returns></returns>
        public static int? GetInt(BloombergDataInstrument instrument, string fieldName)
        {
            object fieldValue = GetFieldValue(instrument, fieldName);
            return GetInt(fieldValue);
        }
        #endregion

        #region Dictionary<string, string>
        public static Dictionary<string, string> GetValuePairs(object fieldValue)
        {
            // string because the values are returned by Bloomberg as a delimited string
            Dictionary<string, string> ret = new Dictionary<string, string>();

            string[] pairs = fieldValue.ToString().Split(new char[] { '{', '}' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string pair in pairs)
            {
                string[] vals = pair.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

                ret.Add(vals[0], vals[1]);
            }

            return ret;
        }
        #endregion
    }

    //public enum BloombergDataInstrumentType
    //{
    //    Security,
    //    Currency,
    //    ReferenceRate,
    //    Company
    //}
}
