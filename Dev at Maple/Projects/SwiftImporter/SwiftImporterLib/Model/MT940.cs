using System;
using System.Collections.Generic;
using Maple;

namespace SwiftImporterLib.Model.Messages
{
    public class MT940 : SwiftMessage
    {
        public MT940(string Content, string applicationID, string serviceID, string logicalTerminalAddress,
            int sessionNumber, int sequenceNumber)
            : base(Content, applicationID, serviceID, logicalTerminalAddress, sessionNumber, sequenceNumber)
        {
            Type = MessageType.MT940;
        }

        public int StatementNo { get; set; }
        public int SequenceNo { get; set; }
        public string AccountNumber { get; set; }
        public string Currency { get; set; }
        public double OpeningBalance { get; set; }
        public double ClosingBalance { get; set; }
        public DateTime Date { get; set; }
        public List<MT940Activity> MT940Activities = new List<MT940Activity>();

        public override void Parse()
        {
            base.ParseApplicationHeaderBlock();

            try
            {
                string[] lines = Lines(source);
                string tag = "", previousTag = "";

                foreach (string stmtLine in lines)
                {
                    string settDate = "";
                    string amount = "";
                    string description = "";
                    previousTag = tag;

                    try
                    {
                        if (stmtLine.Length > 3)
                        {
                            tag = (stmtLine.Substring(1, 3));
                        }
                        else
                        {
                            tag = "n/a";
                        }
                    }
                    catch (Exception ex)
                    {
                        NLogger.Instance.Error(ex);
                    }
                    // Tags we look for = 25, 28C, 60F, 60M, 61, 62F, 62M, 64

                    if (tag.Contains("25")) // Account info
                    {
                        #region  Field 25: Account Identification

                        /*
FORMAT
    35x (Account) 

Presence
    Mandatory

Definition
    This field identifies the account for which the statement is sent.
                        
                         */

                        #endregion

                        AccountNumber = stmtLine.Substring(4);
                    }
                    else if (tag.Contains("28C"))
                    {
                        #region Field 28C: Statement Number/Sequence Number

                        /*
Format
    Option C 5n[/5n] (Statement Number)(Sequence Number) 

Presence
    Mandatory

Definition
    This field contains the sequential number of the statement, optionally followed by the sequence number of the message within that statement when more than one message is sent for one statement.

Usage Rules
    The statement number should be reset to 1 on 1 January of each year.

    If used, the sequence number always starts with 1. When several messages are sent to convey information about a single statement, the first message must contain '/1' in Sequence Number.

    The sequence number must be incremented by one for each additional message.

    Both the statement number and sequence number enable the Receiver to put the different messages into sequence and thus form the complete statement.

Example
    The first message of a statement is :28C:235/1

    The second message is :28C:235/2 and so on.

                         */

                        #endregion

                        int seperatorIndex = stmtLine.IndexOf("/");
                        if (seperatorIndex != -1)
                        {
                            StatementNo = Convert.ToInt32(stmtLine.Substring(5, seperatorIndex - 5));
                            SequenceNo = Convert.ToInt32(stmtLine.Substring(seperatorIndex + 1, stmtLine.Length - seperatorIndex - 1));
                        }
                        else
                        {
                            StatementNo = Convert.ToInt32(stmtLine.Substring(5));
                            SequenceNo = 1; // if none present then there must just be one message in the statement
                        }

                    }
                    else if ((tag.Contains("60F") || tag.Contains("60M"))) // Opening/intermediate balance found (containing ccy)
                    {
                        #region Field 60a: Opening Balance

                        /*
Format
    Option F 1!a6!n3!a15d - (D/C Mark)(Date)(Currency)(Amount) 
    Option M 1!a6!n3!a15d - (D/C Mark)(Date)(Currency)(Amount) 


Presence
    Mandatory

Definition
    This field specifies, for the (intermediate) opening balance, whether it is a debit or credit balance, the date, the currency and the amount of the balance.

Codes
    In option F, or M, D/C Mark must contain one of the following codes (Error code(s): T51):

        C
         The (intermediate) opening balance is a credit balance
 
        D
         The (intermediate) opening balance is a debit balance
 

Network Validated Rules
    Date must be a valid date expressed as YYMMDD (Error code(s): T50).

    Currency must be a valid ISO 4217 currency code (Error code(s): T52).

    The integer part of Amount must contain at least one digit. The decimal comma ',' is mandatory and is included in the maximum length. The number of digits following the comma must not exceed the maximum number allowed for the specified currency (Error code(s): C03, T40, T43).

Usage Rules
    This field must always be the same as field 62a (closing balance) of the previous statement message for this account.

    The first statement message for a specified period must contain field 60F (first opening balance); additional statement messages for the same statement period must contain field 60M (intermediate opening balance).

*/

                        #endregion


                        // currency is only stored in the opening and closing balance lines.  
                        // We can then assume, that everthing in between is of that currency too.

                        //swiftStatement.OpeningBalance.Date = Convert.ToDateTime(val.Substring(6, 6));
                        Currency = stmtLine.Substring(12, 3);
                        OpeningBalance = ExtractBalance(stmtLine);

                    }
                    else if (tag == "61:") // Statement Line found
                    {
                        #region Field 61: Statement Line

                        /*
Format
    6!n[4!n]2a[1!a]15d1!a3!c16x[//16x]
    [34x] 

    where subfields are:

    Subfield
     Format
     Name
 
    1 6!n (Value Date) 
    2 [4!n] (Entry Date) 
    3 2a (Debit/Credit Mark) 
    4 [1!a] (Funds Code) 
    5 15d (Amount) 
    6 1!a3!c (Transaction Type Identification Code) 
    7 16x (Reference for the Account Owner) 
    8 [//16x]
     (Account Servicing Institution's Reference)
 
    9 [34x] (Supplementary Details) 


Presence
    Optional

Definition
    This field contains the details of each transaction.

    Subfield 1 Value Date is a date expressed in full ISO 8601 format YYMMDD.

    Subfield 2 Entry Date is a date expressed in reduced ISO 8601 format MMDD.

    Subfield 4 Funds Code is the 3rd character of the currency code, if needed.

Codes
    Subfield 3 Debit/Credit Mark must contain one of the following codes (Error code(s): T51):

        C
         Credit
 
        D
         Debit
 
        RC
         Reversal of Credit (debit entry)
 
        RD
         Reversal of Debit (credit entry)
 

Codes
    Subfield 6 Transaction Type Identification Code must contain one of the following codes (Error code(s): T53):

        S
         3!n
         For entries related to SWIFT transfer instructions and subsequent charge messages. The last three characters will indicate the message type of the SWIFT message causing the entry (for debit entries) or the message type of the SWIFT message used to advise the account owner (for credit entries).
 
        N
         3!c
         For entries related to payment and transfer instructions, including related charges messages, not sent through SWIFT or where an alpha description is preferred. The last three characters, that is, 3!c, may contain a code (see below).
 
        F
         3!c
         For entries being first advised by the statement (items originated by the account servicing institution). The last three characters, that is, 3!c, may contain a code (see below).
 

Codes
    When the first character of subfield 6 Transaction Type Identification Code is 'N' or 'F', the remaining characters may contain one of the following codes:

    BNK
     Securities Related Item - Bank Fees
 
    BOE
     Bill of Exchange
 
    BRF
     Brokerage Fee
 
    CAR
     Securities Related Item - Corporate Actions Related (should only be used when no specific corporate action event code is available)
 
    CAS
     Securities Related Item - Cash in Lieu
 
    CHG
     Charges and Other Expenses
 
    CHK
     Cheques
 
    CLR
     Cash Letters/Cheques Remittance
 
    CMI
     Cash Management Item - No Detail
 
    CMN
     Cash Management Item - Notional Pooling
 
    CMP
     Compensation Claims
 
    CMS
     Cash Management Item - Sweeping
 
    CMT
     Cash Management Item -Topping
 
    CMZ
     Cash Management Item - Zero Balancing
 
    COL
     Collections (used when entering a principal amount)
 
    COM
     Commission
 
    CPN
     Securities Related Item - Coupon Payments
 
    DCR
     Documentary Credit (used when entering a principal amount)
 
    DDT
     Direct Debit Item
 
    DIS
     Securities Related Item - Gains Disbursement
 
    DIV
     Securities Related Item - Dividends
 
    EQA
     Equivalent Amount
 
    EXT
     Securities Related Item - External Transfer for Own Account
 
    FEX
     Foreign Exchange
 
    INT
     Interest
 
    LBX
     Lock Box
 
    LDP
     Loan Deposit
 
    MAR
     Securities Related Item - Margin Payments/Receipts
 
    MAT
     Securities Related Item - Maturity
 
    MGT
     Securities Related Item - Management Fees
 
    MSC
     Miscellaneous
 
    NWI
     Securities Related Item - New Issues Distribution
 
    ODC
     Overdraft Charge
 
    OPT
     Securities Related Item - Options
 
    PCH
     Securities Related Item - Purchase (including STIF and Time deposits)
 
    POP
     Securities Related Item - Pair-off Proceeds
 
    PRN
     Securities Related Item - Principal Pay-down/Pay-up
 
    REC
     Securities Related Item - Tax reclaim
 
    RED
     Securities Related Item - Redemption/Withdrawal
 
    RIG
     Securities Related Item - Rights
 
    RTI
     Returned Item
 
    SAL
     Securities Related Item - Sale (including STIF and Time deposits)
 
    SEC
     Securities (used when entering a principal amount)
 
    SLE
     Securities Related Item - Securities Lending Related
 
    STO
     Standing Order
 
    STP
     Securities Related Item - Stamp Duty
 
    SUB
     Securities Related Item - Subscription
 
    SWP
     Securities Related Item - SWAP Payment
 
    TAX
     Securities Related Item - Withholding Tax Payment
 
    TCK
     Travellers Cheques
 
    TCM
     Securities Related Item - Tripartite Collateral Management
 
    TRA
     Securities Related Item - Internal Transfer for Own Account
 
    TRF
     Transfer
 
    TRN
     Securities Related Item - Transaction Fee
 
    UWC
     Securities Related Item - Underwriting Commission
 
    VDA
     Value Date Adjustment (used with an entry made to withdraw an incorrectly dated entry - it will be followed by the correct entry with the relevant code)
 
    WAR
     Securities Related Item - Warrant
 

Network Validated Rules
    Subfield 1, Value Date, must be a valid date expressed as YYMMDD (Error code(s): T50).

    The SWIFT System validates subfield 2, Entry Date (Date in reduced ISO form MMDD), using current System Year (Error code(s): T50).

    The integer part of Amount must contain at least one digit. The decimal comma ',' is mandatory and is included in the maximum length (Error code(s): T40, T43).

    When the first character of subfield 6, Transaction Type Identification Code, is an 'S', the remaining characters must be in the range 100-999 (Error code(s): T18).

Usage Rules
    This field may be repeated within the constraints of the maximum input message length.

    'Original' advice for charges, that is, the first time the account owner is informed of a charge, must be identified in subfield 6, Transaction Type Identification Code, with the transaction type code 'FCHG'.

    The following rules apply to subfield 7, Reference for the Account Owner:

    At least one valid character other than a blank must be present.

    For debit entries, the purpose of this subfield is to identify, to the account owner, the instruction which caused the debit. Therefore, the content of this subfield is the field 20 Sender's Transaction Reference Number (or its equivalent) of the original instruction.

    Credit entries may be the result of one of the following situations:

    The account servicing institution is identifying, to the account owner the receipt of funds for its account as a result of a related transaction. In this case, the content of subfield 7, Reference for the Account Owner is the reference for the beneficiary (for example, field 21 Related Reference) of the related transaction.

    The account servicing institution has issued a payment instruction to the account owner and the credit identified in this subfield is for that payment. The content of subfield 7, Reference for the Account Owner is the field 20 Transaction Reference Number (or its equivalent) of the payment instruction issued by the account servicing institution.

    If no reference is available for subfield 7, Reference for the Account Owner, the code NONREF shall be used. The account servicing institution must then supply, in subfield 9, Supplementary Details, what it considers to be the best alternative information.

    This reference must be quoted in all cases when available. In cases where a transaction passes through several financial institutions, the original reference must always be forwarded.

    This reference must always be quoted against any charges or fees debited by the account servicing institution.

    Debits against standing instructions must show the reference of the standing instruction.

    In cases where a mutually agreed alternative reference exists (for example, in foreign exchange or money market transactions), this reference should then be used.

    If the statement entry concerns a cheque, the cheque number should be indicated in this subfield.

    The following rules apply to subfield 8, Account Servicing Institution's Reference:

    The content of this subfield is the account servicing institution's own reference for the transaction.

    When the transaction has been initiated by the account servicing institution, this reference may be identical to subfield 7, Reference for the Account Owner. If this is the case, Account Servicing Institution's Reference, subfield 8 may be omitted.

    The following rules apply to subfield 9, Supplementary Details:

    When no reference for the account owner is available, that is, subfield 7, Reference for the Account Owner contains NONREF, the account servicing institution should provide the best available alternative information in this subfield.

    Supplementary details may be provided when an advice has not been sent for a transaction, or to provide additional information to facilitate reconciliation.

Example
    (1) :61:0901230122C3500,25FCHK304955//4958843

    (2) :61:0901230122C3500,25FCHK304955//4958843
    ADDITIONAL INFORMATION

                         */

                        #endregion


                        // Date is in US format and from index 5 to 10 (i.e. :61:070328)
                        settDate = stmtLine.Substring(8, 2) + "/" + stmtLine.Substring(6, 2) + "/" + stmtLine.Substring(4, 2);

                        //Find the Debit/Credit Mark :- 
                        //  D/DR = Debit, 
                        //  C/CR = Credit, 
                        //  RD = Reversal of Debit (Credit Entry), 
                        //  RC = Reversal of Credit (Debit Entry)

                        int tempCount = stmtLine.IndexOf(","); // the comma is the decimal part of the amount which is always there.

                        // GH 201000422 the start pos could be after 14 or 10
                        int startPos = -1;
                        // try the 14th pos as that is the usual start point
                        bool ok = DetermineStartPos(14, stmtLine, ref amount, ref startPos);
                        // and if that does not work then try the 10th pos
                        if (!ok)
                        {
                            ok = DetermineStartPos(10, stmtLine, ref amount, ref startPos);
                        }
                        // and if that does not work report an error
                        if (!ok)
                        {
                            throw new Exception("The SWIFT code 61 does not contain a Credit/Debit identifier, please check. \r\n" +
                                                "LINE:  " + stmtLine);
                        }

                        int endPos = stmtLine.IndexOf(",");
                        // We only want the numbers so don't include the DR or CR characters
                        //startPos += 2;

                        // Sometimes we get CR/DR sometimes we just get C/D, so just replace if they exist.
                        //amount = amount + val.Substring(startPos, endPos - startPos).Trim(("DCR").ToCharArray());
                        amount = amount + stmtLine.Substring(startPos, endPos - startPos);

                        string dec = "";
                        // Anything after decimal place
                        int decInt = 0;
                        bool isnum = Int32.TryParse(stmtLine.Substring(stmtLine.IndexOf(",") + 1, 1), out decInt); // do we have 1 dp
                        if (isnum) //first one is a number so store and look for the next
                        {
                            dec = decInt.ToString();
                            isnum = Int32.TryParse(stmtLine.Substring(stmtLine.IndexOf(",") + 2, 1), out decInt); // do we have a 2nd dp
                            if (isnum)
                                dec = dec + decInt.ToString(); // if so then set it
                        }

                        amount = amount + "." + dec;

                        endPos += dec.Length + 1;


                        description = stmtLine.Substring(endPos, stmtLine.Length - endPos).Trim();

                        // Create the recItem
                        MT940Activity swiftItem = new MT940Activity
                        {
                            Amount = Convert.ToDouble(amount),
                            Description = description,
                            ValueDate = Convert.ToDateTime(settDate)
                        };

                        MT940Activities.Add(swiftItem);

                    }
                    else if (tag == "86:")
                    {
                        // Description appendage to Tag 61
                        if (previousTag == "61:")
                        {
                            MT940Activity activity = MT940Activities[MT940Activities.Count - 1]; // last activity added
                            activity.Description += ".  " + stmtLine.Substring(4);
                        }
                    }
                    else if ((tag == "62F" || tag == "62M")) // using either final or intermediate as we store both now
                    {
                        #region Field 62a: Closing Balance (Booked Funds)

                        /*
Format
    Option F 1!a6!n3!a15d (D/C Mark)(Date)
    (Currency)(Amount) 
    Option M 1!a6!n3!a15d (D/C Mark)(Date)
    (Currency)(Amount) 


Presence
    Mandatory

Definition
    This field specifies, for the (intermediate) closing balance, whether it is a debit or credit balance, the date, the currency and the amount of the balance.

Codes
    In option F, or M, D/C Mark must contain one of the following codes (Error code(s): T51):

        C
         The (intermediate) closing balance is a credit balance
 
        D
         The (intermediate) closing balance is a debit balance
 

Network Validated Rules
    Date must be a valid date expressed as YYMMDD (Error code(s): T50).

    Currency must be a valid ISO 4217 currency code (Error code(s): T52).

    The integer part of Amount must contain at least one digit. The decimal comma ',' is mandatory and is included in the maximum length. The number of digits following the comma must not exceed the maximum number allowed for the specified currency (Error code(s): C03, T40, T43).

Usage Rules
    The contents of this field will be repeated in field 60a of the subsequent statement message for this account.

    If there is only one statement message transmitted for the period, this field must use tag option F, that is, 62F (final closing balance). When several messages are transmitted for the same statement period, all messages except the last message must contain field 62M (intermediate closing balance); the last message of the statement must contain field 62F.

                         */

                        #endregion


                        // tag found but it must be the last one in the file, so overwrite if found

                        //get the balance date
                        string dateStr = stmtLine.Substring(10, 2) + "/" + stmtLine.Substring(8, 2) + "/" + stmtLine.Substring(6, 2);

                        Date = Convert.ToDateTime(dateStr); // date of the closing balance is statement date

                        try
                        {
                            ClosingBalance = ExtractBalance(stmtLine);
                        }
                        catch (Exception ex)
                        {
                            ClosingBalance = -1.1;
                            throw new Exception("Balance couldn't be updated: \r\n" + ex.Message, ex);
                        }
                    }
                    else if (tag == "64:") // use 64 if it is present (it is an optional tag)  - available funds
                    {
                        #region Field 64: Closing Available Balance (Available Funds)

                        /*
Format
    1!a6!n3!a15d (D/C Mark)(Date)(Currency)(Amount) 


Presence
    Optional

Definition
    This field indicates the funds which are available to the account owner (if credit balance) or the balance which is subject to interest charges (if debit balance).

Codes
    D/C Mark must contain one of the following codes (Error code(s): T51):

    C
     The closing available balance is a credit balance
 
    D
     The closing available balance is a debit balance
 

Network Validated Rules
    Date must be a valid date expressed as YYMMDD (Error code(s): T50).

    Currency must be a valid ISO 4217 currency code (Error code(s): T52).

    The integer part of Amount must contain at least one digit. The decimal comma ',' is mandatory and is included in the maximum length. The number of digits following the comma must not exceed the maximum number allowed for the specified currency (Error code(s): C03, T40, T43).

                         */

                        #endregion


                        string dateStr = stmtLine.Substring(9, 2) + "/" + stmtLine.Substring(7, 2) + "/" + stmtLine.Substring(5, 2);

                        Date = Convert.ToDateTime(dateStr); // date of the closing balance is statement date

                        try
                        {
                            ClosingBalance = ExtractBalance(stmtLine);
                        }
                        catch (Exception ex)
                        {
                            ClosingBalance = -1.1;
                            throw new Exception("Balance couldn't be updated: \r\n" + ex.Message, ex);
                        }

                    }

                }

            }
            catch (Exception e)
            {
                throw new Exception("Error caught in MT940 Parse method.\r\n" + e.Message + "\r\n", e);
            }
        }

        public override string SqlInsertString()
        {
            string sql = "declare @ID int\r\nEXEC @ID=InsertMT940statement 'MT940','{0}','{1}',{2},{3},'{4}','{5}','{6}',{7},{8}".Args(
                SenderAddress,
                AccountNumber,
                StatementNo,
                SequenceNo,
                Date.ToString("yyyyMMdd"),
                ContainingSwiftFileName.Replace(".working", ""),
                Currency,
                OpeningBalance,
                ClosingBalance
                );


            foreach (MT940Activity activity in MT940Activities)
            {
                sql += " \r\nINSERT MT940Activity(ValueDate,Amount,Description,StatementID) VALUES ('{0}','{1}','{2}',@ID)".Args(
                    activity.ValueDate.ToString("yyyyMMdd"),
                    activity.Amount,
                    activity.Description.Replace("'", "''")
                    );
            }
            return sql;
        }

        private static double ExtractBalance(string balanceLine)
        {
            // first we need to figure out where the balance is.  62f tag or 64 shift the position of :
            int startPos = balanceLine.IndexOf(":", 1); // start at pos 1 to ignore the first colon
            int signPos = startPos + 1;
            int datePos = signPos + 1;
            int ccyPos = datePos + 6;
            int balPos = ccyPos + 3;

            string balStr = balanceLine.Substring(balPos, balanceLine.Length - balPos);

            //Make sure we don't have anything else added to end of balance:
            if (balStr.Contains("-}"))
                balStr = balStr.Replace(balStr.Substring(balStr.IndexOf("-}"), balStr.Length - balStr.IndexOf("-}")), "");

            balStr = balStr.Replace(",", "."); // commas are decimal points

            double balance = Convert.ToDouble(balStr);

            if (balanceLine.Substring(signPos, 1).ToUpper().Equals("D"))
            {
                balance = balance*-1;
            }

            return balance;
        }

        private static bool DetermineStartPos(int pos, string val, ref string amount, ref int startPos)
        {

            bool ret = true;
            startPos = -1;
            // The mark is always after the first 14 or 10 characters
            string mark = val.Substring(pos, 2);
            bool markFound = true;
            switch (mark.Substring(0, 2))
            {
                case "CR":
                    startPos = val.IndexOf(mark) + 2;
                    break;
                case "DR":
                    startPos = val.IndexOf(mark) + 2;
                    amount = "-";
                    break;
                case "RD":
                    startPos = val.IndexOf(mark) + 2;
                    if (val.Substring(startPos, 1).ToUpper() == "R") // Sometimes comes as RDR and RD
                        startPos = startPos + 1;
                    break;
                case "RC":
                    startPos = val.IndexOf(mark) + 2;
                    if (val.Substring(startPos, 1).ToUpper() == "R") // Sometimes comes as RCR and RC
                        startPos = startPos + 1;
                    amount = "-";
                    break;
                default:
                    markFound = false;
                    break;
            }

            if (!markFound)
            {
                // mark not found so must be either D or C, or includes a funds code (ISO Currency code)
                int tmp = 0;
                if (Int32.TryParse(mark.Substring(1, 1), out tmp)) // if the 2nd char is numeric then we don't want to include it as it is the amount (subfield 5)
                    startPos = val.IndexOf(mark.Substring(0, 1)) + 1;
                else
                    startPos = val.IndexOf(mark) + 2;

                if (mark.Substring(0, 1) == "D")
                    amount = "-";
                else if (mark.Substring(0, 1) != "C")
                    ret = false;

            }

            // HA 11/10/10, this stems from receiving a swift with a funds code, so it came in with a P after the credit-debit indicator i.e. 
            //:61:1010081008CP3110 and :61:1009301008RDP5440,NM
            // Subfield 4 Funds Code is the 3rd character of the currency code, if needed.  In this case P for gbP 
            int firstAmt = 0;
            if (!Int32.TryParse(val.Substring(startPos, 1), out firstAmt)) // i.e. next char to be used is not part of the amount but is in fact the funds code 
                startPos += 1;

            return ret;
        }

        public override string ToString()
        {
            return this.Type.ToString();
        }

        public override string SqlAlreadyExistsString()
        {
            var sql = "select count(*) from SwiftStatementsAll \r\n";
            sql += "Where Bic='{0}' and Date='{1}' and StatementNumber={2} and SequenceNumber={3}\r\n".Args(
                SenderAddress,
                Date.ToString("yyyyMMdd"),
                StatementNo,
                SequenceNumber);
            sql += "and StatementType='MT940' and AccountNumber='{0}'".Args(
                AccountNumber);
            return sql;
        }

        public class MT940Activity
        {
            public DateTime ValueDate { get; set; }
            public DateTime EntryDate { get; set; }
            public double Amount { get; set; }
            public string Description { get; set; }

        }
    }
}
