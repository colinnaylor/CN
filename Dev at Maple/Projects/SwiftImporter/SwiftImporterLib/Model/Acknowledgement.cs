using System;

namespace SwiftImporterLib.Model.Messages
{
    public class Acknowledgement : SwiftMessage
    {
        public Acknowledgement(string Content, string applicationID, string serviceID, string logicalTerminalAddress,
            int sessionNumber, int sequenceNumber)
            : base(Content, applicationID, serviceID, logicalTerminalAddress, sessionNumber, sequenceNumber)
        {
            Type = MessageType.ACK;
        }

        public override void Parse()
        {
            base.ParseApplicationHeaderBlock();
        }

        public override string SqlInsertString()
        {
            throw new Exception("Message type not handled in SaveSwiftMessages method.");
        }
    }
}
