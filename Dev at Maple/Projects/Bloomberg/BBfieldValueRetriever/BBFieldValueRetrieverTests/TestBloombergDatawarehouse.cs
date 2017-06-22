using System;
using System.Collections.Generic;
using System.Reflection;
using BBfieldValueRetriever.Control;
using BBfieldValueRetriever.Model;

namespace BBFieldValueRetrieverTests
{
    /// <summary>
    ///
    /// </summary>
    public class TestBloombergDatawarehouse : BloombergDatawarehouseController
    {
        public List<string> NaToSecurityErrors = new List<string>();
        private readonly List<string> _naToProductErrors = new List<string>();
        private readonly List<string> _tickersNotFound = new List<string>();

        private readonly QueueTests _tester = new QueueTests();

        public TestBloombergDatawarehouse()
            : base(new BergController())
        {
            _tester.GetErrorRequestsFromLive(NaToSecurityErrors, _naToProductErrors, _tickersNotFound);
        }

        /// <summary>
        /// Same as the live version of this method, except that for more and smarter logging
        /// (checks tickers and fields which arent found against current live berg setup to see if
        /// they are already known problems)
        /// </summary>
        /// <param name="requestItems"></param>

        public override void ProcessDataRequests(IEnumerable<RequestItem> requestItems)
        {
            int tickerCount = 0;
            int tickerCountHits = 0;
            int fieldCount = 0;
            int fieldCountHits = 0;
            foreach (var item in requestItems)
            {
                tickerCount++;
                //calendar holidays
                if (item.BBFieldList.StartsWith("CALENDAR_NON_SETTLEMENT_DATES["))
                {
                    var list = Db.GetCalendarNonSetttlementDates(new CalendarNonSettlementDateRequest(item.BBFieldList));
                    var listString = new List<string>();
                    foreach (var thisDate in list) listString.Add(thisDate.ToString("yyyy-MM-dd"));
                    var returnedValuesFromDatawarehouse = new List<string> { string.Join(";", listString.ToArray()) };
                    item.Data.Add(DateTime.Now, returnedValuesFromDatawarehouse.ToArray());
                }
                else
                {
                    var t = GetDataFromDatawarehouse(item);

                    if (t == null)
                    {
                        item.Errors += "Ticker not found";
                        if (_tickersNotFound.Contains(item.OriginalInputTicker))
                            Console.WriteLine("ticker not found," + item.OriginalInputTicker + ",,known error");
                        else
                            Console.WriteLine("ticker not found," + item.OriginalInputTicker);
                    }
                    else
                    {
                        tickerCountHits++;
                        var returnedValuesFromDatawarehouse = new List<string>();
                        foreach (var field in item.riFields)
                        {
                            fieldCount++;
                            var cleanField = field.Key.ToUpper().Trim();
                            FieldInfo myf = t.GetType().GetField(cleanField);

                            //costing
                            if (t.TOR_PULLTYPE == "E")
                                Console.WriteLine("costing," + item.OriginalInputTicker + ",ticker found," + field);

                            if (myf != null && myf.GetValue(t) != null)
                            {
                                fieldCountHits++;
                                var returnedValue = myf.GetValue(t).ToString();
                                returnedValuesFromDatawarehouse.Add(returnedValue);
                                //error log NA NS or blanks.
                                if (returnedValue.Equals("N.A.") || returnedValue.Equals("N.S.") || returnedValue.Equals(string.Empty))
                                    item.Errors += string.Format("[{0}|returned {1}]", field.Key, returnedValue.Equals(string.Empty) ? "blank" : returnedValue);

                                //analyse for test!
                                //BLAPI returning NAs
                                if (myf.GetValue(t).ToString().Trim().Equals(string.Empty) || myf.GetValue(t).ToString().Trim().Equals("N.S.") || myf.GetValue(t).ToString().Trim().Equals("N.A."))
                                {
                                    if (myf.GetValue(t).ToString().Trim().Equals(string.Empty))
                                        Console.Write("returned blank,");
                                    if (myf.GetValue(t).ToString().Trim().Equals("N.S."))
                                        Console.Write("returned NS,");
                                    if (myf.GetValue(t).ToString().Trim().Equals("N.A."))
                                        Console.Write("returned NA,");

                                    if (_naToProductErrors.Contains(_tester.GetBloombergProduct(item.OriginalInputTicker) + "|" + cleanField))
                                        Console.WriteLine(item.OriginalInputTicker + "," + field.Key + ",known error product level");
                                    else if (NaToSecurityErrors.Contains(item.OriginalInputTicker + "|" + cleanField))
                                        Console.WriteLine(item.OriginalInputTicker + "," + field.Key + ",known error sec level");
                                    else if (field.Key.ToUpper().StartsWith("RTG_"))
                                        Console.WriteLine(item.OriginalInputTicker + "," + field.Key + ",known ignore");
                                    else if (field.Key.ToUpper().StartsWith("BB_COMP"))
                                        Console.WriteLine(item.OriginalInputTicker + "," + field.Key + ",known ignore");
                                    else if (field.Key.ToUpper() == "BID")
                                        Console.WriteLine(item.OriginalInputTicker + "," + field.Key + ",known ignore");
                                    else if (field.Key.ToUpper() == "ASK")
                                        Console.WriteLine(item.OriginalInputTicker + "," + field.Key + ",known ignore");
                                    else if (field.Key.ToUpper() == "DUR_ADJ_BID")
                                        Console.WriteLine(item.OriginalInputTicker + "," + field.Key + ",known ignore");
                                    else if (_tickersNotFound.Contains(item.OriginalInputTicker))
                                        Console.WriteLine(item.OriginalInputTicker + ",,known ignore- ticker not even currently found");
                                    else
                                        Console.WriteLine(item.OriginalInputTicker + "," + field.Key);
                                }
                            }
                            else
                            {
                                returnedValuesFromDatawarehouse.Add(null);
                                item.Errors += string.Format("[{0}|{1}]", field.Key, "field not found");

                                //analyse for test!
                                //our process failed to find it
                                if (_naToProductErrors.Contains(_tester.GetBloombergProduct(item.OriginalInputTicker) + "|" + cleanField))
                                    Console.WriteLine("field not found," + item.OriginalInputTicker + "," + field.Key + ",known error product level");
                                else if (NaToSecurityErrors.Contains(item.OriginalInputTicker + "|" + cleanField))
                                    Console.WriteLine("field not found," + item.OriginalInputTicker + "," + field.Key + ",known error sec level");
                                else if (field.Key.ToUpper().StartsWith("RTG_"))
                                    Console.WriteLine("field not found," + item.OriginalInputTicker + "," + field.Key + ",known ignore");
                                else if (field.Key.ToUpper().StartsWith("BB_COMP"))
                                    Console.WriteLine("field not found," + item.OriginalInputTicker + "," + field.Key + ",known ignore");
                                else if (field.Key.ToUpper() == "BID")
                                    Console.WriteLine("field not found," + item.OriginalInputTicker + "," + field.Key + ",known ignore");
                                else if (field.Key.ToUpper() == "ASK")
                                    Console.WriteLine("field not found," + item.OriginalInputTicker + "," + field.Key + ",known ignore");
                                else if (field.Key.ToUpper() == "DUR_ADJ_BID")
                                    Console.WriteLine("field not found," + item.OriginalInputTicker + "," + field.Key + ",known ignore");
                                else if (_tickersNotFound.Contains(item.OriginalInputTicker))
                                    Console.WriteLine("field not found," + item.OriginalInputTicker + ",,known ignore- ticker not even currently found");
                                else
                                    Console.WriteLine("field not found," + item.OriginalInputTicker + "," + field.Key);
                            }
                        }

                        item.Data.Add(DateTime.Now, returnedValuesFromDatawarehouse.ToArray());
                    }
                }
                //save
                Db.SaveValues(new List<RequestItem> { item });
            }

            Console.WriteLine("Hit rate: Tickers: " + tickerCountHits + "/" + tickerCount + " Fields: " + fieldCountHits + "/" + fieldCount);
        }
    }
}