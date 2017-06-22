using System;
using System.Collections.Generic;
using System.Globalization;

namespace SwiftImporterUI.Model
{
    public static class SwiftImporter
    {
        private enum StatementType
        {
            MT940,
            MT950,
            MT535,
            MT300
        }

        public static List<SwiftStatement> ImportFile(string filePathName)
        {
            // first thing is to read in the file so we look into it
            SwiftFile swiftFile = new SwiftFile(filePathName);

            // now we can determine the statement type so we know which to import
            StatementType statementType = DetermineStatementType(swiftFile.FileArray);

            switch (statementType)
            {
                case StatementType.MT940:
                    return ImportCashStatement(swiftFile);
                case StatementType.MT535:
                    return ImportPositionStatement(swiftFile);
                case StatementType.MT300:
                    // Ignore for now
                    return new List<SwiftStatement>();
                default:
                    throw new NotImplementedException("StatementType not setup");
            }
        }

        private static StatementType DetermineStatementType(string[] swiftFileLines)
        {
            // Need to figure out a better way of doing this.  Would be good to somehow
            // determine the file type from the statement itself.  This will be fine for now, 
            // but will get difficult if we start processing more than just mt940 or mt535.

            // CN 2014 05 08. It's easy, simply look for the second part of the message marked {2:O and read the 3 chars that indicate
            // the type of the message. e.g. 535 and 940

            int pos = swiftFileLines[0].IndexOf("{2:");
            string inputOutput = swiftFileLines[0].Substring(pos + 3, 1);
            string messageType = swiftFileLines[0].Substring(pos + 4, 3);
            if (messageType == "300")
            {
                return StatementType.MT300;
            }

            // otherwise back to the old dodge method.

            string cashStartTag = "20:";
            string positionStartTag = "16R:";

            if (swiftFileLines.Length < 2)
                throw new NotSupportedException("No data in swift file");

            if (swiftFileLines[1].StartsWith(cashStartTag))
                return StatementType.MT940;
            else if (swiftFileLines[1].StartsWith(positionStartTag))
                return StatementType.MT535;
            else

                throw new NotSupportedException("Cannot find the expected start tags in this swift file");
        }


        private static List<SwiftStatement> ImportCashStatement(SwiftFile swiftFile)
        {

            var statements = ParseMT940(swiftFile);

            return statements;
        }

        private static List<SwiftStatement> ImportPositionStatement(SwiftFile swiftFile)
        {
            List<SwiftStatement> statements = ParseMT535(swiftFile);

            return statements;
        }




        #region Old Stuff



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
                    if (val.Substring(startPos, 1).ToUpper() == "R")  // Sometimes comes as RDR and RD
                        startPos = startPos + 1;
                    break;
                case "RC":
                    startPos = val.IndexOf(mark) + 2;
                    if (val.Substring(startPos, 1).ToUpper() == "R")  // Sometimes comes as RCR and RC
                        startPos = startPos + 1;
                    amount = "-";
                    break;
                default:
                    markFound = false;
                    break;
            }

            if (!markFound)
            { // mark not found so must be either D or C, or includes a funds code (ISO Currency code)
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

            balStr = balStr.Replace(",", ".");  // commas are decimal points

            double balance = Convert.ToDouble(balStr);

            if (balanceLine.Substring(signPos, 1).ToUpper().Equals("D"))
            {
                balance = balance * -1;
            }

            return balance;
        }



        private static List<SwiftStatement> ParseMT940(SwiftFile swiftFile)
        {
            // File needs to be MT940

            List<SwiftStatement> statements = new List<SwiftStatement>();
            MT940 swiftStatement = new MT940 { FileName = swiftFile.FileName };

            try
            {
                Console.Out.WriteLine(swiftFile.CurrentLine); // Allows us to debug easier

                // Read the first line in the file, which is the headers including BIC
                string stmtLine = swiftFile.NextStatementLine();


                string bic = "";

                // while (!stream.EndOfStream)
                while (stmtLine != "")
                {
                    string settDate = "";
                    string amount = "";
                    string description = "";
                    string tag = "";

                    try
                    {
                        // first make sure we have a valid file containing a bic
                        int bicBlockStartPos = stmtLine.IndexOf("{2:");
                        if (bicBlockStartPos != -1)// the header block that contains the bic
                        {
                            // new header block so reset the bic
                            bic = "";
                            swiftStatement.StatementType = StatementType.MT940.ToString();
                            bic = stmtLine.Substring(bicBlockStartPos + 17, 12);
                        }
                        if (bic == "")
                            throw new NotSupportedException("Cannot find a BIC code in the file");


                        tag = (stmtLine.Substring(1, 3));
                    }
                    catch (Exception ex)
                    {
                        Console.Out.WriteLine(ex.Message);
                    }
                    // Tags we look for = 25, 28C, 60F, 60M, 61, 62F, 62M, 64

                    if (tag.Contains("25"))  // Account info
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

                        // must be new statement block so create a new one:
                        swiftStatement = new MT940
                        {
                            FileName = swiftFile.FileName,
                            StatementType = StatementType.MT940.ToString(),
                            BIC = bic,
                            AccountNumber = stmtLine.Substring(4)
                        };

                        statements.Add(swiftStatement);
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
                            swiftStatement.StatementNumber = Convert.ToInt32(stmtLine.Substring(5, seperatorIndex - 5));
                            swiftStatement.SequenceNumber = Convert.ToInt32(stmtLine.Substring(seperatorIndex + 1, stmtLine.Length - seperatorIndex - 1));
                        }
                        else
                        {
                            swiftStatement.StatementNumber = Convert.ToInt32(stmtLine.Substring(5));
                            swiftStatement.SequenceNumber = 1; // if none present then there must just be one message in the statement
                        }

                    }
                    else if ((tag.Contains("60F") || tag.Contains("60M")))  // Opening/intermediate balance found (containing ccy)
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
                        swiftStatement.Currency = stmtLine.Substring(12, 3);
                        swiftStatement.OpeningBalance = ExtractBalance(stmtLine);

                    }
                    else if (tag.Contains("61"))  // Statement Line found
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
                        bool isnum = Int32.TryParse(stmtLine.Substring(stmtLine.IndexOf(",") + 1, 1), out decInt);  // do we have 1 dp
                        if (isnum)  //first one is a number so store and look for the next
                        {
                            dec = decInt.ToString();
                            isnum = Int32.TryParse(stmtLine.Substring(stmtLine.IndexOf(",") + 2, 1), out decInt);  // do we have a 2nd dp
                            if (isnum)
                                dec = dec + decInt.ToString();  // if so then set it
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

                        swiftStatement.MT940Activities.Add(swiftItem);

                    }
                    else if ((tag == "62F" || tag == "62M"))  // using either final or intermediate as we store both now
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

                        swiftStatement.Date = Convert.ToDateTime(dateStr); // date of the closing balance is statement date

                        try
                        {
                            swiftStatement.ClosingBalance = ExtractBalance(stmtLine);
                        }
                        catch (Exception ex)
                        {
                            swiftStatement.ClosingBalance = -1.1;
                            throw new Exception("Balance couldn't be updated: \r\n" + ex.Message, ex);
                        }
                    }
                    else if (tag == "64:")  // use 64 if it is present (it is an optional tag)  - available funds
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

                        swiftStatement.Date = Convert.ToDateTime(dateStr); // date of the closing balance is statement date

                        try
                        {
                            swiftStatement.ClosingBalance = ExtractBalance(stmtLine);
                        }
                        catch (Exception ex)
                        {
                            swiftStatement.ClosingBalance = -1.1;
                            throw new Exception("Balance couldn't be updated: \r\n" + ex.Message, ex);
                        }

                    }

                    stmtLine = swiftFile.NextStatementLine();
                }


                return statements;

            }
            catch (Exception e)
            {
                throw new Exception("Error caught in ParseSwiftFileCashActivity().\r\n" + e.Message + "\r\n", e);
            }
        }


        private static List<SwiftStatement> ParseMT535(SwiftFile swiftFile)
        {
            // File needs to be the MT535

            List<SwiftStatement> statements = new List<SwiftStatement>();
            MT535 currentStatement = new MT535 { FileName = swiftFile.FileName };
            MT535Activity statementItem = new MT535Activity();

            try
            {
                // Read the first line in the file
                string stmtLine = swiftFile.NextStatementLine();

                // Note on SWIFT tag....
                // Tag: 35B = Security ID (ISIN) and name
                // Tag: 93B = Balance                 

                bool gotAcc = false;
                int stmtNo = 1;
                // while (!stream.EndOfStream)
                while (stmtLine != "")
                {
                    string tag = (stmtLine.Substring(1, 2));
                    int startPos = -1;
                    int endPos = -1;


                    int bicBlockStartPos = stmtLine.IndexOf("{2:");
                    if (bicBlockStartPos != -1)// the header block that contains the bic
                    {
                        currentStatement = new MT535
                        {
                            FileName = swiftFile.FileName,
                            StatementType = StatementType.MT535.ToString(),
                            StatementNumber = stmtNo++,  // tag 13a is statement number but is optional and cant rely on it being sent
                            BIC = stmtLine.Substring(bicBlockStartPos + 17, 12)
                        };

                        statements.Add(currentStatement);

                    }
                    else if (stmtLine.Contains(":28E:"))  // page number, this should be unique across messages(pages)
                    {
                        var len = stmtLine.IndexOf("/") - 5;
                        currentStatement.StatementNumber = int.Parse(stmtLine.Substring(5, len));
                    }
                    else if (stmtLine.Contains(":98A::STAT")) // statement date line
                    {
                        #region Field 98a: Date/Time

                        /*
FORMAT
    Option A :4!c//8!n - (Qualifier)(Date) 
    Option C :4!c//8!n6!n - (Qualifier)(Date)(Time) 
    Option E :4!c//8!n6!n[,3n][/[N]2!n[2!n]] - (Qualifier)(Date)(Time)(Decimals)(UTC Indicator) 

DEFINITION
    This qualified generic field specifies:

        PREP -  (Preparation Date/Time)   Date/time at which the message was prepared.
        STAT -  (Statement Date/Time) Date/time on which the statement is based (reflecting the situation at that date/time).

NETWORK VALIDATED RULES
    Date must be a valid date expressed as YYYYMMDD (Error code(s): T50).

    Time must be a valid time expressed as HHMMSS (Error code(s): T38).

                         */

                        #endregion

                        currentStatement.Date = DateTime.ParseExact(stmtLine.Substring(stmtLine.Length - 8), "yyyyMMdd", new CultureInfo("en-GB")).Date;
                        if (DateTime.Now.Subtract(currentStatement.Date).TotalDays > 730135)
                        {
                            // Nearly 2,000 years ago. Must have been a two year date
                            currentStatement.Date = DateTime.ParseExact(stmtLine.Substring(stmtLine.Length - 6), "yyMMdd", new CultureInfo("en-GB")).Date;
                            Console.WriteLine(currentStatement.Date.ToString("yyyyMMdd"));
                        }
                    }
                    else if (tag == "97")  // Account number
                    {
                        #region Field 97a: Account: Safekeeping Account

                        /*                         
FORMAT
    Option A :4!c//35x (Qualifier)(Account Number) 
    Option B :4!c/[8c]/4!c/35x (Qualifier)(Data Source Scheme)(Account Type Code)(Account Number) 

DEFINITION
    This qualified generic field specifies:

    SAFE - (Safekeeping Account) Account where financial instruments are maintained.
 
    In option B, Account Type Code specifies the type of account needed to fully identify the account.

CODES
    In option B, the Data Source Scheme must be present and Account Type Code must contain the type of account as defined by the party identified in the Data Source Scheme.

                         */

                        #endregion

                        int accNoStart = stmtLine.IndexOf("//") + 2;
                        currentStatement.AccountNumber = stmtLine.Substring(accNoStart);
                        gotAcc = true;
                    }
                    else if (stmtLine.Contains(":20C::SEME")) // sender unique reference
                    {
                        currentStatement.SenderReference = stmtLine.Substring(12);
                    }
                    else if (tag == "35" && gotAcc)  // Statement Line found
                    {

                        #region Field 35B: Identification of the Financial Instrument
                        /*
FORMAT
Option B [ISIN1!e12!c] - (Identification of Security)
[4*35x] - (Description of Security) 

PRESENCE
Mandatory in optional subsequence B1

DEFINITION
This field identifies the financial instrument.

NETWORK VALIDATED RULES
At least Identification of a Security (Subfield 1) or Description of Security (Subfield 2) must be present; both may be present (Error code(s): T17).

ISIN is used at the beginning of Identification of Security (Subfield 1) and must be composed of uppercase letters only (Error code(s): T12).

USAGE RULES
When used in Description of Security (Subfield 2), codes must start and end with a slash '/'.

When an ISIN identifier is not used it is strongly recommended that one of the following codes be used as the first four characters of the Description of Security (Subfield 2):

[/2!a/] - The ISO two-digit country code, followed by the national scheme number.
 
[/TS/] - Followed by the ticker symbol.
 
[/XX/] - Bilaterally agreed or proprietary scheme which may be further identified by a code or short description identifying the scheme used.
 
It is strongly recommended that the ISIN be used.

*/
                        #endregion



                        startPos = stmtLine.IndexOf("ISIN", 1);
                        int posToAdd = 4;
                        if (startPos == -1)
                        {
                            startPos = stmtLine.IndexOf("ID", 1);
                            posToAdd = 2;
                        }
                        string isin = "";
                        string secName = "";
                        if (startPos != -1) // ISIN 
                        {
                            startPos = startPos + posToAdd;

                            if (stmtLine[startPos] == ' ')  // sometimes a space comes before the actual isin
                                startPos = startPos + 1;

                            //H.A. Had to change this because ISIN and Description were being read on the same line, even though the .out file appears on seperate lines.                            
                            //i.Identifiers.Add("ISIN", val.Substring(startPos, endPos - startPos).Trim());
                            isin = stmtLine.Substring(startPos, 12).Trim();

                            startPos = startPos + 12;
                            secName = stmtLine.Substring(startPos, stmtLine.Length - startPos).Trim();     // in case we want it

                        }
                        else // No ISIN found so simply use the other code plus description as name and ISIN
                        {
                            startPos = 5;  // ignore the                             
                            secName = stmtLine.Substring(startPos);
                            isin = secName;
                        }

                        statementItem = new MT535Activity
                        {
                            SecurityIdentifier = isin,
                            Description = secName
                        };

                    }
                    else if (tag == "93" && gotAcc)
                    {
                        #region Field 93B: Balance

                        /*
FORMAT
    Option B :4!c/[8c]/4!c/[N]15d - (Qualifier)(Data Source Scheme)(Quantity Type Code)(Sign)(Balance) 

DEFINITION
    This qualified generic field specifies:

    AGGR
     Aggregate Balance
     Total quantity of financial instruments for the referenced holding.
 
    AVAI
     Available Balance
     Total quantity of financial instruments of the aggregate balance that is available.
 
    NAVL
     Not Available Balance
     Total quantity of financial instruments of the aggregate balance that is NOT available.
 
CODES
    If Data Source Scheme is not present, Quantity Type Code must contain one of the following codes (Error code(s): K93):

    AMOR
     Amortised Value
     Quantity expressed as an amount representing the current amortised face amount of a bond, for example, a periodic reduction/increase of a bond's principal amount.
 
    FAMT
     Face Amount
     Quantity expressed as an amount representing the face amount, that is, the principal, of a debt instrument.
 
    UNIT
     Unit Number
     Quantity expressed as a number, for example, a number of shares.
 

NETWORK VALIDATED RULES
    The integer part of Balance must contain at least one digit. A decimal comma ',' is mandatory and is included in the maximum length (Error code(s): T40, T43).

    When Sign is present, Balance must not be zero (Error code(s): T14).

USAGE RULES
    Sign must be present only when Balance is negative.

    If the Available Balance and Not Available Balance are both provided, the total of the Available Balance and the Not Available Balance must equal the Aggregate Balance provided in the same sequence.                         

                         */

                        #endregion

                        if (stmtLine.Contains("AGGR")) // HA 25.11.08 - Added as sometimes other balances are sent too, such as AVAI & NAVL
                        {
                            startPos = stmtLine.LastIndexOf("/", stmtLine.Length - 1);
                            endPos = stmtLine.IndexOf(",");
                            if (startPos == -1)
                            {
                                throw new Exception("The SWIFT code 93 does not contain an balance identifier, please check. \r\n" +
                                                    "LINE:  " + stmtLine);
                            }
                            else
                            {
                                startPos++;
                                switch (stmtLine.Substring(startPos, 1).ToUpper())
                                {
                                    case "N":  // Negative amount
                                        statementItem.AggrBalance = Convert.ToDouble(stmtLine.Substring(startPos + 1, endPos - startPos).Trim()) * -1;
                                        break;
                                    default:
                                        statementItem.AggrBalance = Convert.ToDouble(stmtLine.Substring(startPos, endPos - startPos).Trim());
                                        break;
                                }
                            }
                            currentStatement.MT535Activities.Add(statementItem);  // Add to list of items  
                        }

                    }
                    stmtLine = swiftFile.NextStatementLine();
                }

                return statements;
            }
            catch (Exception e)
            {
                throw new Exception("Error caught in ParseSwiftSecurityBalance().\r\n" +
                                    e.Message + "\r\n", e);
            }

        }

        #endregion




    }
}

