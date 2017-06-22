USE [Bloomberg]

GO

/****** Object:  Table [dbo].[TorViewBloombergOption]    Script Date: 28/10/2015 17:04:20 ******/
SET ANSI_NULLS ON

GO

SET QUOTED_IDENTIFIER ON

GO

SET ANSI_PADDING ON

GO

CREATE TABLE [dbo].[TorViewBloombergOption]
  (
     [EffectiveDate]   [SMALLDATETIME] NULL,
     [BloombergSymbol] [VARCHAR](100) NULL,
     [PX_BID]          [VARCHAR](100) NULL,
     [PX_ASK]          [VARCHAR](100) NULL,
     [PX_LAST]         [VARCHAR](100) NULL,
     [LAST_UPDATE]     [VARCHAR](100) NULL,
     [LAST_UPDATE_DT]  [VARCHAR](100) NULL,
     [Name]            [VARCHAR](200) NULL,
     [ID_BB_GLOBAL]    [VARCHAR](20) NULL,
     [EQY_SH_OUT_REAL] [VARCHAR](100) NULL,
     [PX_YEST_CLOSE]   [VARCHAR](100) NULL,
     [QUOTE_TYP]       [VARCHAR](100) NULL,
     [SECURITY_TYP]    [VARCHAR](100) NULL
  )
ON [PRIMARY]

GO

SET ANSI_PADDING OFF

GO 
