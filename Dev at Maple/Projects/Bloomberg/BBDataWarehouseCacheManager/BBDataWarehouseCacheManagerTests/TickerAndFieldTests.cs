using System;
using BBDataWarehouseCacheManager.Controllers;
using BBDataWarehouseCacheManager.Models;
using NUnit.Framework;

namespace BBDataWarehouseCacheManagerTests
{

    [TestFixture]
    public class TickerAndFieldTests
    {

        [Test]
        public void TestForAmericanExchanges()
        {
            var ticker = "amt US";
            Assert.IsTrue(new TickerManager().EndWithsNorthAmericanExchangeCode(ticker));
            Assert.AreEqual("amt US", ticker);
        }

        [Test]
        public void BlankTorontoIDTest()
        {
            var t = new BergTicker("CA135087B1600 Corp");
        }
        [Test]
        public void AreTheseFuturesExpiryCode()
        {
            var mgr = new TickerManager();

            Assert.IsTrue(mgr.IsFuturesExpiryCode("M4"));
            Assert.IsFalse(mgr.IsFuturesExpiryCode("M44"));
            Assert.IsTrue(mgr.IsFuturesExpiryCode("F5"));
            Assert.IsFalse(mgr.IsFuturesExpiryCode("A5"));
            Assert.IsFalse(mgr.IsFuturesExpiryCode("FG"));

        }
        [Test]
        public void AreTheseISINs()
        {
            var mgr = new TickerManager();
            Assert.IsTrue(mgr.IsIsin("SE0005686144"));
            Assert.IsTrue(mgr.IsIsin("US0378331005"));
            Assert.IsTrue(mgr.IsIsin("AU0000XVGZA3"));
            Assert.IsTrue(mgr.IsIsin("GB0002634946"));
            Assert.IsFalse(mgr.IsIsin("XS09756342004"));
            Assert.IsFalse(mgr.IsIsin("WIMHY US"));
            Assert.IsFalse(mgr.IsIsin("AU0000XVGZAA"));

        }
        [Test]
        public void AreTheseTenors()
        {
            var mgr = new TickerManager();
            Assert.IsTrue(mgr.IsTenor("0001M"));
            Assert.IsTrue(mgr.IsTenor("00012Y"));
            Assert.IsTrue(mgr.IsTenor("4D"));
            Assert.IsTrue(mgr.IsTenor("0001W"));
            Assert.IsFalse(mgr.IsTenor("W0001W"));
            Assert.IsFalse(mgr.IsTenor("0001F"));
            Assert.IsFalse(mgr.IsTenor("BP0001M"));
        }

        [Test]
        public void AreTheseBloombergGlobalIDsEquities()
        {
            var mgr = new TickerManager();

            Assert.IsTrue(mgr.BloombergGlobalIsEquity("BBG000BB5SQ7"));
            Assert.IsTrue(mgr.BloombergGlobalIsEquity("BBG000BDZQP5"));
            Assert.IsTrue(mgr.BloombergGlobalIsEquity("BBG000BF2304"));
            Assert.IsTrue(mgr.BloombergGlobalIsEquity("BBG000BGX8Y0"));
            Assert.IsTrue(mgr.BloombergGlobalIsEquity("BBG000BRFCC1"));
            Assert.IsFalse(mgr.BloombergGlobalIsEquity("BBG000FLF7J4"));

            Assert.IsFalse(mgr.BloombergGlobalIsEquity("BBG000FLF6H8"));
            Assert.IsFalse(mgr.BloombergGlobalIsEquity("BBG000DSM9K7"));
            Assert.IsFalse(mgr.BloombergGlobalIsEquity("BBG000F79QX4"));
        }

        [Test]
        public void TickerTests()
        {

            Assert.AreEqual("DPW GY Equity", new BergTicker("DPW GY EQUITY").TorId);
            Assert.AreEqual(string.Empty, new BergTicker("CA135087B1600 Corp").TorId);
            Assert.AreEqual("0150080 LN | SEDOL1", new BergTicker("0150080 LN SEDOL1").TorId);
        }

        [Test]
        public void MakeSureThatPricingCategoriesAreWorking()
        {
            var mgr = new DatawarehouseManager();
            // some equity requests
            var fieldList = "BDVD_NEXT_EST_EX_DT,BDVD_PROJ_DIV_AMT,DVD_CRNCY,VOLATILITY_10D_CALC,VOLATILITY_180D_CALC,VOLATILITY_200D_CALC,VOLATILITY_260D_CALC,VOLATILITY_30D_CALC,VOLATILITY_360D_CALC,VOLATILITY_60D_CALC,VOLATILITY_90D_CALC";

            Assert.AreEqual(1, Convert.ToInt16(mgr.RequiresBloombergLicenseSecurityMaster(fieldList, false)));
            Assert.AreEqual(1, Convert.ToInt16(mgr.RequiresBloombergLicenseDerivedDataEod(fieldList)));
            Assert.AreEqual(0, Convert.ToInt16(mgr.RequiresBloombergLicensePricingDataEod(fieldList)));
            Assert.AreEqual(1, Convert.ToInt16(mgr.RequiresBloombergLicenseEstimates(fieldList)));
            Assert.AreEqual(0, Convert.ToInt16(mgr.RequiresBloombergLicenseCreditRisk(fieldList)));

            fieldList = "COUNTRY,EQY_SH_OUT_REAL,ID_ISIN";
            Assert.AreEqual(1, Convert.ToInt16(mgr.RequiresBloombergLicenseSecurityMaster(fieldList, false)));
            Assert.AreEqual(0, Convert.ToInt16(mgr.RequiresBloombergLicenseDerivedDataEod(fieldList)));
            Assert.AreEqual(0, Convert.ToInt16(mgr.RequiresBloombergLicensePricingDataEod(fieldList)));
            Assert.AreEqual(0, Convert.ToInt16(mgr.RequiresBloombergLicenseEstimates(fieldList)));
            Assert.AreEqual(0, Convert.ToInt16(mgr.RequiresBloombergLicenseCreditRisk(fieldList)));

            fieldList = "ID_BB_GLOBAL,NAME,SECURITY_TYP,ID_BB_ULTIMATE_PARENT_CO,ID_BB_ULTIMATE_PARENT_CO_NAME";
            Assert.AreEqual(0, Convert.ToInt16(mgr.RequiresBloombergLicenseSecurityMaster(fieldList, false)));
            Assert.AreEqual(0, Convert.ToInt16(mgr.RequiresBloombergLicenseDerivedDataEod(fieldList)));
            Assert.AreEqual(0, Convert.ToInt16(mgr.RequiresBloombergLicensePricingDataEod(fieldList)));
            Assert.AreEqual(0, Convert.ToInt16(mgr.RequiresBloombergLicenseEstimates(fieldList)));
            Assert.AreEqual(1, Convert.ToInt16(mgr.RequiresBloombergLicenseCreditRisk(fieldList)));

            fieldList = "ID_BB_GLOBAL,NAME,SECURITY_TYP,ID_BB_ULTIMATE_PARENT_CO,ULT_PARENT_CNTRY_INCORPORATION";
            Assert.AreEqual(0, Convert.ToInt16(mgr.RequiresBloombergLicenseCreditRisk(fieldList)));

            fieldList = " PX_LAST,PX_BID,PX_ASK,  QUOTE_TYP,PX_YEST_CLOSE,PX_CLOSE_2D ";
            Assert.AreEqual(0, Convert.ToInt16(mgr.RequiresBloombergLicenseSecurityMaster(fieldList, false)));
            Assert.AreEqual(0, Convert.ToInt16(mgr.RequiresBloombergLicenseDerivedDataEod(fieldList)));
            Assert.AreEqual(1, Convert.ToInt16(mgr.RequiresBloombergLicensePricingDataEod(fieldList)));

            Assert.AreEqual(0, Convert.ToInt16(mgr.RequiresBloombergLicensePricingDataIntraday(fieldList, true)));
            Assert.AreEqual(1, Convert.ToInt16(mgr.RequiresBloombergLicensePricingDataIntraday(fieldList, false)));

            Assert.AreEqual(0, Convert.ToInt16(mgr.RequiresBloombergLicenseEstimates(fieldList)));
            Assert.AreEqual(0, Convert.ToInt16(mgr.RequiresBloombergLicenseCreditRisk(fieldList)));

            fieldList = "BDVD_NEXT_EST_EX_DT,BDVD_PROJ_DIV_AMT,DVD_CRNCY,ID_ISIN,VOLATILITY_10D_CALC,VOLATILITY_180D_CALC,VOLATILITY_200D_CALC,VOLATILITY_260D_CALC,VOLATILITY_30D_CALC,VOLATILITY_360D_CALC,VOLATILITY_60D_CALC,VOLATILITY_90D_CALC";
            Assert.AreEqual(1, Convert.ToInt16(mgr.RequiresBloombergLicenseDerivedDataEod(fieldList)));
            Assert.AreEqual(0, Convert.ToInt16(mgr.RequiresBloombergLicensePricingDataIntraday(fieldList, true)));
            Assert.AreEqual(0, Convert.ToInt16(mgr.RequiresBloombergLicensePricingDataIntraday(fieldList, false)));
            Assert.AreEqual(0, Convert.ToInt16(mgr.RequiresBloombergLicensePricingDataEod(fieldList)));

            fieldList = "PX_LAST,PX_LAST_Eod,PX_BID,PX_ASK,  QUOTE_TYP,PX_YEST_CLOSE,PX_CLOSE_2D";
            Assert.IsFalse(mgr.RequiresBloombergLicensePricingDataIntraday(fieldList, true));
            Assert.IsTrue(mgr.RequiresBloombergLicensePricingDataEod(fieldList));

            fieldList = "EQY_SH_OUT_REAL,INDUSTRY_SECTOR,COUNTRY_FULL_NAME,SSR_LIQUIDITY_INDICATOR,PX_LAST,PX_LAST_Eod,PX_BID,PX_ASK,  QUOTE_TYP,PX_YEST_CLOSE,PX_CLOSE_2D ";
            Assert.IsTrue(mgr.RequiresBloombergLicensePricingDataIntraday(fieldList, false));
            Assert.IsFalse(mgr.RequiresBloombergLicensePricingDataIntraday(fieldList, true));

            fieldList = "ULT_PARENT_CNTRY_DOMICILE, COUNTRY";
            Assert.IsTrue(mgr.RequiresBloombergLicenseSecurityMaster(fieldList, false));
            Assert.IsFalse(mgr.RequiresBloombergLicenseSecurityMaster(fieldList, true));

            fieldList = "EQY_SH_OUT_REAL,ID_ISIN,ULT_PARENT_CNTRY_DOMICILE, COUNTRY";
            Assert.IsFalse(mgr.RequiresBloombergLicenseSecurityMaster(fieldList, true));

        }
    }


}
