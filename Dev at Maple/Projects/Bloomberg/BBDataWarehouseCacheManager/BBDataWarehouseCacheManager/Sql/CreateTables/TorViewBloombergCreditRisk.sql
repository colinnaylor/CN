USE [Bloomberg]

GO

/****** Object:  Table [dbo].[TorViewBloombergCreditRisk]    Script Date: 28/10/2015 17:02:00 ******/
SET ANSI_NULLS ON

GO

SET QUOTED_IDENTIFIER ON

GO

SET ANSI_PADDING ON

GO

CREATE TABLE [dbo].[TorViewBloombergCreditRisk]
  (
     [ID_BB_COMPANY]             [VARCHAR](255) NULL,
     [ID_BB_ULTIMATE_PARENT_CO]  [VARCHAR](255) NULL,
     [ULT_PARENT_CNTRY_DOMICILE] [VARCHAR](255) NULL,
     [effectivedate]             [SMALLDATETIME] NULL
  )
ON [PRIMARY]

GO

SET ANSI_PADDING OFF

GO

CREATE CLUSTERED INDEX [CI_ID_BB_COMPANY]
  ON [dbo].TorViewBloombergCreditRisk ( [ID_BB_COMPANY] ASC )
  WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]; 
