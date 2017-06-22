using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using BBDataWarehouseCacheManager.Models;

namespace BBDataWarehouseCacheManager.Controllers
{
    public class TickerManager
    {
        public bool IsFuturesExpiryCode(string input)
        {
            if (input.Length == 2)
                return Regex.Match(input, @"[FGHJKMNQUVXZ][0-9]").Success;
            return false;
        }

        public bool IsIsin(string input)
        {
            if (input.Length == 12)
                return Regex.Match(input, @"^[a-zA-Z]{2}[a-zA-Z0-9]{9}\d$").Success;
            return false;
        }

        public bool IsTenor(string input)
        {
            return Regex.Match(input, @"^\d*[DWMY]$").Success;
        }

        public bool IsBloombergGlobalId(string input)
        {
            if (input.Length == 12)
                return input.StartsWith("BBG", StringComparison.OrdinalIgnoreCase);
            return false;
        }

        public bool EndWithsNorthAmericanExchangeCode(string ticker)
        {
            ticker = ticker.ToUpper().Trim();
            var nAmericanExchangeCodes = new List<string>
            {
                            " CF",
                            " CJ",
                            " CN",
                            " CT",
                            " CV",
                            " DG",
                            " DS",
                            " DT",
                            " DV",
                            " TA",
                            " TG",
                            " TJ",
                            " TK",
                            " TN",
                            " TR",
                            " TV",
                            " TW",
                            " TX",
                            " PQ",
                            " UA",
                            " UB",
                            " UC",
                            " UD",
                            " UF",
                            " UM",
                            " UN",
                            " UP",
                            " UQ",
                            " UR",
                            " US",
                            " UT",
                            " UU",
                            " UV",
                            " UW",
                            " UX",
                            " VJ",
                            " VK",
                            " VY"
            };
            //if it ends with " US" for example
            var suffix = ticker.Substring(ticker.Length - 3, 3);
            return nAmericanExchangeCodes.Contains(suffix);
        }

        public bool BloombergGlobalIsEquity(string bloombergId)
        {
            var sql = "select top 1 1 from TorViewBloombergNonNorthAmericanPrice where id_bb_global = '" + bloombergId + "' union all select top 1 1 from TorViewBloombergNorthAmericanPrice where id_bb_global = '" + bloombergId + "';";

            return Utils.DbController.ExecuteExists(sql);
        }

        public List<string> KnownBloombergSuffixes()
        {
            return new List<string> { "EQUITY", "CORP", "GOVT", "INDEX", "SEDOL1", "BBGLOBAL", "ISIN", "CURNCY", "CRNCY", "COMDTY" };
        }

        public string GetBergIdType(string moniker, string torontoAssetType)
        {
            if (moniker.EndsWith("EQUITY") && torontoAssetType.StartsWith("EQUITY")) return "BBTICKER";
            if (moniker.EndsWith("SEDOL1")) return "SEDOL1";
            if (IsBloombergGlobalId(moniker.Split(' ')[0])) return "BB_GLOBAL";
            if (IsIsin(moniker.Split(' ')[0])) return "ISIN";
            return string.Empty;
        }

        public string GetPrimaryLookUpString(string moniker, string torontoAssetType)
        {
            if (torontoAssetType.StartsWith("EQUITY") && moniker.EndsWith("SEDOL1"))
                return moniker.Split(' ')[0];
            if (torontoAssetType.StartsWith("EQUITY") && !moniker.EndsWith("SEDOL1"))
                return moniker.Replace(" EQUITY", "");
            if (torontoAssetType == "FI ISIN")
                return moniker.Split(' ')[0];
            return string.Empty;
        }

        public string GetSecondaryLookUpString(string moniker, string torontoAssetType)
        {
            if (torontoAssetType.StartsWith("EQUITY") && moniker.EndsWith("SEDOL1"))
                return moniker.Split(' ')[1];
            return string.Empty;
        }

        public string GetTorontoLongType(string bergTicker)
        {
            //clean ticker
            var thisTicker = bergTicker.ToUpper().Trim().Replace("  ", " ");
            var tickerParts = thisTicker.Split(' ');
            var bloombergSuffix = tickerParts[tickerParts.Length - 1];
            thisTicker = thisTicker.Replace(" " + bloombergSuffix, "");
            tickerParts = thisTicker.Split(' ');

            if (bloombergSuffix.Equals("EQUITY"))
            {
                if (tickerParts.Length == 4)
                {
                    return "OPTION EQUITY";
                }
                if (tickerParts.Length == 3)
                {
                    return "OPTION EQUITY";
                }
                if (tickerParts.Length == 2)
                {
                    if (EndWithsNorthAmericanExchangeCode(thisTicker))
                        return "EQUITY NAMERICA";
                    return "EQUITY NON NAMERICA";
                }
                if (tickerParts.Length == 1)
                {
                    if (IsFuturesExpiryCode(thisTicker.Substring(thisTicker.Length - 2, 2)))
                        return "FUTURE EQUITY";
                    if (bergTicker.Contains("=")) return GetTorontoLongType(bergTicker.Replace("=", " "));
                    return "EQUITY NAMERICA";
                }
            }
            if (bloombergSuffix.Equals("INDEX"))
            {
                if (tickerParts.Length >= 3)
                {
                    return "OPTION INDEX";
                }
                if (tickerParts.Length == 2)
                {
                    if (IsFuturesExpiryCode(tickerParts[1]))
                        return "FUTURE INDEX";
                    return "RATES EQUITY INDEX";
                }
                if (tickerParts.Length == 1)
                {
                    if (thisTicker.Length <= 4)
                        if (IsFuturesExpiryCode(thisTicker.Substring(thisTicker.Length - 2, 2)))
                            return "FUTURE INDEX";
                        else
                            return "RATES EQUITY INDEX";
                    if (thisTicker.Length == 7)
                        if (IsTenor(thisTicker.Substring(2, 5))) return "RATES LIBOR";
                }
                return "RATES - UNKNOWN";
            }
            if (bloombergSuffix.Equals("CORP") || bloombergSuffix.Equals("GOVT"))
            {
                if (IsIsin(tickerParts[0])) return "FI ISIN";

                return "FI NON ISIN";
            }
            if (bloombergSuffix.Equals("COMDTY"))
            {
                if (IsFuturesExpiryCode(thisTicker.Substring(thisTicker.Length - 2, 2)))
                    return "FUTURE COMMODITY";
            }
            if (bloombergSuffix.Equals("SEDOL1"))
            {
                if (EndWithsNorthAmericanExchangeCode(thisTicker))
                    return "EQUITY NAMERICA";
                return "EQUITY NON NAMERICA";
            }
            if (bloombergSuffix.Equals("CURNCY"))
            {
                if (thisTicker.Length == 3) return "RATES CURRENCY";
                if (IsFuturesExpiryCode(thisTicker.Substring(thisTicker.Length - 2, 2))) return "RATES FX FUTURE";
                return "RATES OTHER INT RATES";
            }
            if (IsBloombergGlobalId(thisTicker))
            {
                if (BloombergGlobalIsEquity(thisTicker))
                    return ("EQUITY");
                return "UNKNOWN BB_GLOBAL";
            }
            if (IsIsin(tickerParts[0])) return "UNKNOWN ISIN";

            //add a space before equity if not there
            if (bergTicker.EndsWith("EQUITY", StringComparison.OrdinalIgnoreCase) && !bergTicker.EndsWith(" EQUITY", StringComparison.OrdinalIgnoreCase))
                return GetTorontoLongType(bergTicker.ToUpper().Replace("EQUITY", "") + " EQUITY");

            return "UNKNOWN";
        }

        public string GetTorontoId(BergTicker bergTicker)
        {
            string ret = bergTicker.BergTickerClean;
            if (bergTicker.TorLongType.StartsWith("UNKNOWN BB_GLOBAL"))
                return String.Format("{0} | {1}", bergTicker.BergTickerClean, bergTicker.BergIdType);
            if (bergTicker.TorLongType.StartsWith("FI "))
                return string.IsNullOrEmpty(bergTicker.TorTickerLookup) ? string.Empty : String.Format("{0} | {1}", bergTicker.TorTickerLookup, bergTicker.BergIdType);
            if ((bergTicker.BergIdType.Equals("SEDOL1") && bergTicker.TorLongType.StartsWith("EQUITY")))
                return String.Format("{0} {1} | SEDOL1", bergTicker.TorTickerLookup, bergTicker.TorTickerLookup2.Replace("SEDOL1", ""));
            if (bergTicker.BergIdType.Equals("BB_GLOBAL"))
                return String.Format("{0} | BB_GLOBAL", bergTicker.TorTickerLookup);

            if (bergTicker.TorLongType.StartsWith("FUTURE"))
                ret = bergTicker.BergTickerClean;
            if (bergTicker.TorLongType.StartsWith("OPTION"))
                ret = bergTicker.BergTickerClean;
            if (bergTicker.TorLongType.StartsWith("RATES"))
            {
                ret = bergTicker.BergTickerClean;
                ret = Regex.Replace(ret, " CURNCY", " Crncy", RegexOptions.IgnoreCase);
            }
            if (bergTicker.TorLongType.StartsWith("RATES LIBOR"))
                ret = Regex.Replace(ret, " INDEX", " Comdty", RegexOptions.IgnoreCase);

            //title case all the bloomberg suffixes
            foreach (var suffix in KnownBloombergSuffixes())
                ret = Regex.Replace(ret, " " + suffix, " " + new CultureInfo("en-US", false).TextInfo.ToTitleCase(suffix.ToLower()), RegexOptions.IgnoreCase);

            //everything failed. dont return just the pipe
            if (string.IsNullOrEmpty(ret.Replace("|", "").Trim())) return string.Empty;

            return ret;
        }

        public string GetTorontoPullType(string torontoLongType)
        {
            if (torontoLongType.StartsWith("RATES CURRENCY"))
                return "X";
            if (torontoLongType.StartsWith("RATES") || (torontoLongType.StartsWith("FUTURE")))
                return "T";
            if ((torontoLongType.StartsWith("FI ")) || torontoLongType.Equals("UNKNOWN BB_GLOBAL"))
                return "F";
            if (torontoLongType.StartsWith("OPTION"))
                return "O";
            return "E";
        }
    }
}