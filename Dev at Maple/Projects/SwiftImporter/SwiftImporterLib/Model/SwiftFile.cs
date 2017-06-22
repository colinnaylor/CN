using System;
using System.Collections.Generic;
using System.IO;
using Maple;
using SwiftImporterLib.Model.Messages;
using SwiftImporterUI.Model;

namespace SwiftImporterLib.Model
{
    public class SwiftFile
    {

        public List<SwiftMessage> Messages = new List<SwiftMessage>();
        private string allLines = "";

        public SwiftFile(string SwiftFileName)
        {
            filename = SwiftFileName;

            ReadFile();

            // Split into SwiftMessages
            List<string> blocks = SplitIntoMessages();

            SwiftMessage message = null;

            // for each SwiftMessage
            foreach (string block in blocks)
            {
                // now we can determine the message type so we know which to import
                message = SelectMessageType(block);
                message.ContainingSwiftFileName = SwiftFileName;
                if (message.Type != MessageType.ACK)
                {
                    Messages.Add(message);
                }
            }
        }

        private List<string> SplitIntoMessages()
        {
            List<string> ret = new List<string>();
            string block;

            int pos = allLines.IndexOf("{1:");
            while (pos > -1)
            {
                int end = allLines.IndexOf("{1:", pos + 1);

                if (end == -1)
                { // last block
                    block = allLines.Substring(pos);
                }
                else
                {
                    block = allLines.Substring(pos, end - pos);
                }
                // Remove odd non char characters
                block = block.Replace((char)3, ' ').Replace((char)1, ' ').Trim();

                ret.Add(block);

                pos = end;
            }

            return ret;
        }

        private string filename = "";
        public string Filename
        {
            get
            {
                return filename;
            }
        }

        private void ReadFile()
        {
            if (!File.Exists(filename)) //file not found
            {
                throw new FileNotFoundException("The filename, " + filename + ", does not exist. \r\n");
            }

            allLines = File.ReadAllText(filename);

        }

        private static SwiftMessage SelectMessageType(string Content)
        {
            SwiftMessage ret = null;

            string[] lines = SwiftMessage.Lines(Content);

            int pos = lines[0].IndexOf("{1:");
            string applicationID = lines[0].Substring(pos + 3, 1);
            string serviceID = lines[0].Substring(pos + 4, 2);
            string logicalTerminalAddress = lines[0].Substring(pos + 6, 12);
            int sessionNumber = int.Parse(lines[0].Substring(pos + 18, 4));
            int sequenceNumber = int.Parse(lines[0].Substring(pos + 22, 6));

            switch (serviceID)
            {
                case "21": // ACK/NAK
                    ret = new Acknowledgement(Content, applicationID, serviceID, logicalTerminalAddress, sessionNumber, sequenceNumber);
                    break;
                case "01": // FIN/GPA
                    pos = lines[0].IndexOf("{2:");
                    if (pos == -1)
                    {
                        throw new Exception("Section 2 missing from Swift message\r\n----------\r\n{0}".Args(Content));
                    }
                    string messageType = lines[0].Substring(pos + 4, 3);
                    switch (messageType)
                    {
                        case "300":
                            ret = new MT300(Content, applicationID, serviceID, logicalTerminalAddress, sessionNumber, sequenceNumber);
                            break;
                        case "535":
                            ret = new MT535(Content, applicationID, serviceID, logicalTerminalAddress, sessionNumber, sequenceNumber);
                            break;
                        case "940":
                            ret = new MT940(Content, applicationID, serviceID, logicalTerminalAddress, sessionNumber, sequenceNumber);
                            break;
                        case "950":
                            ret = new MT950(Content, applicationID, serviceID, logicalTerminalAddress, sessionNumber, sequenceNumber);
                            break;
                        default:
                            throw new NotImplementedException("Message Type not supported.");
                    }

                    break;
                default:
                    throw new Exception("Service type in message is not supported [{0}]".Args(serviceID));
            }

            ret.Parse();

            return ret;
        }

        public override string ToString()
        {
            if (File.Exists(filename))
            {
                return Path.GetFileName(filename);
            }
            else
            {
                return "SwiftFile (empty)";
            }
        }
    }
}
