use Boss2000

SELECT ProcID, o.Type, o.Name, u.UsedWhen 
FROM objectusage u 
JOIN sys.objects o on o.object_id = u.procid

Create Table ObjectUsage(ProcID int, UsedWhen datetime )
Create clustered index ixClust on ObjectUsage(ProcID)
GRANT INSERT ON ObjectUsage to public

GO
Alter Proc MonitorObjectUsage
as

IF GetDate() < '11 Jun 13'
	RETURN

IF exists( SELECT * FROM objectusage )
begin
	declare @html varchar(max)
	set @html = '<html>
	<body>
	<p>Object usage on ' + @@ServerName + '.' + db_name() + '
	<table border="1">
	<tr>
	<th>ProcID</th>
	<th>Type</th>
	<th>Name</th>
	<th>Used When</th>
	</tr>
	<font face="arial" size=2>
	' +
	--   select 
	cast((
		SELECT 
		ProcID as 'td', '',
		o.Type as 'td', '',
		o.Name as 'td', '',
		u.UsedWhen as 'td'
		FROM objectusage u 
		JOIN sys.objects o on o.object_id = u.procid
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
	@subject = 'Object Usage',
	@body_format = 'html',
	@body = @html
end
