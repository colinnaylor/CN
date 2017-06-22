-- DR Procedure 
------------------------------------------------------------------------------------------------------------------------
  -- DR SQL Server Configuration Script
  -- Last edited by Colin Naylor 2015 12 17
------------------------------------------------------------------------------------------------------------------------

--########################################################################################################
--Step -- Check Access to the sql server -- sql

SELECT @@Servername

--########################################################################################################
--Step -- Check ability to write to s:\Apps -- cmd

echo Testing > s:\Apps\TestWrite.txt

--########################################################################################################
--Step -- Ensure Admin database exists. -- sql
IF not exists( SELECT * FROM sys.databases WHERE Name = 'Admin' )
begin
	RAISERROR('Admin database does not exist.',17,1)
end

--########################################################################################################
--Step -- Ensure RenamedDatabase table exists. -- sql
use master
go
IF not exists( select * from sys.objects where name = 'RenamedDatabase' and type = 'U' )
begin
	CREATE TABLE [dbo].[RenamedDatabase](
		[OldName] [varchar](100) NULL,
		[NewName] [varchar](100) NULL,
		[TestDay] [Date]
	)
end

--########################################################################################################
--Step -- Stop reporting services services -- sql
xp_cmdshell 'net stop "SQL Server Reporting Services (MSSQLSERVER)"'

--########################################################################################################
--Step -- Remove connections from adder_reportserver -- sql
--  For the adder_reportserver database, ensure that nothing is connected to it before running this script.
Declare @spid int, @sql varchar(500) 
Declare mc cursor for
SELECT Spid FROM master.dbo.cn_info where db like 'adder_repo%'
open mc
fetch next from mc into @spid
while @@fetch_status = 0
begin
	SET @sql = 'Kill ' + convert(varchar(50),@spid)

	EXEC(@sql)

	fetch next from mc into @spid
end
close mc
deallocate mc

--########################################################################################################
--Step -- Bring databases online. -- sql

DECLARE @restoreSql varchar(200)

DECLARE restoreDbs CURSOR FOR
SELECT 'RESTORE DATABASE ' + name + ' WITH RECOVERY' as restoreSql
FROM sys.databases 
WHERE ( state = 1 or is_in_standby = 1 )
    and name not like '%[_]master'
    and name not like '%[_]msdb'

OPEN restoreDbs

FETCH NEXT FROM restoreDbs INTO @restoreSql

WHILE @@FETCH_STATUS = 0
BEGIN
	PRINT @restoreSql
	EXEC (@restoreSql)
	FETCH NEXT FROM restoreDbs INTO @restoreSql	
END

CLOSE restoreDbs
DEALLOCATE restoreDbs


--########################################################################################################
--Step -- Rename all xxx_copy dbs to xxx. -- sql

DECLARE @old_name varchar(200)
DECLARE @new_name varchar(200)

DELETE RenamedDatabase WHERE TestDay = convert(date,GetDate())
INSERT RenamedDatabase(OldName,TestDay)
SELECT name, convert(date,GetDate())
FROM sys.databases
WHERE name LIKE '%_copy'
--  SELECT * FROM RenamedDatabase  WHERE TestDay = convert(date,GetDate())

UPDATE RenamedDatabase SET NewName = left(OldName, len(OldName) - 5)  WHERE TestDay = convert(date,GetDate())

DECLARE renameDbs CURSOR FOR
SELECT OldName, NewName FROM RenamedDatabase
WHERE TestDay = convert(date,GetDate())

OPEN renameDbs

FETCH NEXT FROM renameDbs INTO @old_name, @new_name

WHILE @@FETCH_STATUS = 0
begin
--	select @old_name, @new_name
	EXEC sp_renamedb @old_name, @new_name

	FETCH NEXT FROM renameDbs INTO @old_name, @new_name
end
CLOSE renameDbs
DEALLOCATE renameDbs

--########################################################################################################
--Step -- Rename reporting services dbs. -- sql

DECLARE @old_name varchar(200)
DECLARE @new_name varchar(200)

DELETE RenamedDatabase WHERE TestDay = convert(date,GetDate()) and OldName LIKE 'Adder_report%'
INSERT RenamedDatabase(OldName,TestDay)
SELECT name, convert(date,GetDate())
FROM sys.databases
WHERE name LIKE 'Adder_report%'
--  SELECT * FROM RenamedDatabase  WHERE TestDay = convert(date,GetDate()) and OldName LIKE 'Adder_report%'

UPDATE RenamedDatabase SET NewName = replace(OldName, 'Adder_','') WHERE TestDay = convert(date,GetDate()) and OldName LIKE 'Adder_report%'

DECLARE renameDbs CURSOR FOR
SELECT OldName, NewName FROM RenamedDatabase
WHERE TestDay = convert(date,GetDate()) and OldName LIKE 'Adder_report%'

OPEN renameDbs

FETCH NEXT FROM renameDbs INTO @old_name, @new_name

WHILE @@FETCH_STATUS = 0
begin
--	select @old_name, @new_name
	EXEC sp_renamedb @old_name, @new_name

	FETCH NEXT FROM renameDbs INTO @old_name, @new_name
end
CLOSE renameDbs
DEALLOCATE renameDbs

--########################################################################################################
--Step -- Make sure some specific logins exist -- sql

IF NOT EXISTS (SELECT loginname FROM master.dbo.syslogins WHERE name='ReportServerAdmin')
begin
	CREATE LOGIN [ReportServerAdmin] WITH PASSWORD=N'Rsadmin;' , DEFAULT_DATABASE=[Adder_ReportServer]
end
GO

--########################################################################################################
--Step -- Fix orphaned users -- sql

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


--########################################################################################################
--Step --  Setup SUN database users -- sql

/* Master Database users */
USE [master]
GO

IF NOT EXISTS (SELECT loginname FROM master.dbo.syslogins WHERE name='DRsql2\SunSystemsClients')
BEGIN
	CREATE LOGIN [DRsql2\SunSystemsClients] FROM WINDOWS WITH DEFAULT_DATABASE=[SUNDB], DEFAULT_LANGUAGE=[us_english]
END
GO
IF NOT EXISTS (SELECT loginname FROM master.dbo.syslogins WHERE name='DRsql2\SunSystemServices')
BEGIN
	CREATE LOGIN [DRsql2\SunSystemServices] FROM WINDOWS WITH DEFAULT_DATABASE=[SUNDB], DEFAULT_LANGUAGE=[us_english]
END
GO

/* SUNDB users */
USE [sundb]
GO
IF not exists( SELECT * FROM sysusers WHERE name = 'DRsql2\SunSystemsClients' )
	CREATE USER [DRsql2\SunSystemsClients]
GO
IF not exists( SELECT * FROM sysusers WHERE name = 'DRsql2\SunSystemServices' )
	CREATE USER [DRsql2\SunSystemServices]
GO

ALTER USER [DRsql2\SunSystemsClients] WITH DEFAULT_SCHEMA=NULL
GO
ALTER ROLE [db_datareader] ADD MEMBER [DRsql2\SunSystemsClients]
GO
ALTER ROLE [db_datawriter] ADD MEMBER [DRsql2\SunSystemsClients]
GO
ALTER ROLE [db_ddladmin] ADD MEMBER [DRsql2\SunSystemsClients]
GO
ALTER USER [DRsql2\SunSystemServices] WITH DEFAULT_SCHEMA=NULL
GO
ALTER ROLE [db_owner] ADD MEMBER [DRsql2\SunSystemServices]
GO

/* sunsystemssecurity users */
USE [sunsystemssecurity]
GO

IF not exists( SELECT * FROM sysusers WHERE name = 'DRsql2\SunSystemsClients' )
	CREATE USER [DRsql2\SunSystemsClients]
GO
IF not exists( SELECT * FROM sysusers WHERE name = 'DRsql2\SunSystemServices' )
	CREATE USER [DRsql2\SunSystemServices]
GO

ALTER USER [DRsql2\SunSystemsClients] WITH DEFAULT_SCHEMA=NULL
GO
ALTER USER [DRsql2\SunSystemServices] WITH DEFAULT_SCHEMA=NULL
GO
ALTER ROLE [db_owner] ADD MEMBER [DRsql2\SunSystemServices]
GO

/* QA users */
USE [qa]
GO
IF not exists( SELECT * FROM sysusers WHERE name = 'DRsql2\SunSystemsClients' )
	CREATE USER [DRsql2\SunSystemsClients]
GO
IF not exists( SELECT * FROM sysusers WHERE name = 'DRsql2\SunSystemServices' )
	CREATE USER [DRsql2\SunSystemServices]
GO

ALTER USER [DRsql2\SunSystemsClients] WITH DEFAULT_SCHEMA=NULL
GO
ALTER ROLE [db_datareader] ADD MEMBER [DRsql2\SunSystemsClients]
GO
ALTER ROLE [db_datawriter] ADD MEMBER [DRsql2\SunSystemsClients]
GO
ALTER ROLE [db_ddladmin] ADD MEMBER [DRsql2\SunSystemsClients]
GO
ALTER USER [DRsql2\SunSystemServices] WITH DEFAULT_SCHEMA=NULL
GO
ALTER ROLE [db_owner] ADD MEMBER [DRsql2\SunSystemServices]
GO


--########################################################################################################
--Step --  Create a share on the E drive -- sql

--  This is so the VAR paths work
master.dbo.xp_cmdshell 'net share E=C:\'

--########################################################################################################
--Step --  Set location for Rm_Var database -- sql
-- To point the VAR database to the correct location
--  SELECT * FROM rm_var.dbo.LookupValue WHERE LOOKUPNAME = 'Live Server'
use rm_var;
UPDATE LookupValue set value = 'DRsql2' WHERE LOOKUPNAME = 'Live Server'

UPDATE LookupValue SET Value = replace(Value,'Lynx','DRsql2') where Value like '\\lynx%'


--########################################################################################################
--Step --  Setup PubSub to be on DRFS3 -- sql

USE FMOP2;
UPDATE tblUserConfig SET Data = '172.26.64.58' WHERE [Application] = 'Message' and Component = 'Server'
--  PubSub should already be running on DR server as an administrator via a Windows scheduled job

--########################################################################################################
--Step --  Run Mage utility-- cmd

S:
cd \Dev\DR\Workstation\Mage
"Mage Install Location Mover.exe" d=S:\Dev\Publish

--########################################################################################################
--Step --  Add users to specific databases -- sql

USE Boss2000
GO
IF not exists( SELECT * FROM sysusers WHERE Name = 'RiskUser' )
begin
	CREATE USER [RiskUser] FOR LOGIN [RiskUser]
end
GO
USE Fmop2
GO
IF not exists( SELECT * FROM sysusers WHERE Name = 'RiskUser' )
begin
	CREATE USER [RiskUser] FOR LOGIN [RiskUser]
end
GO
USE BossReport
GO
IF not exists( SELECT * FROM sysusers WHERE Name = 'RiskUser' )
begin
	CREATE USER [RiskUser] FOR LOGIN [RiskUser]
end
GO
USE [msdb]
GO
--########################################################################################################
--Step --  Add users to database roles -- sql
USE [msdb]
GO
ALTER ROLE [DatabaseMailUserRole] ADD MEMBER [MPUK\neil]
ALTER ROLE [DatabaseMailUserRole] ADD MEMBER [MPUK\rhys]
ALTER ROLE [DatabaseMailUserRole] ADD MEMBER [MPUK\emmaj]
ALTER ROLE [DatabaseMailUserRole] ADD MEMBER [MPUK\trusha]
GO

--########################################################################################################
--Step --  Modify the Synonym in Boss2000 -- sql

Use Boss2000
GO
DROP SYNONYM [dbo].[MMATrades]
CREATE SYNONYM [dbo].[MMATrades] FOR [FMOP2].[dbo].[TradeView4]

--########################################################################################################
--Step --  Remove references to linked server BOSS -- cmd

RemoveLinkedServerCalls %SqlServerName%, Boss

--########################################################################################################
--Step --  Remove references to linked server LYNX -- cmd

RemoveLinkedServerCalls %SqlServerName%, Lynx

--########################################################################################################
--Step --  Remove references to linked server MMA -- cmd

RemoveLinkedServerCalls %SqlServerName%, MMA

