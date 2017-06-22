SET NOCOUNT ON

--  sp_helptext GetBloombergDataRequestItems
--  To enable multiple clients processing this data, set the flag to 1 in a transaction and
----  return those records. The flag will be set to 2 by the retrieval application
--CREATE TABLE #row
--  (
--     ID          INT,
--     RequestType VARCHAR(20),
--     BBTicker    VARCHAR(250),
--     BBFieldList VARCHAR(2000),
--     DateFrom    DATETIME,
--     DateTo      DATETIME,
--     Periodicity VARCHAR(50)
--  )

----SET ROWCOUNT 50

--UPDATE i
--SET    Processed = 1,
--       ProcessedBy = Host_name()
--Output inserted.ID,
--       inserted.RequestType,
--       inserted.BBTicker,
--       inserted.BBFieldList,
--       inserted.DateFrom,
--       inserted.DateTo,
--       inserted.Periodicity
--INTO #row
--FROM   BloombergDataRequestItem i
--WHERE
--  --Processed = 0  AND 
--  Dateadd(day, 0, Datediff(day, 0, insertedwhen)) = '1sep2014'
--  --AND ( TargetServer IS NULL         OR TargetServer = Host_name() )

--SELECT DISTINCT *
--FROM   #row
--ORDER  BY RequestType DESC,
--          BBTicker,
--          BBFieldList,
--          DateFrom

--DROP TABLE #row 

SELECT i.ID,
       i.RequestType,
       i.BBTicker,
       i.BBFieldList,
       i.DateFrom,
       i.DateTo,
       i.Periodicity,
	   i.UserId
FROM   BloombergDataRequestItem i
WHERE
  Dateadd(day, 0, Datediff(day, 0, insertedwhen)) = 
  Dateadd(day, 0, Datediff(day, 0, getdate())) 

ORDER  BY RequestType DESC,
          BBTicker,
          BBFieldList,
          DateFrom 

