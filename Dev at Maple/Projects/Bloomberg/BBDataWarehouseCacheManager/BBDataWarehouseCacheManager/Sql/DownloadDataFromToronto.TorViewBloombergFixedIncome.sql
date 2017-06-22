DECLARE @thisDate DATE;

SET @thisdate = '{Position_Date}'

SELECT @thisdate;

IF EXISTS (SELECT TOP 1 1
           FROM   [HELIUM].[BloombergDataLicense].[dbo].[ve_Bloombergfixedincome]
           WHERE  [EffectiveDate] = @thisdate)
  BEGIN
      DELETE dbo.TorViewBloombergfixedincome;

      INSERT TorViewBloombergfixedincome
      SELECT ADR_SH_PER_ADR,
             AMT_ISSUED,
             CALLABLE,
             CNTRY_OF_DOMICILE,
             COUNTRY_FULL_NAME,
             CPN,
             CPN_FREQ,
             CPN_TYP,
             CRNCY,
             DAY_CNT_DES,
             DL_ASSET_CLASS,
             DL_SECURITY_TYPE,
             DUR_ADJ_BID,
             DVD_CRNCY,
             EFFECTIVEDATE,
             EQY_PRIM_EXCH,
             EQY_PRIM_EXCH_SHRT,
             EQY_SH_OUT,
             EQY_SH_OUT_REAL,
             EXCH_CODE,
             FLOATER,
             ID_BB_GLOBAL,
             ID_BB_UNIQUE,
             ID_CUSIP,
             ID_ISIN,
             ID_SEDOL1,
             ID_SEDOL2,
             INDUSTRY_SECTOR,
             ISSUER,
             IS_SECURED,
             LAST_UPDATE,
             LN_SECURED_UNSECURED,
             MATURITY,
             NAME,
             NXT_CPN_DT,
             PAR_AMT,
             PUTABLE,
             PX_ASK,
             PX_BID,
             PX_DISC_MID,
             PX_LAST,
             PX_QUOTE_LOT_SIZE,
             PX_YEST_CLOSE,
             QUOTE_TYP,
             REDEMP_VAL,
             RTG_SP_LT_LC_ISSUER_CREDIT,
             SECURITY_TYP,
             SSR_LIQUIDITY_INDICATOR,
             START_ACC_DT,
             TICKER,
             VOLUME_AVG_10D,
             VOLUME_AVG_20D,
             VOLUME_AVG_5D
      FROM   [HELIUM].[BloombergDataLicense].[dbo].[ve_Bloombergfixedincome]
      WHERE  [EffectiveDate] = @thisdate;
  END 
