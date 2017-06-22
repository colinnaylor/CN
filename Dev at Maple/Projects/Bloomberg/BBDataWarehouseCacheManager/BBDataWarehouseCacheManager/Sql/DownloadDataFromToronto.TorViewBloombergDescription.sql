DECLARE @thisDate DATE;

SET @thisdate = '{Position_Date}'

SELECT @thisdate;

DELETE dbo.TorViewBloombergDescription;

INSERT TorViewBloombergDescription
SELECT ADR_SH_PER_ADR,
       COUNTRY,
       DVD_CRNCY,
       ID_BB_UNIQUE,
       INDUSTRY_SECTOR,
       PAR_AMT,
       TICKER,
       EFFECTIVEDATE
FROM   [HELIUM].[BloombergDataLicense].[dbo].[ve_BloombergDescription]
WHERE  [EffectiveDate] = @thisdate;

SET QUOTED_IDENTIFIER ON
SET ANSI_PADDING ON 
