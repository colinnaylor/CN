DECLARE @PositionDate DATE;

SET @PositionDate='{Position_Date}'

--SET @PositionDate =Dateadd(DAY, CASE Datename(WEEKDAY, Getdate())
--                              WHEN 'Sunday' THEN -2
--                              WHEN 'Monday' THEN -3
--                              ELSE -1
--                            END, Datediff(DAY, 0, Getdate()))
-- JOIN ON NNAP FIRST - USING 3 TYPES OF TICKERS
UPDATE DW
SET    dw.DOWNLOADED = 1,
       dw.FOUND_NNAP = 1,
       dw.CNTRY_OF_DOMICILE = tor.CNTRY_OF_DOMICILE,
       dw.CRNCY = tor.CRNCY,
       dw.EQY_PRIM_EXCH = tor.EQY_PRIM_EXCH,
       dw.EQY_PRIM_EXCH_SHRT = tor.EQY_PRIM_EXCH_SHRT,
       dw.EQY_SH_OUT = tor.EQY_SH_OUT,
       dw.EQY_SH_OUT_REAL = tor.EQY_SH_OUT_REAL,
       dw.EXCH_CODE = tor.EXCH_CODE,
       dw.ID_BB_GLOBAL = tor.ID_BB_GLOBAL,
       dw.ID_BB_UNIQUE = tor.ID_BB_UNIQUE,
       dw.ID_BB_company = tor.ID_BB_company,
       dw.ID_CUSIP = tor.ID_CUSIP,
       dw.ID_ISIN = tor.ID_ISIN,
       dw.ID_SEDOL1 = tor.ID_SEDOL1,
       dw.ID_SEDOL2 = tor.ID_SEDOL2,
       dw.LAST_UPDATE = tor.LAST_UPDATE,
       dw.NAME = tor.NAME,
       dw.PX_ASK = tor.PX_ASK,
       dw.PX_BID = tor.PX_BID,
       dw.PX_LAST = tor.PX_LAST,
       dw.PX_QUOTE_LOT_SIZE = tor.PX_QUOTE_LOT_SIZE,
       dw.SECURITY_TYP = tor.SECURITY_TYP,
       dw.TICKER_AND_EXCH_CODE = tor.TICKER_AND_EXCH_CODE
FROM   [BloombergDataWarehouse] dw
       INNER JOIN TorViewBloombergNonNorthAmericanPrice tor
               ON BERG_ID_TYPE = 'BBTICKER'
                  AND dw.TOR_MONIKER_LOOKUP = TOR.ticker_and_exch_code COLLATE Latin1_General_CI_AS
WHERE  TOR_LONGTYPE LIKE 'EQUITY%'
       AND dw.Effective_Date = @PositionDate;

UPDATE DW
SET    dw.DOWNLOADED = 1,
       dw.FOUND_NNAP = 1,
       dw.CNTRY_OF_DOMICILE = tor.CNTRY_OF_DOMICILE,
       dw.CRNCY = tor.CRNCY,
       dw.EQY_PRIM_EXCH = tor.EQY_PRIM_EXCH,
       dw.EQY_PRIM_EXCH_SHRT = tor.EQY_PRIM_EXCH_SHRT,
       dw.EQY_SH_OUT = tor.EQY_SH_OUT,
       dw.EQY_SH_OUT_REAL = tor.EQY_SH_OUT_REAL,
       dw.EXCH_CODE = tor.EXCH_CODE,
       dw.ID_BB_GLOBAL = tor.ID_BB_GLOBAL,
       dw.ID_BB_UNIQUE = tor.ID_BB_UNIQUE,
       dw.ID_BB_company = tor.ID_BB_company,
       dw.ID_CUSIP = tor.ID_CUSIP,
       dw.ID_ISIN = tor.ID_ISIN,
       dw.ID_SEDOL1 = tor.ID_SEDOL1,
       dw.ID_SEDOL2 = tor.ID_SEDOL2,
       dw.LAST_UPDATE = tor.LAST_UPDATE,
       dw.NAME = tor.NAME,
       dw.PX_ASK = tor.PX_ASK,
       dw.PX_BID = tor.PX_BID,
       dw.PX_LAST = tor.PX_LAST,
       dw.PX_QUOTE_LOT_SIZE = tor.PX_QUOTE_LOT_SIZE,
       dw.SECURITY_TYP = tor.SECURITY_TYP,
       dw.TICKER_AND_EXCH_CODE = tor.TICKER_AND_EXCH_CODE
FROM   [BloombergDataWarehouse] dw
       INNER JOIN TorViewBloombergNonNorthAmericanPrice tor
               ON BERG_ID_TYPE = 'BB_GLOBAL'
                  AND TOR_MONIKER_LOOKUP = TOR.ID_BB_GLOBAL COLLATE Latin1_General_CI_AS
WHERE  TOR_LONGTYPE LIKE 'EQUITY%'
       AND dw.Effective_Date = @PositionDate;

UPDATE DW
SET    dw.DOWNLOADED = 1,
       dw.FOUND_NNAP = 1,
       dw.CNTRY_OF_DOMICILE = tor.CNTRY_OF_DOMICILE,
       dw.CRNCY = tor.CRNCY,
       dw.EQY_PRIM_EXCH = tor.EQY_PRIM_EXCH,
       dw.EQY_PRIM_EXCH_SHRT = tor.EQY_PRIM_EXCH_SHRT,
       dw.EQY_SH_OUT = tor.EQY_SH_OUT,
       dw.EQY_SH_OUT_REAL = tor.EQY_SH_OUT_REAL,
       dw.EXCH_CODE = tor.EXCH_CODE,
       dw.ID_BB_GLOBAL = tor.ID_BB_GLOBAL,
       dw.ID_BB_UNIQUE = tor.ID_BB_UNIQUE,
       dw.ID_BB_company = tor.ID_BB_company,
       dw.ID_CUSIP = tor.ID_CUSIP,
       dw.ID_ISIN = tor.ID_ISIN,
       dw.ID_SEDOL1 = tor.ID_SEDOL1,
       dw.ID_SEDOL2 = tor.ID_SEDOL2,
       dw.LAST_UPDATE = tor.LAST_UPDATE,
       dw.NAME = tor.NAME,
       dw.PX_ASK = tor.PX_ASK,
       dw.PX_BID = tor.PX_BID,
       dw.PX_LAST = tor.PX_LAST,
       dw.PX_QUOTE_LOT_SIZE = tor.PX_QUOTE_LOT_SIZE,
       dw.SECURITY_TYP = tor.SECURITY_TYP,
       dw.TICKER_AND_EXCH_CODE = tor.TICKER_AND_EXCH_CODE
FROM   [BloombergDataWarehouse] dw
       INNER JOIN TorViewBloombergNonNorthAmericanPrice tor
               ON BERG_ID_TYPE = 'SEDOL1'
                  AND TOR_MONIKER_LOOKUP = TOR.id_sedol1 COLLATE Latin1_General_CI_AS
                  AND ( dw.TOR_MONIKER_LOOKUP2 = tor.EXCH_CODE COLLATE Latin1_General_CI_AS
                         OR dw.TOR_MONIKER_LOOKUP2 = 'SEDOL1'
                         OR dw.TOR_MONIKER_LOOKUP2 = tor.PRICING_SOURCE COLLATE Latin1_General_CI_AS
                         OR Replace(dw.TOR_MONIKER_LOOKUP2, 'ID', 'IX') = tor.PRICING_SOURCE COLLATE Latin1_General_CI_AS )
WHERE  TOR_LONGTYPE LIKE 'EQUITY%'
       AND dw.Effective_Date = @PositionDate;

-- NOW JOIN ON NAP ON 3 TICKERS
UPDATE DW
SET    dw.DOWNLOADED = 1,
       dw.FOUND_NAP = 1,
       dw.CNTRY_OF_DOMICILE = tor.CNTRY_OF_DOMICILE,
       dw.CRNCY = tor.CRNCY,
       dw.EQY_PRIM_EXCH = tor.EQY_PRIM_EXCH,
       dw.EQY_PRIM_EXCH_SHRT = tor.EQY_PRIM_EXCH_SHRT,
       dw.EQY_SH_OUT = tor.EQY_SH_OUT,
       dw.EQY_SH_OUT_REAL = tor.EQY_SH_OUT_REAL,
       dw.EXCH_CODE = tor.EXCH_CODE,
       dw.ID_BB_GLOBAL = tor.ID_BB_GLOBAL,
       dw.ID_BB_UNIQUE = tor.ID_BB_UNIQUE,
       dw.ID_BB_company = tor.ID_BB_company,
       dw.ID_CUSIP = tor.ID_CUSIP,
       dw.ID_ISIN = tor.ID_ISIN,
       dw.ID_SEDOL1 = tor.ID_SEDOL1,
       dw.ID_SEDOL2 = tor.ID_SEDOL2,
       dw.LAST_UPDATE = tor.LAST_UPDATE,
       dw.NAME = tor.NAME,
       dw.PX_ASK = tor.PX_ASK,
       dw.PX_BID = tor.PX_BID,
       dw.PX_LAST = tor.PX_LAST,
       dw.PX_QUOTE_LOT_SIZE = tor.PX_QUOTE_LOT_SIZE,
       dw.SECURITY_TYP = tor.SECURITY_TYP,
       dw.TICKER_AND_EXCH_CODE = tor.TICKER_AND_EXCH_CODE
FROM   [BloombergDataWarehouse] dw
       INNER JOIN TorViewBloombergNorthAmericanPrice tor
               ON BERG_ID_TYPE = 'BBTICKER'
                  AND dw.TOR_MONIKER_LOOKUP = TOR.ticker_and_exch_code COLLATE Latin1_General_CI_AS
WHERE  TOR_LONGTYPE LIKE 'EQUITY%'
       AND dw.Effective_Date = @PositionDate;

UPDATE DW
SET    dw.DOWNLOADED = 1,
       dw.FOUND_NAP = 1,
       dw.CNTRY_OF_DOMICILE = tor.CNTRY_OF_DOMICILE,
       dw.CRNCY = tor.CRNCY,
       dw.EQY_PRIM_EXCH = tor.EQY_PRIM_EXCH,
       dw.EQY_PRIM_EXCH_SHRT = tor.EQY_PRIM_EXCH_SHRT,
       dw.EQY_SH_OUT = tor.EQY_SH_OUT,
       dw.EQY_SH_OUT_REAL = tor.EQY_SH_OUT_REAL,
       dw.EXCH_CODE = tor.EXCH_CODE,
       dw.ID_BB_GLOBAL = tor.ID_BB_GLOBAL,
       dw.ID_BB_UNIQUE = tor.ID_BB_UNIQUE,
       dw.ID_BB_company = tor.ID_BB_company,
       dw.ID_CUSIP = tor.ID_CUSIP,
       dw.ID_ISIN = tor.ID_ISIN,
       dw.ID_SEDOL1 = tor.ID_SEDOL1,
       dw.ID_SEDOL2 = tor.ID_SEDOL2,
       dw.LAST_UPDATE = tor.LAST_UPDATE,
       dw.NAME = tor.NAME,
       dw.PX_ASK = tor.PX_ASK,
       dw.PX_BID = tor.PX_BID,
       dw.PX_LAST = tor.PX_LAST,
       dw.PX_QUOTE_LOT_SIZE = tor.PX_QUOTE_LOT_SIZE,
       dw.SECURITY_TYP = tor.SECURITY_TYP,
       dw.TICKER_AND_EXCH_CODE = tor.TICKER_AND_EXCH_CODE
FROM   [BloombergDataWarehouse] dw
       INNER JOIN TorViewBloombergNorthAmericanPrice tor
               ON BERG_ID_TYPE = 'BB_GLOBAL'
                  AND TOR_MONIKER_LOOKUP = TOR.ID_BB_GLOBAL COLLATE Latin1_General_CI_AS
WHERE  TOR_LONGTYPE LIKE 'EQUITY%'
       AND dw.Effective_Date = @PositionDate;

UPDATE DW
SET    dw.DOWNLOADED = 1,
       dw.FOUND_NAP = 1,
       dw.CNTRY_OF_DOMICILE = tor.CNTRY_OF_DOMICILE,
       dw.CRNCY = tor.CRNCY,
       dw.EQY_PRIM_EXCH = tor.EQY_PRIM_EXCH,
       dw.EQY_PRIM_EXCH_SHRT = tor.EQY_PRIM_EXCH_SHRT,
       dw.EQY_SH_OUT = tor.EQY_SH_OUT,
       dw.EQY_SH_OUT_REAL = tor.EQY_SH_OUT_REAL,
       dw.EXCH_CODE = tor.EXCH_CODE,
       dw.ID_BB_GLOBAL = tor.ID_BB_GLOBAL,
       dw.ID_BB_UNIQUE = tor.ID_BB_UNIQUE,
       dw.ID_BB_company = tor.ID_BB_company,
       dw.ID_CUSIP = tor.ID_CUSIP,
       dw.ID_ISIN = tor.ID_ISIN,
       dw.ID_SEDOL1 = tor.ID_SEDOL1,
       dw.ID_SEDOL2 = tor.ID_SEDOL2,
       dw.LAST_UPDATE = tor.LAST_UPDATE,
       dw.NAME = tor.NAME,
       dw.PX_ASK = tor.PX_ASK,
       dw.PX_BID = tor.PX_BID,
       dw.PX_LAST = tor.PX_LAST,
       dw.PX_QUOTE_LOT_SIZE = tor.PX_QUOTE_LOT_SIZE,
       dw.SECURITY_TYP = tor.SECURITY_TYP,
       dw.TICKER_AND_EXCH_CODE = tor.TICKER_AND_EXCH_CODE
FROM   [BloombergDataWarehouse] dw
       INNER JOIN TorViewBloombergNorthAmericanPrice tor
               ON BERG_ID_TYPE = 'SEDOL1'
                  AND TOR_MONIKER_LOOKUP = TOR.id_sedol1 COLLATE Latin1_General_CI_AS
                  AND ( dw.TOR_MONIKER_LOOKUP2 = tor.EXCH_CODE COLLATE Latin1_General_CI_AS
                         OR dw.TOR_MONIKER_LOOKUP2 = 'SEDOL1'
                         OR dw.TOR_MONIKER_LOOKUP2 = tor.PRICING_SOURCE COLLATE Latin1_General_CI_AS )
WHERE  TOR_LONGTYPE LIKE 'EQUITY%'
       AND dw.Effective_Date = @PositionDate;

-- NOW ENRICH ALL EQUITIES AGAINST ADDITIONAL TABLES.
UPDATE DW
SET    dw.DOWNLOADED = 1,
       dw.found_description = 1,
       dw.ADR_SH_PER_ADR = d.ADR_SH_PER_ADR,
       dw.COUNTRY = d.COUNTRY,
       dw.DVD_CRNCY = d.DVD_CRNCY,
       dw.INDUSTRY_SECTOR = d.INDUSTRY_SECTOR,
       dw.PAR_AMT = d.PAR_AMT,
       dw.TICKER = d.TICKER
FROM   [BloombergDataWarehouse] dw
       INNER JOIN TorViewBloombergDescription d
               ON dw.id_bb_unique = d.id_bb_unique COLLATE Latin1_General_CI_AS
WHERE  TOR_LONGTYPE LIKE 'EQUITY%'
       AND dw.Effective_Date = @PositionDate;

UPDATE DW
SET    dw.DOWNLOADED = 1,
       dw.FOUND_CREDITRISK = 1,
       dw.ID_BB_ULTIMATE_PARENT_CO = cr.ID_BB_ULTIMATE_PARENT_CO,
       dw.ULT_PARENT_CNTRY_DOMICILE = cr.ULT_PARENT_CNTRY_DOMICILE
FROM   [BloombergDataWarehouse] dw
       INNER JOIN TorViewBloombergCreditRisk CR
               ON dw.id_bb_COMPANY = CR.ID_BB_COMPANY COLLATE Latin1_General_CI_AS
WHERE  TOR_LONGTYPE LIKE 'EQUITY%'
       AND dw.Effective_Date = @PositionDate;

UPDATE DW
SET    dw.DOWNLOADED = 1,
       dw.FOUND_EQPERSECPULL = 1,
       dw.BDVD_NEXT_EST_EX_DT = eq.BDVD_NEXT_EST_EX_DT,
       dw.BDVD_PROJ_DIV_AMT = eq.BDVD_PROJ_DIV_AMT,
       dw.COUNTRY_FULL_NAME = eq.COUNTRY_FULL_NAME,
       dw.ID_BB_ULTIMATE_PARENT_CO_NAME = eq.ID_BB_ULTIMATE_PARENT_CO_NAME,
       dw.ID_BB_GLOBAL = eq.ID_BB_GLOBAL,
       dw.ID_BB_UNIQUE = eq.ID_BB_UNIQUE,
       dw.PX_CLOSE_2D = eq.PX_CLOSE_2D,
       dw.PX_YEST_CLOSE = eq.PX_YEST_CLOSE,
       dw.PX_LAST_EOD = eq.PX_LAST_EOD,
       dw.LAST_UPDATE = eq.LAST_UPDATE,
       dw.QUOTE_TYP = eq.QUOTE_TYP,
       dw.SSR_LIQUIDITY_INDICATOR = eq.SSR_LIQUIDITY_INDICATOR,
       dw.VOLATILITY_10D = eq.VOLATILITY_10D,
       dw.VOLATILITY_120D = eq.VOLATILITY_120D,
       dw.VOLATILITY_150D = eq.VOLATILITY_150D,
       dw.VOLATILITY_180D = eq.VOLATILITY_180D,
       dw.VOLATILITY_200D = eq.VOLATILITY_200D,
       dw.VOLATILITY_20D = eq.VOLATILITY_20D,
       dw.VOLATILITY_260D = eq.VOLATILITY_260D,
       dw.VOLATILITY_30D = eq.VOLATILITY_30D,
       dw.VOLATILITY_360D = eq.VOLATILITY_360D,
       dw.VOLATILITY_60D = eq.VOLATILITY_60D,
       dw.VOLATILITY_90D = eq.VOLATILITY_90D,
       dw.VOLATILITY_10D_CALC = eq.VOLATILITY_10D_CALC,
       dw.VOLATILITY_180D_CALC = eq.VOLATILITY_180D_CALC,
       dw.VOLATILITY_200D_CALC = eq.VOLATILITY_200D_CALC,
       dw.VOLATILITY_260D_CALC = eq.VOLATILITY_260D_CALC,
       dw.VOLATILITY_30D_CALC = eq.VOLATILITY_30D_CALC,
       dw.VOLATILITY_360D_CALC = eq.VOLATILITY_360D_CALC,
       dw.VOLATILITY_60D_CALC = eq.VOLATILITY_60D_CALC,
       dw.VOLATILITY_90D_CALC = eq.VOLATILITY_90D_CALC,
       dw.VOLUME_AVG_10D = eq.VOLUME_AVG_10D,
       dw.VOLUME_AVG_20D = eq.VOLUME_AVG_20D,
       dw.VOLUME_AVG_20D_CALL = eq.VOLUME_AVG_20D_CALL,
       dw.VOLUME_AVG_20D_PUT = eq.VOLUME_AVG_20D_PUT,
       dw.VOLUME_AVG_5D = eq.VOLUME_AVG_5D,
       dw.VOLUME_AVG_5D_CALL = eq.VOLUME_AVG_5D_CALL,
       dw.VOLUME_AVG_5D_PUT = eq.VOLUME_AVG_5D_PUT,
       dw.BloombergErrorCode = eq.BloombergErrorCode,
       dw.DL_ASSET_CLASS = eq.DL_ASSET_CLASS
FROM   [BloombergDataWarehouse] dw
       INNER JOIN TorViewBloombergPerSecurityPull Eq
               ON eq.securities = Replace(dw.tor_id, ' | SEDOL1', '') COLLATE Latin1_General_CI_AS
WHERE  TOR_LONGTYPE LIKE 'EQUITY%'
       AND dw.Effective_Date = @PositionDate;

-- FI -- high hit rate
UPDATE DW
SET    dw.downloaded = 1,
       DW.FOUND_FIXEDINCOME = 1,
       dw.ADR_SH_PER_ADR = tor.ADR_SH_PER_ADR,
       dw.AMT_ISSUED = tor.AMT_ISSUED,
       dw.CALLABLE = tor.CALLABLE,
       dw.CNTRY_OF_DOMICILE = tor.CNTRY_OF_DOMICILE,
       dw.COUNTRY_FULL_NAME = tor.COUNTRY_FULL_NAME,
       dw.CPN = tor.CPN,
       dw.CPN_FREQ = tor.CPN_FREQ,
       dw.CPN_TYP = tor.CPN_TYP,
       dw.CRNCY = tor.CRNCY,
       dw.DAY_CNT_DES = tor.DAY_CNT_DES,
       dw.DUR_ADJ_BID = tor.DUR_ADJ_BID,
       dw.DVD_CRNCY = tor.DVD_CRNCY,
       dw.EQY_PRIM_EXCH = tor.EQY_PRIM_EXCH,
       dw.EQY_PRIM_EXCH_SHRT = tor.EQY_PRIM_EXCH_SHRT,
       dw.EQY_SH_OUT = tor.EQY_SH_OUT,
       dw.EQY_SH_OUT_REAL = tor.EQY_SH_OUT_REAL,
       dw.EXCH_CODE = tor.EXCH_CODE,
       dw.FLOATER = tor.FLOATER,
       dw.ID_BB_GLOBAL = tor.ID_BB_GLOBAL,
       dw.ID_BB_UNIQUE = tor.ID_BB_UNIQUE,
       dw.ID_CUSIP = tor.ID_CUSIP,
       dw.ID_ISIN = tor.ID_ISIN,
       dw.ID_SEDOL1 = tor.ID_SEDOL1,
       dw.ID_SEDOL2 = tor.ID_SEDOL2,
       dw.INDUSTRY_SECTOR = tor.INDUSTRY_SECTOR,
       dw.ISSUER = tor.ISSUER,
       dw.IS_SECURED = tor.IS_SECURED,
       dw.LAST_UPDATE = tor.LAST_UPDATE,
       dw.LN_SECURED_UNSECURED = tor.LN_SECURED_UNSECURED,
       dw.MATURITY = tor.MATURITY,
       dw.NAME = tor.NAME,
       dw.NXT_CPN_DT = tor.NXT_CPN_DT,
       dw.PAR_AMT = tor.PAR_AMT,
       dw.PUTABLE = tor.PUTABLE,
       dw.PX_ASK = tor.PX_ASK,
       dw.PX_BID = tor.PX_BID,
       dw.PX_DISC_MID = tor.PX_DISC_MID,
       dw.PX_LAST = tor.PX_LAST,
       dw.PX_QUOTE_LOT_SIZE = tor.PX_QUOTE_LOT_SIZE,
       dw.PX_YEST_CLOSE = tor.PX_YEST_CLOSE,
       dw.QUOTE_TYP = tor.QUOTE_TYP,
       dw.REDEMP_VAL = tor.REDEMP_VAL,
       dw.RTG_SP_LT_LC_ISSUER_CREDIT = tor.RTG_SP_LT_LC_ISSUER_CREDIT,
       dw.SECURITY_TYP = tor.SECURITY_TYP,
       dw.SSR_LIQUIDITY_INDICATOR = tor.SSR_LIQUIDITY_INDICATOR,
       dw.START_ACC_DT = tor.START_ACC_DT,
       dw.TICKER = tor.TICKER,
       dw.VOLUME_AVG_10D = tor.VOLUME_AVG_10D,
       dw.VOLUME_AVG_20D = tor.VOLUME_AVG_20D,
       dw.VOLUME_AVG_5D = tor.VOLUME_AVG_5D,
       dw.DL_ASSET_CLASS = tor.DL_ASSET_CLASS,
       dw.DL_SECURITY_TYPE = tor.DL_SECURITY_TYPE
FROM   [BloombergDataWarehouse] dw
       INNER JOIN [TorViewBloombergfixedincome] tor
               ON( ( TOR_LONGTYPE = 'FI ISIN'
                     AND TOR_MONIKER_LOOKUP = TOR.id_isin COLLATE Latin1_General_CI_AS
                     AND BERG_ID_TYPE = 'ISIN' )
                    OR ( TOR_LONGTYPE = 'FI ISIN'
                         AND TOR_MONIKER_LOOKUP = TOR.ID_BB_GLOBAL COLLATE Latin1_General_CI_AS
                         AND BERG_ID_TYPE = 'BB_GLOBAL' )
                    OR ( TOR_LONGTYPE = 'UNKNOWN BB_GLOBAL'
                         AND berg_MONIKER = TOR.ID_BB_GLOBAL COLLATE Latin1_General_CI_AS
                         AND BERG_ID_TYPE = 'BB_GLOBAL' ) )
WHERE  dw.Effective_Date = @PositionDate;

UPDATE DW
SET    DW.DOWNLOADED = 1,
       dw.FOUND_EQPERSECPULL = 1,
       DW.delta = OPT.delta,
       DW.delta_mid = OPT.delta_mid,
       DW.PX_BID = OPT.PX_BID,
       DW.PX_ASK = OPT.PX_ASK,
       DW.PX_LAST = OPT.PX_LAST,
       DW.PX_LAST_EOD = OPT.PX_LAST_EOD,
       DW.LAST_UPDATE = OPT.LAST_UPDATE,
       DW.NAME = OPT.NAME,
       DW.ID_BB_GLOBAL = OPT.ID_BB_GLOBAL,
       DW.EQY_SH_OUT_REAL = OPT.EQY_SH_OUT_REAL,
       DW.PX_YEST_CLOSE = OPT.PX_YEST_CLOSE,
       DW.QUOTE_TYP = OPT.QUOTE_TYP,
       DW.SECURITY_TYP = OPT.SECURITY_TYP,
       dw.BloombergErrorCode = opt.BloombergErrorCode,
       dw.DL_ASSET_CLASS = opt.DL_ASSET_CLASS
FROM   BLOOMBERGDATAWAREHOUSE DW
       INNER JOIN TorViewBloombergPerSecurityPull opt
               ON opt.securities = DW.BERG_MONIKER COLLATE SQL_LATIN1_GENERAL_CP1_CI_AS
                  AND TOR_LONGTYPE LIKE 'OPTION%'
                  AND DW.EFFECTIVE_DATE = @POSITIONDATE;

--FUTURE COMMODITY
UPDATE dw
SET    dw.downloaded = 1,
       dw.FOUND_EQPERSECPULL = 1,
       dw.AMT_ISSUED = tor.AMT_ISSUED,
       dw.COUNTRY = tor.COUNTRY,
       dw.COUNTRY_FULL_NAME = tor.COUNTRY_FULL_NAME,
       dw.EQY_SH_OUT_REAL = tor.EQY_SH_OUT_REAL,
       dw.ID_BB_GLOBAL = tor.ID_BB_GLOBAL,
       dw.ID_ISIN = tor.ID_ISIN,
       dw.INDUSTRY_SECTOR = tor.INDUSTRY_SECTOR,
       dw.INDX_MWEIGHT = tor.INDX_MWEIGHT,
       dw.LAST_TRADEABLE_DT = tor.LAST_TRADEABLE_DT,
       dw.LAST_UPDATE = tor.LAST_UPDATE,
       dw.NAME = tor.NAME,
       dw.PX_ASK = tor.PX_ASK,
       dw.PX_BID = tor.PX_BID,
       dw.PX_LAST = COALESCE(tor.px_last_eod, tor.PX_LAST),
       dw.PX_YEST_CLOSE = tor.PX_YEST_CLOSE,
       dw.PX_last_eod = tor.px_last_eod,
       dw.QUOTE_TYP = tor.QUOTE_TYP,
       dw.SECURITY_TYP = tor.SECURITY_TYP,
       dw.SSR_LIQUIDITY_INDICATOR = tor.SSR_LIQUIDITY_INDICATOR,
       dw.ULT_PARENT_CNTRY_DOMICILE = tor.ULT_PARENT_CNTRY_DOMICILE,
       dw.VOLUME_AVG_10D = tor.VOLUME_AVG_10D,
       dw.VOLUME_AVG_20D = tor.VOLUME_AVG_20D,
       dw.VOLUME_AVG_20D_CALL = tor.VOLUME_AVG_20D_CALL,
       dw.VOLUME_AVG_20D_PUT = tor.VOLUME_AVG_20D_PUT,
       dw.VOLUME_AVG_5D = tor.VOLUME_AVG_5D,
       dw.VOLUME_AVG_5D_CALL = tor.VOLUME_AVG_5D_CALL,
       dw.VOLUME_AVG_5D_PUT = tor.VOLUME_AVG_5D_PUT,
       dw.BloombergErrorCode = tor.BloombergErrorCode,
       dw.DL_ASSET_CLASS = tor.DL_ASSET_CLASS
FROM   bloombergdatawarehouse dw
       INNER JOIN TorViewBloombergPerSecurityPull tor
               ON tor.securities = dw.tor_id COLLATE SQL_Latin1_General_CP1_CI_AS
                  AND ( TOR_LONGTYPE IN ( 'FUTURE COMMODITY', 'FUTURE INDEX', 'FUTURE EQUITY' )
                         OR TOR_LONGTYPE LIKE 'RATES%' )
                  AND dw.Effective_Date = @PositionDate;

------unknown sedols - they must be FI/converts
UPDATE dw
SET    dw.downloaded = 1,
       DW.FOUND_FIXEDINCOME = 1,
       dw.ADR_SH_PER_ADR = tor.ADR_SH_PER_ADR,
       dw.AMT_ISSUED = tor.AMT_ISSUED,
       dw.CALLABLE = tor.CALLABLE,
       dw.CNTRY_OF_DOMICILE = tor.CNTRY_OF_DOMICILE,
       dw.COUNTRY_FULL_NAME = tor.COUNTRY_FULL_NAME,
       dw.CPN = tor.CPN,
       dw.CPN_FREQ = tor.CPN_FREQ,
       dw.CPN_TYP = tor.CPN_TYP,
       dw.CRNCY = tor.CRNCY,
       dw.DAY_CNT_DES = tor.DAY_CNT_DES,
       dw.DUR_ADJ_BID = tor.DUR_ADJ_BID,
       dw.DVD_CRNCY = tor.DVD_CRNCY,
       dw.EQY_PRIM_EXCH = tor.EQY_PRIM_EXCH,
       dw.EQY_PRIM_EXCH_SHRT = tor.EQY_PRIM_EXCH_SHRT,
       dw.EQY_SH_OUT = tor.EQY_SH_OUT,
       dw.EQY_SH_OUT_REAL = tor.EQY_SH_OUT_REAL,
       dw.EXCH_CODE = tor.EXCH_CODE,
       dw.FLOATER = tor.FLOATER,
       dw.ID_BB_GLOBAL = tor.ID_BB_GLOBAL,
       dw.ID_BB_UNIQUE = tor.ID_BB_UNIQUE,
       dw.ID_CUSIP = tor.ID_CUSIP,
       dw.ID_ISIN = tor.ID_ISIN,
       dw.ID_SEDOL1 = tor.ID_SEDOL1,
       dw.ID_SEDOL2 = tor.ID_SEDOL2,
       dw.INDUSTRY_SECTOR = tor.INDUSTRY_SECTOR,
       dw.ISSUER = tor.ISSUER,
       dw.IS_SECURED = tor.IS_SECURED,
       dw.LAST_UPDATE = tor.LAST_UPDATE,
       dw.LN_SECURED_UNSECURED = tor.LN_SECURED_UNSECURED,
       dw.MATURITY = tor.MATURITY,
       dw.NAME = tor.NAME,
       dw.NXT_CPN_DT = tor.NXT_CPN_DT,
       dw.PAR_AMT = tor.PAR_AMT,
       dw.PUTABLE = tor.PUTABLE,
       dw.PX_ASK = tor.PX_ASK,
       dw.PX_BID = tor.PX_BID,
       dw.PX_DISC_MID = tor.PX_DISC_MID,
       dw.PX_LAST = tor.PX_LAST,
       dw.PX_QUOTE_LOT_SIZE = tor.PX_QUOTE_LOT_SIZE,
       dw.PX_YEST_CLOSE = tor.PX_YEST_CLOSE,
       dw.QUOTE_TYP = tor.QUOTE_TYP,
       dw.REDEMP_VAL = tor.REDEMP_VAL,
       dw.RTG_SP_LT_LC_ISSUER_CREDIT = tor.RTG_SP_LT_LC_ISSUER_CREDIT,
       dw.SECURITY_TYP = tor.SECURITY_TYP,
       dw.SSR_LIQUIDITY_INDICATOR = tor.SSR_LIQUIDITY_INDICATOR,
       dw.START_ACC_DT = tor.START_ACC_DT,
       dw.TICKER = tor.TICKER,
       dw.VOLUME_AVG_10D = tor.VOLUME_AVG_10D,
       dw.VOLUME_AVG_20D = tor.VOLUME_AVG_20D,
       dw.VOLUME_AVG_5D = tor.VOLUME_AVG_5D,
       dw.DL_ASSET_CLASS = tor.DL_ASSET_CLASS,
       dw.DL_SECURITY_TYPE = tor.DL_SECURITY_TYPE
FROM   [BloombergDataWarehouse] dw
       INNER JOIN TorViewBloombergFixedIncome tor
               ON dw.TOR_MONIKER_LOOKUP = tor.id_sedol1 COLLATE Latin1_General_CI_AS
WHERE  dw.found_nnap IS NULL
       AND dw.found_nap IS NULL
       AND dw.BERG_ID_TYPE = 'sedol1'
       AND TOR_LONGTYPE LIKE 'equity%%'
       AND dw.Effective_Date = @PositionDate;

------ bonds with NA prices.
UPDATE dw
SET    dw.found_statpro = 1,
       px_ask = tor.ask,
       px_bid = tor.bid,
       px_yest_close = tor.[close]
FROM   [BloombergDataWarehouse] dw
       INNER JOIN TorViewStatPro tor
               ON dw.TOR_MONIKER_LOOKUP = tor.securityid COLLATE Latin1_General_CI_AS
WHERE  dw.found_fixedincome IS NOT NULL
       AND px_ask = 'N.A.'
       AND TOR_PULLTYPE = 'F'
       AND dw.Effective_Date = @PositionDate; 
