namespace SwiftImporterLib.Model
{
    public interface ISwiftMessage
    {
        string ApplicationID { get; set; }
        string ContainingSwiftFileName { get; set; }
        string DeliveryMonitor { get; set; }
        string Direction { get; set; }
        string InputDate { get; set; }
        string InputTime { get; set; }
        string LogicalTerminalAddress { get; set; }
        string MessageInputReference { get; set; }
        string ObsolescencePeriod { get; set; }
        string OutputDate { get; set; }
        string OutputTime { get; set; }
        string Priority { get; set; }
        string ReceiverAddress { get; set; }
        string SenderAddress { get; set; }
        int SequenceNumber { get; set; }
        string ServiceID { get; set; }
        int SessionNumber { get; set; }
        MessageType Type { get; set; }

        void Parse();
        void ParseApplicationHeaderBlock();
        string SqlAlreadyExistsString();
        string SqlInsertString();
    }
}