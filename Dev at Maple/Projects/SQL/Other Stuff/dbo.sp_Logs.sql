SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROC sp_Logs
as

SET NOCOUNT ON

create table #out([Database Name] varchar(80), [Data File(s) Size (GB)] money, [Log File(s) Size (GB)] money, [Log Percent Used] float,
	Last int)
 
INSERT #out
SELECT instance_name AS DatabaseName, 
       [Data File(s) Size (KB)], 
       [LOG File(s) Size (KB)], 
       [Percent Log Used] , 0
FROM 
( 
   SELECT * 
   FROM sys.dm_os_performance_counters 
   WHERE counter_name IN 
   ( 
       'Data File(s) Size (KB)', 
       'Log File(s) Size (KB)', 
       'Percent Log Used' 
   ) 
     AND instance_name not in ('_Total','mssqlsystemresource')
) AS Src 
PIVOT 
( 
   MAX(cntr_value) 
   FOR counter_name IN 
   ( 
       [Data File(s) Size (KB)], 
       [LOG File(s) Size (KB)], 
       [Percent Log Used] 
   ) 
) AS pvt 

--  SELECT * FROM #out

--  Update to Gigabytes
UPDATE #out SET [Data File(s) Size (GB)] = [Data File(s) Size (GB)] / 1048576, 
[Log File(s) Size (GB)] = [Log File(s) Size (GB)] / 1048576

UPDATE #out SET Last = 1 WHERE [Database Name] in ('msdb','master','model','tempdb')
--  ###################################################################################################
--  results
SELECT [Database Name], convert(varchar(12), [Data File(s) Size (GB)],1) [Data File(s) Size (GB)],
	convert(varchar(12), [Log File(s) Size (GB)],1) [Log File(s) Size (GB)],
	[Log Percent Used]
FROM #out
ORDER BY Last, [Database Name]
------------------------------
DROP TABLE #out

GO
