using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SwiftImporterUI.Model;

namespace SwiftImporterConsole.Model.Messages {
    public class Acknowledgement : SwiftMessage {
        public Acknowledgement(string Content, string applicationID, string serviceID, string logicalTerminalAddress,
            int sessionNumber, int sequenceNumber)
            : base(Content,applicationID,serviceID,logicalTerminalAddress,sessionNumber,sequenceNumber) {
                Type = MessageType.ACK;
        }

        public override void Parse() {
            base.ParseApplicationHeaderBlock();
        }
    }
}
