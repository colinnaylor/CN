using System.Collections.Generic;
using System.Linq;
using BBfieldValueRetriever.Control;
using BBfieldValueRetriever.Model;
using NUnit.Framework;
using Shared;

namespace BBFieldValueRetrieverTests
{
    [TestFixture]
    public class BloombergControllerTests
    {
        [Test, Ignore("Need Bloomberg terminal for this test")]
        public void GetDataFromBloombergApiSynchronously()
        {
            var s = new List<RequestItem>
            {
                new RequestItem
                {
                  ID= 1,
                  riFields = new Dictionary<string,RequestItemField> { { "SHORT_NAME", new RequestItemField("SHORT_NAME")} }    ,
                  BBFieldList = "SHORT_NAME" ,
                    BBTicker = "SP3A2PAI Corp"  , SendToBloomberg = true , RequestType = BloombergDataInstrument.eRequestType.Reference
                }
            ,
                new RequestItem
                {
                  ID= 1,
                  riFields = new Dictionary<string,RequestItemField> { { "SHORT_NAME", new RequestItemField("SHORT_NAME")} }    ,
                  BBFieldList = "SHORT_NAME" ,
                    BBTicker = "IT0004840788 Corp"  , SendToBloomberg = true , RequestType = BloombergDataInstrument.eRequestType.Reference
                }
            };

            new BloombergApiController(new BergController()).RetrieveSynchronously(s);

            Assert.AreEqual("MGM Resorts International", s[0].Data.Values.ToArray()[0][0]);
            Assert.AreEqual("BTPS", s[1].Data.Values.ToArray()[0][0]);
        }
    }
}