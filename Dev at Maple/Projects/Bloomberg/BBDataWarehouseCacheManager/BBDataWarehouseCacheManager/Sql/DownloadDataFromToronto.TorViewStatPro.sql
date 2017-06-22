
SET @thisdate = '{Position_Date}'

SELECT @thisdate;

DELETE dbo.TorViewStatPro;

INSERT TorViewStatPro
SELECT [SecurityId],
       [AccuredInterest],
       [AdjustedPrice],
       [Ask],
       [AskDate],
       [AskDirtyPrice],
       [AskModifiedDuration],
       [AskYield],
       [AssetType],
       [Bid],
       [BidDate],
       [BidDirtyPrice],
       [BidModifiedDuration],
       [BidYield],
       [Coupon],
       [Currency],
       [Description],
       [High],
       [Issuer],
       [IssuersLongName],
       [Low],
       [MarketCode],
       [MarketDescription],
       [MaturityDate],
       [Mid],
       [MidDate],
       [MidDirtyPrice],
       [MidModifiedDuration],
       [MidYield],
       [OpenPrice],
       [Close],
       [CloseDate],
       [Source],
       [SourceDescription],
       [Terms_years],
       [Volume],
       [Yield],
       [EffectiveDate]
FROM   [HELIUM].BloombergDataLicense.dbo.ve_StatPro
WHERE  effectivedate = @thisdate; 
