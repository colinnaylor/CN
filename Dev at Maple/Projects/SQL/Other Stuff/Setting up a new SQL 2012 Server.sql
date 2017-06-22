--  Setting up a new SQL 2012 Server to Maple Standards

--  ####################################################################################################################
/*  This is the Maple SQL Server setup process. This process has been built from knowledge gained over many years
	as well as knowledge gained by viewing the recommended Best Practices according to Microsoft.
	
	Best Practices info can be found here:-  http://technet.microsoft.com/en-us/library/cc645723.aspx
	
	SELECT @@version	

	INSTRUCTIONS: Set the name of the Server to be inserted into #Name.
					Run the script. It will error at the end as it restarts the SQL services
					Confirm the servername is correct and send a test email.
*/

--  RUN ALL FROM HERE after setting the appropriate values

--  Drop table #Detail
CREATE TABLE #Detail(ServerName varchar(50), MemoryInstalled int, DataDrive varchar(12), LogDrive varchar(12), ); 

INSERT #Detail(ServerName,      MemoryInstalled,DataDrive,LogDrive)
SELECT         'UAT-LYNX-MSCL', 10,              'C',      'C'

--  EXEC sp_configure 'max server memory (MB)'
--  ###########################################################################################################################################################
--  
use master
GO

EXEC sp_configure 'show advanced options', 1
EXEC sp_configure 'default language', 23	-- British English
reconfigure with override

--  Ensure an sa exists
IF not exists( SELECT * FROM sysLogins WHERE name = 'MPUK\Dev_Admins' )
	CREATE LOGIN [MPUK\Dev_Admins] FROM WINDOWS WITH DEFAULT_DATABASE=[master]

EXEC master..sp_addsrvrolemember @loginame = N'MPUK\Dev_Admins', @rolename = N'sysadmin'
GO
--  
 default paths. OS, Data and Logs should be on seperate physical drives for maximum performance and recoverability.
--Data file
--  EXEC xp_instance_regread N'HKEY_LOCAL_MACHINE', N'Software\Microsoft\MSSQLServer\MSSQLServer', N'DefaultData'
--  EXEC xp_instance_regread N'HKEY_LOCAL_MACHINE', N'Software\Microsoft\MSSQLServer\MSSQLServer', N'DefaultLog'
Declare @DataDrive varchar(50) 
SELECT @DataDrive = Datadrive + ':\MSSQL\DATA' From #Detail

EXEC xp_instance_regwrite N'HKEY_LOCAL_MACHINE', N'Software\Microsoft\MSSQLServer\MSSQLServer', N'DefaultData', REG_SZ, @DataDrive

-- Log file
SELECT @DataDrive = LogDrive + ':\MSSQL\LOG' From #Detail

EXEC xp_instance_regwrite N'HKEY_LOCAL_MACHINE', N'Software\Microsoft\MSSQLServer\MSSQLServer', N'DefaultLog', REG_SZ, @DataDrive
GO
--  ###################################################################################################
/*
--  Configure memory - If on a 32 bit system with greater than 4gb of memory installed.
	--  Enable 'lock pages in memory' so that AWE can be enabled
		--  On the Start menu, click Run. In the Open box, type gpedit.msc.
		--  On the Group Policy console, expand Computer Configuration, and then expand Windows Settings.
		--  Expand Security Settings, and then expand Local Policies.
		--  Select the User Rights Assignment folder.
		--  The policies will be displayed in the details pane.
		--  In the pane, double-click Lock pages in memory.
		--  In the Local Security Policy Setting dialog box, click Add.
		--  In the Select Users or Groups dialog box, add an account with privileges to run sqlservr.exe. mpuk\sqlAdmin
		
	--  enable AWE memory if more than 4gb available and not 64 bit
	--	SELECT @@Version
		sp_configure 'awe enabled', 1
		reconfigure
*/
declare @ram int,@ramMsg varchar(50) 
SELECT @ram = (MemoryInstalled * 950) - 600 FROM #Detail
SET @ramMsg = 'max server memory = ' + convert(varchar(50),@ram)
Print @ramMsg

EXEC sp_configure 'max server memory (MB)', @ram
EXEC sp_configure 'optimize for ad hoc workloads', 1
	
--  ###################################################################################################
--  Copy any required Operators by scripting from another server
USE [msdb]
GO
IF not exists( SELECT * FROM msdb.dbo.sysoperators WHERE Name = 'Developer Group' )
	EXEC sp_add_operator @name=N'Developer Group', 
		@enabled=1, 
		@weekday_pager_start_time=90000, 
		@weekday_pager_end_time=180000, 
		@saturday_pager_start_time=90000, 
		@saturday_pager_end_time=180000, 
		@sunday_pager_start_time=90000, 
		@sunday_pager_end_time=180000, 
		@pager_days=0, 
		@email_address=N'DUOC@mpuk.com', 
		@category_name=N'[Uncategorized]', 
		@netsend_address=N'gnu'
GO

--  enable cmdShell etc.
EXEC sp_configure 'xp_cmdshell', 1
EXEC sp_configure 'Agent XPs', 1
EXEC sp_configure 'Database Mail XPs',1 
EXEC sp_configure 'priority boost', 0
EXEC sp_configure 'backup compression default', 1
GO
reconfigure with override
GO
--  ###################################################################################################
--  Setup Email
-- Create a Database Mail profile 
IF not exists( SELECT * FROM sysmail_profile WHERE name = 'sqlAdmin' )
	EXECUTE msdb.dbo.sysmail_add_profile_sp 
	@profile_name = 'sqlAdmin', 
	@description = 'Notification service for SQL Server' ; 
GO
-- Create a Database Mail account 
IF not exists( SELECT * FROM sysmail_account WHERE name = 'SQL Server Notification Service' )
	EXECUTE msdb.dbo.sysmail_add_account_sp 
	@account_name = 'SQL Server Notification Service', 
	@description = 'SQL Server Notification Service', 
	@email_address = 'sqlAdmin@MPUK.com', 
	@replyto_address = 'sqlAdmin@MPUK.com', 
	@display_name = 'SQL Admin', 
	@mailserver_name = 'LondonEx1' ; --  Used to be Dove
GO
-- Add the account to the profile 
IF not exists( SELECT * FROM sysmail_profileaccount a JOIN sysmail_profile p ON p.profile_id = a.profile_id WHERE p.name = 'sqlAdmin' )
	EXECUTE msdb.dbo.sysmail_add_profileaccount_sp 
	@profile_name = 'sqlAdmin', 
	@account_name = 'SQL Server Notification Service', 
	@sequence_number =1 ; 
GO
-- Grant access to the profile to the DBMailUsers role 
IF not exists( SELECT * FROM sysmail_principalprofile r JOIN sysmail_profile p ON p.profile_id = r.profile_id WHERE p.name = 'sqlAdmin' )
	EXECUTE msdb.dbo.sysmail_add_principalprofile_sp 
	@profile_name = 'sqlAdmin', 
	@principal_id = 0, 
	@is_default = 1 ; 
GO
Declare @Subject varchar(50) ; SET @Subject = 'Test Email from ' + @@Servername;
EXEC MSDB..sp_send_dbmail  @profile_name = 'sqlAdmin',
@recipients = 'colin.naylor@mpuk.com',
@subject = @Subject,
@body = 'Body of email'

--  ###################################################################################################
--  Copy Robocopy if necessary
--  xp_cmdshell 'dir c:\Windows\System32\Robo*.*'
--  xp_cmdshell 'dir \\LondonFS2\d$\ITsupport\Software\Robocopy\RoboCopy.exe'
--  EXEC master..xp_cmdshell 'xcopy \\LondonFS2\d$\ITsupport\Software\Robocopy\RoboCopy.exe c:\windows\system32\*.* /Y /D'

--  ###################################################################################################
--  Check DTC setting. Run dcomcnfg.exe and go to component service.Computers.My computer.Distributed Transaction Coordinator.Local DTC
--  Right Click-Properties-  and select the Security tab
--  Enable 'Network DTC Access' and 'Allow Outbound' and 'Allow Inbound'. Leave authentication as 'Mutual Authentication Required'
--  See http://technet.microsoft.com/en-us/library/cc753620(WS.10).aspx for detail

--  EXEC xp_instance_regread N'HKEY_LOCAL_MACHINE', N'Software\Microsoft\MSDTC\Security', N'NetworkDtcAccess'
--  EXEC xp_instance_regread N'HKEY_LOCAL_MACHINE', N'Software\Microsoft\MSDTC\Security', N'NetworkDtcAccessTransactions'
--  EXEC xp_instance_regread N'HKEY_LOCAL_MACHINE', N'Software\Microsoft\MSDTC\Security', N'NetworkDtcAccessInbound'
--  EXEC xp_instance_regread N'HKEY_LOCAL_MACHINE', N'Software\Microsoft\MSDTC\Security', N'NetworkDtcAccessOutbound'
--  EXEC xp_instance_regread N'HKEY_LOCAL_MACHINE', N'Software\Microsoft\MSDTC', N'AllowOnlySecureRpcCalls'
--  EXEC xp_instance_regread N'HKEY_LOCAL_MACHINE', N'Software\Microsoft\MSDTC', N'FallbackToUnsecureRPCIfNecessary'
--  EXEC xp_instance_regread N'HKEY_LOCAL_MACHINE', N'Software\Microsoft\MSDTC', N'TurnOffRpcSecurity'

EXEC xp_instance_regwrite N'HKEY_LOCAL_MACHINE', N'Software\Microsoft\MSDTC\Security', N'NetworkDtcAccess', REG_DWORD, 1
EXEC xp_instance_regwrite N'HKEY_LOCAL_MACHINE', N'Software\Microsoft\MSDTC\Security', N'NetworkDtcAccessTransactions', REG_DWORD, 1
EXEC xp_instance_regwrite N'HKEY_LOCAL_MACHINE', N'Software\Microsoft\MSDTC\Security', N'NetworkDtcAccessInbound', REG_DWORD, 1
EXEC xp_instance_regwrite N'HKEY_LOCAL_MACHINE', N'Software\Microsoft\MSDTC\Security', N'NetworkDtcAccessOutbound', REG_DWORD, 1
EXEC xp_instance_regwrite N'HKEY_LOCAL_MACHINE', N'Software\Microsoft\MSDTC', N'AllowOnlySecureRpcCalls', REG_DWORD, 1
EXEC xp_instance_regwrite N'HKEY_LOCAL_MACHINE', N'Software\Microsoft\MSDTC', N'FallbackToUnsecureRPCIfNecessary', REG_DWORD, 0
EXEC xp_instance_regwrite N'HKEY_LOCAL_MACHINE', N'Software\Microsoft\MSDTC', N'TurnOffRpcSecurity', REG_DWORD, 0


--  ###################################################################################################
--  Ensure the model db has Checksum page_verify set. This should be the default.
Declare @Name varchar(50), @PV varchar(50), @sql varchar(500) 
Declare cs cursor for
SELECT
Name,
CASE m.page_verify_option 
WHEN 0 THEN 'None'
WHEN 1 THEN 'Torn Page Detection'
WHEN 2 THEN 'Checksum'
ELSE 'Unknown'
END AS [PageVerify]
FROM master.sys.databases m
WHERE m.page_verify_option != 2

open cs
fetch next from cs into @Name, @Pv
while @@fetch_status = 0
begin
	SET @sql = 'ALTER DATABASE ' + @Name + ' SET PAGE_VERIFY CHECKSUM'
	EXEC(@sql)

	fetch next from cs into @Name, @Pv
end
close cs
deallocate cs

GO
--  ###################################################################################################
--  Remove guest user from each database
PRINT 'Removing Guest account from all databases'
EXEC sp_MSforeachdb 'USE [?];
begin
	if (select db_name()) not in (''master'',''tempdb'')
	begin
		print db_name()
		REVOKE CONNECT FROM GUEST
	end
end'
GO
--  ###################################################################################################
--  Setup Linked Servers as required.
PRINT 'Please setup any linked servers that are required.'
--  ###################################################################################################
--  Copy any required jobs
	--  by scripting etc.

--  ###################################################################################################
--/*
--  Change server name if required      This hasn't been tested on 2012
IF not exists( SELECT * FROM sys.servers s JOIN #Detail n ON n.ServerName=s.name WHERE server_id = 0 )
begin
	Declare @sql varchar(500) 
	SELECT @sql = 'Changing local server name from [' + name + ']' FROM sys.servers WHERE server_id = 0
	SELECT @sql += ' to [' + ServerName + ']' FROM #Detail
	Print @sql

	SELECT @sql = 'sp_dropserver ''' + name + '''' FROM sys.servers WHERE server_id = 0
	EXEC(@sql)

	SELECT @sql = 'sp_addserver ''' + ServerName + ''',''local''' FROM #Detail
	EXEC(@sql)
end
--  'Restart server to take effect'
--  select @@servername
--  SELECT * FROM sys.servers s WHERE server_id = 0
--*/
--  ###################################################################################################
--  Ensure to create directories on either the D or E drive depending on where the data and log files will be stored.
--  EXEC xp_cmdshell 'dir D:\MSSQL' ; EXEC xp_cmdshell 'dir E:\MSSQL' ;

SELECT @sql = 'EXEC xp_cmdshell ''md ' + DataDrive + ':\MSSQL\Data'''   From #Detail
EXEC(@sql)
SELECT @sql = 'EXEC xp_cmdshell ''md ' + DataDrive + ':\MSSQL\Backup'''   From #Detail
EXEC(@sql)
SELECT @sql = 'EXEC xp_cmdshell ''md ' + LogDrive + ':\MSSQL\Log'''   From #Detail
EXEC(@sql)

--  ###################################################################################################
--  create the following table and add in the default values
use master
GO
IF not exists( SELECT * FROM sys.objects WHERE name = 'DBtimeDefaults' )
begin
	create table DBtimeDefaults(DataPath varchar(200), LogPath varchar(200) )
	--  Insert just one of the following depending on where data and logs will be stored:
	insert DBtimeDefaults(DataPath,LogPath) select DataDrive + ':\mssql\data', LogDrive + ':\mssql\Log' From #Detail
end
--  select * from DBtimeDefaults 
--  delete DBtimeDefaults 

--  ###################################################################################################
--  Add backup/restore user that is used by the Backup and Restore application.
IF not exists( SELECT * FROM syslogins WHERE name = 'BackupOperator' )
begin
	CREATE LOGIN [BackupOperator] WITH PASSWORD=N'BackupOp7[]', DEFAULT_DATABASE=[master], CHECK_EXPIRATION=OFF

	EXEC sys.sp_addsrvrolemember @loginame = N'BackupOperator', @rolename = N'sysadmin'
end
--  ###################################################################################################
--  WinRar       NOT needed for 2012
--  Install WinRar following the instruction found in Sharepoint.
--  http://sharepoint/SiteDirectory/ITDev/MARS%20Wiki/RAR.aspx

--  Ensure you Add the Winrar folder to the end of the Path System Environment Variable. 

--  ###################################################################################################
--  Set the number of logs to retain to 30 so that we have a full history on our tape backups. This avoid letting log files 
--	become too large.
EXEC xp_instance_regwrite N'HKEY_LOCAL_MACHINE', N'SOFTWARE\Microsoft\MSSQLServer\MSSQLServer', N'NumErrorLogs', REG_DWORD, 30
--  Actually writes to HKEY_LOCAL_MACHINE', 'SOFTWARE\Microsoft\Microsoft SQL Server\MSSQL10_50.MSSQLSERVER\MSSQLServer'
--  EXEC xp_instance_regRead N'HKEY_LOCAL_MACHINE', N'SOFTWARE\Microsoft\MSSQLServer\MSSQLServer', N'NumErrorLogs'

--  Restart SQL Server service after setting the path and the NumErrorLogs setting so that it can see the new values

--  ###################################################################################################
USE [msdb]
GO
EXEC msdb.dbo.sp_set_sqlagent_properties @jobhistory_max_rows=999999, @jobhistory_max_rows_per_job=10000
--  EXEC msdb.dbo.sp_Get_sqlagent_properties @jobhistory_max_rows=999999, @jobhistory_max_rows_per_job=10000

GO
--  ############################################################################################################################################################
--  ############################################################################################################################################################
--  #####  You can run the rest as one from here  ########  Expect an error at the end as the server restarts  #################################################
--  ############################################################################################################################################################
--  ############################################################################################################################################################

--  Create a Check DB job to check all database every day
use master
GO
IF not exists( SELECT * FROM sys.objects WHERE name = 'CheckDbOutput' )
	CREATE TABLE CheckDbOutput(ID int identity(1,1), DatabaseName varchar(50), Line varchar(5000), Inserted datetime default GetDate() )
GO
IF exists( SELECT * FROM sys.objects o WHERE o.Name = 'CheckDatabases' AND o.Type = 'P' )
	DROP PROC CheckDatabases
GO
CREATE PROC CheckDatabases
as
DECLARE @ID int
SELECT @ID = MAX(ID) from CheckDbOutput

DELETE master.dbo.CheckDbOutput WHERE Inserted < GetDate() - 31

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
--  ###################################################################################################
--  Create SQL Job to run the Checks
USE [msdb]
GO

/****** Object:  Job [Check for FAILED sql jobs]    Script Date: 29/08/2014 14:26:20 ******/
BEGIN TRANSACTION
DECLARE @ReturnCode INT
SELECT @ReturnCode = 0
/****** Object:  JobCategory [[Uncategorized (Local)]]]    Script Date: 29/08/2014 14:26:20 ******/
IF NOT EXISTS (SELECT name FROM msdb.dbo.syscategories WHERE name=N'[Uncategorized (Local)]' AND category_class=1)
BEGIN
EXEC @ReturnCode = msdb.dbo.sp_add_category @class=N'JOB', @type=N'LOCAL', @name=N'[Uncategorized (Local)]'
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback

END

DECLARE @jobId BINARY(16)
EXEC @ReturnCode =  msdb.dbo.sp_add_job @job_name=N'Check for FAILED sql jobs', 
		@enabled=1, 
		@notify_level_eventlog=0, 
		@notify_level_email=0, 
		@notify_level_netsend=0, 
		@notify_level_page=0, 
		@delete_level=0, 
		@description=N'Check to see if any jobs have failed and emails out to advise IT', 
		@category_name=N'[Uncategorized (Local)]', 
		@owner_login_name=N'sa', @job_id = @jobId OUTPUT
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
/****** Object:  Step [Job Check]    Script Date: 29/08/2014 14:26:20 ******/
EXEC @ReturnCode = msdb.dbo.sp_add_jobstep @job_id=@jobId, @step_name=N'Job Check', 
		@step_id=1, 
		@cmdexec_success_code=0, 
		@on_success_action=1, 
		@on_success_step_id=0, 
		@on_fail_action=2, 
		@on_fail_step_id=0, 
		@retry_attempts=0, 
		@retry_interval=0, 
		@os_run_priority=0, @subsystem=N'TSQL', 
		@command=N'CREATE TABLE #job(JobID uniqueidentifier, Status varchar(50), runDate varchar(50), runTime varchar(50), RunDateTime datetime)
--  DELETE #job

INSERT #job(JobID,runDate,runTime,Status)
SELECT s.job_id, s.run_date, s.run_time,run_Status
FROM sysjobhistory s
WHERE s.step_id=0
	AND convert(date, convert(varchar(50), s.run_date) ) > getdate() - 1

UPDATE j SET RunDateTime = convert(datetime, 
		j.rundate + '' '' 
	+ left(REPLACE(STR(j.runtime, 6), SPACE(1), ''0''),2) 
	+ '':'' + substring(REPLACE(STR(j.runtime, 6), SPACE(1), ''0''),3,2)
	+ '':'' + right(REPLACE(STR(j.runtime, 6), SPACE(1), ''0''),2)  )
FROM #job j

--  Select recent jobs
SELECT s.name, j.RunDateTime, ''Failure'' OutCome INTO #togo
FROM #job j
JOIN sysjobs s ON s.job_id = j.JobID
WHERE Status = ''0''
	AND j.RunDateTime > getdate() - (1.0/24.0) --  For hourly running

IF EXISTS(
	SELECT * FROM #togo
)
BEGIN
	declare @html varchar(max)
	set @html = ''<html>
	<body>
	<table border="1">
	<tr>
	<th>Job Name</th>
	<th>Run Time</th>
	<th>Outcome</th>
	</tr>
	<font face="arial" size=2>
	'' +
	--   select 
	cast((
		SELECT 
		name as ''td'', '''', 
		RunDateTime as ''td'', '''',
		Outcome as ''td''
		FROM #togo
		for xml path(''tr'') 
		) 
	as nvarchar(max))
	+ ''
	</font>
	</table>
	<p>Sent from '' + @@ServerName + ''.'' + db_name() + ''.'' + isnull(OBJECT_Name(@@procid),system_user) + ''</p>
	</body>
	</html>''
	PRINT @html

	declare @recips varchar(500), @Sub varchar(150) 
	SET @recips = ''DUOC@mpuk.com''
	SET @recips = ''colin.naylor@mpuk.com''
	SET @Sub = ''SQL job failure on '' + @@servername

	exec msdb..sp_send_dbmail  @profile_name = ''sqladmin'',
	@recipients = @recips,
	@subject = @Sub,
	@body_format = ''html'',
	@body = @html
END

DROP TABLE #job
', 
		@database_name=N'msdb', 
		@flags=0
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
EXEC @ReturnCode = msdb.dbo.sp_update_job @job_id = @jobId, @start_step_id = 1
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
EXEC @ReturnCode = msdb.dbo.sp_add_jobschedule @job_id=@jobId, @name=N'Every hour', 
		@enabled=1, 
		@freq_type=4, 
		@freq_interval=1, 
		@freq_subday_type=8, 
		@freq_subday_interval=1, 
		@freq_relative_interval=0, 
		@freq_recurrence_factor=0, 
		@active_start_date=20140826, 
		@active_end_date=99991231, 
		@active_start_time=0, 
		@active_end_time=235900, 
		@schedule_uid=N'63ea32cd-76bf-4582-bf65-ee519121bac1'
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
EXEC @ReturnCode = msdb.dbo.sp_add_jobserver @job_id = @jobId, @server_name = N'(local)'
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
COMMIT TRANSACTION
GOTO EndSave
QuitWithRollback:
    IF (@@TRANCOUNT > 0) ROLLBACK TRANSACTION
EndSave:

GO

/****** Object:  Job [Database Checks]    Script Date: 01/11/2013 14:31:58 ******/
BEGIN TRANSACTION
DECLARE @ReturnCode INT
SELECT @ReturnCode = 0
/****** Object:  JobCategory [[Uncategorized (Local)]]]    Script Date: 01/11/2013 14:31:58 ******/
IF NOT EXISTS (SELECT name FROM msdb.dbo.syscategories WHERE name=N'[Uncategorized (Local)]' AND category_class=1)
BEGIN
	EXEC @ReturnCode = msdb.dbo.sp_add_category @class=N'JOB', @type=N'LOCAL', @name=N'[Uncategorized (Local)]'
	IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback

END

IF exists( SELECT * FROM sysjobs o WHERE o.Name = 'Database Checks' )
    EXEC sp_delete_job @job_name = 'Database Checks'

DECLARE @jobId BINARY(16)
EXEC @ReturnCode =  msdb.dbo.sp_add_job @job_name=N'Database Checks', 
		@enabled=1, 
		@notify_level_eventlog=0, 
		@notify_level_email=0, 
		@notify_level_netsend=0, 
		@notify_level_page=0, 
		@delete_level=0, 
		@description=N'Check database consistencies etc.', 
		@category_name=N'[Uncategorized (Local)]', 
		@owner_login_name=N'sa', @job_id = @jobId OUTPUT
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
/****** Object:  Step [Check DB]    Script Date: 01/11/2013 14:31:58 ******/
EXEC @ReturnCode = msdb.dbo.sp_add_jobstep @job_id=@jobId, @step_name=N'Check DB', 
		@step_id=1, 
		@cmdexec_success_code=0, 
		@on_success_action=1, 
		@on_success_step_id=0, 
		@on_fail_action=2, 
		@on_fail_step_id=0, 
		@retry_attempts=0, 
		@retry_interval=0, 
		@os_run_priority=0, @subsystem=N'TSQL', 
		@command=N'EXEC CheckDatabases

', 
		@database_name=N'master', 
		@flags=0
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
EXEC @ReturnCode = msdb.dbo.sp_update_job @job_id = @jobId, @start_step_id = 1
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
EXEC @ReturnCode = msdb.dbo.sp_add_jobschedule @job_id=@jobId, @name=N'Daily', 
		@enabled=1, 
		@freq_type=8, 
		@freq_interval=62, 
		@freq_subday_type=1, 
		@freq_subday_interval=0, 
		@freq_relative_interval=0, 
		@freq_recurrence_factor=1, 
		@active_start_date=20130111, 
		@active_end_date=99991231, 
		@active_start_time=011500, 
		@active_end_time=235959 
		,@schedule_uid=N'd91355e8-fc8a-4804-8ba6-03c5f513825d'
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
EXEC @ReturnCode = msdb.dbo.sp_add_jobserver @job_id = @jobId, @server_name = N'(local)'
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
COMMIT TRANSACTION
GOTO EndSave
QuitWithRollback:
    IF (@@TRANCOUNT > 0) ROLLBACK TRANSACTION
EndSave:

GO
--  ###################################################################################################
--  ###################################################################################################
GO
use master
GO
--  ###################################################################################################
IF exists( SELECT * FROM sys.objects o WHERE o.Name = 'sp_TableSizes' AND o.Type = 'P' )
	DROP PROC sp_TableSizes
GO
Create proc sp_TableSizes
as
declare MC cursor FOR
select o.name
from sysObjects o
WHERE type = 'u' and name not like 'dt%'

SET NOCOUNT ON 

DECLARE @Table varchar(80), @Sql varchar(1000)
open MC

Create table #Results(Name varchar(80),Rows money, Reserved varchar(20),DataSize varchar(20),IndexSize varchar(20),Unused varchar(20))
Create table #Results2(Name varchar(80),Rows money, Reserved money,DataSize money,IndexSize money,Unused money)

fetch next from mc into @Table
while @@Fetch_Status <> -1
begin
	select @Sql = 'INSERT #Results EXEC sp_spaceused ' + @Table 
--	print @Sql	
	EXEC (@Sql)

	fetch next from mc into @Table
end

close mc
deallocate mc
--  SELECT * FROM #results

insert #Results2
select name, rows,convert(int,LEFT(Reserved,DATALENGTH(Reserved) - 3)),convert(int,LEFT(DataSize,DATALENGTH(DataSize) - 3)),
convert(int,LEFT(IndexSize,DATALENGTH(IndexSize) - 3)),convert(int,LEFT(Unused,DATALENGTH(Unused) - 3))
FROM #Results
--  SELECT * FROM #results;  SELECT * FROM #results2

-- turn into Mbytes
UPDATE #Results2 SET Reserved = Reserved / 1024.0, DataSize = DataSize / 1024.0, IndexSize = IndexSize / 1024.0, Unused = Unused / 1024.0

--  ###################################################################################################
--  Results
select sum(convert(float,Reserved)) [Reserved MB] from #Results2

select Name, 
left(  convert(varchar(30), Rows, 1)  ,  len(convert(varchar(30), Rows, 1)) - 3) Rows,
left(  convert(varchar(30), Reserved, 1)  ,  len(convert(varchar(30),Reserved , 1)) - 3) [Reserved Size MB],
left(  convert(varchar(30), DataSize, 1)  ,  len(convert(varchar(30), DataSize, 1)) - 3) [Data Size MB],
left(  convert(varchar(30), IndexSize, 1)  ,  len(convert(varchar(30), IndexSize, 1)) - 3) [Index Size MB],
left(  convert(varchar(30), Unused, 1)  ,  len(convert(varchar(30),Unused , 1)) - 3) [Unused Size MB]
--  SELECT * 
from #Results2 ORDER BY Reserved DESC
--select * from #Results2 ORDER BY Rows DESC

drop table #Results
drop table #Results2

set nocount off
GO
---   ########################################################################################################################
GO
IF exists( SELECT * FROM sys.objects o WHERE o.Name = 'cn_info' AND o.Type = 'V' )
	DROP VIEW cn_info
GO
Create VIEW cn_info
as

SELECT CASE WHEN spid = @@Spid THEN '(' + CONVERT(varchar(4),spid) + ')' ELSE CONVERT(varchar(4),spid) END Spid,
ecid,
convert(varchar(4),blocked) Blk , 
open_tran,
convert(varchar(12),db_name(p.dbid)) DB ,
waittime,
lastwaittype,
Cmd , 
convert(varchar(19),last_batch,21) 'Last batch',
convert(varchar(16),hostname) Hostname , 
convert(varchar(12),nt_username) 'NT Username', 
convert(varchar(25),program_name) 'Program Name' ,
convert(varchar(10),loginame) Loginname ,
convert(varchar(19),login_time,21) 'Login time',
cpu,
memusage
FROM master..sysprocesses p
--*/
-- ############################################################################
-- ############################################################################
go
IF exists( SELECT * FROM sys.objects o WHERE o.Name = 'sp_u' AND o.Type = 'P' )
	DROP PROC sp_u
GO
--  sp_u 2
CREATE PROC sp_u   @Active varchar(40) = '0'
AS
--          sp_u
--        dbcc inputbuffer(53)
--        dbcc outputbuffer(53)
SET NOCOUNT ON
declare @machine varchar(40)
IF @Active not in ('0','1','2') SET @machine = @Active

SELECT * FROM master.dbo.cn_info
WHERE cmd NOT IN ('AWAITING COMMAND','TASK MANAGER','SIGNAL HANDLER','LOCK MONITOR','LAZY WRITER','LOG WRITER','CHECKPOINT SLEEP',
				  'TRACE QUEUE TASK','BRKR TASK','CHECKPOINT','BRKR EVENT HNDLR','RESOURCE MONITOR','UNKNOWN TOKEN' )
AND waittime = 0
AND ( HostName = @machine  OR  @machine is null )

SELECT * FROM master.dbo.cn_info
WHERE cmd NOT IN ('AWAITING COMMAND','TASK MANAGER','SIGNAL HANDLER','LOCK MONITOR','LAZY WRITER','LOG WRITER','CHECKPOINT SLEEP',
				  'TRACE QUEUE TASK','BRKR TASK','CHECKPOINT','BRKR EVENT HNDLR')
AND waittime <> 0
AND ( HostName = @machine  OR  @machine is null )

IF @Active IN ('0','2')
begin
	SELECT * FROM master.dbo.cn_info
	WHERE ( @Active != 2 AND cmd IN ('AWAITING COMMAND','TASK MANAGER','SIGNAL HANDLER','LOCK MONITOR','LAZY WRITER','LOG WRITER','CHECKPOINT SLEEP',
				  'TRACE QUEUE TASK','BRKR TASK','CHECKPOINT','BRKR EVENT HNDLR','RESOURCE MONITOR','UNKNOWN TOKEN') )
		OR
		  ( @Active = 2 AND cmd IN ('AWAITING COMMAND' ) )

	IF @Active = '0'
	begin
		select 	DISTINCT
			convert (smallint, req_spid) As spid, 
			convert(varchar(10),db_name(rsc_dbid)) As dbid, 
			rsc_objid As ObjId,
			rsc_indid As IndId,
			substring (v.name, 1, 4) As Type,
			substring (u.name, 1, 8) As Mode,
			substring (x.name, 1, 5) As Status

		from 	master.dbo.syslockinfo,
			master.dbo.spt_values v,
			master.dbo.spt_values x,
			master.dbo.spt_values u

		where   master.dbo.syslockinfo.rsc_type = v.number
				and v.type = 'LR'
				and master.dbo.syslockinfo.req_status = x.number
				and x.type = 'LS'
				and master.dbo.syslockinfo.req_mode + 1 = u.number
				and u.type = 'L'
				AND rsc_objid <> 0
	end
end

IF @machine is not null
begin
	SELECT * FROM master.dbo.cn_info
	WHERE cmd IN ('AWAITING COMMAND','TASK MANAGER','SIGNAL HANDLER','LOCK MONITOR','LAZY WRITER','LOG WRITER','CHECKPOINT SLEEP',
					  'TRACE QUEUE TASK','BRKR TASK','CHECKPOINT','BRKR EVENT HNDLR')
	AND ( HostName = @machine  OR  @machine is null )
end

GO
---   ########################################################################################################################
go
IF exists( SELECT * FROM sys.objects o WHERE o.Name = 'sp_Logs' AND o.Type = 'P' )
	DROP PROC sp_Logs
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
go
--  ###################################################################################################
IF exists( SELECT * FROM sys.objects o WHERE o.Name = 'sp_LongRunning' AND o.Type = 'P' )
	DROP PROC sp_LongRunning
GO
CREATE proc sp_LongRunning
as
SELECT * FROM master.dbo.cn_info
WHERE cmd NOT IN ('AWAITING COMMAND','TASK MANAGER','SIGNAL HANDLER','LOCK MONITOR','LAZY WRITER','LOG WRITER','CHECKPOINT SLEEP',
				  'TRACE QUEUE TASK','BRKR TASK','CHECKPOINT','BRKR EVENT HNDLR','RESOURCE MONITOR','UNKNOWN TOKEN' )
AND waittime = 0
and DateDiff(minute, GetDate(), [Last batch]) > 30
go
--  ###################################################################################################
IF exists( SELECT * FROM sys.objects o WHERE o.Name = 'sp_selectpages' AND o.Type = 'P' )
	DROP PROC sp_selectpages
GO
CREATE PROCEDURE sp_selectpages(@object_id int, @db_id int = NULL,  @max_pages int = 100)
AS
SET NOCOUNT ON

DECLARE @SQL      nvarchar(MAX),
        @PageFID  smallint, 
        @PagePID  int,
        @rowcount int
        
IF @db_id IS NULL
  SET @db_id = DB_ID()

IF object_name(@object_id, @db_id) IS NULL
begin
	RAISERROR ('The object with id [%d] does not exist in the database with id [%d]', 16, 1, @object_id, @db_id);
	RETURN(-1)
end

CREATE TABLE #DBCC_IND(ROWID           integer IDENTITY(1,1) PRIMARY KEY, 
                       PageFID         smallint, 
                       PagePID         integer, 
                       IAMFID          integer, 
                       IAMPID          integer, 
                       ObjectID        integer,
                       IndexID         integer,
                       PartitionNumber bigint,
                       PartitionID     bigint, 
                       Iam_Chain_Type  varchar(80), 
                       PageType        integer,
                       IndexLevel      integer,
                       NexPageFID      integer,
                       NextPagePID     integer,
                       PrevPageFID     integer,
                       PrevPagePID     integer)

CREATE TABLE #DBCC_Page(ROWID        integer IDENTITY(1, 1) PRIMARY KEY, 
                        ParentObject varchar(500),
                        Object       varchar(500), 
                        Field        varchar(500), 
                        Value        varchar(MAX))

CREATE TABLE #Results(ROWID     integer PRIMARY KEY, 
                      Page      varchar(100), 
                      Slot      varchar(300), 
                      Object    varchar(300), 
                      FieldName varchar(300), 
                      Value     varchar(6000))

CREATE TABLE #Columns(ColumnID integer PRIMARY KEY, 
                      Name     varchar(800))

select @SQL = N'SELECT colid, name
                 FROM ' + QUOTENAME(DB_NAME(@db_id)) + N'..syscolumns
                WHERE id = @object_id'

INSERT INTO #Columns
exec sp_executesql @SQL, N'@object_id int', @object_id

SELECT @rowcount = @@ROWCOUNT

IF @rowcount = 0 
begin
	RAISERROR('No columns to return for table with id [%d]', 16, 1, @object_id)
	RETURN(-1)
end

SELECT @SQL = N'DBCC IND(' + QUOTENAME(DB_NAME(@db_id)) + N', ' + CONVERT(varchar(10), @object_id) + N', 1) WITH NO_INFOMSGS'

DBCC TRACEON(3604) WITH NO_INFOMSGS

INSERT INTO #DBCC_IND
EXEC (@SQL)

DECLARE cCursor CURSOR LOCAL READ_ONLY FOR
SELECT TOP (@max_pages) PageFID, PagePID 
FROM #DBCC_IND WHERE PageType = 1

OPEN cCursor

FETCH NEXT FROM cCursor INTO @PageFID, @PagePID 

WHILE @@FETCH_STATUS = 0
begin
	DELETE #DBCC_Page

	SELECT @SQL = N'DBCC PAGE (' + QUOTENAME(DB_NAME(@db_id)) + N',' + CONVERT(varchar(10), @PageFID) + N',' + CONVERT(varchar(10), @PagePID) + N', 3) WITH TABLERESULTS, NO_INFOMSGS '

	INSERT INTO #DBCC_Page
	EXEC (@SQL)

	DELETE FROM #DBCC_Page 
	WHERE Object NOT LIKE 'Slot %' 
	  OR Field = '' 
	  OR Field IN ('Record Type', 'Record Attributes') 
	  OR ParentObject in ('PAGE HEADER:')

	INSERT INTO #Results
	SELECT ROWID, cast(@PageFID as varchar(20)) + ':' + CAST(@PagePID as varchar(20)), ParentObject, Object, Field, Value 
	FROM #DBCC_Page

	FETCH NEXT FROM cCursor INTO @PageFID, @PagePID 
end
CLOSE cCursor
DEALLOCATE cCursor

SELECT @SQL = N' SELECT ' + STUFF(CAST((SELECT N',' + QUOTENAME(Name) + N'' 
                                         FROM #Columns 
                                     ORDER BY ColumnID 
                                          FOR XML PATH('')) AS varchar(MAX)), 1, 1,'') + '
                FROM (SELECT CONVERT(varchar(20), Page) + CONVERT(varchar(500), Slot) p
                           , FieldName x_FieldName_x
                           , Value x_Value_x 
                        FROM #Results) Tab
                PIVOT(MAX(Tab.x_Value_x) FOR Tab.x_FieldName_x IN( ' + STUFF((SELECT ',' +  QUOTENAME(Name) + '' 
                                                                                FROM #Columns 
                                                                            ORDER BY ColumnID 
                                                                                 FOR XML PATH('')), 1, 1,'') + ' )
                ) AS pvt'

EXEC (@SQL)

RETURN (0)
GO
--  ###################################################################################################
IF exists( SELECT * FROM sys.objects o WHERE o.Name = 'sp_select' AND o.Type = 'P' )
	DROP PROC sp_select
GO
CREATE PROCEDURE dbo.sp_select(@table_name sysname, @spid int = NULL, @max_pages int = 1000)
AS
SET NOCOUNT ON

DECLARE @object_id int
    , @table     sysname
    , @db_name   sysname
    , @db_id     int
    , @file_name varchar(MAX)  
    , @status    int
    , @rowcount  int
  
IF PARSENAME(@table_name, 3) = 'tempdb'
begin
	SET @table = PARSENAME(@table_name, 1)

	IF (SELECT COUNT(*)  from tempdb.sys.tables where name like @table + '[_][_]%') > 1
	begin
		-- determine the default trace file
		SELECT @file_name = SUBSTRING(path, 0, LEN(path) - CHARINDEX('\', REVERSE(path)) + 1) + '\Log.trc'  
		FROM sys.traces   
		WHERE is_default = 1;  

		CREATE TABLE #objects (ObjectId sysname primary key)
	    
		-- Match the spid with db_id and object_id via the default trace file
		insert into #objects
		SELECT o.OBJECT_ID
		FROM sys.fn_trace_gettable(@file_name, DEFAULT) AS gt  
		JOIN tempdb.sys.objects AS o   
			 ON gt.ObjectID = o.OBJECT_ID  
		LEFT JOIN (SELECT distinct spid, dbid 
					 FROM master..sysprocesses 
					WHERE spid = @spid or @spid is null) dr ON dr.spid = gt.SPID
		WHERE gt.DatabaseID = 2 
		  AND gt.EventClass = 46 -- (Object:Created Event from sys.trace_events)  
		  AND o.create_date >= DATEADD(ms, -100, gt.StartTime)   
		  AND o.create_date <= DATEADD(ms, 100, gt.StartTime)
		  AND o.name like @table + '[_][_]%'
		  AND (gt.SPID = @spid or (@spid is null and dr.dbid = DB_ID()))
	      
		SET @rowcount = @@ROWCOUNT
	    
		IF @rowcount = 0 
		begin
			RAISERROR('Unable to figure out which temp table with name [%s] to select, please run the procedure on a specific database, or specify a @spid to filter on.', 16,1, @table_name)
			RETURN(-1)
		end
	    
		IF @rowcount > 1 and @spid is null
		begin
			RAISERROR('There are %d temp tables with the name [%s] active in your database. Please specify the @spid you wish to find it for.', 16, 1, @rowcount, @table_name)
			RETURN(-1)
		end   
	    
		IF @rowcount > 1
		begin
			RAISERROR('There are %d temp tables with the name [%s] active on the spid %d. There must be something wrong in this procedure. Showing the first one', 16, 1, @rowcount, @table_name, @spid)
		-- We'll continue with the first match.
		end

		SELECT TOP 1 @object_id = ObjectId 
		FROM #objects
		ORDER BY ObjectId
	end
	ELSE
	begin
	  SELECT @object_id = object_id FROM tempdb.sys.tables WHERE name LIKE @table + '[_][_]%'
	end
end
ELSE 
begin
	SET @object_id = OBJECT_ID(@table_name)
end

IF @object_id IS NULL
begin 
	RAISERROR('The table [%s] does not exist', 16, 1, @table_name)
	RETURN (-1)
end

SET @db_id = DB_ID(PARSENAME(@table_name, 3))

EXEC @status = master..sp_selectpages @object_id = @object_id, @db_id = @db_id, @max_pages = @max_pages

--SELECT 'For debugging only!' as Note

RETURN (@status)
GO
--  ###################################################################################################
GO
USE [msdb]
GO

--  ###################################################################################################
--  Add Error Log management job
--  that calls DBCC ERRORLOG every weekday

/****** Object:  Job [Error Log Cycle]    Script Date: 01/11/2013 15:26:21 ******/
BEGIN TRANSACTION
DECLARE @ReturnCode INT
SELECT @ReturnCode = 0
/****** Object:  JobCategory [[Uncategorized (Local)]]]    Script Date: 01/11/2013 15:26:21 ******/
IF NOT EXISTS (SELECT name FROM msdb.dbo.syscategories WHERE name=N'[Uncategorized (Local)]' AND category_class=1)
BEGIN
EXEC @ReturnCode = msdb.dbo.sp_add_category @class=N'JOB', @type=N'LOCAL', @name=N'[Uncategorized (Local)]'
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback

END

IF exists( SELECT * FROM sysjobs o WHERE o.Name = 'Error Log Cycle' )
    EXEC sp_delete_job @job_name = 'Error Log Cycle'

DECLARE @jobId BINARY(16)
EXEC @ReturnCode =  msdb.dbo.sp_add_job @job_name=N'Error Log Cycle', 
		@enabled=1, 
		@notify_level_eventlog=0, 
		@notify_level_email=0, 
		@notify_level_netsend=0, 
		@notify_level_page=0, 
		@delete_level=0, 
		@description=N'Cycles the error logs so that they don''t get too big in accordance with Best Practices', 
		@category_name=N'[Uncategorized (Local)]', 
		@owner_login_name=N'sa', @job_id = @jobId OUTPUT
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
/****** Object:  Step [Cycle error log]    Script Date: 01/11/2013 15:26:21 ******/
EXEC @ReturnCode = msdb.dbo.sp_add_jobstep @job_id=@jobId, @step_name=N'Cycle error log', 
		@step_id=1, 
		@cmdexec_success_code=0, 
		@on_success_action=1, 
		@on_success_step_id=0, 
		@on_fail_action=2, 
		@on_fail_step_id=0, 
		@retry_attempts=0, 
		@retry_interval=0, 
		@os_run_priority=0, @subsystem=N'TSQL', 
		@command=N'DBCC ERRORLOG
', 
		@database_name=N'master', 
		@flags=0
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
EXEC @ReturnCode = msdb.dbo.sp_update_job @job_id = @jobId, @start_step_id = 1
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
EXEC @ReturnCode = msdb.dbo.sp_add_jobschedule @job_id=@jobId, @name=N'Nightly Run', 
		@enabled=1, 
		@freq_type=8, 
		@freq_interval=62, 
		@freq_subday_type=1, 
		@freq_subday_interval=0, 
		@freq_relative_interval=0, 
		@freq_recurrence_factor=1, 
		@active_start_date=20130111, 
		@active_end_date=99991231, 
		@active_start_time=200, 
		@active_end_time=235959, 
		@schedule_uid=N'e0f3b93c-d9c3-4e50-8e51-25e7c6af004f'
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
EXEC @ReturnCode = msdb.dbo.sp_add_jobserver @job_id = @jobId, @server_name = N'(local)'
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
COMMIT TRANSACTION
GOTO EndSave
QuitWithRollback:
    IF (@@TRANCOUNT > 0) ROLLBACK TRANSACTION
EndSave:

GO
USE [msdb]
GO

/****** Object:  Job [Purge Job History]    Script Date: 30/07/2013 10:09:51 ******/
BEGIN TRANSACTION
DECLARE @ReturnCode INT
SELECT @ReturnCode = 0
/****** Object:  JobCategory [[Uncategorized (Local)]]]    Script Date: 30/07/2013 10:09:51 ******/
IF NOT EXISTS (SELECT name FROM msdb.dbo.syscategories WHERE name=N'[Uncategorized (Local)]' AND category_class=1)
BEGIN
EXEC @ReturnCode = msdb.dbo.sp_add_category @class=N'JOB', @type=N'LOCAL', @name=N'[Uncategorized (Local)]'
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback

END

IF exists( SELECT * FROM sysjobs o WHERE o.Name = 'Purge Job History' )
    EXEC sp_delete_job @job_name = 'Purge Job History'

DECLARE @jobId BINARY(16)
EXEC @ReturnCode =  msdb.dbo.sp_add_job @job_name=N'Purge Job History', 
		@enabled=1, 
		@notify_level_eventlog=0, 
		@notify_level_email=0, 
		@notify_level_netsend=0, 
		@notify_level_page=0, 
		@delete_level=0, 
		@description=N'Removes old job history items.', 
		@category_name=N'[Uncategorized (Local)]', 
		@owner_login_name=N'sa', @job_id = @jobId OUTPUT
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
/****** Object:  Step [Daily run]    Script Date: 30/07/2013 10:09:51 ******/
EXEC @ReturnCode = msdb.dbo.sp_add_jobstep @job_id=@jobId, @step_name=N'Daily run', 
		@step_id=1, 
		@cmdexec_success_code=0, 
		@on_success_action=1, 
		@on_success_step_id=0, 
		@on_fail_action=2, 
		@on_fail_step_id=0, 
		@retry_attempts=0, 
		@retry_interval=0, 
		@os_run_priority=0, @subsystem=N'TSQL', 
		@command=N'declare @Day datetime
SET @Day = GetDate() - 7
EXEC msdb.dbo.sp_purge_jobhistory  @oldest_date=@Day
', 
		@database_name=N'master', 
		@flags=0
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
EXEC @ReturnCode = msdb.dbo.sp_update_job @job_id = @jobId, @start_step_id = 1
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
EXEC @ReturnCode = msdb.dbo.sp_add_jobschedule @job_id=@jobId, @name=N'Daily run', 
		@enabled=1, 
		@freq_type=8, 
		@freq_interval=62, 
		@freq_subday_type=1, 
		@freq_subday_interval=0, 
		@freq_relative_interval=0, 
		@freq_recurrence_factor=1, 
		@active_start_date=20130730, 
		@active_end_date=99991231, 
		@active_start_time=81000, 
		@active_end_time=235959, 
		@schedule_uid=N'82f3a436-b60a-4a7f-98f8-b0d61ec91c68'
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
EXEC @ReturnCode = msdb.dbo.sp_add_jobserver @job_id = @jobId, @server_name = N'(local)'
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
COMMIT TRANSACTION
GOTO EndSave
QuitWithRollback:
    IF (@@TRANCOUNT > 0) ROLLBACK TRANSACTION
EndSave:

GO
use master
GO
IF OBJECT_ID ('sp_hexadecimal') IS NOT NULL
  DROP PROCEDURE sp_hexadecimal
GO
CREATE PROCEDURE sp_hexadecimal
    @binvalue varbinary(256),
    @hexvalue varchar (514) OUTPUT
AS
DECLARE @charvalue varchar (514)
DECLARE @i int
DECLARE @length int
DECLARE @hexstring char(16)

SELECT @charvalue = '0x'
SELECT @i = 1
SELECT @length = DATALENGTH (@binvalue)
SELECT @hexstring = '0123456789ABCDEF'

WHILE (@i <= @length)
begin
  DECLARE @tempint int
  DECLARE @firstint int
  DECLARE @secondint int
  SELECT @tempint = CONVERT(int, SUBSTRING(@binvalue,@i,1))
  SELECT @firstint = FLOOR(@tempint/16)
  SELECT @secondint = @tempint - (@firstint*16)
  SELECT @charvalue = @charvalue +
    SUBSTRING(@hexstring, @firstint+1, 1) +
    SUBSTRING(@hexstring, @secondint+1, 1)
  SELECT @i = @i + 1
end

SELECT @hexvalue = @charvalue
GO
 
IF OBJECT_ID ('sp_help_revlogin') IS NOT NULL
  DROP PROCEDURE sp_help_revlogin
GO
CREATE PROCEDURE sp_help_revlogin @login_name sysname = NULL 
AS
SET NOCOUNT ON
CREATE TABLE #output(Row int identity(1,1), Line varchar(5000) )
--  sp_help_revlogin 'CreditTrader'

DECLARE @name sysname
DECLARE @type varchar (1)
DECLARE @hasaccess int
DECLARE @denylogin int
DECLARE @is_disabled int
DECLARE @PWD_varbinary  varbinary (256)
DECLARE @PWD_string  varchar (514)
DECLARE @SID_varbinary varbinary (85)
DECLARE @SID_string varchar (514)
DECLARE @tmpstr  varchar (1024)
DECLARE @is_policy_checked varchar (3)
DECLARE @is_expiration_checked varchar (3)
DECLARE @defaultdb sysname
 
IF (@login_name IS NULL)
	DECLARE login_curs CURSOR FOR
    SELECT p.sid, p.name, p.type, p.is_disabled, p.default_database_name, l.hasaccess, l.denylogin 
	FROM sys.server_principals p 
	LEFT JOIN sys.syslogins l ON ( l.name = p.name ) 
	WHERE p.type IN ( 'S', 'G', 'U' ) AND p.name <> 'sa'
ELSE
	DECLARE login_curs CURSOR FOR
    SELECT p.sid, p.name, p.type, p.is_disabled, p.default_database_name, l.hasaccess, l.denylogin 
	FROM sys.server_principals p 
	LEFT JOIN sys.syslogins l ON ( l.name = p.name ) 
	WHERE p.type IN ( 'S', 'G', 'U' ) AND p.name = @login_name

OPEN login_curs

FETCH NEXT FROM login_curs INTO @SID_varbinary, @name, @type, @is_disabled, @defaultdb, @hasaccess, @denylogin
IF (@@fetch_status = -1)
begin
	PRINT 'No login(s) found.'
	CLOSE login_curs
	DEALLOCATE login_curs
	RETURN -1
end

SET @tmpstr = '/* sp_help_revlogin script */'
INSERT #output(Line) SELECT @tmpstr
WHILE (@@fetch_status <> -1)
begin
	IF (@@fetch_status <> -2)
	begin
		SET @tmpstr = '-- Login: ' + @name
		INSERT #output(Line)
		SELECT @tmpstr

		IF (@type IN ( 'G', 'U'))
		begin -- NT authenticated account/group
			SET @tmpstr = 'IF not exists( SELECT * FROM sysLogins WHERE name = ''' + @name + ''' )
begin
	PRINT ''CREATE LOGIN ' + QUOTENAME( @name ) + ' FROM WINDOWS WITH DEFAULT_DATABASE = [' + @defaultdb + ']''
	CREATE LOGIN ' + QUOTENAME( @name ) + ' FROM WINDOWS WITH DEFAULT_DATABASE = [' + @defaultdb + ']'
		end
		ELSE 
		begin -- SQL Server authentication
			-- obtain password and sid
			SET @PWD_varbinary = CAST( LOGINPROPERTY( @name, 'PasswordHash' ) AS varbinary (256) )
			EXEC sp_hexadecimal @PWD_varbinary, @PWD_string OUT
			EXEC sp_hexadecimal @SID_varbinary,@SID_string OUT
 
			-- obtain password policy state
			SELECT @is_policy_checked = CASE is_policy_checked WHEN 1 THEN 'ON' WHEN 0 THEN 'OFF' ELSE NULL END FROM sys.sql_logins WHERE name = @name
			SELECT @is_expiration_checked = CASE is_expiration_checked WHEN 1 THEN 'ON' WHEN 0 THEN 'OFF' ELSE NULL END FROM sys.sql_logins WHERE name = @name
 
				SET @tmpstr = 'IF not exists( SELECT * FROM sysLogins WHERE name = ''' + @name + ''' )
begin
	PRINT ''CREATE LOGIN ' + QUOTENAME( @name ) + ' WITH DEFAULT_DATABASE = [' + @defaultdb + ']''
	CREATE LOGIN ' + QUOTENAME( @name ) + ' WITH PASSWORD = ' + @PWD_string + ' HASHED, SID = ' + @SID_string + ', DEFAULT_DATABASE = [' + @defaultdb + ']'

			IF ( @is_policy_checked IS NOT NULL )
			begin
				SET @tmpstr = @tmpstr + ', CHECK_POLICY = ' + @is_policy_checked
			end
			IF ( @is_expiration_checked IS NOT NULL )
			begin
				SET @tmpstr = @tmpstr + ', CHECK_EXPIRATION = ' + @is_expiration_checked
			end
		end
		IF (@denylogin = 1)
		begin -- login is denied access
			SET @tmpstr = @tmpstr + '; DENY CONNECT SQL TO ' + QUOTENAME( @name )
		end
		ELSE IF (@hasaccess = 0)
		begin -- login exists but does not have access
			SET @tmpstr = @tmpstr + '; REVOKE CONNECT SQL TO ' + QUOTENAME( @name )
		end
		IF (@is_disabled = 1)
		begin -- login is disabled
			SET @tmpstr = @tmpstr + '; ALTER LOGIN ' + QUOTENAME( @name ) + ' DISABLE'
		end
		SET @tmpstr = @tmpstr + '
end'
		INSERT #output(Line)
		SELECT @tmpstr
	end

	FETCH NEXT FROM login_curs INTO @SID_varbinary, @name, @type, @is_disabled, @defaultdb, @hasaccess, @denylogin
end
CLOSE login_curs
DEALLOCATE login_curs

SELECT Line FROM #output Order by Row

RETURN 0
GO
--  ###########################################################################################################################################################
--  Logins, Users and Perms
go
IF exists( SELECT * FROM sys.objects Where name = 'ScriptLoginsUsersRolesAndPermissions' AND type = 'P' )
	DROP PROCEDURE ScriptLoginsUsersRolesAndPermissions
GO
--  ###########################################################################################################################################################
--  ###########################################################################################################################################################
go
CREATE Proc ScriptLoginsUsersRolesAndPermissions @Database varchar(50) = null, @Logins bit = 0, @Users bit = 0, @Permissions bit = 0
AS
Declare @sql varchar(max) 

--  ###################################################################################################
IF @Logins = 1  --  Script all logins
	EXEC sp_help_revlogin

--  ###################################################################################################
IF @Users = 1
begin
	--  Users for each individual database together with the roles they belong to.
	declare @dbName varchar(50),@User varchar(50)

	declare mc cursor for
	select name from sys.databases
	Where name not in ('master','tempdb','model','SystemInfo')
	  and ( name = @Database OR @Database is null )

	SET NOCOUNT ON

	open mc
	fetch next from mc into @dbName
	while @@fetch_status = 0
	begin
		SET @Sql = 'USE [' + @dbName + '];
	SELECT ''USE ' + REPLACE(@dbName, '_Copy','') + ';
	GO''
	SELECT ''IF not exists ( SELECT * FROM sys.database_principals WHERE name = '''''' + name + '''''' )
	begin
		PRINT ''''CREATE USER ['' + name + ''] FOR LOGIN ['' + name + '']''''
		CREATE USER ['' + name + ''] FOR LOGIN ['' + name + '']
	end'' 
	FROM sys.database_principals WHERE type != ''R'' AND name not in (''dbo'',''sys'',''guest'') and name not like ''##%''

	select ''IF not exists( select u.name Principal, r.name Role
		from sys.database_principals u
		join sys.database_role_members m on m.member_principal_id = u.principal_id
		join sys.database_principals r on m.role_principal_id = r.principal_id
		where u.name = '''''' + u.name + '''''' AND r.name = '''''' + r.name + '''''' )
	begin
		PRINT ''''ALTER ROLE ['' + r.name + ''] ADD MEMBER ['' + u.name + '']''''
		ALTER ROLE ['' + r.name + ''] ADD MEMBER ['' + u.name + '']
	end''
	--  select u.name, r.name
	from sys.database_principals u
	join sys.database_role_members m on m.member_principal_id = u.principal_id
	join sys.database_principals r on m.role_principal_id = r.principal_id
	where u.type_desc <> ''DATABASE_ROLE''
	AND u.name not in (''dbo'',''guest'',''INFORMATION_SCHEMA'',''sys'')
	-----------------------------------------------------------------------------
	'

		--Print @sql
		EXEC(@Sql)

		fetch next from mc into @dbName
	end
	close mc
	deallocate mc
end
--  ###################################################################################################
IF @Permissions = 1
begin
	--  Permissions
	--  declare @dbName varchar(50),@User varchar(50) , @sql varchar(5000) 

	declare mc cursor for
	select name from sys.databases
	Where name not in ('master','tempdb','model','msdb','SystemInfo')
	  and ( name = @Database OR @Database is null )

	SET NOCOUNT ON

	open mc
	fetch next from mc into @dbName
	while @@fetch_status = 0
	begin
		SET @Sql = 'USE [' + @dbName + '];
	SELECT ''USE ' + REPLACE(@dbName, '_Copy','') + ';
	GO''
	SELECT ''IF not exists( select *
		from sys.objects AS obj
		JOIN sys.database_permissions AS prmssn ON prmssn.major_id=obj.object_id AND prmssn.minor_id=0 AND prmssn.class=1
		JOIN sys.database_principals AS grantee_principal ON grantee_principal.principal_id = prmssn.grantee_principal_id
		WHERE prmssn.minor_id = 0 
			AND prmssn.State_desc = '''''' + prmssn.State_desc + ''''''
			AND permission_name = '''''' + permission_name + ''''''
			AND obj.name = '''''' + obj.Name COLLATE LATIN1_GENERAL_CI_AS + ''''''
			AND grantee_principal.name = '''''' + grantee_principal.name + ''''''
		)
	begin
		PRINT '''''' + prmssn.State_desc + '' '' + permission_name + '' ON ['' + obj.Name COLLATE LATIN1_GENERAL_CI_AS + ''] TO ['' + grantee_principal.name + '']''''
		'' + prmssn.State_desc + '' '' + permission_name + '' ON ['' + obj.Name COLLATE LATIN1_GENERAL_CI_AS + ''] TO ['' + grantee_principal.name + '']
	end''
	from sys.objects AS obj
	JOIN sys.database_permissions AS prmssn ON prmssn.major_id=obj.object_id AND prmssn.minor_id=0 AND prmssn.class=1
	JOIN sys.database_principals AS grantee_principal ON grantee_principal.principal_id = prmssn.grantee_principal_id
	WHERE prmssn.minor_id = 0

	'
	--	Print @sql
		EXEC(@Sql)

		fetch next from mc into @dbName

	/*
	 select prmssn.State_desc, permission_name, obj.Name, grantee_principal.name
		from sys.objects AS obj
		JOIN sys.database_permissions AS prmssn ON prmssn.major_id=obj.object_id AND prmssn.minor_id=0 AND prmssn.class=1
		JOIN sys.database_principals AS grantee_principal ON grantee_principal.principal_id = prmssn.grantee_principal_id
		WHERE prmssn.minor_id = 0 
			AND prmssn.State_desc = 'GRANT'
			AND permission_name IN ( 'SELECT','EXECUTE')
			AND obj.name = 'SystemStatus'
			AND grantee_principal.name = '''''' + grantee_principal.name + ''''''*/
	end
	close mc
	deallocate mc
end
go
--  ###########################################################################################################################################################
GO
use master
go
IF exists( SELECT * FROM sys.objects o WHERE o.Name = 'sp_FixOrphanedUsersInAllDbs' AND o.Type = 'P' )
	DROP PROC sp_FixOrphanedUsersInAllDbs

IF exists( SELECT * FROM sys.objects o WHERE o.Name = 'sp_FixOrphanedUsers' AND o.Type = 'P' )
	DROP PROC sp_FixOrphanedUsers

GO
CREATE PROC [dbo].[sp_FixOrphanedUsers]
as

CREATE table #t(UserName varchar(50), UserSID varchar(150) )

INSERT #t
EXEC sp_change_users_login @Action='Report';

declare @Name varchar(50), @sql varchar(500) 
declare mc cursor for
Select UserName from #t Where UserName != 'dbo'
open mc

fetch next from mc into @Name
while @@fetch_status = 0
begin
	IF exists( SELECT * FROM sysLogins WHERE name = @Name )
	begin
		raiserror('Fixing login %s',10,1,@Name)
		SET @Sql = 'ALTER USER [' + @Name + '] WITH Login = [' + @Name + ']'

		--print @sql
		EXEC(@sql)
	end
	ELSE
	begin
		raiserror('Login [%s] does not exist.',10,1,@Name)
	end

	fetch next from mc into @Name
end
close mc
deallocate mc

DROP TABLE #T
GO
CREATE PROC [dbo].[sp_FixOrphanedUsersInAllDbs]
as

declare @Name varchar(50), @sql varchar(500) 
declare db cursor for
select name from sys.databases 
where name not in ('master','msdb','model','tempdb')
    and name not like '%[_]master'
    and name not like '%[_]msdb'
--

open db
fetch next from db into @Name
while @@fetch_status = 0
begin
	SET @sql = 'use ' + @Name + ';  EXEC sp_FixOrphanedUsers'
	Print @sql
	EXEC(@sql)

	fetch next from db into @Name
end
close db
deallocate db

go
--  ###################################################################################################
--  Stop and restart the services
Print ''
Print ''
Print '------------------------------------------'
Print 'Stopping and starting SQL Server services.'
Print ''
GO
EXEC xp_cmdshell 'net stop SQLSERVERAGENT && net stop MSSQLSERVER && net start MSSQLSERVER && net start SQLSERVERAGENT'
GO


