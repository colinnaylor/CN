using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using SwiftImporterUI.Model;

namespace SwiftImporterConsole.Model.Messages {
    public class MT535 :SwiftMessage {
        public MT535(string Content, string applicationID, string serviceID, string logicalTerminalAddress,
            int sessionNumber, int sequenceNumber)
            : base(Content,applicationID,serviceID,logicalTerminalAddress,sessionNumber,sequenceNumber) {
                Type = MessageType.MT535;
        }
        public int StatementNo { get; set; }
        public string AccountNumber { get; set; }
        public List<MT535Activity> MT535Activities = new List<MT535Activity>();
        public DateTime Date { get; set; }
        public string SenderReference { get; set; }

        public override void Parse() {
            base.ParseApplicationHeaderBlock();

            MT535Activity statementItem = new MT535Activity();

            try {
                string[] lines = SwiftMessage.Lines(source);
                    
                bool gotAcc = false;
                int stmtNo = 1;
                // Note on SWIFT tag....
                // Tag: 35B = Security ID (ISIN) and name
                // Tag: 93B = Balance                 
                foreach(string stmtLine in lines){
                    string tag = (stmtLine.Substring(1, 2));
                    int startPos = -1;
                    int endPos = -1;


                    int bicBlockStartPos = stmtLine.IndexOf("{2:");
                    if (bicBlockStartPos != -1)// the header block that contains the bic
                    {
                        StatementNo = stmtNo++;  // tag 13a is statement number but is optional and cant rely on it being sent
                        SenderAddress = stmtLine.Substring(bicBlockStartPos + 17, 12);

                    } else if (stmtLine.Contains(":28E:"))  // page number, this should be unique across messages(pages)
                    {
                        var len = stmtLine.IndexOf("/") - 5;
                        StatementNo = int.Parse(stmtLine.Substring(5, len));
                    } else if (stmtLine.Contains(":98A::STAT")) // statement date line
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

                        Date = DateTime.ParseExact(stmtLine.Substring(stmtLine.Length - 8), "yyyyMMdd", new CultureInfo("en-GB")).Date;
                        if (DateTime.Now.Subtract(Date).TotalDays > 730135) {
                            // Nearly 2,000 years ago. Must have been a two year date
                            Date = DateTime.ParseExact(stmtLine.Substring(stmtLine.Length - 6), "yyMMdd", new CultureInfo("en-GB")).Date;
                        }
                    } else if (tag == "97")  // Account number
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
                        AccountNumber = stmtLine.Substring(accNoStart);
                        gotAcc = true;
                    } else if (stmtLine.Contains(":20C::SEME")) // sender unique reference
                    {
                        SenderReference = stmtLine.Substring(12);
                    } else if (tag == "35" && gotAcc)  // Statement Line found
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
                        if (startPos == -1) {
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

                        } else // No ISIN found so simply use the other code plus description as name and ISIN
                        {
                            startPos = 5;  // ignore the                             
                            secName = stmtLine.Substring(startPos);
                            isin = secName;
                        }

                        statementItem = new MT535Activity {
                            SecurityIdentifier = isin,
                            Description = secName
                        };

                    } else if (tag == "93" && gotAcc) {
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
                            // This only needs adding once for each security found. AGGR may however appear twice
                            if (statementItem != null) {
                                startPos = stmtLine.LastIndexOf("/", stmtLine.Length - 1);
                                endPos = stmtLine.IndexOf(",");
                                if (startPos == -1) {
                                    throw new Exception("The SWIFT code 93 does not contain an balance identifier, please check. \r\n" +
                                                        "LINE:  " + stmtLine);
                                } else {
                                    startPos++;
                                    switch (stmtLine.Substring(startPos, 1).ToUpper()) {
                                        case "N":  // Negative amount
                                            statementItem.AggrBalance = Convert.ToDouble(stmtLine.Substring(startPos + 1, endPos - startPos).Trim()) * -1;
                                            break;
                                        default:
                                            statementItem.AggrBalance = Convert.ToDouble(stmtLine.Substring(startPos, endPos - startPos).Trim());
                                            break;
                                    }
                                }
                                MT535Activities.Add(statementItem);  // Add to list of items  
                                statementItem = null;
                            }
                        }

                    }
                }

            } catch (Exception e) {
                throw new Exception("Error caught in MT535 Parse method.\r\n" + e.Message + "\r\n", e);
            }

        }

        public override string ToString() {
            return this.Type.ToString();
        }
    }

    public class MT535Activity {
        public string SecurityIdentifier { get; set; }
        public string Description { get; set; }
        public double AggrBalance { get; set; }
        public int StatementID { get; set; }

    }
}
