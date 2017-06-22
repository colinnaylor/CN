DECLARE @thisDate DATE;

SET @thisdate = '{Position_Date}'

--SET @thisdate =Dateadd(DAY, CASE Datename(WEEKDAY, Getdate())
--                              WHEN 'Sunday' THEN -2
--                              WHEN 'Monday' THEN -3
--                              ELSE -1
--                            END, Datediff(DAY, 0, Getdate()))
SELECT @thisdate;

IF EXISTS (SELECT TOP 1 1
           FROM   [HELIUM].[BloombergDataLicense].[dbo].[ve_BloombergNorthAmericanPrice]
           WHERE  [EffectiveDate] = @thisdate)
  BEGIN
      DELETE dbo.TorViewBloombergNorthAmericanPrice;

      INSERT TorViewBloombergNorthAmericanPrice
      SELECT CNTRY_OF_DOMICILE,
             CRNCY,
             EQY_PRIM_EXCH,
             EQY_PRIM_EXCH_SHRT,
             EQY_SH_OUT,
             EQY_SH_OUT_REAL,
             EXCH_CODE,
             ID_BB_COMPANY,
             ID_BB_GLOBAL,
             ID_BB_UNIQUE,
             ID_CUSIP,
             ID_ISIN,
             ID_SEDOL1,
             ID_SEDOL2,
             LAST_UPDATE,
             NAME,
             PRICING_SOURCE,
             PX_ASK,
             PX_BID,
             PX_LAST,
             PX_QUOTE_LOT_SIZE,
             SECURITY_TYP,
             TICKER_AND_EXCH_CODE,
             EFFECTIVEDATE
      FROM   [HELIUM].[BloombergDataLicense].[dbo].[ve_BloombergNorthAmericanPrice]
      WHERE  [EffectiveDate] = @thisdate;
  END 
