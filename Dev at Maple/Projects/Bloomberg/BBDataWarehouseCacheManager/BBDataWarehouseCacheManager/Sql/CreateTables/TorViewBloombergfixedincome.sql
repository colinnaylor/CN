USE [Bloomberg]

GO

/****** Object:  Table [dbo].[TorViewBloombergfixedincome]    Script Date: 28/10/2015 17:03:45 ******/
SET ANSI_NULLS ON

GO

SET QUOTED_IDENTIFIER ON

GO

SET ANSI_PADDING ON

GO

CREATE TABLE [dbo].[TorViewBloombergfixedincome]
  (
     [ADR_SH_PER_ADR]             [VARCHAR](255) NULL,
     [AMT_ISSUED]                 [VARCHAR](255) NULL,
     [CALLABLE]                   [VARCHAR](255) NULL,
     [CNTRY_OF_DOMICILE]          [VARCHAR](255) NULL,
     [COUNTRY_FULL_NAME]          [VARCHAR](100) NULL,
     [CPN]                        [VARCHAR](255) NULL,
     [CPN_FREQ]                   [VARCHAR](255) NULL,
     [CPN_TYP]                    [VARCHAR](255) NULL,
     [CRNCY]                      [VARCHAR](255) NULL,
     [DAY_CNT_DES]                [VARCHAR](255) NULL,
     [DL_ASSET_CLASS]             [VARCHAR](255) NULL,
     [DL_SECURITY_TYPE]           [VARCHAR](255) NULL,
     [DUR_ADJ_BID]                [VARCHAR](100) NULL,
     [DVD_CRNCY]                  [VARCHAR](255) NULL,
     [EFFECTIVEDATE]              [SMALLDATETIME] NULL,
     [EQY_PRIM_EXCH]              [VARCHAR](255) NULL,
     [EQY_PRIM_EXCH_SHRT]         [VARCHAR](255) NULL,
     [EQY_SH_OUT]                 [VARCHAR](255) NULL,
     [EQY_SH_OUT_REAL]            [VARCHAR](255) NULL,
     [EXCH_CODE]                  [VARCHAR](255) NULL,
     [FLOATER]                    [VARCHAR](255) NULL,
     [ID_BB_GLOBAL]               [VARCHAR](255) NULL,
     [ID_BB_UNIQUE]               [VARCHAR](255) NULL,
     [ID_CUSIP]                   [VARCHAR](255) NULL,
     [ID_ISIN]                    [VARCHAR](255) NULL,
     [ID_SEDOL1]                  [VARCHAR](255) NULL,
     [ID_SEDOL2]                  [VARCHAR](255) NULL,
     [INDUSTRY_SECTOR]            [VARCHAR](255) NULL,
     [ISSUER]                     [VARCHAR](255) NULL,
     [IS_SECURED]                 [VARCHAR](100) NULL,
     [LAST_UPDATE]                [VARCHAR](255) NULL,
     [LN_SECURED_UNSECURED]       [VARCHAR](100) NULL,
     [MATURITY]                   [VARCHAR](255) NULL,
     [NAME]                       [VARCHAR](255) NULL,
     [NXT_CPN_DT]                 [VARCHAR](255) NULL,
     [PAR_AMT]                    [VARCHAR](255) NULL,
     [PUTABLE]                    [VARCHAR](255) NULL,
     [PX_ASK]                     [VARCHAR](255) NULL,
     [PX_BID]                     [VARCHAR](255) NULL,
     [PX_DISC_MID]                [VARCHAR](255) NULL,
     [PX_LAST]                    [VARCHAR](255) NULL,
     [PX_QUOTE_LOT_SIZE]          [VARCHAR](255) NULL,
     [PX_YEST_CLOSE]              [VARCHAR](255) NULL,
     [QUOTE_TYP]                  [VARCHAR](255) NULL,
     [REDEMP_VAL]                 [VARCHAR](255) NULL,
     [RTG_SP_LT_LC_ISSUER_CREDIT] [VARCHAR](255) NULL,
     [SECURITY_TYP]               [VARCHAR](255) NULL,
     [SSR_LIQUIDITY_INDICATOR]    [VARCHAR](255) NULL,
     [START_ACC_DT]               [VARCHAR](255) NULL,
     [TICKER]                     [VARCHAR](255) NULL,
     [VOLUME_AVG_10D]             [VARCHAR](255) NULL,
     [VOLUME_AVG_20D]             [VARCHAR](255) NULL,
     [VOLUME_AVG_5D]              [VARCHAR](255) NULL
  )
ON [PRIMARY]

GO

SET ANSI_PADDING OFF

GO 
