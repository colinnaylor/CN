USE [Bloomberg]
GO

/****** Object:  Table [dbo].[TorViewStatPro]    Script Date: 28/10/2015 17:05:01 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[TorViewStatPro](
	[SecurityId] [varchar](255) NULL,
	[AccuredInterest] [varchar](255) NULL,
	[AdjustedPrice] [varchar](255) NULL,
	[Ask] [varchar](255) NULL,
	[AskDate] [varchar](255) NULL,
	[AskDirtyPrice] [varchar](255) NULL,
	[AskModifiedDuration] [varchar](255) NULL,
	[AskYield] [varchar](255) NULL,
	[AssetType] [varchar](255) NULL,
	[Bid] [varchar](255) NULL,
	[BidDate] [varchar](255) NULL,
	[BidDirtyPrice] [varchar](255) NULL,
	[BidModifiedDuration] [varchar](255) NULL,
	[BidYield] [varchar](255) NULL,
	[Coupon] [varchar](255) NULL,
	[Currency] [varchar](255) NULL,
	[Description] [varchar](255) NULL,
	[High] [varchar](255) NULL,
	[Issuer] [varchar](255) NULL,
	[IssuersLongName] [varchar](255) NULL,
	[Low] [varchar](255) NULL,
	[MarketCode] [varchar](255) NULL,
	[MarketDescription] [varchar](255) NULL,
	[MaturityDate] [varchar](255) NULL,
	[Mid] [varchar](255) NULL,
	[MidDate] [varchar](255) NULL,
	[MidDirtyPrice] [varchar](255) NULL,
	[MidModifiedDuration] [varchar](255) NULL,
	[MidYield] [varchar](255) NULL,
	[OpenPrice] [varchar](255) NULL,
	[Close] [varchar](255) NULL,
	[CloseDate] [varchar](255) NULL,
	[Source] [varchar](255) NULL,
	[SourceDescription] [varchar](255) NULL,
	[Terms_years] [varchar](255) NULL,
	[Volume] [varchar](255) NULL,
	[Yield] [varchar](255) NULL,
	[EffectiveDate] [smalldatetime] NULL
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO


