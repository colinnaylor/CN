SELECT ID,
       RequestType,
       BBTicker,
       BBFieldList,
       DateFrom,
       DateTo,
       Periodicity,
       UserID,
	   InsertedWhen
FROM   BloombergDataRequestItem
WHERE  Processed = 1
       AND ProcessedBy = host_name()
       AND insertedwhen > Dateadd(day, -2, Getdate())
ORDER  BY id 
