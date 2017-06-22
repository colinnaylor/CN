USE [Bloomberg]

GO

/****** Object:  Table [dbo].[TorViewBloombergPerSecurityPull]    Script Date: 28/10/2015 17:04:31 ******/
SET ANSI_NULLS ON

GO

SET QUOTED_IDENTIFIER ON

GO

SET ANSI_PADDING ON

GO

CREATE TABLE [dbo].[TorViewBloombergPerSecurityPull]
  (
     [SECURITIES]                    [VARCHAR](255) NULL,
     [BloombergErrorCode]            [VARCHAR](255) NULL,
     [ID_BB_GLOBAL]                  [VARCHAR](255) NULL,
     [ID_BB_UNIQUE]                  [VARCHAR](255) NULL,
     [Name]                          [VARCHAR](255) NULL,
     [SECURITY_TYP]                  [VARCHAR](255) NULL,
     [EffectiveDate]                 [SMALLDATETIME] NULL,
     [DL_SECURITY_TYPE]              [VARCHAR](255) NULL,
     [DL_ASSET_CLASS]                [VARCHAR](255) NULL,
     [ID_ISIN]                       [VARCHAR](255) NULL,
     [ID_CUSIP]                      [VARCHAR](255) NULL,
     [ID_BELGIUM]                    [VARCHAR](255) NULL,
     [ID_COMMON]                     [VARCHAR](255) NULL,
     [ID_DUTCH]                      [VARCHAR](255) NULL,
     [ID_FRENCH]                     [VARCHAR](255) NULL,
     [ID_LOCAL]                      [VARCHAR](255) NULL,
     [ID_SEDOL1]                     [VARCHAR](255) NULL,
     [ID_SEDOL2]                     [VARCHAR](255) NULL,
     [ID_SEDOL3]                     [VARCHAR](255) NULL,
     [ID_VALOREN]                    [VARCHAR](255) NULL,
     [ID_WERTPAPIER]                 [VARCHAR](255) NULL,
     [CRNCY]                         [VARCHAR](255) NULL,
     [POST_EURO_ID_ISIN]             [VARCHAR](255) NULL,
     [PRE_EURO_ID_ISIN]              [VARCHAR](255) NULL,
     [EQY_FUND_CRNCY]                [VARCHAR](255) NULL,
     [ID_BB_PRIM_SECURITY]           [VARCHAR](255) NULL,
     [ID_BB_PRIM_SECURITY_FLAG]      [VARCHAR](255) NULL,
     [ADR_SH_PER_ADR]                [VARCHAR](255) NULL,
     [COUNTRY]                       [VARCHAR](255) NULL,
     [COUNTRY_FULL_NAME]             [VARCHAR](255) NULL,
     [DVD_CRNCY]                     [VARCHAR](255) NULL,
     [INDUSTRY_SECTOR]               [VARCHAR](255) NULL,
     [SSR_LIQUIDITY_INDICATOR]       [VARCHAR](255) NULL,
     [EQY_SH_OUT_REAL]               [VARCHAR](255) NULL,
     [INDX_MWEIGHT]                  [VARCHAR](max) NULL,
     [LAST_TRADEABLE_DT]             [VARCHAR](255) NULL,
     [AMT_ISSUED]                    [VARCHAR](255) NULL,
     [VOLUME_AVG_10D]                [VARCHAR](255) NULL,
     [VOLUME_AVG_20D]                [VARCHAR](255) NULL,
     [VOLUME_AVG_20D_CALL]           [VARCHAR](255) NULL,
     [VOLUME_AVG_20D_PUT]            [VARCHAR](255) NULL,
     [VOLUME_AVG_5D]                 [VARCHAR](255) NULL,
     [VOLUME_AVG_5D_CALL]            [VARCHAR](255) NULL,
     [VOLUME_AVG_5D_PUT]             [VARCHAR](255) NULL,
     [VOLATILITY_10D_CALC]           [VARCHAR](255) NULL,
     [VOLATILITY_180D_CALC]          [VARCHAR](255) NULL,
     [VOLATILITY_200D_CALC]          [VARCHAR](255) NULL,
     [VOLATILITY_260D_CALC]          [VARCHAR](255) NULL,
     [VOLATILITY_30D_CALC]           [VARCHAR](255) NULL,
     [VOLATILITY_360D_CALC]          [VARCHAR](255) NULL,
     [VOLATILITY_60D_CALC]           [VARCHAR](255) NULL,
     [VOLATILITY_90D_CALC]           [VARCHAR](255) NULL,
     [VOLATILITY_10D]                [VARCHAR](255) NULL,
     [VOLATILITY_120D]               [VARCHAR](255) NULL,
     [VOLATILITY_150D]               [VARCHAR](255) NULL,
     [VOLATILITY_180D]               [VARCHAR](255) NULL,
     [VOLATILITY_200D]               [VARCHAR](255) NULL,
     [VOLATILITY_20D]                [VARCHAR](255) NULL,
     [VOLATILITY_260D]               [VARCHAR](255) NULL,
     [VOLATILITY_30D]                [VARCHAR](255) NULL,
     [VOLATILITY_360D]               [VARCHAR](255) NULL,
     [VOLATILITY_60D]                [VARCHAR](255) NULL,
     [VOLATILITY_90D]                [VARCHAR](255) NULL,
     [DELTA]                         [VARCHAR](255) NULL,
     [DELTA_ASK]                     [VARCHAR](255) NULL,
     [DELTA_BID]                     [VARCHAR](255) NULL,
     [DELTA_HEDGE_RATIO]             [VARCHAR](255) NULL,
     [DELTA_LAST]                    [VARCHAR](255) NULL,
     [DELTA_MID]                     [VARCHAR](255) NULL,
     [DELTA_TM]                      [VARCHAR](255) NULL,
     [PX_CLOSE_2D]                   [VARCHAR](255) NULL,
     [PX_YEST_CLOSE]                 [VARCHAR](255) NULL,
     [QUOTE_TYP]                     [VARCHAR](255) NULL,
     [LAST_UPDATE]                   [VARCHAR](255) NULL,
     [LAST_UPDATE_DT]                [VARCHAR](255) NULL,
     [PX_LAST_EOD]                   [VARCHAR](255) NULL,
     [PRIOR_CLOSE_ASK]               [VARCHAR](255) NULL,
     [PRIOR_CLOSE_BID]               [VARCHAR](255) NULL,
     [PRIOR_CLOSE_MID]               [VARCHAR](255) NULL,
     [ULT_PARENT_CNTRY_DOMICILE]     [VARCHAR](255) NULL,
     [ID_BB_ULTIMATE_PARENT_CO]      [VARCHAR](255) NULL,
     [ID_BB_ULTIMATE_PARENT_CO_NAME] [VARCHAR](255) NULL,
     [BDVD_NEXT_EST_EX_DT]           [VARCHAR](255) NULL,
     [BDVD_PROJ_DIV_AMT]             [VARCHAR](255) NULL,
     [PX_ASK]                        [VARCHAR](255) NULL,
     [PX_BID]                        [VARCHAR](255) NULL,
     [PX_LAST]                       [VARCHAR](255) NULL,
     [PX_MID]                        [VARCHAR](255) NULL,
     [CUR_MKT_CAP]                   [VARCHAR](255) NULL,
     [HIGH_52WEEK]                   [VARCHAR](255) NULL,
     [LOW_52WEEK]                    [VARCHAR](255) NULL,
     [PREV_CLS_ASK_DISC]             [VARCHAR](255) NULL,
     [PREV_CLS_MID_DISC]             [VARCHAR](255) NULL,
     [PX_FIXING]                     [VARCHAR](255) NULL,
     [PX_HIGH]                       [VARCHAR](255) NULL,
     [PX_LOW]                        [VARCHAR](255) NULL,
     [PX_VOLUME]                     [VARCHAR](255) NULL,
     [YLD_YTM_MID]                   [VARCHAR](255) NULL
  )
ON [PRIMARY]
TEXTIMAGE_ON [PRIMARY]

GO

SET ANSI_PADDING OFF

GO 
