using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using BBfieldValueRetriever;
using BBfieldValueRetriever.Control;
using BBfieldValueRetriever.Model;
using NUnit.Framework;

namespace BBFieldValueRetrieverTests
{
    [TestFixture]
    public class CalendarNonSettlementDatesTests
    {
        [Test]
        public void LoggerNameTest()
        {
            Console.WriteLine(Assembly.GetExecutingAssembly().GetName().FullName);
            Console.WriteLine(Assembly.GetExecutingAssembly().GetName().Name);
        }

        [Test]
        public void DateFormatTests()
        {
            Assert.AreEqual(new DateTime(2015, 1, 22), DateTime.ParseExact("01/22/2015", "MM/dd/yyyy", CultureInfo.InvariantCulture));
            //            Assert.AreEqual(new DateTime(2015, 1, 22), DateTime.ParseExact("1/22/2015", "MM/dd/yyyy", CultureInfo.InvariantCulture));

            Assert.AreEqual(new DateTime(2015, 1, 22), DateTime.ParseExact("01/22/2015", "M/dd/yyyy", CultureInfo.InvariantCulture));
            Assert.AreEqual(new DateTime(2015, 1, 22), DateTime.ParseExact("1/22/2015", "M/dd/yyyy", CultureInfo.InvariantCulture));
            Assert.AreEqual(new DateTime(2015, 1, 22), DateTime.ParseExact("1/22/15", "M/dd/yy", CultureInfo.InvariantCulture));

            //Assert.AreEqual(new DateTime(2015, 1, 22), DateTime.ParseExact("01/22/2015", "M/dd/yy", CultureInfo.InvariantCulture));
            //Assert.AreEqual(new DateTime(2015, 1, 22), DateTime.ParseExact("1/22/2015", "M/dd/yy", CultureInfo.InvariantCulture));
            Assert.AreEqual(new DateTime(2015, 1, 22), DateTime.ParseExact("1/22/15", "M/dd/yy", CultureInfo.InvariantCulture));
        }

        [Test]
        public void EnsureCalendarNonSettlementDateRequestConstructionIsCorrect()
        {
            var request = new CalendarNonSettlementDateRequest("CALENDAR_NON_SETTLEMENT_DATES[CALENDAR_START_DATE,20131205,CALENDAR_END_DATE,20180123,SETTLEMENT_CALENDAR_CODE,EN]");
            Assert.AreEqual(new DateTime(2013, 12, 05), request.CalendarStartDate);
            Assert.AreEqual(DateTime.Parse("23jan18"), request.CalendarEndDate);
            Assert.AreEqual("EN", request.SettlementCalendarCode);

            request = new CalendarNonSettlementDateRequest();
            Assert.AreEqual(null, request.SettlementCalendarCode);
            Assert.AreEqual(DateTime.MinValue, request.CalendarEndDate);
            Assert.AreEqual(DateTime.MinValue, request.CalendarStartDate);
        }

        [Test]
        public void RetrieveCalendarNonSettlementDatesFromToronto()
        {
            var list = new Database().GetCalendarNonSetttlementDates(
            new CalendarNonSettlementDateRequest("CALENDAR_NON_SETTLEMENT_DATES[CALENDAR_START_DATE,20131205,CALENDAR_END_DATE,20180123,SETTLEMENT_CALENDAR_CODE,EN]")
                );
        }

        [Test]
        public void ProcessCalendarNonSettlementDateRequest()
        {
            Utils.DbController.ExecuteNonQuery("delete BloombergDataResult WHERE BloombergDataRequestItemID = 9999999");

            var warehouse = new BloombergDatawarehouseController(new BergController());
            var request = new RequestItem
            {
                ID = 9999999,
                UserId = "Doesnt matter",
                BBTicker = "Not used",
                BBFieldList = "CALENDAR_NON_SETTLEMENT_DATES[CALENDAR_START_DATE,20140626,CALENDAR_END_DATE,20180814,SETTLEMENT_CALENDAR_CODE,EN]"
            };

            //this is the test
            warehouse.ProcessDataRequests(new List<RequestItem> { request });

            //this is retrieving result from the result queue.
            var res = Utils.DbController.GetScalar<string>("SELECT c1 FROM BloombergDataResult WHERE BloombergDataRequestItemID = 9999999");

            Assert.AreEqual("2014-08-25;2014-12-25;2014-12-26;2015-01-01;2015-04-03;2015-04-06;2015-05-04;2015-05-25;2015-08-31;2015-12-25;2015-12-28;2016-01-01;2016-03-25;2016-03-28;2016-05-02;2016-05-30;2016-08-29;2016-12-26;2016-12-27;2017-01-02;2017-04-14;2017-04-17;2017-05-01;2017-05-29;2017-08-28;2017-12-25;2017-12-26;2018-01-01;2018-03-30;2018-04-02;2018-05-07;2018-05-28;", res);
        }
    }

    [TestFixture]
    public class MiscTests
    {
        [Test]
        public void TimeZoneTests()
        {
            //4pm toronto time
            DateTime easternTime = new DateTime(2015, 4, 10, 16, 0, 0);

            TimeZoneInfo easternZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
            Console.WriteLine(easternZone.IsDaylightSavingTime(easternTime));

            Console.WriteLine("The date and time are {0} UTC.",
                  TimeZoneInfo.ConvertTime(easternTime, easternZone,
                  TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time")));

            Console.WriteLine("The date and time are {0} ",
                  TimeZoneInfo.ConvertTime(
                  DateTime.Parse("10apr2015 16:11:00"),
                  TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time"),
                  TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time")
                  ));
        }

        [Test]
        public void TinyIntTests()
        {
            Assert.AreEqual(1, Convert.ToInt16(true)); ;
            Assert.AreEqual(0, Convert.ToInt16(false)); ;
        }

        [Test]
        public void SplitWithStringDelimetersTest()
        {
            var t = Static.SplitWithStringDelimeters(
                "TICKER,BETA_ADJ_OVERRIDABLE[BETA_OVERRIDE_PERIOD,D,BETA_OVERRIDE_START_DT,20130411,BETA_OVERRIDE_END_DT,20140411,BETA_OVERRIDE_REL_INDEX,SPX Index]"
                , ',', '[', ']');

            Assert.AreEqual(2, t.Length);

            t = Static.SplitWithStringDelimeters(
                "TICKER,BETA_ADJ_OVERRIDABLE[BETA_OVERRIDE_PERIOD,D,BETA_OVERRIDE_START_DT,20130411,BETA_OVERRIDE_END_DT,20140411,BETA_OVERRIDE_REL_INDEX,SPX Index]"
                , ',', '£', '£');
            Assert.AreEqual(9, t.Length);

            t = Static.SplitWithStringDelimeters(
                "TICKER,BETA_ADJ_OVERRIDABLE[BETA_OVERRIDE_PERIOD,D,BETA_OVERRIDE_START_DT,20130411,BETA_OVERRIDE_END_DT,20140411,BETA_OVERRIDE_REL_INDEX,SPX Index],DELTA_OVERRIDABLE[BETA_OVERRIDE_PERIOD,D,BETA_OVERRIDE_START_DT]"
                , ',', '[', ']');
            Assert.AreEqual(3, t.Length);
        }

        [Test]
        public void ComparingRoutingRules()
        {
            var list1 = new List<RequestItemRoutingRule>
            {
                new RequestItemRoutingRule {  Datasource="BLAPI" , UserIdMatchRegex="RefRates" }  ,
                new RequestItemRoutingRule {  Datasource="Warehouse" , UserIdMatchRegex="OptionValueManager" }  ,
                new RequestItemRoutingRule {  Datasource="Warehouse" , UserIdMatchRegex="InsertUpdatePrices", FieldListMatchRegex="*" }
            };
            var list2 = new List<RequestItemRoutingRule>
            {
                new RequestItemRoutingRule {  Datasource="BLAPI" , UserIdMatchRegex="RefRates" }  ,
                new RequestItemRoutingRule {  Datasource="Warehouse" , UserIdMatchRegex="OptionValueManager" }  ,
                new RequestItemRoutingRule {  Datasource="Warehouse" , UserIdMatchRegex="InsertUpdatePrices", FieldListMatchRegex="*" }
            };

            Assert.IsTrue(list1.SequenceEqual(list2));
            list2.FirstOrDefault(x => x.UserIdMatchRegex == "OptionValueManager").Datasource = "BLAPI";
            Assert.IsFalse(list1.SequenceEqual(list2));

            var diff = list2.Except(list1, new RequestItemRoutingRuleEqualityComparer()).ToList();
            var diff2 = list1.Except(list2, new RequestItemRoutingRuleEqualityComparer()).ToList();

            list2.FirstOrDefault(x => x.UserIdMatchRegex == "OptionValueManager").Datasource = "Warehouse";
            Assert.IsTrue(list1.SequenceEqual(list2));

            list2.Add(new RequestItemRoutingRule { Datasource = "Warehouse", UserIdMatchRegex = "InsertUpdatePrices", FieldListMatchRegex = "*" });
            Assert.IsFalse(list1.SequenceEqual(list2));
        }
    }
}