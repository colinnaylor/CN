using NUnit.Framework;
using SwiftImporterLib.Model;
using SwiftImporterUI.Model;

namespace SwiftImporterTests
{
    [TestFixture]
    public class MT300Tests
    {
        [Test]
        public void DeserializeMT300()
        {
            SwiftFile swiftFile = new SwiftFile("TestFiles\\M300_20160114.114822.inc");

            Assert.AreEqual(2, swiftFile.Messages.Count);

            Assert.IsTrue(swiftFile.Messages[0] is MT300);
            Assert.IsTrue(swiftFile.Messages[1] is MT300);

            Assert.AreEqual("EUR", (swiftFile.Messages[0] as MT300).BoughtCurrency);
            Assert.AreEqual("USD", (swiftFile.Messages[1] as MT300).BoughtCurrency);

        }
    }
}
