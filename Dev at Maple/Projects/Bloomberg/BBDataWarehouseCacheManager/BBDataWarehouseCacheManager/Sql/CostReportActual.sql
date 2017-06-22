DECLARE @effectiveDate DATE

SET @effectiveDate = '{0:ddMMMyyyy}'

SELECT effective_date,
       BERG_MONIKER,
       dl_asset_class,
       'SecurityMaster' AS DataLicense
FROM   BloombergDataWarehouse dw
WHERE  COALESCE(REQ_DATA_SECURITY_MASTER, 'N') = 'Y'
       AND effective_date = @effectiveDate
GROUP  BY dl_asset_class,
          effective_date,
          BERG_MONIKER
UNION ALL
SELECT effective_date,
       BERG_MONIKER,
       dl_asset_class,
       'Estimates' AS DataLicense
FROM   BloombergDataWarehouse dw
WHERE  COALESCE(REQ_DATA_estimates, 'N') = 'Y'
       AND effective_date = @effectiveDate
GROUP  BY dl_asset_class,
          berg_moniker,
          effective_date
UNION ALL
SELECT effective_date,
       BERG_MONIKER,
       dl_asset_class,
       'CreditRisk' AS DataLicense
FROM   BloombergDataWarehouse dw
WHERE  COALESCE(REQ_DATA_credit_risk, 'N') = 'Y'
       AND effective_date = @effectiveDate
GROUP  BY dl_asset_class,
          berg_moniker,
          effective_date
UNION ALL
SELECT effective_date,
       BERG_MONIKER,
       dl_asset_class,
       'PricingEOD' AS DataLicense
FROM   BloombergDataWarehouse dw
WHERE  COALESCE(REQ_DATA_pricing_eod, 'N') = 'Y'
       AND effective_date = @effectiveDate
GROUP  BY dl_asset_class,
          berg_moniker,
          effective_date
UNION ALL
SELECT effective_date,
       BERG_MONIKER,
       dl_asset_class,
       'PricingIntraday' AS DataLicense
FROM   BloombergDataWarehouse dw
WHERE  COALESCE(REQ_DATA_pricing_intraday, 'N') = 'Y'
       AND effective_date = @effectiveDate
GROUP  BY dl_asset_class,
          berg_moniker,
          effective_date
UNION ALL
SELECT effective_date,
       BERG_MONIKER,
       dl_asset_class,
       'DerivedEOD' AS DataLicense
FROM   BloombergDataWarehouse dw
WHERE  COALESCE(REQ_DATA_derived_eod, 'N') = 'Y'
       AND effective_date = @effectiveDate
GROUP  BY dl_asset_class,
          berg_moniker,
          effective_date
UNION ALL
SELECT effective_date,
       BERG_MONIKER,
       dl_asset_class,
       'DerivedIntraday' AS DataLicense
FROM   BloombergDataWarehouse dw
WHERE  COALESCE(REQ_DATA_derived_intraday, 'N') = 'Y'
       AND effective_date = @effectiveDate
GROUP  BY dl_asset_class,
          berg_moniker,
          effective_date 
