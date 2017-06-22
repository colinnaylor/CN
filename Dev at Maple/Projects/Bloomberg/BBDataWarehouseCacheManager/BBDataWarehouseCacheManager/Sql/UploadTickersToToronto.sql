DECLARE @PositionDate DATE;

SET @PositionDate = '{Position_Date}'

------------upload tickers to toronto! 
DELETE [HELIUM].[BloombergDataLicense].[dbo].[BloombergAdditionalPull];

UPDATE bloombergdatawarehouse
SET    uploaded = NULL
WHERE  effective_date = @PositionDate;

--EQUITY NAMERICA
--EQUITY NON NAMERICA
--FI ISIN
--FI NON ISIN
--FUTURE COMMODITY
--FUTURE EQUITY 
--FUTURE INDEX
--OPTION EQUITY
--OPTION INDEX
--RATES - UNKNOWN
--RATES CURRENCY
--RATES EQUITY INDEX 
--RATES FX FUTURE
--RATES LIBOR
--RATES OTHER INT RATES
PRINT 'Start...'

INSERT [HELIUM].[BloombergDataLicense].[dbo].[BloombergAdditionalPull]
       (pulltype,
        [SecurityIdentifier],
        category_estimates,
        category_creditrisk,
        category_pricingendofday,
        category_pricingintraday,
        category_derivedendofday,
        category_derivedintraday,
        category_securitymaster)
SELECT DISTINCT tor_pulltype,
                tor_id,
                REQ_DATA_ESTIMATES,
                REQ_DATA_CREDIT_RISK,
                REQ_DATA_PRICING_EOD,
                REQ_DATA_PRICING_INTRADAY,
                REQ_DATA_DERIVED_EOD,
                REQ_DATA_DERIVED_INTRADAY,
                REQ_DATA_SECURITY_MASTER
FROM   bloombergdatawarehouse
WHERE  effective_date = @PositionDate
       AND tor_id <> ''
       AND tor_id IS NOT NULL;

UPDATE bloombergdatawarehouse
SET    uploaded = 1
WHERE  effective_date = @PositionDate
       AND tor_id <> ''
       AND tor_id IS NOT NULL; 
