using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Maple;

namespace SwiftImporterConsole.Model
{
    public enum MessageType
    {
        Unknown,
        ACK,
        MT300,
        MT535,
        MT940,
        MT950
    }

    public abstract class SwiftMessage
    {
        public SwiftMessage(string Content, string applicationID, string serviceID, string logicalTerminalAddress,
            int sessionNumber, int sequenceNumber)
        {
            source = Content;
            ApplicationID = applicationID;
            ServiceID = serviceID;
            LogicalTerminalAddress = logicalTerminalAddress;
            SessionNumber = sessionNumber;
            SequenceNumber = sequenceNumber;

        }
        // Basic header block
        public string ApplicationID { get; set; }
        public string ServiceID { get; set; }
        public string LogicalTerminalAddress { get; set; }
        public int SessionNumber { get; set; }
        public int SequenceNumber { get; set; }
        // Application header block
        public string Direction { get; set; }
        public MessageType Type { get; set; }
        public string ReceiverAddress { get; set; }
        public string Priority { get; set; }  // S=system, N=normal, U=urgent
        public string DeliveryMonitor { get; set; }  // 1=Non delivery  2=Delivery notification  3=Both valid
        public string ObsolescencePeriod { get; set; } // the number of 5 minutes before a non delivery notification
        public string InputDate { get; set; }
        public string InputTime { get; set; }
        public string SenderAddress { get; set; }
        public string MessageInputReference { get; set; }
        public string OutputDate { get; set; }
        public string OutputTime { get; set; }
        // ---
        protected string source = "";
        protected string block1 = "";
        protected string block2 = "";
        protected string block3 = "";
        protected string block4 = "";
        protected string block5 = "";

        /// <summary>
        /// Split the message block into the lines that it contains.
        /// </summary>
        /// <param name="Content"></param>
        /// <returns></returns>
        internal static string[] Lines(string Content)
        {
            // split on each new tag that appears on a new line
            string[] ret = Content.Split(new string[] { "\n:" }, StringSplitOptions.RemoveEmptyEntries);

            for (int line = 0; line < ret.Length; line++)
            {
                ret[line] = ":" + ret[line].Replace("\r", "").Replace("\n", "").Trim();
            }

            return ret;
        }

        public abstract void Parse();

        public void ParseApplicationHeaderBlock()
        {

            block1 = GetBlock(1);
            block2 = GetBlock(2);
            block3 = GetBlock(3);
            block4 = GetBlock(4);
            block5 = GetBlock(5);

            if (block2.Length > 0)
            {
                // Application block is present
                Direction = block2.Substring(0, 1);
                try
                { // If the other data is missing, leave the fields as empty
                    // The minimum data must be at least {2:I300MAPAGB2LXXXX which covers Direction, Type (processed earlier) and ReceiverAddress
                    if (Direction == "I")
                    {
                        ReceiverAddress = block2.Substring(4, 12);
                        Priority = block2.Substring(16, 1);
                        DeliveryMonitor = block2.Substring(17, 1);
                        ObsolescencePeriod = block2.Substring(18, 3);
                    }
                    if (Direction == "O")
                    {
                        InputTime = block2.Substring(4, 4);
                        InputDate = block2.Substring(8, 6);
                        MessageInputReference = block2.Substring(14, 22);
                        SenderAddress = block2.Substring(14, 12);
                        OutputDate = block2.Substring(36, 6);
                        OutputTime = block2.Substring(42, 4);
                        Priority = block2.Substring(46, 1);
                    }
                }
                catch
                {
                    // If data is missing from Block 2 there is not a lot we can do.
                }
            }
        }

        private string GetBlock(int Block)
        {
            string ret = "";
            int level = 0;

            int pos = source.IndexOf("{" + Block.ToString() + ":");
            if (pos > -1)
            {
                pos += 3;
                level = 1;
                for (int ch = pos; ch < source.Length; ch++)
                {
                    if (source.Substring(ch, 1) == "{") level++;
                    else if (source.Substring(ch, 1) == "}") level--;

                    if (level == 0)
                    {
                        ret = source.Substring(pos, ch - pos);
                        break;
                    }
                }
            }

            return ret;

        }
    }
}
