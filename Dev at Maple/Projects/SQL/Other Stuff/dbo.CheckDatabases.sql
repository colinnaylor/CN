SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROC CheckDatabases
as
DECLARE @ID int
SELECT @ID = MAX(ID) from CheckDbOutput

EXEC sp_MSforeachdb 'USE [?];
begin
	INSERT master.dbo.CheckDbOutput(DatabaseName,Line) 
	SELECT db_name(),''Start of CheckDB''
	
	INSERT master.dbo.CheckDbOutput(Line) 
	EXEC (''DBCC CHECKDB WITH NO_INFOMSGS'')
	
end'
--  Adding a param of ", TABLERESULTS" gives more detailed results but they are not document on the Microsoft site
--  DBCC CHECKDB WITH TABLERESULTS

IF exists(SELECT * FROM master.dbo.CheckDbOutput WHERE ID > @ID AND Line != 'Start of CheckDB')
begin
	declare @html varchar(max)
	set @html = '<html>
	<body>
	<table border="1">
	<tr>
	<th>Database</th>
	<th>Output</th>
	<th>Time</th>
	</tr>
	<font face="arial" size=2>
	' +
	--   select 
	cast((
		SELECT
		DatabaseName as 'td', '',
		Line as 'td', '',
		Inserted as 'td'
		FROM master.dbo.CheckDbOutput
		WHERE ID > @ID
		Order By ID
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
	@subject = 'DBCC CHECKDB Results',
	@body_format = 'html',
	@body = @html
end

GO
