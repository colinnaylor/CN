SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE proc [dbo].[LogSpaceAlert]
as

SET NOCOUNT ON
create table #res(Name varchar(80), LogSizeMB float, LogSpaceUsedPercent float, Status varchar(20) )

INSERT #res
EXEC ('dbcc sqlperf(logspace)')

--  SELECT * FROM #res where LogSpaceUsedPercent > 50
--  SELECT * FROM LookupValue

IF exists(
	SELECT * 
	FROM #res r
	JOIN LookupValue v ON v.Lookup = 'LogSpacePercentLimit' AND v.Value < r.LogSpaceUsedPercent
	where Name not in ('Model')
	)
begin
	declare @html varchar(max)
	set @html = '<html>
	<body>
	<table border="1">
	<tr>
	<th>Database Name</th>
	<th>Log Percent Used</th>
	</tr>
	<font face="arial" size=2>
	' +
	--   select 
	cast((
		SELECT 
		Name as 'td', '',
		convert(varchar(20), LogSpaceUsedPercent, 0) as 'td'
		FROM #res r
		JOIN LookupValue v ON v.Lookup = 'LogSpacePercentLimit' AND v.Value < r.LogSpaceUsedPercent
		where Name not in ('Model')
		for xml path('tr') 
		) 
	as nvarchar(max))
	+ '
	</font>
	</table>
	<p>Sent from ' + @@ServerName + '.' + db_name() + '.' + isnull(OBJECT_Name(@@procid),system_user) + '</p>
	</body>
	</html>'

	exec msdb..sp_send_dbmail  @profile_name = 'sqladmin',
	@recipients = 'colin.naylor@mpuk.com',
	@subject = 'Log Space Alert',
	@body_format = 'html',
	@body = @html
	
	--  Keep a record
	INSERT LogSpaceRecord(Name, LogSizeMB, LogSpaceUsedPercent,Status)
	SELECT Name, LogSizeMB, LogSpaceUsedPercent,Status
	FROM #res r
	JOIN LookupValue v ON v.Lookup = 'LogSpacePercentLimit' AND v.Value < r.LogSpaceUsedPercent
	where Name not in ('Model')
	--  SELECT * FROM LogSpaceRecord
	--  delete LogSpaceRecord
end
DROP TABLE #res


GO
