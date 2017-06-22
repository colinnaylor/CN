USE [bloomberg]

-- create dw
/****** Object:  Table [dbo].[BloombergDataWarehouse]    Script Date: 07/08/2014 17:42:51 ******/
IF ( EXISTS (SELECT *
             FROM   INFORMATION_SCHEMA.TABLES
             WHERE  TABLE_NAME = 'BloombergDataWarehouse') )
  BEGIN
      DROP TABLE [dbo].[BloombergDataWarehouse]
  END

-- create 
CREATE TABLE [dbo].[BloombergDataWarehouse]
  (
     BERG_MONIKER                  VARCHAR (100) NOT NULL,
     EFFECTIVE_DATE                DATE NOT NULL,
     BERG_ID_TYPE                  VARCHAR (50),
     TOR_MONIKER_LOOKUP            VARCHAR (100),
     TOR_MONIKER_LOOKUP2           VARCHAR (100),
     UPLOADED                      TINYINT,
     TOR_ID                        VARCHAR (100),
     TOR_PULLTYPE                  VARCHAR (1),
     TOR_LONGTYPE                  VARCHAR (50) NULL,
     FOUND_CREDITRISK              TINYINT,
     FOUND_DESCRIPTION             TINYINT,
     FOUND_FIXEDINCOME             TINYINT,
     FOUND_STATPRO                 TINYINT,
     FOUND_NNAP                    TINYINT,
     FOUND_NAP                     TINYINT,
     FOUND_EQPERSECPULL            TINYINT,
     DOWNLOADED                    TINYINT,
     REQ_DATA_SECURITY_MASTER      CHAR,
     REQ_DATA_DERIVED_INTRADAY     CHAR,
     REQ_DATA_DERIVED_EOD          CHAR,
     REQ_DATA_PRICING_INTRADAY     CHAR,
     REQ_DATA_PRICING_EOD          CHAR,
     REQ_DATA_ESTIMATES            CHAR,
     REQ_DATA_CREDIT_RISK          CHAR,
     ADR_SH_PER_ADR                VARCHAR (100) NULL,
     AMT_ISSUED                    VARCHAR (100) NULL,
     ASK                           VARCHAR (100) NULL,
     BB_COMPOSITE                  VARCHAR (100) NULL,
     BDVD_NEXT_EST_EX_DT           VARCHAR (100) NULL,
     BDVD_PROJ_DIV_AMT             VARCHAR (100) NULL,
     BETA_OVERRIDE_END_DT          VARCHAR (100) NULL,
     BETA_OVERRIDE_REL_INDEX       VARCHAR (100) NULL,
     BETA_OVERRIDE_START_DT        VARCHAR (100) NULL,
     BID                           VARCHAR (100) NULL,
     CALENDAR_END_DATE             VARCHAR (100) NULL,
     CALLABLE                      VARCHAR (100) NULL,
     CNTRY_OF_DOMICILE             VARCHAR (100) NULL,
     COMPOSITE_EFF_DT              VARCHAR (100) NULL,
     COUNTRY                       VARCHAR (100) NULL,
     COUNTRY_FULL_NAME             VARCHAR (100) NULL,
     CPN                           VARCHAR (100) NULL,
     CPN_FREQ                      VARCHAR (100) NULL,
     CPN_TYP                       VARCHAR (100) NULL,
     CRNCY                         VARCHAR (100) NULL,
     CV_CNVS_RATIO                 VARCHAR (100) NULL,
     DAY_CNT_DES                   VARCHAR (100) NULL,
     DUR_ADJ_BID                   VARCHAR (100) NULL,
     DVD_CRNCY                     VARCHAR (100) NULL,
     EQY_PRIM_EXCH                 VARCHAR (100) NULL,
     EQY_PRIM_EXCH_SHRT            VARCHAR (100) NULL,
     EQY_SH_OUT                    VARCHAR (100) NULL,
     EQY_SH_OUT_REAL               VARCHAR (100) NULL,
     EXCH_CODE                     VARCHAR (100) NULL,
     FITCH_EFF_DT                  VARCHAR (100) NULL,
     FLOATER                       VARCHAR (100) NULL,
     FUTURES_CATEGORY              VARCHAR (100) NULL,
     FUT_CONT_SIZE                 VARCHAR (100) NULL,
     FUT_LAST_TRADE_DT             VARCHAR (100) NULL,
     FUT_NOTL_MTY                  VARCHAR (100) NULL,
     HIGH                          VARCHAR (100) NULL,
     ID_BB_COMPANY                 VARCHAR (100) NULL,
     ID_BB_GLOBAL                  VARCHAR (100) NULL,
     ID_BB_ULTIMATE_PARENT_CO      VARCHAR (100) NULL,
     ID_BB_ULTIMATE_PARENT_CO_NAME VARCHAR (100) NULL,
     ID_BB_UNIQUE                  VARCHAR (100) NULL,
     ID_CUSIP                      VARCHAR (100) NULL,
     ID_ISIN                       VARCHAR (100) NULL,
     ID_SEDOL1                     VARCHAR (100) NULL,
     ID_SEDOL2                     VARCHAR (100) NULL,
     INDUSTRY_SECTOR               VARCHAR (100) NULL,
     INDX_MWEIGHT                  VARCHAR (max) NULL,
     INSERTEDWHEN                  DATETIME,
     ISSUER                        VARCHAR (100) NULL,
     IS_SECURED                    VARCHAR (100) NULL,
     LAST_TRADEABLE_DT             VARCHAR (100) NULL,
     LAST_UPDATE                   VARCHAR (100) NULL,
     LN_SECURED_UNSECURED          VARCHAR (100) NULL,
     LOW                           VARCHAR (100) NULL,
     MATURITY                      VARCHAR (100) NULL,
     MOODY_EFF_DT                  VARCHAR (100) NULL,
     NAME                          VARCHAR (max) NULL,
     NXT_CPN_DT                    VARCHAR (100) NULL,
     OPT_CONT_SIZE_REAL            VARCHAR (100) NULL,
     OPT_DELTA                     VARCHAR (100) NULL,
     OPT_DELTA_MID                 VARCHAR (100) NULL,
     OPT_EXER_TYP                  VARCHAR (100) NULL,
     OPT_PUT_CALL                  VARCHAR (100) NULL,
     OPT_STRIKE_PX                 VARCHAR (100) NULL,
     OPT_UNDL_CRNCY                VARCHAR (100) NULL,
     OPT_UNDL_TICKER               VARCHAR (100) NULL,
     PAR_AMT                       VARCHAR (100) NULL,
     PUTABLE                       VARCHAR (100) NULL,
     PX_ASK                        VARCHAR (100) NULL,
     PX_BD                         VARCHAR (100) NULL,
     PX_BID                        VARCHAR (100) NULL,
     PX_CLOSE_2D                   VARCHAR (100) NULL,
     PX_DISC_MID                   VARCHAR (100) NULL,
     PX_LAST                       VARCHAR (100) NULL,
     PX_QUOTE_LOT_SIZE             VARCHAR (100) NULL,
     PX_YEST_CLOSE                 VARCHAR (100) NULL,
     QUOTE_TYP                     VARCHAR (100) NULL,
     REDEMP_VAL                    VARCHAR (100) NULL,
     RTG_FITCH                     VARCHAR (100) NULL,
     RTG_FITCH_ISSUER_EFF_DT       VARCHAR (100) NULL,
     RTG_FITCH_ISSUER_RATING       VARCHAR (100) NULL,
     RTG_FITCH_LT_FC_DEBT          VARCHAR (100) NULL,
     RTG_FITCH_LT_FC_DEBT_RTG_DT   VARCHAR (100) NULL,
     RTG_FITCH_SEN_UNSECURED       VARCHAR (100) NULL,
     RTG_FITCH_SEN_UNSEC_RTG_DT    VARCHAR (100) NULL,
     RTG_MDY_ISSUER_EFF_DT         VARCHAR (100) NULL,
     RTG_MDY_ISSUER_RATING         VARCHAR (100) NULL,
     RTG_MDY_LT_FC_DEBT_RATING     VARCHAR (100) NULL,
     RTG_MDY_LT_FC_DEBT_RTG_DT     VARCHAR (100) NULL,
     RTG_MDY_SEN_SUB_DEBT          VARCHAR (100) NULL,
     RTG_MDY_SEN_UNSECURED_DEBT    VARCHAR (100) NULL,
     RTG_MDY_SEN_UNSEC_RTG_DT      VARCHAR (100) NULL,
     RTG_MDY_SUBORDINATED_DEBT     VARCHAR (100) NULL,
     RTG_MDY_SUB_DEBT_RTG_DT       VARCHAR (100) NULL,
     RTG_MOODY                     VARCHAR (100) NULL,
     RTG_MOODY_SEN_SUB_RTG_DT      VARCHAR (100) NULL,
     RTG_SP                        VARCHAR (100) NULL,
     RTG_SP_ISSUER_EFF_DT          VARCHAR (100) NULL,
     RTG_SP_ISSUER_RATING          VARCHAR (100) NULL,
     RTG_SP_LT_FC_ISSUER_CREDIT    VARCHAR (100) NULL,
     RTG_SP_LT_FC_ISS_CRED_RTG_DT  VARCHAR (100) NULL,
     RTG_SP_LT_LC_ISSUER_CREDIT    VARCHAR (100) NULL,
     RTG_SP_LT_LC_ISS_CRED_RTG_DT  VARCHAR (100) NULL,
     SECURITY_DES                  VARCHAR (100) NULL,
     SECURITY_NAME                 VARCHAR (100) NULL,
     SECURITY_TYP                  VARCHAR (max) NULL,
     SETTLEMENT_CALENDAR_CODE      VARCHAR (100) NULL,
     SETTLE_DT                     VARCHAR (100) NULL,
     SP_EFF_DT                     VARCHAR (100) NULL,
     SSR_LIQUIDITY_INDICATOR       VARCHAR (100) NULL,
     START_ACC_DT                  VARCHAR (100) NULL,
     SW_PAR_CPN                    VARCHAR (100) NULL,
     TICKER                        VARCHAR (100) NULL,
     TICKER_AND_EXCH_CODE          VARCHAR (100) NULL,
     TRADE                         VARCHAR (100) NULL,
     ULT_PARENT_CNTRY_DOMICILE     VARCHAR (100) NULL,
     VOLATILITY_10D                VARCHAR (100) NULL,
     VOLATILITY_120D               VARCHAR (100) NULL,
     VOLATILITY_150D               VARCHAR (100) NULL,
     VOLATILITY_180D               VARCHAR (100) NULL,
     VOLATILITY_200D               VARCHAR (100) NULL,
     VOLATILITY_20D                VARCHAR (100) NULL,
     VOLATILITY_260D               VARCHAR (100) NULL,
     VOLATILITY_30D                VARCHAR (100) NULL,
     VOLATILITY_360D               VARCHAR (100) NULL,
     VOLATILITY_60D                VARCHAR (100) NULL,
     VOLATILITY_90D                VARCHAR (100) NULL,
     VOLATILITY_10D_CALC           VARCHAR (100) NULL,
     VOLATILITY_180D_CALC          VARCHAR (100) NULL,
     VOLATILITY_200D_CALC          VARCHAR (100) NULL,
     VOLATILITY_260D_CALC          VARCHAR (100) NULL,
     VOLATILITY_30D_CALC           VARCHAR (100) NULL,
     VOLATILITY_360D_CALC          VARCHAR (100) NULL,
     VOLATILITY_60D_CALC           VARCHAR (100) NULL,
     VOLATILITY_90D_CALC           VARCHAR (100) NULL,
     VOLUME_AVG_10D                VARCHAR (100) NULL,
     VOLUME_AVG_20D                VARCHAR (100) NULL,
     VOLUME_AVG_20D_CALL           VARCHAR (100) NULL,
     VOLUME_AVG_20D_PUT            VARCHAR (100) NULL,
     VOLUME_AVG_5D                 VARCHAR (100) NULL,
     VOLUME_AVG_5D_CALL            VARCHAR (100) NULL,
     VOLUME_AVG_5D_PUT             VARCHAR (100) NULL,
     WRT_EXPIRE_DT                 VARCHAR (100) NULL,
     REQ_DATA_FIELD_LIST           VARCHAR (MAX) NULL
  )
ON [primary];

/****** Object:  Index [PK_AccrualAdjustLink]    Script Date: 07/08/2014 17:38:30 ******/
ALTER TABLE [dbo].[BloombergDataWarehouse]
  ADD CONSTRAINT [PK_BloombergDataWarehouse] PRIMARY KEY CLUSTERED ( BERG_MONIKER ASC, effective_date DESC)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

CREATE NONCLUSTERED INDEX [NCI_ID_ISIN]
  ON [dbo].[BloombergDataWarehouse] ( [ID_ISIN] ASC )
  WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

CREATE NONCLUSTERED INDEX [NCI_TICKER_AND_EXCH_CODE]
  ON [dbo].[BloombergDataWarehouse] ( [TICKER_AND_EXCH_CODE] ASC )
  WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

CREATE NONCLUSTERED INDEX [NCI_ID_SEDOL1]
  ON [dbo].[BloombergDataWarehouse] ( [ID_SEDOL1] ASC )
  WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

CREATE NONCLUSTERED INDEX NCI_TOR_MONIKER_LOOKUP
  ON [dbo].[BloombergDataWarehouse] ( [TOR_MONIKER_LOOKUP] ASC )
  WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

CREATE NONCLUSTERED INDEX [NCI_TOR_MONIKER_LOOKUP2]
  ON [dbo].[BloombergDataWarehouse] ( TOR_MONIKER_LOOKUP2 ASC )
  WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

CREATE NONCLUSTERED INDEX [NCI_TOR_ID]
  ON [dbo].[BloombergDataWarehouse] ( TOR_ID ASC )
  WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY] 
