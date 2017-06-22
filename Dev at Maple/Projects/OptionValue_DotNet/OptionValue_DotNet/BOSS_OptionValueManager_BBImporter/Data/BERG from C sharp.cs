using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Maple;
using System.Data.SqlClient;

namespace BBGShared
{
    class BERG_from_C_sharp
    {
        #region Using Central BERG system
        internal static SQLServer GetDB()
        {
#if DEBUG
            return new SQLServer("Bloomberg_Test");
#else
            return new SQLServer("Bloomberg");
#endif
        }

        /// <summary>
        /// Requests data via the BERG system in tablular form
        /// </summary>
        /// <param name="Requests">The list of request items to fetch</param>
        /// <param name="AppID">An ID used to identify the calling process</param>
        /// <returns>The Reference ID with which to find the results</returns>
        internal static int RequestBBdata(List<BloombergDataInstrument> Requests, string AppID)
        {
            int fieldCount;
            if (FieldCountIsNotEqual(Requests, out fieldCount))
            {
                throw new Exception("The number of fields for each instrument must be equal.");
            }

            int ret = 0;
            string sql = "SET NOCOUNT ON\r\n"
                + "CREATE TABLE #RequestDetail(RequestType varchar(50),Ticker varchar(50),Fields varchar(1000),DateFrom datetime,Problem varchar(250)";
            for (int i = 1; i <= fieldCount; i++)
            {
                sql += string.Format(",Value{0} varchar(250)", i);
            }
            sql += " )";
            sql += "\r\nINSERT #RequestDetail(RequestType,Ticker,DateFrom,Fields)\r\n";
            sql += "VALUES \r\n";

            foreach (BloombergDataInstrument ins in Requests)
            {
                string row = "('";
                if (ins.RequestType == BloombergDataInstrument.eRequestType.Historical)
                {
                    row += string.Format("historic','{0}','{1}','", ins.Ticker, ins.DateFrom.ToString("yyyyMMdd"));
                }
                else
                {
                    row += string.Format("Reference','{0}',null,'", ins.Ticker);
                }

                foreach (string field in ins.BBFields.Keys)
                {
                    row += field + ",";
                }
                // Take off trailing comma
                row = row.Substring(0, row.Length - 1);
                row += "'),\r\n";

                sql += row;
            }
            // Take off trailing comma \r\n
            sql = sql.Substring(0, sql.Length - 3) + "\r\n";

            sql += "Declare @ref int \r\n"
                + string.Format("EXEC @ref = FetchBloombergValues '{0}', {1}\r\n", AppID, fieldCount)
                + "SELECT @ref";

            SQLServer db = GetDB();
            try
            {
                SqlDataReader dr = db.FetchDataReader(sql);
                if (dr.Read())
                {
                    int r;
                    if (int.TryParse(dr[0].ToString(), out r))
                    {
                        ret = r;
                    }
                }
                dr.Close();

            }
            finally
            {
                db.Close();
            }

            //  The data will be availble in the temp table and in the data view with the appropriate UserID and Reference
            //  "SELECT * FROM BloombergDataView WHERE UserID = 'OptionValueRetrieval' AND Reference = @ref";
            //  SELECT * FROM #RequestDetail

            return ret;
        }

        /// <summary>
        /// Requests data via the BERG system vertically, with one field per row
        /// </summary>
        /// <param name="Requests">The list of request items to fetch</param>
        /// <param name="AppID">An ID used to identify the calling process</param>
        /// <returns>The Reference ID with which to find the results</returns>
        internal static int RequestBBdataVertically(List<BloombergDataInstrument> Requests, string AppID)
        {
            int ret = 0;
            string sql = "SET NOCOUNT ON\r\n"
                + "CREATE TABLE #BBGRequestDetail(RequestType varchar(50),Ticker varchar(50),Fields varchar(1000),"
                + "DateFrom datetime,DateTo datetime,Periodicity VARCHAR(50),Problem varchar(250),Value1 varchar(2000) ),\r\n";

            int counter = 1000;
            foreach (BloombergDataInstrument ins in Requests)
            {
                foreach (string field in ins.BBFields.Keys)
                {

                    string row = "('";
                    if (ins.RequestType == BloombergDataInstrument.eRequestType.Historical)
                    {
                        row += string.Format("historic','{0}','{1}','DAILY','", ins.Ticker, ins.DateFrom.ToString("yyyyMMdd"));
                    }
                    else
                    {
                        row += string.Format("Reference','{0}',null,null,'", ins.Ticker);
                    }

                    row += field + "'),\r\n";

                    if (counter == 1000)
                    {
                        counter = 0;
                        //need an insert and values
                        //remove trailing comma
                        sql = sql.Substring(0, sql.Length - 3) + "\r\n";
                        sql += "\r\nINSERT #BBGRequestDetail(RequestType,Ticker,DateFrom,Periodicity,Fields)\r\n";
                        sql += "VALUES \r\n";
                    }

                    sql += row;
                    counter++;
                }
            }
            sql = sql.Substring(0, sql.Length - 3) + "\r\n";

            int timeout = OTCOptionValuation_BBImporter.Properties.Settings.Default.BBtimeoutSeconds;
            int polingDelay = OTCOptionValuation_BBImporter.Properties.Settings.Default.BBpolingSeconds;

            sql += "Declare @ref int \r\n"
                + string.Format("EXEC @ref = GetBloombergData '{0}', {1}, {2}\r\n", AppID,timeout, polingDelay)
                + "SELECT @ref";

            SQLServer db = GetDB();
            try
            {
                SqlDataReader dr = db.FetchDataReader(sql);
                if (dr.Read())
                {
                    int r;
                    if (int.TryParse(dr[0].ToString(), out r))
                    {
                        ret = r;
                    }
                }
                dr.Close();

            }
            finally
            {
                db.Close();
            }

            //  The data will be availble in the temp table and in the data view with the appropriate UserID and Reference
            //  "SELECT * FROM BloombergDataView WHERE UserID = '%APPID%' AND Reference = @ref";
            //  SELECT * FROM #RequestDetail

            return ret;
        }

        private static bool FieldCountIsNotEqual(List<BloombergDataInstrument> Requests, out int fieldCount)
        {
            bool ret = false;
            int count = 0;
            fieldCount = 0;

            foreach (BloombergDataInstrument ins in Requests)
            {
                int items = 0;
                foreach (BloombergDataInstrumentField field in ins.BBFields.Values)
                {
                    items++;
                }
                if (count == 0)
                {
                    count = items;
                    fieldCount = count;
                }
                else
                {
                    if (count != items)
                    {
                        ret = true;
                        break;
                    }
                }
            }

            return ret;
        }
        #endregion

    }
}
