using System;
using System.Linq;
using Maple;

namespace SwiftImporterUI.Model
{
    partial class SwiftStatement
    {

        public virtual void Insert(SwiftDataDataContext dataContext)
        {
            ID = dataContext.InsertSwiftStatement(StatementType, BIC, AccountNumber, StatementNumber, SequenceNumber, Date, FileName);
        }

        public virtual bool Exists(SwiftDataDataContext dataContext)
        {
            bool ret;
            try
            {
                ret = dataContext.SwiftStatements.Where(s =>
                   BIC == s.BIC
                   && Date == s.Date
                   && StatementNumber == s.StatementNumber
                   && SequenceNumber == s.SequenceNumber
                   && StatementType == s.StatementType
                   && AccountNumber == s.AccountNumber).Count() > 0;
            }
            catch (Exception ex)
            {
                ret = true; // Assume it exists because we had an error
                string err = "Database query failed whilst determining whether a swift message was already in the database.";
                Notifier.DUOC("Swift Importer Error", string.Format("{0}\r\n\r\n{1}\r\n\r\n{2}", err, ex.Message, ex.StackTrace));
            }
            return ret;
        }
    }

    partial class SwiftDataDataContext
    {
        public bool StatementExists(SwiftStatement statement)
        {
            return statement.Exists(this);
        }
    }
    partial class MT535
    {

        public override void Insert(SwiftDataDataContext dataContext)
        {
            ID = dataContext.InsertMT535Statement(StatementType, BIC, AccountNumber, StatementNumber, SequenceNumber, Date, FileName, SenderReference);
        }

        public override bool Exists(SwiftDataDataContext dataContext)
        {
            bool ret;
            try
            {
                ret = dataContext.SwiftStatements.Where(s => s is MT535 &&
                    s.BIC == BIC &&
                    AccountNumber == s.AccountNumber &&
                    s.Date == Date &&
                    s.StatementNumber == StatementNumber &&
                    ((MT535)s).SenderReference == SenderReference).Count() > 0;
            }
            catch (Exception ex)
            {
                ret = true; // Assume it exists because we had an error
                string err = "Database query failed whilst determining whether a swift 535 message was already in the database.";
                Notifier.DUOC("Swift Importer Error", string.Format("{0}\r\n\r\n{1}\r\n\r\n{2}", err, ex.Message, ex.StackTrace));
            }
            return ret;
        }
    }

    partial class MT940
    {
        public override void Insert(SwiftDataDataContext dataContext)
        {
            ID = dataContext.InsertMT940Statement(StatementType, BIC, AccountNumber, StatementNumber, SequenceNumber, Date, FileName, Currency, OpeningBalance, ClosingBalance);
        }

        public override bool Exists(SwiftDataDataContext dataContext)
        {
            bool ret;
            try
            {
                ret = dataContext.SwiftStatements.Where(s =>
                    s is MT940 &&
                    BIC == s.BIC
                    && Date == s.Date
                    && StatementNumber == s.StatementNumber
                    && SequenceNumber == s.SequenceNumber
                    && StatementType == s.StatementType
                    && AccountNumber == s.AccountNumber
                    && Currency == ((MT940)s).Currency).Count() > 0;
            }
            catch (Exception ex)
            {
                ret = true; // Assume it exists because we had an error
                string err = "Database query failed whilst determining whether a swift 940 message was already in the database.";
                Notifier.DUOC("Swift Importer Error", string.Format("{0}\r\n\r\n{1}\r\n\r\n{2}", err, ex.Message, ex.StackTrace));
            }
            return ret;
        }
    }

    partial class SwiftStatement_Old
    {
        /// <summary>
        /// Create a new swift statement object
        /// </summary>
        /// <param name="swiftFileName">The filename of a processed swift file</param>
        public SwiftStatement_Old(string swiftFileName)
            : this()  // cannot override the default one unfort, so can't enforce this is called.  
        {
            FileName = swiftFileName;
        }
    }
}
