USE bloomberg;

DECLARE @PositionDate DATE;

SET @PositionDate = '{Position_Date}'

---------------------------------------------------------------------------------------------------------------------
--
--userid	(No column name)
--InsertUpdateSecurityPricesFromBERG	865
--RiskInterfaceLiquidityHorizon	517
--SecurityDetailBloombergReader	455
--IndexPrice	266
--SSRADR	204
--SSRCountryListUnd	148
--GetCreditRatings	141
--OptionValueManager	131
--ZeroCouponGenerator	115
--SSRISINUnd	92
--security_master_auto	76
--StaticDataImport	59
--SSRCountryList	42
--InsertUpdateCurrencyRatesFromBERG	- staying on API
--SSRISIN	29
--RefRates	25
--SSRStock	22
--UpdateStockAndIndices2	11
--UpdateStockAndIndices	10
--SSRSector	9
--BloombergGetFloatingRateBond	4
--SSRBond	4
--SSRStockUnd	2
--SSRBossSwap	1
--SSRIndexConst	1
--SSRIndexConst2	1
--BloombergChecker	1
---------------------------------------------------------------------------------------------------------------------
IF Object_id('tempdb..#Tickers') IS NOT NULL
  DROP TABLE #Tickers;

CREATE TABLE #Tickers
  (
     Ticker    VARCHAR(50),
     fieldlist VARCHAR(max),
     UserId    VARCHAR(50)
  )

---------------------------------------------------------------------------------------------------------------------
-- sub InsertUpdateSecurityPricesFromBERG
---sp_helptext InsertUpdateSecurityPricesFromBERG
USE boss2000;

INSERT #tickers
SELECT DISTINCT ticker,
                'PX_LAST,PX_BID,PX_ASK,QUOTE_TYP' --Intrday fields
                ,'InsertUpdateSecurityPricesFromBERG_EOD'
FROM   (SELECT bbg.BBString AS Ticker
        FROM   BBGSecurities bbg
               JOIN tblsecurity s
                 ON ( s.securityid = bbg.SecurityID )
               JOIN CurrencyBBGField c
                 ON( c.CurrencyID = s.CurrencyID )
        WHERE  ( bbg.BBString != '' )
        UNION ALL
        SELECT t.BBString Ticker
        FROM   BBGSecurities_All T
               JOIN IndexConstituents I
                 ON T.SecurityID = I.SecurityID) a;

-- Sub ProcessLiquidityHorizonData - RiskInterfaceLiquidityHorizon - out of scope for now.
--INSERT #tickers
--SELECT DISTINCT t.BBString,
--                'VOLUME_AVG_20D,VOLUME_AVG_10D,VOLUME_AVG_5D,VOLUME_AVG_20D_CALL,VOLUME_AVG_5D_CALL,VOLUME_AVG_20D_PUT,VOLUME_AVG_5D_PUT,INDUSTRY_SECTOR,RTG_MOODY,RTG_SP,AMT_ISSUED',
--                'ProcessLiquidityHorizonData'
--FROM   [lynx].rm_var.[dbo].PositionSecurityView p
--       JOIN [lynx].rm_var.[dbo].LH_SecurityTickersView t
--         ON ( p.RemoteSecID = t.RemoteSecID )
--WHERE  ( p.SourceID = 44 )
--       AND ( p.PositionDate = @PositionDate )
--       AND ( t.BBString IS NOT NULL )
--       AND NOT EXISTS (SELECT ticker
--                       FROM   #tickers
--                       WHERE  ticker = t.bbstring)
--- SecurityDetailBloombergReader	455
-- taken from  GetEquitySecurityDetailTickers
SELECT Max(bbg.SecurityID) SecurityID,
       bbg.BBString        Ticker
INTO   #T
FROM   BBGSecurities bbg
       JOIN tblsecurity s
         ON ( s.securityid = bbg.SecurityID ) -- AND (s.SecurityTypeID = 12) -- stock only  
       JOIN CurrencyBBGField c
         ON( c.CurrencyID = s.CurrencyID )
WHERE  ( bbg.BBString != '' )
       AND ISNULL(SecuritySubTypeID, 14) = 14
GROUP  BY BBString

--CA:20121031: Changed to bring in all indices that we hold - also limited securities so that we only get one when linking on bbg ticker...   
INSERT #T
SELECT DISTINCT T.SecurityID,
                BBString Ticker
FROM   BBGSecurities_All T
       LEFT JOIN #T TT
              ON T.SecurityID = TT.SecurityID
       JOIN IndexConstituents I
         ON T.SecurityID = I.SecurityID
WHERE  TT.SecurityID IS NULL

INSERT #tickers
SELECT DISTINCT ticker,
                'EQY_SH_OUT_REAL,INDUSTRY_SECTOR,COUNTRY_FULL_NAME,SSR_LIQUIDITY_INDICATOR',
                'SecurityDetailBloombergReader'
FROM   #T

------------------------------------------
--sub SSRADR (shortseller process) CREATE PROC ShortSellingProcess
IF Object_id('tempdb..#ShortSellingPosition') IS NOT NULL
  DROP TABLE #ShortSellingPosition;

IF Object_id('tempdb..#ShortSellingIndexWeighting') IS NOT NULL
  DROP TABLE #ShortSellingIndexWeighting;

IF Object_id('tempdb..#ShortSellingIndexWeightingsub') IS NOT NULL
  DROP TABLE #ShortSellingIndexWeightingsub;

SELECT *
INTO   #ShortSellingPosition
FROM   [lynx].rm_var.[dbo].ShortSellingPosition
WHERE  1 = 2

ALTER TABLE #ShortSellingPosition
  DROP COLUMN ID;

ALTER TABLE #ShortSellingPosition
  ADD ID INT IDENTITY;

---Get required data by sectype from RMVAR view
INSERT #ShortSellingPosition
       (PositionDate,
        Position,
        Entity,
        SecID,
        ClientPositionID)
SELECT PositionDate,
       Sum(Position)Pos,
       Entity,
       SecID,
       ClientPositionID
FROM   [lynx].rm_var.[dbo].PositionSecurityView T
WHERE  ( PositionDate = @PositionDate
          --include 1 day ago as positions are not in this table until the next day.
          OR PositionDate = CASE
                              WHEN Datename (dw, @PositionDate) = 'Monday' THEN Dateadd(d, -3, @PositionDate)
                              ELSE Dateadd(d, -1, @PositionDate)
                            END )
       AND SecType IN (SELECT SecType
                       FROM   [lynx].rm_var.[dbo].ShortSellingSecTypeInclude
                       WHERE  Include = 1)
       AND Strategy NOT IN (SELECT Strategy
                            FROM   [lynx].rm_var.[dbo].ShortSellingStrategyExclude
                            WHERE  Exclude = 1)
       AND SecID NOT IN (SELECT SecID
                         FROM   [lynx].rm_var.[dbo].ShortSellingExclusionBySecID)
GROUP  BY PositionDate,
          Description,
          RiskMetricID,
          ISIN,
          Sedol,
          Entity,
          SecID,
          SecType,
          ClientPositionID
HAVING Sum(position) <> 0;

SELECT Max(RunDate) MaxRunDate,
       Ticker
INTO   #ShortSellingIndexWeightingSub
FROM   [lynx].rm_var.[dbo].ShortSellingIndexWeighting
WHERE  RunDate <= @PositionDate
GROUP  BY Ticker;

SELECT tt.*
INTO   #ShortSellingIndexWeighting
FROM   #ShortSellingIndexWeightingSub t
       JOIN [lynx].rm_var.[dbo].ShortSellingIndexWeighting tt
         ON t.Ticker = tt.Ticker
            AND t.MaxRunDate = tt.RunDate;

INSERT #tickers
SELECT DISTINCT
--'Reference',
UnderlyingTicker,
'PX_LAST',
'IndexPrice'
--          SecID --removed Country_Full_Name
FROM   [lynx].rm_var.[dbo].ShortSellingIndexWeighting t
WHERE  RunDate = @PositionDate
        OR RunDate = CASE
                       WHEN Datename (dw, @PositionDate) = 'Monday' THEN Dateadd(d, -3, @PositionDate)
                       ELSE Dateadd(d, -1, @PositionDate)
                     END

INSERT #tickers
SELECT DISTINCT
--'Reference',
b.Ticker,
'ULT_PARENT_CNTRY_DOMICILE, COUNTRY',
'SSRCountryList'
--            1 --SecID --removed Country_Full_Name
FROM   #ShortSellingPosition t
       JOIN [lynx].rm_var.[dbo].BloombergSecurityMapping b
         ON t.secid = securityid
WHERE  --PositionDate = @PositionDate and 
  b.Ticker IS NOT NULL
  AND ( COALESCE(UnderlyingTicker, '') = '' )
  AND t.SecID NOT IN (SELECT SecID
                      FROM   [lynx].rm_var.[dbo].ShortSellingExclusionBySecID)

INSERT #tickers
SELECT DISTINCT
--'Reference',
UnderlyingTicker,
'ULT_PARENT_CNTRY_DOMICILE, COUNTRY',
'SSRCountryListUnd'
--          2 --SecID  --removed Country_Full_Name
FROM   #ShortSellingPosition t
       JOIN [lynx].rm_var.[dbo].BloombergSecurityMapping b
         ON t.secid = securityid
WHERE  --PositionDate = @PositionDate 
  --AND b.BBID = '0'AND 
  UnderlyingTicker IS NOT NULL
  AND UnderlyingTicker != ''
  AND t.SecID NOT IN (SELECT SecID
                      FROM   [lynx].rm_var.[dbo].ShortSellingExclusionBySecID)

INSERT #tickers
SELECT DISTINCT
--'Reference',
T.Ticker,
'COUNTRY',
'SSRIndexConst'
--          2 --SecID  --removed Country_Full_Name
FROM   [lynx].rm_var.[dbo].BloombergSecurityMapping T
       JOIN #ShortSellingIndexWeighting S
         ON T.SecurityID = S.SecID
WHERE  T.Ticker IS NOT NULL

INSERT #tickers
SELECT DISTINCT
--'Reference',
T.Ticker,
'EQY_SH_OUT_REAL',
'SSRIndexConst2'
--          2 --SecID  --removed Country_Full_Name
FROM   [lynx].rm_var.[dbo].BloombergSecurityMapping T
       JOIN #ShortSellingIndexWeighting S
         ON T.SecurityID = S.SecID
WHERE  T.Ticker IS NOT NULL

INSERT #tickers
SELECT DISTINCT
--'Reference',
b.Ticker,
'INDUSTRY_SECTOR',
'SSRSector'
--          3--SecID
FROM   #ShortSellingPosition t
       JOIN [lynx].rm_var.[dbo].BloombergSecurityMapping B
         ON t.secid = b.securityid
       LEFT JOIN [lynx].rm_var.[dbo].Country C
              ON B.CountryName = C.CountryName
WHERE  b.SecType IN ( 'b', 'n' )
       AND ( EuropeanSovereign = 1
              OR B.CountryName IS NULL )
       AND t.SecID NOT IN (SELECT SecID
                           FROM   [lynx].rm_var.[dbo].ShortSellingExclusionBySecID)

INSERT #tickers
SELECT DISTINCT
--'Reference',
t.ticker,
'ID_ISIN',
'SSRISIN'
--          4-- t.SecurityID  --, sectype
FROM   [lynx].rm_var.[dbo].BloombergSecurityMapping T
       JOIN #ShortSellingPosition P
         ON T.SecurityID = p.SecID
       JOIN [lynx].rm_var.[dbo].Country C
         ON T.CountryName = C.CountryName
WHERE  T.sectype IN ( 'S' )
       AND isnull(t.Ticker, '0') != '0'
       AND ( isnull(t.UnderlyingTicker, '0') = '0'
              OR UnderlyingTicker = '' )
       AND EEA = 1
       AND T.SecurityID NOT IN (SELECT SecID
                                FROM   [lynx].rm_var.[dbo].ShortSellingExclusionBySecID)

INSERT #tickers
SELECT DISTINCT
--'Reference',
t.UnderlyingTicker,
'ID_ISIN',
'SSRISINUnd'
--          5-- t.SecurityID  --, sectype
FROM   [lynx].rm_var.[dbo].BloombergSecurityMapping T
       JOIN #ShortSellingPosition P
         ON T.SecurityID = p.SecID
       JOIN [lynx].rm_var.[dbo].Country C
         ON T.CountryName = C.CountryName
WHERE  t.sectype IN ( 'S' )
       AND isnull(t.UnderlyingTicker, '0') != '0'
       AND UnderlyingTicker != ''
       AND EEA = 1
       AND T.SecurityID NOT IN (SELECT SecID
                                FROM   [lynx].rm_var.[dbo].ShortSellingExclusionBySecID)

INSERT #tickers
SELECT DISTINCT
-- 'Reference',
t.Ticker,
'ID_ISIN',
'SSRISIN'
--          6--SecurityID  --, sectype
FROM   [lynx].rm_var.[dbo].BloombergSecurityMapping T
       JOIN #ShortSellingPosition P
         ON T.SecurityID = p.SecID
       JOIN [lynx].rm_var.[dbo].Country C
         ON T.CountryName = C.CountryName
WHERE  ISNULL(t.Ticker, '') <> '0'
       AND t.sectype IN ( 'n', 'b', 'v', 'i' )
       AND EEA = 1
       AND ISNULL(T.IndustrySector, 'Government') = 'Government'
       AND T.SecurityID NOT IN (SELECT SecID
                                FROM   [lynx].rm_var.[dbo].ShortSellingExclusionBySecID)

INSERT #tickers
SELECT DISTINCT
--'Reference',
underlyingticker,
'ID_ISIN',
'SSRISINUnd'
--          7-- SecurityID  --, sectype
FROM   [lynx].rm_var.[dbo].BloombergSecurityMapping T
       JOIN #ShortSellingPosition P
         ON T.SecurityID = p.SecID
       JOIN [lynx].rm_var.[dbo].Country C
         ON T.CountryName = C.CountryName
WHERE  t.sectype IN ( 'O' )
       AND isnull(underlyingticker, '0') != '0'
       AND UnderlyingTicker != ''
       AND EEA = 1
       AND T.SecurityID NOT IN (SELECT SecID
                                FROM   [lynx].rm_var.[dbo].ShortSellingExclusionBySecID)
       AND t.UnderlyingTicker NOT IN (SELECT Ticker
                                      FROM   #ShortSellingIndexWeighting)

INSERT #tickers
SELECT DISTINCT
--'Reference',
t.ticker,
'OPT_UNDL_ISIN',
'SSRISIN'
--          8-- SecurityID  --, sectype
FROM   [lynx].rm_var.[dbo].BloombergSecurityMapping T
       JOIN #ShortSellingPosition P
         ON T.SecurityID = p.SecID
       JOIN [lynx].rm_var.[dbo].Country C
         ON T.CountryName = C.CountryName
WHERE  t.sectype IN ( 'O' )
       AND isnull(underlyingticker, '0') = '0'
       AND ISNULL(t.ticker, '0') != '0'
       AND UnderlyingTicker != ''
       AND EEA = 1
       AND T.SecurityID NOT IN (SELECT SecID
                                FROM   [lynx].rm_var.[dbo].ShortSellingExclusionBySecID)

INSERT #tickers
SELECT DISTINCT
--'Reference',
t.underlyingticker,
'ID_ISIN',
'SSRISINUnd'
--          9-- SecurityID  --, sectype
FROM   [lynx].rm_var.[dbo].BloombergSecurityMapping T
       JOIN #ShortSellingPosition P
         ON P.SecID = t.SecurityID
       JOIN [lynx].rm_var.[dbo].Country C
         ON T.CountryName = C.CountryName
WHERE  t.sectype = 'f'
       AND isnull(t.Ticker, '0') != '0'
       AND ISNULL(UnderlyingTicker, '0') != '0'
       AND UnderlyingTicker != ''
       AND EEA = 1
       AND T.SecurityID NOT IN (SELECT SecID
                                FROM   [lynx].rm_var.[dbo].ShortSellingExclusionBySecID)
       AND t.UnderlyingTicker NOT IN (SELECT Ticker
                                      FROM   #ShortSellingIndexWeighting)

INSERT #tickers
SELECT DISTINCT
--'Reference',
T.Ticker,
'ID_ISIN',
'SSRISIN'
--          9-- SecurityID  --, sectype
FROM   [lynx].rm_var.[dbo].BloombergSecurityMapping T
       JOIN #ShortSellingIndexWeighting S
         ON S.SecID = t.SecurityID

--AND NOT EXISTS (SELECT ticker
--                FROM   #tickers
--                WHERE  ticker = t.Ticker);
INSERT #tickers
SELECT DISTINCT
-- 'Reference',
b.Ticker,
'CRNCY, EQY_SH_OUT_REAL',
'SSRStock'
--          10--B.SecurityID ULT_PARENT_TICKER_EXCHANGE
FROM   #ShortSellingPosition T
       JOIN [lynx].rm_var.[dbo].BloombergSecurityMapping B
         ON t.secid = securityid
       JOIN [lynx].rm_var.[dbo].Country C
         ON C.CountryName = B.CountryName
WHERE  b.sectype IN ( 's' )
       AND EEA = 1
       AND B.SecurityID NOT IN (SELECT SecID
                                FROM   [lynx].rm_var.[dbo].ShortSellingExclusionBySecID)
       AND ( ISNULL(UnderlyingTicker, '') = ''
              OR UnderlyingTicker = '0' )

INSERT #tickers
SELECT DISTINCT
-- 'Reference',
underlyingTicker,
'CRNCY, EQY_SH_OUT_REAL',
'SSRStockUnd'
--          10--B.SecurityID ULT_PARENT_TICKER_EXCHANGE
FROM   #ShortSellingPosition T
       JOIN [lynx].rm_var.[dbo].BloombergSecurityMapping B
         ON t.secid = securityid
       JOIN [lynx].rm_var.[dbo].Country C
         ON C.CountryName = B.CountryName
WHERE  b.sectype IN ( 's' )
       AND EEA = 1
       AND B.SecurityID NOT IN (SELECT SecID
                                FROM   [lynx].rm_var.[dbo].ShortSellingExclusionBySecID)
       AND ISNULL(UnderlyingTicker, '') != ''
       AND UnderlyingTicker != '0'

INSERT #tickers
SELECT DISTINCT
-- 'Reference',
b.Ticker,
'ADR_SH_PER_ADR',
'SSRADR'
--10--B.SecurityID ULT_PARENT_TICKER_EXCHANGE
FROM   #ShortSellingPosition T
       JOIN [lynx].rm_var.[dbo].BloombergSecurityMapping B
         ON t.secid = securityid
       JOIN [lynx].rm_var.[dbo].Country C
         ON C.CountryName = B.CountryName
WHERE  b.sectype IN ( 's' )
       AND EEA = 1
       AND B.SecurityID NOT IN (SELECT SecID
                                FROM   [lynx].rm_var.[dbo].ShortSellingExclusionBySecID)
       AND ISNULL(UnderlyingTicker, '0') != '0'
       AND UnderlyingTicker != ''

INSERT #tickers
SELECT DISTINCT
--'Reference',
b.Ticker,
'AMT_ISSUED, DUR_ADJ_BID',
'SSRBond'
-- 11--SecID 
FROM   #ShortSellingPosition t
       JOIN [lynx].rm_var.[dbo].BloombergSecurityMapping b
         ON t.secid = securityid
       JOIN [lynx].rm_var.[dbo].Country C
         ON B.CountryName = C.CountryName
WHERE  b.Ticker <> '0'
       AND B.sectype IN ( 'b', 'n' )
       AND C.EuropeanSovereign = 1
       AND ISNULL(B.IndustrySector, 'Government') = 'Government'
       AND t.SecID NOT IN (SELECT SecID
                           FROM   [lynx].rm_var.[dbo].ShortSellingExclusionBySecID)

INSERT #tickers
SELECT DISTINCT
--'Reference',
b.UnderlyingTicker,
'EQY_SH_OUT_REAL',
'SSRFuture'
--          12--SecID
FROM   #ShortSellingPosition t
       JOIN [lynx].rm_var.[dbo].BloombergSecurityMapping b
         ON t.secid = securityid
       JOIN [lynx].rm_var.[dbo].Country C
         ON C.CountryName = B.CountryName
WHERE  b.Ticker <> '0'
       AND b.SecType IN ( 'F' )
       AND EEA = 1
       AND t.SecID NOT IN (SELECT SecID
                           FROM   [lynx].rm_var.[dbo].ShortSellingExclusionBySecID)
       AND B.UnderlyingTicker NOT IN (SELECT Ticker
                                      FROM   #ShortSellingIndexWeighting)
       AND ISNULL(B.UnderlyingTicker, '0') != '0'
       AND UnderlyingTicker != ''

-------------------zerocoupongenerator - out of scope - moving to adhoc
--EXEC CurveGeneratorGetTickers
--USE boss2000;
--IF Object_id('tempdb..#security') IS NOT NULL
--  DROP TABLE #security;
--CREATE TABLE #security
--  (
--     StampID  INT,
--     ID       INT,
--     TypeName VARCHAR(50),
--     Type     VARCHAR(50),
--     Currency VARCHAR(50),
--     Ticker   VARCHAR(50)
--  )
--INSERT INTO #security
--            (StampID,
--             Id,
--             TypeName,
--             Type,
--             Currency,
--             Ticker)
--EXEC CurveGeneratorGetTickers
--INSERT #tickers
--SELECT DISTINCT Ticker,
--                'PX_ASK,PX_BID',
--                'zerocoupongenerator - mm'
--FROM   #security
--WHERE  TypeName <> 'Future'
--INSERT #tickers
--SELECT DISTINCT Ticker,
--                'PX_ASK,PX_BID,LAST_TRADEABLE_DT',
--                'zerocoupongenerator - futures'
--FROM   #security
--WHERE  TypeName = 'Future'
------------refrates
USE boss2000;

INSERT #tickers
SELECT DISTINCT --'Reference',
BBGTicker,
'PX_LAST,LAST_UPDATE',
'RefRates'
-- RefRateID,
--  '',--@RequestSetID,
-- 'WALRUS'
--SELECT DISTINCT  'Reference',BBGTicker, 'PX_LAST',	'RefRates', RefRateID, 1, 0
FROM   RefRates
WHERE  islive = 1

-------------------UpdateStockAndIndices
USE boss2000;

--BloombergIndexCapturer
--EXEC GetOutstandingStockShares
--stocks
IF Object_id('tempdb..#BBGRequestDetail') IS NOT NULL
  DROP TABLE #BBGRequestDetail;

CREATE TABLE #BBGRequestDetail
  (
     RequestType VARCHAR(50),
     Ticker      VARCHAR(50),
     Fields      VARCHAR(2000),
     DateFrom    DATE,
     DateTo      DATETIME,
     Periodicity VARCHAR(50),
     Value1      VARCHAR(2000),
     Problem     VARCHAR(1000)
  )

DECLARE @stocks TABLE
  (
     Ticker           VARCHAR(50),
     Description      VARCHAR(250),
     SharesInIssue    FLOAT,
     TickerRequest    VARCHAR(60) NULL,
     NewSharesInIssue FLOAT
  )

INSERT INTO @stocks
            (Ticker,
             Description,
             SharesInIssue)
EXEC GetOutstandingStockShares

UPDATE @stocks
SET    TickerRequest = Ticker + ' EQUITY'

INSERT #tickers
SELECT DISTINCT TickerRequest,
                'EQY_SH_OUT',
                'BloombergIndexCapturer - stocks'
FROM   @stocks

--indices
DECLARE @Indices TABLE
  (
     IndexId INT,
     Ticker  VARCHAR(50)
  )

INSERT INTO @Indices
            (IndexId,
             Ticker)
EXEC GetLiveIndices

INSERT #tickers
SELECT DISTINCT Ticker,
                'INDX_MWEIGHT',
                'BloombergIndexCapturer - index'
FROM   @Indices

--------BloombergGetFloatingRateBond
USE boss2000;

IF Object_id('tempdb..#SecToUpdate') IS NOT NULL
  DROP TABLE #SecToUpdate;

SELECT t.SecurityID,
       ISIN + ' CORP' Ticker,
       Min(pointdate) PrevPointDate
INTO   #SecToUpdate
FROM   tblTermStructure t
       JOIN tblSecurity s
         ON t.SecurityID = s.SecurityID
       JOIN tblBond b
         ON t.SecurityID = b.SecurityID
WHERE  PointDate = Datediff(dd, 0, Getdate())
       --PointDate = Datediff(dd, 0, Getdate()-5)
       AND IsFixed = 0
       AND Active = 1
       AND Fixed = 0
       AND SecurityTypeID = 2
GROUP  BY t.SecurityID,
          ISIN;

INSERT #tickers
SELECT DISTINCT Ticker,
                'CPN',
                'BloombergGetFloatingRateBond'
FROM   #SecToUpdate

------------
--OptionValueManager	131  
IF Object_id('tempdb..#tickersButWithTwoColumnsAndColumnsReversed') IS NOT NULL
  DROP TABLE #tickersButWithTwoColumnsAndColumnsReversed;

CREATE TABLE #tickersButWithTwoColumnsAndColumnsReversed
  (
     Fields VARCHAR(max),
     Ticker VARCHAR(50)
  );

--  vols
--exec spOTCOptionPrice_GetBBTickers_Volatility @PositionDate;
DECLARE @Secs TABLE
  (
     SecurityID  INT,
     Description VARCHAR(250),
     MatDate     DATETIME
  )

INSERT INTO @Secs
            (SecurityID,
             Description,
             MatDate)
EXEC [spOTCOptionPrice_GetSecuritiesToPrice]
  @PositionDate

--get all vol points per name....
INSERT #tickers
SELECT DISTINCT US.BBGTicker + ' Equity' AS BBTicker,
                vol.BBField,
                'OptionValueManager - vols'
FROM   @Secs OP
       JOIN tblSecurity S
         ON S.SecurityID = OP.SecurityID
       JOIN tblSecurity US
         ON S.UnderlyingSecurityID = US.SecurityID
       CROSS JOIN tblOTCOptionPrice_Volatility_BBConfig VOL

----- OptionValueManager - rates
INSERT #tickersButWithTwoColumnsAndColumnsReversed
EXEC spOTCOptionPrice_GetBBTickers_RiskFreeRate
  @PositionDate;

INSERT #tickers
SELECT DISTINCT ticker,
                fields,
                'OptionValueManager - rates'
FROM   #tickersButWithTwoColumnsAndColumnsReversed;

-- divs
IF Object_id('tempdb..#tickersButWithTwoColumns') IS NOT NULL
  DROP TABLE #tickersButWithTwoColumns;

CREATE TABLE #tickersButWithTwoColumns
  (
     Ticker VARCHAR(50),
     Fields VARCHAR(max)
  );

INSERT #tickersButWithTwoColumns
EXEC spOTCOptionPrice_GetBBTickers_Dividend
  @PositionDate;

INSERT #tickers
SELECT DISTINCT ticker,
                fields,
                'OptionValueManager - divs'
FROM   #tickersButWithTwoColumns;

-----------security master auto
IF Object_id('tempdb..#ss') IS NOT NULL
  DROP TABLE #ss;

CREATE TABLE #ss
  (
     SecurityID                    INT,
     Description                   VARCHAR(500),
     BBString                      VARCHAR(500),
     ID_BB_GLOBAL                  VARCHAR(500),
     NAME                          VARCHAR(500),
     SECURITY_TYP                  VARCHAR(500),
     ID_BB_ULTIMATE_PARENT_CO      VARCHAR(500),
     ID_BB_ULTIMATE_PARENT_CO_NAME VARCHAR(500)
  )

INSERT #ss
       (SecurityID,
        Description,
        BBString)
SELECT b.SecurityID,
       s.Description,
       B.BBString
FROM   tblSecurity S
       JOIN BBGSecurities_All B
         ON ( B.SecurityID = S.SecurityID )
WHERE  ( S.Active = 1 )
       AND ( S.SecurityID NOT IN (SELECT sm.SourceSecurityID
                                  FROM   security.dbo.SecurityMap sm
                                  WHERE  ( sm.SourceID = 1 )) )
       AND BBString IS NOT NULL
ORDER  BY 1

--  SELECT * FROM #ss
INSERT #tickers
SELECT DISTINCT BBString,
                'ID_BB_GLOBAL,NAME,SECURITY_TYP',
                'security master auto'
FROM   #ss

--------------------GetCreditRatings --out of scope for now.
--IF Object_id('tempdb..#sec') IS NOT NULL
--  DROP TABLE #sec;
--IF Object_id('tempdb..#field') IS NOT NULL
--  DROP TABLE #field;
--CREATE TABLE #sec
--  (
--     ID   INT,
--     Isin VARCHAR(50)
--  )
--CREATE TABLE #field
--  (
--     Field VARCHAR(50)
--  )
----  Get securities
--INSERT #sec
--EXEC GetSecuritiesForRatings
----  uses dbo.BloombergSecurityId_Static()
----  SELECT * FROM #sec order by ID desc
----  SELECT * FROM #sec where id in (20895,25301)
----  Get field names
--INSERT #Field
--SELECT t.BBGField
--FROM   SecurityRatingTypes t
--WHERE  t.Active = 1
--UNION
--SELECT t.EffectiveDateBBGField
--FROM   SecurityRatingTypes t
--WHERE  t.Active = 1
--       AND isnull(t.EffectiveDateBBGField, '') != ''
--INSERT #tickers
--SELECT s.Isin,
--       Field
--FROM   #sec s
--       JOIN #field f
--         ON 1 = 1 
---------------SSRBOSSSWAP
IF Object_id('tempdb..#BossSwap') IS NOT NULL
  DROP TABLE #BossSwap;

SELECT s.SecurityID,
       S.Description,
       Sum(T.AQty)             Balance,
       s.ISIN,
       S.BBGTicker + ' Equity' AS bbgticker
INTO   #BossSwap
FROM   tblSwap T
       JOIN tblPosition P
         ON T.PositionID = P.PositionID
       JOIN tblBook B
         ON P.BookID = B.BookID
       JOIN tblSecurity S
         ON T.ASecurityID = S.SecurityID
--JOIN tblSwapType ST ON T.SwapTypeID = ST.SwapTypeID
WHERE  B.CompanyID = 9
       AND ( T.StartDate <= @PositionDate )
       AND ( T.FinishDate IS NULL
              OR T.FinishDate > @PositionDate ) --Must check finish date for swaps
       AND T.SwapTypeID = 5 --Swap
GROUP  BY S.SecurityID,
          s.ISIN,
          S.BBGTicker,
          S.Description
HAVING Abs(Sum(T.AQty)) >= 0.01;

INSERT #tickers
SELECT DISTINCT BBGTicker,
                'COUNTRY, EQY_SH_OUT_REAL',
                'SSRBossSwap'
--          SecurityID
FROM   #BossSwap
WHERE  SecurityID NOT IN (SELECT s.RemoteSecID
                          FROM   lynx.rm_var.dbo.ShortSellingExclusionBySecID ex
                                 INNER JOIN lynx.rm_var.dbo.security s
                                         ON s.ID = ex.SecID
                          WHERE  s.SourceID = 44);

--getbossdelta - delta and delta_mid

SELECT S.SecurityID
INTO   #TempGetBossDelta
FROM   tblTransaction T
       JOIN tblPosition P
         ON T.PositionID = P.PositionID
       JOIN tblBook B
         ON P.BookID = B.BookID
       JOIN tblSecurity S
         ON T.SecurityID = S.SecurityID
            AND S.ExchangeID <> 33
WHERE  B.CompanyID = 9
       AND T.TradeDate <= @PositionDate
       AND S.SecurityTypeID = 9
GROUP  BY S.SecurityID
HAVING ABS(Sum(t.Quantity * S.FaceValueLotSize)) >= 0.01

INSERT #TempGetBossDelta
SELECT T.ASecurityID
FROM   tblSwap T
       JOIN tblPosition P
         ON T.PositionID = P.PositionID
       JOIN tblBook B
         ON P.BookID = B.BookID
       JOIN tblSecurity S
         ON T.ASecurityID = S.SecurityID
            AND S.ExchangeID <> 33
WHERE  B.CompanyID = 9
       AND T.StartDate <= @PositionDate
       AND ( T.FinishDate IS NULL
              OR T.FinishDate > @PositionDate )
       AND S.SecurityTypeID = 9
GROUP  BY T.ASecurityID
HAVING ABS(Sum(t.AQty * S.FaceValueLotSize)) >= 0.01

INSERT #tickers
SELECT DISTINCT S.BBGTicker + ' EQUITY',
                'DELTA,DELTA_MID',
                'GetBossDelta'
FROM   #TempGetBossDelta T
       JOIN tblSecurity S
         ON S.SecurityID = T.SecurityID

-- put into datawarehouse! 
-- return a list of (group by ticker and concat field list.). http://stackoverflow.com/questions/15154644/sql-group-by-to-combine-concat-a-column
-- for a cheap test only - get one from each userid.
---- Select, one from each subscriber
--WITH tickersWithRank
--     AS (SELECT ticker,
--                fieldlist,
--                userid,
--                dense_rank()
--                  OVER (
--                    partition BY userid
--                    ORDER BY ticker) AS rank
--         FROM   #tickers),
--     trimmedTickers
--     AS (SELECT *
--         FROM   tickersWithRank
--         WHERE  rank = 2)
--SELECT ticker,
--       Stuff((SELECT DISTINCT ',' + fieldlist
--              FROM   trimmedTickers
--              WHERE  ticker = a.ticker
--              FOR XML PATH ('')), 1, 1, '') AS fieldlist
--FROM   trimmedTickers AS a
--GROUP  BY ticker; 
----Test - trimmed by subscriber
--SELECT ticker,
--       Stuff((SELECT DISTINCT ',' + fieldlist
--              FROM   #tickers
--              WHERE  ticker = a.ticker
--			  and userid in ('SSRISIN','OptionValueManager - divs','OptionValueManager - vols','OptionValueManager - rates')
--              FOR XML PATH ('')), 1, 1, '') AS fieldlist
--FROM   #tickers AS a
--where userid in ('SSRISIN','OptionValueManager - divs','OptionValueManager - vols','OptionValueManager - rates')
--GROUP  BY ticker; 
--Select all ! 
SELECT ticker,
       Stuff((SELECT DISTINCT ',' + fieldlist
              FROM   #tickers
              WHERE  ticker = a.ticker
              FOR XML PATH ('')), 1, 1, '') AS fieldlist
FROM   #tickers AS a
GROUP  BY ticker; 
