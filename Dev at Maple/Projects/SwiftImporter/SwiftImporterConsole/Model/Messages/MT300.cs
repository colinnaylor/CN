using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Maple;

namespace SwiftImporterConsole.Model.Messages
{
    public class MT300 : SwiftMessage
    {
        public MT300(string Content, string applicationID, string serviceID, string logicalTerminalAddress,
            int sessionNumber, int sequenceNumber)
            : base(Content, applicationID, serviceID, logicalTerminalAddress, sessionNumber, sequenceNumber)
        {
            Type = MessageType.MT300;
        }
        public string TypeOfOperation { get; set; } // NEWT, AMND or CANC
        public string CommonReference { get; set; }
        public string PartyA { get; set; }
        public string PartyB { get; set; }
        public DateTime TradeDate { get; set; }
        public DateTime ValueDate { get; set; }
        public double ExchangeRate { get; set; }
        public string BoughtCurrency { get; set; }
        public double BoughtAmount { get; set; }
        public string BoughtReceivingAgent { get; set; }
        public string SoldCurrency { get; set; }
        public double SoldAmount { get; set; }
        public string SoldReceivingAgent { get; set; }
        public string SenderReference { get; set; }

        public override void Parse()
        {
            base.ParseApplicationHeaderBlock();

            try
            {

                string[] lines = SwiftMessage.Lines(source);

                bool boughtSection = true; // bought section is the first Subsequence B1 section
                foreach (string line in lines)
                {
                    string tag = line.Substring(1, 3).ToUpper();

                    switch (tag)
                    {
                        case "20:":
                            SenderReference = line.Substring(4);
                            break;
                        case "22A":
                            TypeOfOperation = line.Substring(5);
                            break;
                        case "22C":
                            CommonReference = line.Substring(5);
                            break;
                        case "82A":
                            PartyA = line.Substring(5);
                            break;
                        case "87A":
                            PartyB = line.Substring(5);
                            break;
                        case "30T":
                            // Date is in US format and from index 5 to 10 (i.e. :61:070328)
                            TradeDate = DateTime.Parse(line.Substring(11, 2) + "/" + line.Substring(9, 2) + "/" + line.Substring(5, 4));
                            break;
                        case "30V":
                            ValueDate = DateTime.Parse(line.Substring(11, 2) + "/" + line.Substring(9, 2) + "/" + line.Substring(5, 4));
                            break;
                        case "36:":
                            ExchangeRate = double.Parse(line.Substring(4).Replace(",", "."));
                            break;
                        case "32B":
                            BoughtCurrency = line.Substring(5, 3);
                            BoughtAmount = double.Parse(line.Substring(8).Replace(",", "."));
                            break;
                        case "57A":
                            if (boughtSection)
                            {
                                BoughtReceivingAgent = line.Substring(5);
                                boughtSection = false;
                            }
                            else
                            {
                                SoldReceivingAgent = line.Substring(5);
                            }
                            break;
                        case "33B":
                            SoldCurrency = line.Substring(5, 3);
                            SoldAmount = double.Parse(line.Substring(8).Replace(",", "."));
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception("Error caught in MT300 Parse method.\r\n" + e.Message + "\r\n", e);
            }
        }

        public override string ToString()
        {
            return this.Type.ToString();
        }

    }
}
