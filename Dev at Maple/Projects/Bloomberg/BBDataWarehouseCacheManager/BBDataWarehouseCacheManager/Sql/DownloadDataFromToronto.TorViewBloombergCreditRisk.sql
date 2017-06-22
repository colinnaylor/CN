DECLARE @thisDate DATE;

SET @thisdate = '{Position_Date}'

SELECT @thisdate;

IF EXISTS (SELECT TOP 1 1
           FROM   [HELIUM].[BloombergDataLicense].[dbo].[ve_BloombergCreditRisk]
           WHERE  [EffectiveDate] = @thisdate)
  BEGIN
      DELETE dbo.TorViewBloombergCreditRisk;

      INSERT TorViewBloombergCreditRisk
      SELECT ID_BB_COMPANY,
             ID_BB_ULTIMATE_PARENT_CO,
             ULT_PARENT_CNTRY_DOMICILE,
             effectivedate
      FROM   [HELIUM].[BloombergDataLicense].[dbo].[ve_BloombergCreditRisk]
      WHERE  [EffectiveDate] = @thisdate;

      
  END 
