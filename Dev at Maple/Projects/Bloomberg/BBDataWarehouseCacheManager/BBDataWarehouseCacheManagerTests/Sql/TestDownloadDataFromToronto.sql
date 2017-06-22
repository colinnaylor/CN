USE bloomberg;

DECLARE @thisDate DATE;

SET @thisdate = '{Position_Date}'

SELECT @thisdate;

IF Object_id('dbo.TorViewBloombergEquityPerSecurityPull', 'U') IS NOT NULL
  DROP TABLE TorViewBloombergEquityPerSecurityPull;



SELECT Getdate() AS InsertedWhen,
       *
INTO   TorViewBloombergEquityPerSecurityPull
FROM   [HELIUM].BloombergDataLicense.dbo.ve_BloombergEquityPerSecurityPull
WHERE  effectivedate = @thisdate;


USE bloomberg;

SET QUOTED_IDENTIFIER ON
SET ANSI_PADDING ON

SELECT Count(*)
FROM   TorViewBloombergEquityPerSecurityPull;

