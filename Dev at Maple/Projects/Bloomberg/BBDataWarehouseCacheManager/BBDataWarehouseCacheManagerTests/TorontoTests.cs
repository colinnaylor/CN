using System;
using BBDataWarehouseCacheManager;
using BBDataWarehouseCacheManager.Controllers;
using NUnit.Framework;

namespace BBDataWarehouseCacheManagerTests
{

    [TestFixture]
    public class TorontoTests
    {
        [Test]
        public void EnsureWeCanTellWhenTorontoViewsAreReady()
        {

            var Tminus1 = new DateUtils().PreviousWorkDay(DateTime.Now.Date);
            var m = new TorontoViewManager();

            Assert.IsTrue(m.DataIsReadyInViewForDate("ve_BloombergDescription", Tminus1));

            Assert.IsTrue(m.DataIsReadyInViewForDate("ve_BloombergPerSecurityPull", Tminus1)); //different logic for ve_BloombergPerSecurityPull

            Assert.IsTrue(m.DataIsReadyInViewForDate("ve_BloombergCreditRisk", Tminus1)); //historic not kept for credit risk - too big.

            var res = Utils.DbController.GetScalar<DateTime>("select top 1 FileDate from helium.bloombergdatalicense.dbo.BloombergPerSecurityfiledate where 1=2");

            //looking for reasons to fail - null, or the date is wrong.
            Assert.AreEqual(DateTime.MinValue, res);

        }
    }
}
