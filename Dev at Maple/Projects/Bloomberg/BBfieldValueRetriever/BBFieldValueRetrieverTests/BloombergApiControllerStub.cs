using System;
using System.Collections.Generic;
using System.Threading;
using Shared;

namespace BBfieldValueRetriever.Control
{
    public class BloombergApiControllerStub : BloombergApiController
    {
        public BloombergApiControllerStub()
            : base(new BergController())
        {
        }

        public override void GetBbgData(List<BloombergDataInstrument> bbdis)
        {
            RaiseMessageEvent("GetBBGData called! ");
            Console.WriteLine("called ");
            Completed.Reset();
            var t = new Timer(x => Completed.Set(), null, 4000, 4000);
            Completed.WaitOne();
            Console.WriteLine("waiting");
            //simulate wait and signal on another thread
            //Task.Factory.StartNew(() => { System.Threading.Thread.Sleep(3000); completed.Set(); });

            Console.WriteLine("done");
        }
    }
}