------------------------------------------------------------------------------------------------------------------------
-- DR SQL Server Configuration Script
-- Last edited by Colin Naylor 2012 10 31
------------------------------------------------------------------------------------------------------------------------

/*

INSTRUCTIONS

IF YOU'RE USING Koala:-

JUST FIRE AND FORGET.

OTHERWISE:-

FIND/REPLACE *ALL* REFERENCES TO Koala TO YOUR DATABASE IN S:\DEV\DOCS\DR\*.* BEFORE USING *ANYTHING*.
FIND/REPLACE Koala WITH THE DATABASE YOU'RE USING.

*/

-- RESTORE DATABASES ---------------------------------------------------------------------------------------------------
--  For the adder_reportserver database, ensure that nothing is connected to it before running this script.
--  SELECT * FROM cn_info where db like 'adder_repo%'
--  kill 53

-- HA 12 nov 2010, loop through all the dbs that are not online and bring them up.
DECLARE @restoreSql varchar(200)

DECLARE restoreDbs CURSOR FOR
SELECT 'RESTORE DATABASE ' + name + ' WITH RECOVERY' as restoreSql
FROM sys.databases 
WHERE state = 1 or is_in_standby = 1

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


GO

-- LS 2011-06-22: Rename all xxx_copy dbs to xxx.

DECLARE @name varchar(200)
DECLARE @new_name varchar(200)

DECLARE renameDbs CURSOR FOR
SELECT name
FROM sys.databases
WHERE name LIKE '%_copy'

OPEN renameDbs

FETCH NEXT FROM renameDbs INTO @name

WHILE @@FETCH_STATUS = 0
begin
	SET @new_name = left(@name, len(@name) - 5)
	
--	select @name, @new_name
	EXEC sp_renamedb @name, @new_name
	FETCH NEXT FROM renameDbs INTO @name
end
CLOSE renameDbs
DEALLOCATE renameDbs

GO

-- /RESTORE DATABASES --------------------------------------------------------------------------------------------------

-- LOCAL GROUPS --------------------------------------------------------------------------------------------------------

-- Note: this is for legacy purposes *only*. We now use domain controller groups. It is likely this is unnecessary,
-- however we include them just in case some muppet-like bit of code somewhere relies on them.

-- Keep in mind that if permissioning errors occur in a DR scenario it is acceptable to simply grant a DB role to the
-- user as time will be a factor, and security less so.

USE Master
GO

--  Note that these command will fail if the Group already exists. This is OK.
EXEC xp_cmdshell 'NET LOCALGROUP lgrpBossBackOffice /COMMENT:"BOSS back office staff" /ADD'
EXEC xp_cmdshell 'NET LOCALGROUP lgrpBossBackOfficeAdmin /COMMENT:"BOSS back office Admin staff" /ADD'
EXEC xp_cmdshell 'NET LOCALGROUP lgrpBossProductControl /COMMENT:"Boss product control staff" /ADD'
EXEC xp_cmdshell 'NET LOCALGROUP lgrpBossCompliance /COMMENT:"Boss Compliance staff" /ADD'
EXEC xp_cmdshell 'NET LOCALGROUP lgrpBossAccounts /COMMENT:"Boss accounts staff" /ADD'
EXEC xp_cmdshell 'NET LOCALGROUP lgrpBossSystem /COMMENT:"Boss Administrators" /ADD'
EXEC xp_cmdshell 'NET LOCALGROUP lgrpBossReadOnly /COMMENT:"Read only access to boss data" /ADD'
EXEC xp_cmdshell 'NET LOCALGROUP lgrpBossExtractors /COMMENT:"Boss Extractors" /ADD'
EXEC xp_cmdshell 'NET LOCALGROUP lgrpBossOwner /COMMENT:"Boss Owner" /ADD'
EXEC xp_cmdshell 'NET LOCALGROUP lgrpBossTest /COMMENT:"Boss Owner" /ADD'

GO

-- /LOCAL GROUPS -------------------------------------------------------------------------------------------------------

-- SCRIPT SP_U ---------------------------------------------------------------------------------------------------------

USE Master
GO

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[dbo].[cn_info]') AND OBJECTPROPERTY(id, N'IsView') = 1)
   DROP VIEW [dbo].[cn_info]
GO

CREATE VIEW cn_info
AS
        SELECT
                CASE WHEN spid = @@Spid THEN '(' + CONVERT(VARCHAR(4),spid) + ')' ELSE CONVERT(VARCHAR(4),spid) END Spid,
                CONVERT(VARCHAR(4),blocked) Blk, 
                open_tran,
                CONVERT(VARCHAR(10),db_name(p.dbid)) DB ,
                waittime,
                lastwaittype,
                Cmd , 
                CONVERT(VARCHAR(19), last_batch,21) 'Last batch',
                CONVERT(VARCHAR(16), hostname) Hostname , 
                CONVERT(VARCHAR(12), nt_username) 'NT Username', 
                CONVERT(VARCHAR(25), program_name) 'Program Name' ,
                CONVERT(VARCHAR(10), loginame) Loginname ,
                CONVERT(VARCHAR(19), login_time,21) 'Login time',
                cpu,
                memusage
        FROM sysprocesses p

GO

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[dbo].[sp_u]') AND OBJECTPROPERTY(id, N'IsProcedure') = 1)
   DROP PROCEDURE [dbo].[sp_u]
GO

CREATE PROC sp_u
AS
        SELECT * FROM cn_info
        WHERE cmd NOT IN ('AWAITING COMMAND','TASK MANAGER','SIGNAL HANDLER','LOCK MONITOR','LAZY WRITER','LOG WRITER','CHECKPOINT SLEEP')
              AND waittime = 0

SELECT * FROM cn_info
       WHERE cmd NOT IN ('AWAITING COMMAND','TASK MANAGER','SIGNAL HANDLER','LOCK MONITOR','LAZY WRITER','LOG WRITER','CHECKPOINT SLEEP')
             AND waittime <> 0

SELECT * FROM cn_info
       WHERE cmd IN ('AWAITING COMMAND','TASK MANAGER','SIGNAL HANDLER','LOCK MONITOR','LAZY WRITER','LOG WRITER','CHECKPOINT SLEEP')

GO

-- /SCRIPT SP_U --------------------------------------------------------------------------------------------------------

-- FIXUP BOSS2000 USERS ------------------------------------------------------------------------------------------------

USE Boss2000
GO

-- USERs in DB
-- Individual logins to override normal system setup. Only required if a user is having trouble accessing Boss
--  EXEC sp_grantlogin 'MPUK\Emmaj'
--  EXEC sp_grantdbaccess 'MPUK\Emmaj','Emmaj'

-- List orphaned users. This shows the users that need fixing in the database.
EXEC sp_change_users_login @Action='Report';
GO

-- Fix these orphaned users.

-- LS 2009-01-28 - Auto-fix up users using sp_change_users_login with a dummy strong password,
--                 then use ALTER LOGIN to set the real password. We do this because problems
--                 have arisen where the DR domain controller has an overly strong password
--                 policy preventing the use of short/simple passwords. We work around this by
--                 specifying CHECK_POLICY = OFF in a subsequent ALTER LOGIN statement.
--                 Unfortunately one cannot achieve this using sp_change_users_login directly,
--                 hence this work-around.

-- The result of this sql should be something like the following:
-- The row for user 'BOSSviewer' will be fixed by updating its login link to a login already in existence.
-- The number of orphaned users fixed by updating users was 1.
-- The number of orphaned users fixed by adding new logins and then updating users was 0.

DECLARE @DummyComplexPassword varchar(50)
SELECT  @DummyComplexPassword = 'FOObarzajkshd8we0123456789abc:;-@'
EXEC sp_change_users_login @Action = 'Auto_Fix', @UserNamePattern = 'adapter', @LoginName = NULL,
     @Password = @DummyComplexPassword
ALTER LOGIN [adapter] WITH PASSWORD = 'adapter', CHECK_POLICY = OFF

GO

-- See comment at declaration of @DummyComplexPassword for explanation.
DECLARE @DummyComplexPassword varchar(50)
SELECT  @DummyComplexPassword = 'FOObarzajkshd8we0123456789abc:;-@'
EXEC sp_change_users_login @Action = 'Auto_Fix', @UserNamePattern = 'LinkedUser', @LoginName = NULL,
     @Password = @DummyComplexPassword
ALTER LOGIN [LinkedUser] WITH PASSWORD = 'linkacce55', CHECK_POLICY = OFF

GO

-- See comment at declaration of @DummyComplexPassword for explanation.
DECLARE @DummyComplexPassword varchar(50)
SELECT  @DummyComplexPassword = 'FOObarzajkshd8we0123456789abc:;-@'
EXEC sp_change_users_login @Action = 'Auto_Fix', @UserNamePattern = 'BOSSviewer', @LoginName = NULL,
     @Password = @DummyComplexPassword
ALTER LOGIN [BOSSviewer] WITH PASSWORD = 'bossv', CHECK_POLICY = OFF

GO

-- See comment at declaration of @DummyComplexPassword for explanation.
DECLARE @DummyComplexPassword varchar(50)
SELECT  @DummyComplexPassword = 'FOObarzajkshd8we0123456789abc:;-@'
EXEC sp_change_users_login @Action = 'Auto_Fix', @UserNamePattern = 'MsdbAuthorisedUser', @LoginName = NULL,
     @Password = @DummyComplexPassword
ALTER LOGIN [MsdbAuthorisedUser] WITH PASSWORD = 'msdbacce55', CHECK_POLICY = OFF

GO

-- See comment at declaration of @DummyComplexPassword for explanation.
DECLARE @DummyComplexPassword varchar(50)
SELECT  @DummyComplexPassword = 'FOObarzajkshd8we0123456789abc:;-@'
EXEC sp_change_users_login @Action = 'Auto_Fix', @UserNamePattern = 'USdailyPLreader', @LoginName = NULL,
     @Password = @DummyComplexPassword
ALTER LOGIN [USdailyPLreader] WITH PASSWORD = '2TimeSquare', CHECK_POLICY = OFF

GO

DECLARE @DummyComplexPassword varchar(50)
SELECT  @DummyComplexPassword = 'FOObarzajkshd8we0123456789abc:;-@'
EXEC sp_change_users_login @Action = 'Auto_Fix', @UserNamePattern = 'ApexReader', @LoginName = NULL,
     @Password = @DummyComplexPassword
ALTER LOGIN [USdailyPLreader] WITH PASSWORD = ' DefaultPassword', CHECK_POLICY = OFF

GO

DECLARE @DummyComplexPassword varchar(50)
SELECT  @DummyComplexPassword = 'FOObarzajkshd8we0123456789abc:;-@'
EXEC sp_change_users_login @Action = 'Auto_Fix', @UserNamePattern = 'KONDORXRT', @LoginName = NULL,
     @Password = @DummyComplexPassword
ALTER LOGIN [USdailyPLreader] WITH PASSWORD = 'DefaultPassword', CHECK_POLICY = OFF

GO

DECLARE @DummyComplexPassword varchar(50)
SELECT  @DummyComplexPassword = 'FOObarzajkshd8we0123456789abc:;-@'
EXEC sp_change_users_login @Action = 'Auto_Fix', @UserNamePattern = 'ReportViewer', @LoginName = NULL,
     @Password = @DummyComplexPassword
ALTER LOGIN [USdailyPLreader] WITH PASSWORD = 'DefaultPassword', CHECK_POLICY = OFF

GO

DECLARE @DummyComplexPassword varchar(50)
SELECT  @DummyComplexPassword = 'FOObarzajkshd8we0123456789abc:;-@'
EXEC sp_change_users_login @Action = 'Auto_Fix', @UserNamePattern = 'WebLogin', @LoginName = NULL,
     @Password = @DummyComplexPassword
ALTER LOGIN [USdailyPLreader] WITH PASSWORD = 'DefaultPassword', CHECK_POLICY = OFF

GO

-- The following command should now show no results
EXEC sp_change_users_login @Action='Report';

GO

-- /FIXUP BOSS2000 USERS -----------------------------------------------------------------------------------------------

-- ADJUST BOSS2000 PERMISSIONS -----------------------------------------------------------------------------------------

-- Adjust permission for BOSS, grant all permissions on all objects
-- PERMISSIONS ON ALL OBJECTS WITHIN DB

DECLARE MC CURSOR FOR
        SELECT NAME FROM SYSOBJECTS WHERE TYPE IN ('U','V') AND STATUS >= 0 AND Name NOT LIKE '%vuMilan%'

DECLARE @NAME VARCHAR(80), @sql VARCHAR(8000)

OPEN MC

     FETCH NEXT FROM MC INTO @NAME

     WHILE @@FETCH_STATUS = 0
           BEGIN
           	SELECT @sql = 'GRANT SELECT, UPDATE, INSERT, DELETE ON [' + @name + '] to public'	

                PRINT @sql
	        EXEC(@sql)

	        FETCH NEXT FROM MC INTO @NAME
     END

CLOSE mc
DEALLOCATE mc

GO

DECLARE MC CURSOR FOR
        SELECT NAME FROM SYSOBJECTS WHERE TYPE IN ('P') AND STATUS >= 0

DECLARE @NAME VARCHAR(80), @sql VARCHAR(8000)

OPEN MC

     FETCH NEXT FROM MC INTO @NAME

     WHILE @@FETCH_STATUS = 0
           BEGIN
           	SELECT @sql = 'GRANT EXEC ON [' + @name + '] to public'	

                PRINT @sql
                EXEC(@sql)

                FETCH NEXT FROM MC INTO @NAME
           END

CLOSE mc
DEALLOCATE mc

GO

-- /ADJUST BOSS2000 PERMISSIONS ----------------------------------------------------------------------------------------

-- FIXUP BOSSREPORT USERS ----------------------------------------------------------------------------------------------

USE BossReport
GO

-- List orphaned users. This shows the users that need fixing in the database.
EXEC sp_change_users_login @Action='Report';

-- Fix these orphaned users. 

-- See comment at declaration of @DummyComplexPassword for explanation.
DECLARE @DummyComplexPassword varchar(50)
SELECT  @DummyComplexPassword = 'FOObarzajkshd8we0123456789abc:;-@'
EXEC sp_change_users_login @Action = 'Auto_Fix', @UserNamePattern = 'adapter', @LoginName = NULL,
     @Password = @DummyComplexPassword
ALTER LOGIN [adapter] WITH PASSWORD = 'adapter', CHECK_POLICY = OFF

GO


-- See comment at declaration of @DummyComplexPassword for explanation.
DECLARE @DummyComplexPassword varchar(50)
SELECT  @DummyComplexPassword = 'FOObarzajkshd8we0123456789abc:;-@'
EXEC sp_change_users_login @Action = 'Auto_Fix', @UserNamePattern = 'BOSSviewer', @LoginName = NULL,
     @Password = @DummyComplexPassword
ALTER LOGIN [BOSSviewer] WITH PASSWORD = 'bossv', CHECK_POLICY = OFF

GO

-- See comment at declaration of @DummyComplexPassword for explanation.
DECLARE @DummyComplexPassword varchar(50)
SELECT  @DummyComplexPassword = 'FOObarzajkshd8we0123456789abc:;-@'
EXEC sp_change_users_login @Action = 'Auto_Fix', @UserNamePattern = 'LinkedUser', @LoginName = NULL,
     @Password = @DummyComplexPassword
ALTER LOGIN [USdailyPLreader] WITH PASSWORD = 'linkacce55', CHECK_POLICY = OFF

GO

-- See comment at declaration of @DummyComplexPassword for explanation.
DECLARE @DummyComplexPassword varchar(50)
SELECT  @DummyComplexPassword = 'FOObarzajkshd8we0123456789abc:;-@'
EXEC sp_change_users_login @Action = 'Auto_Fix', @UserNamePattern = 'ReportViewer', @LoginName = NULL,
     @Password = @DummyComplexPassword
ALTER LOGIN [USdailyPLreader] WITH PASSWORD = 'reeport1', CHECK_POLICY = OFF

GO

-- See comment at declaration of @DummyComplexPassword for explanation.
DECLARE @DummyComplexPassword varchar(50)
SELECT  @DummyComplexPassword = 'FOObarzajkshd8we0123456789abc:;-@'
EXEC sp_change_users_login @Action = 'Auto_Fix', @UserNamePattern = 'USdailyPLreader', @LoginName = NULL,
     @Password = @DummyComplexPassword
ALTER LOGIN [USdailyPLreader] WITH PASSWORD = '2TimeSquare', CHECK_POLICY = OFF

GO
--	EXEC sp_change_users_login @Action='Report';

GO
-- /FIXUP BOSSREPORT USERS ---------------------------------------------------------------------------------------------

-- ADJUST BOSSREPORT PERMISSIONS ---------------------------------------------------------------------------------------

-- PERMISSIONS ON ALL OBJECTS WITHIN DB
DECLARE MC CURSOR FOR
SELECT NAME FROM SYSOBJECTS WHERE TYPE IN ('U','V') AND STATUS >= 0 AND Name NOT LIKE '%vuMilan%'

DECLARE @NAME VARCHAR(80), @sql VARCHAR(8000)

OPEN MC

FETCH NEXT FROM MC INTO @NAME

WHILE @@FETCH_STATUS = 0
BEGIN
	SELECT @sql = 'GRANT SELECT, UPDATE, INSERT, DELETE ON [' + @name + '] to public'	

	PRINT @sql
	EXEC(@sql)

	FETCH NEXT FROM MC INTO @NAME
END

CLOSE mc
DEALLOCATE mc
GO
DECLARE MC CURSOR FOR
SELECT NAME FROM SYSOBJECTS WHERE TYPE IN ('P') AND STATUS >= 0

DECLARE @NAME VARCHAR(80), @sql VARCHAR(8000)

OPEN MC

FETCH NEXT FROM MC INTO @NAME

WHILE @@FETCH_STATUS = 0
BEGIN
	SELECT @sql = 'GRANT EXEC ON [' + @name + '] to public'	

	PRINT @sql
	EXEC(@sql)

	FETCH NEXT FROM MC INTO @NAME
END

CLOSE mc
DEALLOCATE mc

GO

-- /ADJUST BOSSREPORT PERMISSIONS --------------------------------------------------------------------------------------

-- FIXUP BOSSAUDIT USERS -----------------------------------------------------------------------------------------------

USE BossAudit				
GO
EXEC sp_change_users_login @Action='Report';
-- fix these orphaned users. 

-- See comment at declaration of @DummyComplexPassword for explanation.
DECLARE @DummyComplexPassword varchar(50)
SELECT  @DummyComplexPassword = 'FOObarzajkshd8we0123456789abc:;-@'
EXEC sp_change_users_login @Action = 'Auto_Fix', @UserNamePattern = 'ApexReader', @LoginName = NULL,
     @Password = @DummyComplexPassword
ALTER LOGIN ApexReader WITH PASSWORD = 'DefaultPassword', CHECK_POLICY = OFF

GO
DECLARE @DummyComplexPassword varchar(50)
SELECT  @DummyComplexPassword = 'FOObarzajkshd8we0123456789abc:;-@'
EXEC sp_change_users_login @Action = 'Auto_Fix', @UserNamePattern = 'BOSSviewer', @LoginName = NULL,
     @Password = @DummyComplexPassword
ALTER LOGIN [BOSSviewer] WITH PASSWORD = 'bossv', CHECK_POLICY = OFF

GO
DECLARE @DummyComplexPassword varchar(50)
SELECT  @DummyComplexPassword = 'FOObarzajkshd8we0123456789abc:;-@'
EXEC sp_change_users_login @Action = 'Auto_Fix', @UserNamePattern = 'KondorXRT', @LoginName = NULL,
     @Password = @DummyComplexPassword
ALTER LOGIN KondorXRT WITH PASSWORD = 'DefaultPassword', CHECK_POLICY = OFF

GO


-- See comment at declaration of @DummyComplexPassword for explanation.
DECLARE @DummyComplexPassword varchar(50)
SELECT  @DummyComplexPassword = 'FOObarzajkshd8we0123456789abc:;-@'
EXEC sp_change_users_login @Action = 'Auto_Fix', @UserNamePattern = 'LinkedUser', @LoginName = NULL,
     @Password = @DummyComplexPassword
ALTER LOGIN [LinkedUser] WITH PASSWORD = 'linkacce55', CHECK_POLICY = OFF

GO
DECLARE @DummyComplexPassword varchar(50)
SELECT  @DummyComplexPassword = 'FOObarzajkshd8we0123456789abc:;-@'
EXEC sp_change_users_login @Action = 'Auto_Fix', @UserNamePattern = 'ReportViewer', @LoginName = NULL,
     @Password = @DummyComplexPassword
ALTER LOGIN ReportViewer WITH PASSWORD = 'reeport1', CHECK_POLICY = OFF

GO
DECLARE @DummyComplexPassword varchar(50)
SELECT  @DummyComplexPassword = 'FOObarzajkshd8we0123456789abc:;-@'
EXEC sp_change_users_login @Action = 'Auto_Fix', @UserNamePattern = 'WebLogin', @LoginName = NULL,
     @Password = @DummyComplexPassword
ALTER LOGIN WebLogin WITH PASSWORD = 'DefaultPassword', CHECK_POLICY = OFF

GO
--  EXEC sp_change_users_login @Action='Report';

-- /FIXUP BOSSAUDIT USERS ----------------------------------------------------------------------------------------------

-- ADJUST BOSSAUDIT PERMISSIONS ----------------------------------------------------------------------------------------

-- PERMISSIONS ON ALL OBJECTS WITHIN DB
DECLARE MC CURSOR FOR
SELECT NAME FROM SYSOBJECTS WHERE TYPE IN ('U','V') AND STATUS >= 0 AND Name NOT LIKE '%vuMilan%'

DECLARE @NAME VARCHAR(80), @sql VARCHAR(8000)

OPEN MC

FETCH NEXT FROM MC INTO @NAME

WHILE @@FETCH_STATUS = 0
BEGIN
	SELECT @sql = 'GRANT SELECT, UPDATE, INSERT, DELETE ON [' + @name + '] to public'	

	PRINT @sql
	EXEC(@sql)

	FETCH NEXT FROM MC INTO @NAME
END

CLOSE mc
DEALLOCATE mc
GO
DECLARE MC CURSOR FOR
SELECT NAME FROM SYSOBJECTS WHERE TYPE IN ('P') AND STATUS >= 0

DECLARE @NAME VARCHAR(80), @sql VARCHAR(8000)

OPEN MC

FETCH NEXT FROM MC INTO @NAME

WHILE @@FETCH_STATUS = 0
BEGIN
	SELECT @sql = 'GRANT EXEC ON [' + @name + '] to public'	

	PRINT @sql
	EXEC(@sql)

	FETCH NEXT FROM MC INTO @NAME
END

CLOSE mc
DEALLOCATE mc

GO

-- /ADJUST BOSSAUDIT PERMISSIONS ---------------------------------------------------------------------------------------

-- FIXUP BOIDATA USERS -------------------------------------------------------------------------------------------------

USE BOIData
GO
EXEC sp_change_users_login @Action='Report';
-- fix these orphaned users. 
--  
-- See comment at declaration of @DummyComplexPassword for explanation.
DECLARE @DummyComplexPassword varchar(50)
SELECT  @DummyComplexPassword = 'FOObarzajkshd8we0123456789abc:;-@'
EXEC sp_change_users_login @Action = 'Auto_Fix', @UserNamePattern = 'adapter', @LoginName = NULL,
     @Password = @DummyComplexPassword
ALTER LOGIN [adapter] WITH PASSWORD = 'adapter', CHECK_POLICY = OFF

GO

-- /FIXUP BOIDATA USERS ------------------------------------------------------------------------------------------------

-- ADJUST BOIDATA PERMISSIONS ------------------------------------------------------------------------------------------

-- PERMISSIONS ON ALL OBJECTS WITHIN DB
DECLARE MC CURSOR FOR
SELECT NAME FROM SYSOBJECTS WHERE TYPE IN ('U','V') AND STATUS >= 0 AND Name NOT LIKE '%vuMilan%'

DECLARE @NAME VARCHAR(80), @sql VARCHAR(8000)

OPEN MC

FETCH NEXT FROM MC INTO @NAME

WHILE @@FETCH_STATUS = 0
BEGIN
	SELECT @sql = 'GRANT SELECT, UPDATE, INSERT, DELETE ON [' + @name + '] to public'	

	PRINT @sql
	EXEC(@sql)

	FETCH NEXT FROM MC INTO @NAME
END

CLOSE mc
DEALLOCATE mc
GO
DECLARE MC CURSOR FOR
SELECT NAME FROM SYSOBJECTS WHERE TYPE IN ('P') AND STATUS >= 0

DECLARE @NAME VARCHAR(80), @sql VARCHAR(8000)

OPEN MC

FETCH NEXT FROM MC INTO @NAME

WHILE @@FETCH_STATUS = 0
BEGIN
	SELECT @sql = 'GRANT EXEC ON [' + @name + '] to public'	

	PRINT @sql
	EXEC(@sql)

	FETCH NEXT FROM MC INTO @NAME
END

CLOSE mc
DEALLOCATE mc

GO

-- /ADJUST BOIDATA PERMISSIONS -----------------------------------------------------------------------------------------

-- FIXUP BOSSRECONCILER USERS ------------------------------------------------------------------------------------------

USE BossReconciler
GO
EXEC sp_change_users_login @Action='Report';
-- fix these orphaned users. 

-- See comment at declaration of @DummyComplexPassword for explanation.
DECLARE @DummyComplexPassword varchar(50)
SELECT  @DummyComplexPassword = 'FOObarzajkshd8we0123456789abc:;-@'
EXEC sp_change_users_login @Action = 'Auto_Fix', @UserNamePattern = 'BOSSviewer', @LoginName = NULL,
     @Password = @DummyComplexPassword
ALTER LOGIN [BOSSviewer] WITH PASSWORD = 'bossv', CHECK_POLICY = OFF

GO

DECLARE @DummyComplexPassword varchar(50)
SELECT  @DummyComplexPassword = 'FOObarzajkshd8we0123456789abc:;-@'
EXEC sp_change_users_login @Action = 'Auto_Fix', @UserNamePattern = 'LinkedUser', @LoginName = NULL,
     @Password = @DummyComplexPassword
ALTER LOGIN LinkedUser WITH PASSWORD = 'linkacce55', CHECK_POLICY = OFF

GO

-- /FIXUP BOSSRECONCILER USERS -----------------------------------------------------------------------------------------

-- ADJUST BOSSRECONCILER PERMISSIONS -----------------------------------------------------------------------------------

-- PERMISSIONS ON ALL OBJECTS WITHIN DB
DECLARE MC CURSOR FOR
SELECT NAME FROM SYSOBJECTS WHERE TYPE IN ('U','V') AND STATUS >= 0 AND Name NOT LIKE '%vuMilan%'

DECLARE @NAME VARCHAR(80), @sql VARCHAR(8000)

OPEN MC

FETCH NEXT FROM MC INTO @NAME

WHILE @@FETCH_STATUS = 0
BEGIN
	SELECT @sql = 'GRANT SELECT, UPDATE, INSERT, DELETE ON [' + @name + '] to public'	

	PRINT @sql
	EXEC(@sql)

	FETCH NEXT FROM MC INTO @NAME
END

CLOSE mc
DEALLOCATE mc
GO
DECLARE MC CURSOR FOR
SELECT NAME FROM SYSOBJECTS WHERE TYPE IN ('P') AND STATUS >= 0

DECLARE @NAME VARCHAR(80), @sql VARCHAR(8000)

OPEN MC

FETCH NEXT FROM MC INTO @NAME

WHILE @@FETCH_STATUS = 0
BEGIN
	SELECT @sql = 'GRANT EXEC ON [' + @name + '] to public'	

	PRINT @sql
	EXEC(@sql)

	FETCH NEXT FROM MC INTO @NAME
END

CLOSE mc
DEALLOCATE mc

GO

-- /ADJUST BOSSRECONCILER PERMISSIONS ----------------------------------------------------------------------------------

-- FIXUP GERMANREPORTING USERS -----------------------------------------------------------------------------------------

use GermanReporting
GO
EXEC sp_change_users_login @Action='Report';
-- fix these orphaned users. 

-- See comment at declaration of @DummyComplexPassword for explanation.
DECLARE @DummyComplexPassword varchar(50)
SELECT  @DummyComplexPassword = 'FOObarzajkshd8we0123456789abc:;-@'
EXEC sp_change_users_login @Action = 'Auto_Fix', @UserNamePattern = 'adapter', @LoginName = NULL,
     @Password = @DummyComplexPassword
ALTER LOGIN [adapter] WITH PASSWORD = 'adapter', CHECK_POLICY = OFF

GO
DECLARE @DummyComplexPassword varchar(50)
SELECT  @DummyComplexPassword = 'FOObarzajkshd8we0123456789abc:;-@'
EXEC sp_change_users_login @Action = 'Auto_Fix', @UserNamePattern = 'ReportViewer', @LoginName = NULL,
     @Password = @DummyComplexPassword
ALTER LOGIN ReportViewer WITH PASSWORD = 'reeport1', CHECK_POLICY = OFF

GO

-- /FIXUP GERMANREPORTING USERS ----------------------------------------------------------------------------------------

-- ADJUST BOSSRECONCILER PERMISSIONS -----------------------------------------------------------------------------------

-- PERMISSIONS ON ALL OBJECTS WITHIN DB
DECLARE MC CURSOR FOR
SELECT NAME FROM SYSOBJECTS WHERE TYPE IN ('U','V') AND STATUS >= 0 AND Name NOT LIKE '%vuMilan%'

DECLARE @NAME VARCHAR(80), @sql VARCHAR(8000)

OPEN MC

FETCH NEXT FROM MC INTO @NAME

WHILE @@FETCH_STATUS = 0
BEGIN
	SELECT @sql = 'GRANT SELECT, UPDATE, INSERT, DELETE ON [' + @name + '] to public'	

	PRINT @sql
	EXEC(@sql)

	FETCH NEXT FROM MC INTO @NAME
END

CLOSE mc
DEALLOCATE mc
GO
DECLARE MC CURSOR FOR
SELECT NAME FROM SYSOBJECTS WHERE TYPE IN ('P') AND STATUS >= 0

DECLARE @NAME VARCHAR(80), @sql VARCHAR(8000)

OPEN MC

FETCH NEXT FROM MC INTO @NAME

WHILE @@FETCH_STATUS = 0
BEGIN
	SELECT @sql = 'GRANT EXEC ON [' + @name + '] to public'	

	PRINT @sql
	EXEC(@sql)

	FETCH NEXT FROM MC INTO @NAME
END

CLOSE mc
DEALLOCATE mc

GO

-- /ADJUST BOSSRECONCILER PERMISSIONS ----------------------------------------------------------------------------------

-- FIXUP GERMANREPORTING_MFE USERS -------------------------------------------------------------------------------------

USE GermanReporting_MFE
GO

EXEC sp_change_users_login @Action='Report';
-- fix these orphaned users. 
--
-- See comment at declaration of @DummyComplexPassword for explanation.
DECLARE @DummyComplexPassword varchar(50)
SELECT  @DummyComplexPassword = 'FOObarzajkshd8we0123456789abc:;-@'
EXEC sp_change_users_login @Action = 'Auto_Fix', @UserNamePattern = 'adapter', @LoginName = NULL,
     @Password = @DummyComplexPassword
ALTER LOGIN [adapter] WITH PASSWORD = 'adapter', CHECK_POLICY = OFF

GO

-- /FIXUP GERMANREPORTING_MFE USERS ------------------------------------------------------------------------------------

-- ADJUST GERMANREPORTING_MFE PERMISSIONS ------------------------------------------------------------------------------

-- PERMISSIONS ON ALL OBJECTS WITHIN DB
DECLARE MC CURSOR FOR
SELECT NAME FROM SYSOBJECTS WHERE TYPE IN ('U','V') AND STATUS >= 0 AND Name NOT LIKE '%vuMilan%'

DECLARE @NAME VARCHAR(80), @sql VARCHAR(8000)

OPEN MC

FETCH NEXT FROM MC INTO @NAME

WHILE @@FETCH_STATUS = 0
BEGIN
	SELECT @sql = 'GRANT SELECT, UPDATE, INSERT, DELETE ON [' + @name + '] to public'	

	PRINT @sql
	EXEC(@sql)

	FETCH NEXT FROM MC INTO @NAME
END

CLOSE mc
DEALLOCATE mc
GO
DECLARE MC CURSOR FOR
SELECT NAME FROM SYSOBJECTS WHERE TYPE IN ('P') AND STATUS >= 0

DECLARE @NAME VARCHAR(80), @sql VARCHAR(8000)

OPEN MC

FETCH NEXT FROM MC INTO @NAME

WHILE @@FETCH_STATUS = 0
BEGIN
	SELECT @sql = 'GRANT EXEC ON [' + @name + '] to public'	

	PRINT @sql
	EXEC(@sql)

	FETCH NEXT FROM MC INTO @NAME
END

CLOSE mc
DEALLOCATE mc

GO

-- /ADJUST GERMANREPORTING_MFE PERMISSIONS -----------------------------------------------------------------------------

-- ADJUST PRR PERMISSIONS ----------------------------------------------------------------------------------------------

-- PERMISSIONS ON ALL OBJECTS WITHIN DB
DECLARE MC CURSOR FOR
SELECT NAME FROM SYSOBJECTS WHERE TYPE IN ('U','V') AND STATUS >= 0 AND Name NOT LIKE '%vuMilan%'

DECLARE @NAME VARCHAR(80), @sql VARCHAR(8000)

OPEN MC

FETCH NEXT FROM MC INTO @NAME

WHILE @@FETCH_STATUS = 0
BEGIN
	SELECT @sql = 'GRANT SELECT, UPDATE, INSERT, DELETE ON [' + @name + '] to public'	

	PRINT @sql
	EXEC(@sql)

	FETCH NEXT FROM MC INTO @NAME
END

CLOSE mc
DEALLOCATE mc
GO
DECLARE MC CURSOR FOR
SELECT NAME FROM SYSOBJECTS WHERE TYPE IN ('P') AND STATUS >= 0

DECLARE @NAME VARCHAR(80), @sql VARCHAR(8000)

OPEN MC

FETCH NEXT FROM MC INTO @NAME

WHILE @@FETCH_STATUS = 0
BEGIN
	SELECT @sql = 'GRANT EXEC ON [' + @name + '] to public'	

	PRINT @sql
	EXEC(@sql)

	FETCH NEXT FROM MC INTO @NAME
END

CLOSE mc
DEALLOCATE mc

GO

-- /ADJUST PRR PERMISSIONS ---------------------------------------------------------------------------------------------

-- FIXUP RECONCILIATION USERS ------------------------------------------------------------------------------------------

USE Reconciliation
GO
EXEC sp_change_users_login @Action='Report';
-- fix these orphaned users. 

-- See comment at declaration of @DummyComplexPassword for explanation.
DECLARE @DummyComplexPassword varchar(50)
SELECT  @DummyComplexPassword = 'FOObarzajkshd8we0123456789abc:;-@'
EXEC sp_change_users_login @Action = 'Auto_Fix', @UserNamePattern = 'BOSSviewer', @LoginName = NULL,
     @Password = @DummyComplexPassword
ALTER LOGIN BOSSviewer WITH PASSWORD = 'bossv', CHECK_POLICY = OFF

GO
DECLARE @DummyComplexPassword varchar(50)
SELECT  @DummyComplexPassword = 'FOObarzajkshd8we0123456789abc:;-@'
EXEC sp_change_users_login @Action = 'Auto_Fix', @UserNamePattern = 'LinkedUser', @LoginName = NULL,
     @Password = @DummyComplexPassword
ALTER LOGIN LinkedUser WITH PASSWORD = 'linkacce55', CHECK_POLICY = OFF

GO

DECLARE @DummyComplexPassword varchar(50)
SELECT  @DummyComplexPassword = 'FOObarzajkshd8we0123456789abc:;-@'
EXEC sp_change_users_login @Action = 'Auto_Fix', @UserNamePattern = 'ReportViewer', @LoginName = NULL,
     @Password = @DummyComplexPassword
ALTER LOGIN ReportViewer WITH PASSWORD = 'reeport1', CHECK_POLICY = OFF

GO
DECLARE @DummyComplexPassword varchar(50)
SELECT  @DummyComplexPassword = 'FOObarzajkshd8we0123456789abc:;-@'
EXEC sp_change_users_login @Action = 'Auto_Fix', @UserNamePattern = 'WriteableUser', @LoginName = NULL,
     @Password = @DummyComplexPassword
ALTER LOGIN [WriteableUser] WITH PASSWORD = 'Wr1teAcce55', CHECK_POLICY = OFF

GO

-- /FIXUP RECONCILIATION USERS -----------------------------------------------------------------------------------------

-- ADJUST RECONCILIATION PERMISSIONS -----------------------------------------------------------------------------------

-- PERMISSIONS ON ALL OBJECTS WITHIN DB
DECLARE MC CURSOR FOR
SELECT NAME FROM SYSOBJECTS WHERE TYPE IN ('U','V') AND STATUS >= 0 AND Name NOT LIKE '%vuMilan%'

DECLARE @NAME VARCHAR(80), @sql VARCHAR(8000)

OPEN MC

FETCH NEXT FROM MC INTO @NAME

WHILE @@FETCH_STATUS = 0
BEGIN
	SELECT @sql = 'GRANT SELECT, UPDATE, INSERT, DELETE ON [' + @name + '] to public'	

	PRINT @sql
	EXEC(@sql)

	FETCH NEXT FROM MC INTO @NAME
END

CLOSE mc
DEALLOCATE mc
GO
DECLARE MC CURSOR FOR
SELECT NAME FROM SYSOBJECTS WHERE TYPE IN ('P') AND STATUS >= 0

DECLARE @NAME VARCHAR(80), @sql VARCHAR(8000)

OPEN MC

FETCH NEXT FROM MC INTO @NAME

WHILE @@FETCH_STATUS = 0
BEGIN
	SELECT @sql = 'GRANT EXEC ON [' + @name + '] to public'	

	PRINT @sql
	EXEC(@sql)

	FETCH NEXT FROM MC INTO @NAME
END

CLOSE mc
DEALLOCATE mc

GO

-- /ADJUST RECONCILIATION PERMISSIONS ----------------------------------------------------------------------------------

-- FIXUP FMOP2 USERS ---------------------------------------------------------------------------------------------------

USE FMOP2

GO

EXEC sp_change_users_login @Action='Report';
-- fix these orphaned users. 

-- See comment at declaration of @DummyComplexPassword for explanation.
DECLARE @DummyComplexPassword varchar(50)
SELECT  @DummyComplexPassword = 'FOObarzajkshd8we0123456789abc:;-@'
EXEC sp_change_users_login @Action = 'Auto_Fix', @UserNamePattern = 'AccessCheck', @LoginName = NULL,
     @Password = @DummyComplexPassword
ALTER LOGIN [AccessCheck] WITH PASSWORD = 'AccessChecker', CHECK_POLICY = OFF

GO


-- See comment at declaration of @DummyComplexPassword for explanation.
DECLARE @DummyComplexPassword varchar(50)
SELECT  @DummyComplexPassword = 'FOObarzajkshd8we0123456789abc:;-@'
EXEC sp_change_users_login @Action = 'Auto_Fix', @UserNamePattern = 'BOSS', @LoginName = NULL,
     @Password = @DummyComplexPassword
ALTER LOGIN [BOSS] WITH PASSWORD = 'download', CHECK_POLICY = OFF

GO


-- See comment at declaration of @DummyComplexPassword for explanation.
DECLARE @DummyComplexPassword varchar(50)
SELECT  @DummyComplexPassword = 'FOObarzajkshd8we0123456789abc:;-@'
EXEC sp_change_users_login @Action = 'Auto_Fix', @UserNamePattern = 'credittrader', @LoginName = NULL,
     @Password = @DummyComplexPassword
ALTER LOGIN [credittrader] WITH PASSWORD = 'hsbc', CHECK_POLICY = OFF

GO


-- See comment at declaration of @DummyComplexPassword for explanation.
DECLARE @DummyComplexPassword varchar(50)
SELECT  @DummyComplexPassword = 'FOObarzajkshd8we0123456789abc:;-@'
EXEC sp_change_users_login @Action = 'Auto_Fix', @UserNamePattern = 'LinkedUser', @LoginName = NULL,
     @Password = @DummyComplexPassword
ALTER LOGIN [LinkViewer] WITH PASSWORD = 'linkacce55', CHECK_POLICY = OFF

GO

-- See comment at declaration of @DummyComplexPassword for explanation.
DECLARE @DummyComplexPassword varchar(50)
SELECT  @DummyComplexPassword = 'FOObarzajkshd8we0123456789abc:;-@'
EXEC sp_change_users_login @Action = 'Auto_Fix', @UserNamePattern = 'LinkViewer', @LoginName = NULL,
     @Password = @DummyComplexPassword
ALTER LOGIN [LinkViewer] WITH PASSWORD = 'Link', CHECK_POLICY = OFF

GO


-- See comment at declaration of @DummyComplexPassword for explanation.
DECLARE @DummyComplexPassword varchar(50)
SELECT  @DummyComplexPassword = 'FOObarzajkshd8we0123456789abc:;-@'
EXEC sp_change_users_login @Action = 'Auto_Fix', @UserNamePattern = 'mpapps', @LoginName = NULL,
     @Password = @DummyComplexPassword
ALTER LOGIN [mpapps] WITH PASSWORD = 'Ryder14', CHECK_POLICY = OFF

GO
-- See comment at declaration of @DummyComplexPassword for explanation.
DECLARE @DummyComplexPassword varchar(50)
SELECT  @DummyComplexPassword = 'FOObarzajkshd8we0123456789abc:;-@'
EXEC sp_change_users_login @Action = 'Auto_Fix', @UserNamePattern = 'NewJersayViewer', @LoginName = NULL,
     @Password = @DummyComplexPassword
ALTER LOGIN NewJersayViewer WITH PASSWORD = '2timesquare', CHECK_POLICY = OFF

GO
-- See comment at declaration of @DummyComplexPassword for explanation.
DECLARE @DummyComplexPassword varchar(50)
SELECT  @DummyComplexPassword = 'FOObarzajkshd8we0123456789abc:;-@'
EXEC sp_change_users_login @Action = 'Auto_Fix', @UserNamePattern = 'ReportViewer', @LoginName = NULL,
     @Password = @DummyComplexPassword
ALTER LOGIN ReportViewer WITH PASSWORD = 'reeport1', CHECK_POLICY = OFF

GO


-- See comment at declaration of @DummyComplexPassword for explanation.
DECLARE @DummyComplexPassword varchar(50)
SELECT  @DummyComplexPassword = 'FOObarzajkshd8we0123456789abc:;-@'
EXEC sp_change_users_login @Action = 'Auto_Fix', @UserNamePattern = 'VarReaderUS', @LoginName = NULL,
     @Password = @DummyComplexPassword
ALTER LOGIN [VarReaderUS] WITH PASSWORD = 'View2Risk', CHECK_POLICY = OFF

GO

-- /FIXUP FMOP2 USERS --------------------------------------------------------------------------------------------------

-- SCRIPT SP_U ---------------------------------------------------------------------------------------------------------

-- LS 2009-02-11 - Hm. seems to be duplicated...

USE master
GO

if EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[dbo].[cn_info]') AND OBJECTPROPERTY(id, N'IsView') = 1)
drop view [dbo].[cn_info]
GO

CREATE VIEW cn_info
as
SELECT CASE WHEN spid = @@Spid THEN '(' + CONVERT(VARCHAR(4),spid) + ')' ELSE CONVERT(VARCHAR(4),spid) END Spid,
CONVERT(VARCHAR(4),blocked) Blk , 
open_tran,
CONVERT(VARCHAR(10),db_name(p.dbid)) DB ,
waittime,
lastwaittype,
Cmd , 
CONVERT(VARCHAR(19),last_batch,21) 'Last batch',
CONVERT(VARCHAR(16),hostname) Hostname , 
CONVERT(VARCHAR(12),nt_username) 'NT Username', 
CONVERT(VARCHAR(25),program_name) 'Program Name' ,
CONVERT(VARCHAR(10),loginame) Loginname ,
CONVERT(VARCHAR(19),login_time,21) 'Login time',
cpu,
memusage
FROM master..sysprocesses p

GO
if EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[dbo].[sp_u]') AND OBJECTPROPERTY(id, N'IsProcedure') = 1)
DROP PROCEDURE [dbo].[sp_u]
GO

CREATE PROC sp_u
AS

SELECT * FROM cn_info
WHERE cmd NOT IN ('AWAITING COMMAND','TASK MANAGER','SIGNAL HANDLER','LOCK MONITOR','LAZY WRITER','LOG WRITER','CHECKPOINT SLEEP')
AND waittime = 0

SELECT * FROM cn_info
WHERE cmd NOT IN ('AWAITING COMMAND','TASK MANAGER','SIGNAL HANDLER','LOCK MONITOR','LAZY WRITER','LOG WRITER','CHECKPOINT SLEEP')
AND waittime <> 0

SELECT * FROM cn_info
WHERE cmd IN ('AWAITING COMMAND','TASK MANAGER','SIGNAL HANDLER','LOCK MONITOR','LAZY WRITER','LOG WRITER','CHECKPOINT SLEEP')

GO

-- /SCRIPT SP_U --------------------------------------------------------------------------------------------------------

-- FIXUP TRADEREPORTING USERS ------------------------------------------------------------------------------------------

USE TradeReporting
go
EXEC sp_change_users_login @Action='Report';
-- fix these orphaned users. 

-- See comment at declaration of @DummyComplexPassword for explanation.
DECLARE @DummyComplexPassword varchar(50)
SELECT  @DummyComplexPassword = 'FOObarzajkshd8we0123456789abc:;-@'
EXEC sp_change_users_login @Action = 'Auto_Fix', @UserNamePattern = 'BOSS', @LoginName = NULL,
     @Password = @DummyComplexPassword
ALTER LOGIN [BOSS] WITH PASSWORD = 'download', CHECK_POLICY = OFF

GO


-- See comment at declaration of @DummyComplexPassword for explanation.
DECLARE @DummyComplexPassword varchar(50)
SELECT  @DummyComplexPassword = 'FOObarzajkshd8we0123456789abc:;-@'
EXEC sp_change_users_login @Action = 'Auto_Fix', @UserNamePattern = 'LinkViewer', @LoginName = NULL,
     @Password = @DummyComplexPassword
ALTER LOGIN [LinkViewer] WITH PASSWORD = 'Link', CHECK_POLICY = OFF

GO


-- See comment at declaration of @DummyComplexPassword for explanation.
DECLARE @DummyComplexPassword varchar(50)
SELECT  @DummyComplexPassword = 'FOObarzajkshd8we0123456789abc:;-@'
EXEC sp_change_users_login @Action = 'Auto_Fix', @UserNamePattern = 'mpapps', @LoginName = NULL,
     @Password = @DummyComplexPassword
ALTER LOGIN [mpapps] WITH PASSWORD = 'Ryder14', CHECK_POLICY = OFF

GO

-- /FIXUP TRADEREPORTING USERS -----------------------------------------------------------------------------------------

-- FIXUP CREDITTRADING USERS -------------------------------------------------------------------------------------------

USE CreditTrading
go
EXEC sp_change_users_login @Action='Report';
-- fix these orphaned users. 

-- See comment at declaration of @DummyComplexPassword for explanation.
DECLARE @DummyComplexPassword varchar(50)
SELECT  @DummyComplexPassword = 'FOObarzajkshd8we0123456789abc:;-@'
EXEC sp_change_users_login @Action = 'Auto_Fix', @UserNamePattern = 'BOSS', @LoginName = NULL,
     @Password = @DummyComplexPassword
ALTER LOGIN [BOSS] WITH PASSWORD = 'download', CHECK_POLICY = OFF

GO


-- See comment at declaration of @DummyComplexPassword for explanation.
DECLARE @DummyComplexPassword varchar(50)
SELECT  @DummyComplexPassword = 'FOObarzajkshd8we0123456789abc:;-@'
EXEC sp_change_users_login @Action = 'Auto_Fix', @UserNamePattern = 'credittrader', @LoginName = NULL,
     @Password = @DummyComplexPassword
ALTER LOGIN [credittrader] WITH PASSWORD = 'hsbc', CHECK_POLICY = OFF

GO


-- See comment at declaration of @DummyComplexPassword for explanation.
DECLARE @DummyComplexPassword varchar(50)
SELECT  @DummyComplexPassword = 'FOObarzajkshd8we0123456789abc:;-@'
EXEC sp_change_users_login @Action = 'Auto_Fix', @UserNamePattern = 'LinkViewer', @LoginName = NULL,
     @Password = @DummyComplexPassword
ALTER LOGIN [LinkViewer] WITH PASSWORD = 'Link', CHECK_POLICY = OFF

GO


-- See comment at declaration of @DummyComplexPassword for explanation.
DECLARE @DummyComplexPassword varchar(50)
SELECT  @DummyComplexPassword = 'FOObarzajkshd8we0123456789abc:;-@'
EXEC sp_change_users_login @Action = 'Auto_Fix', @UserNamePattern = 'mpapps', @LoginName = NULL,
     @Password = @DummyComplexPassword
ALTER LOGIN [mpapps] WITH PASSWORD = 'Ryder14', CHECK_POLICY = OFF

GO

-- /FIXUP CREDITTRADING USERS ------------------------------------------------------------------------------------------

-- FIXUP SUN426 USERS --------------------------------------------------------------------------------------------------

USE SUN426
GO

EXEC sp_change_users_login @Action='Report';
-- fix these orphaned users. 

ALTER LOGIN [SSA] WITH LOGIN = [SSA]
ALTER LOGIN [SUN] WITH LOGIN = [SUN]
ALTER LOGIN [LAS] WITH LOGIN = [LAS]
EXEC sp_changedbowner 'SUN','dbo'                                                                     --  Set as db owner of SUN426


GO

-- /FIXUP SUN426 USERS -------------------------------------------------------------------------------------------------



--=====================================================================================================
--  Need to find out whether the following should be run or whether the SUN logins will work already

/*
	The SUN system logs in to SQL as 'SSA' to start with. This then gives the user a prompt to log into the system.
	The system then logs into SQL as 'SUN'
	The SUN account is the owner of the SUN426 database and therefore cannot be easily dropped and recreated.

	SELECT l.suid, l.name LoginName, u.name NameInDB, u.uid, * 
	FROM master..syslogins l
	join sysusers u on u.sid = l.sid

	sp_droplogin 'SSA'								--  Remove login for SSA
	EXEC sp_addlogin 'SSA', 'SUNSSP'				--  Add login back in
	EXEC sp_grantdbaccess 'SSA','SSA'				--  Grant access to sun426 as SSA

	SELECT * FROM sysusers
	SELECT * FROM master.dbo.syslogins
*/

-- FIXUP RM_VAR USERS --------------------------------------------------------------------------------------------------

USE RM_VAR
GO

EXEC sp_change_users_login @Action='Report';
-- fix these orphaned users. 

-- See comment at declaration of @DummyComplexPassword for explanation.
DECLARE @DummyComplexPassword varchar(50)
SELECT  @DummyComplexPassword = 'FOObarzajkshd8we0123456789abc:;-@'
EXEC sp_change_users_login @Action = 'Auto_Fix', @UserNamePattern = 'LinkedUser', @LoginName = NULL,
     @Password = @DummyComplexPassword
ALTER LOGIN [LinkedUser] WITH PASSWORD = 'linkacce55', CHECK_POLICY = OFF

GO


-- See comment at declaration of @DummyComplexPassword for explanation.
DECLARE @DummyComplexPassword varchar(50)
SELECT  @DummyComplexPassword = 'FOObarzajkshd8we0123456789abc:;-@'
EXEC sp_change_users_login @Action = 'Auto_Fix', @UserNamePattern = 'LinkUser', @LoginName = NULL,
     @Password = @DummyComplexPassword
ALTER LOGIN [LinkUser] WITH PASSWORD = 'link', CHECK_POLICY = OFF

GO


-- See comment at declaration of @DummyComplexPassword for explanation.
DECLARE @DummyComplexPassword varchar(50)
SELECT  @DummyComplexPassword = 'FOObarzajkshd8we0123456789abc:;-@'
EXEC sp_change_users_login @Action = 'Auto_Fix', @UserNamePattern = 'MPAPPS', @LoginName = NULL,
     @Password = @DummyComplexPassword
ALTER LOGIN [MPAPPS] WITH PASSWORD = 'Ryder14', CHECK_POLICY = OFF

GO

-- See comment at declaration of @DummyComplexPassword for explanation.
DECLARE @DummyComplexPassword varchar(50)
SELECT  @DummyComplexPassword = 'FOObarzajkshd8we0123456789abc:;-@'
EXEC sp_change_users_login @Action = 'Auto_Fix', @UserNamePattern = 'NewJerseyViewer', @LoginName = NULL,
     @Password = @DummyComplexPassword
ALTER LOGIN NewJerseyViewer WITH PASSWORD = '2timesquare', CHECK_POLICY = OFF

GO

-- See comment at declaration of @DummyComplexPassword for explanation.
DECLARE @DummyComplexPassword varchar(50)
SELECT  @DummyComplexPassword = 'FOObarzajkshd8we0123456789abc:;-@'
EXEC sp_change_users_login @Action = 'Auto_Fix', @UserNamePattern = 'ReportViewer', @LoginName = NULL,
     @Password = @DummyComplexPassword
ALTER LOGIN [ReportViewer] WITH PASSWORD = 'reeport1', CHECK_POLICY = OFF

GO

-- See comment at declaration of @DummyComplexPassword for explanation.
DECLARE @DummyComplexPassword varchar(50)
SELECT  @DummyComplexPassword = 'FOObarzajkshd8we0123456789abc:;-@'
EXEC sp_change_users_login @Action = 'Auto_Fix', @UserNamePattern = 'RiskUser', @LoginName = NULL,
     @Password = @DummyComplexPassword
ALTER LOGIN RiskUser WITH PASSWORD = 'DefaultPassword', CHECK_POLICY = OFF

GO

-- See comment at declaration of @DummyComplexPassword for explanation.
DECLARE @DummyComplexPassword varchar(50)
SELECT  @DummyComplexPassword = 'FOObarzajkshd8we0123456789abc:;-@'
EXEC sp_change_users_login @Action = 'Auto_Fix', @UserNamePattern = 'VarReaderUS', @LoginName = NULL,
     @Password = @DummyComplexPassword
ALTER LOGIN [VarReaderUS] WITH PASSWORD = 'View2Risk', CHECK_POLICY = OFF

GO

-- /FIXUP RM_VAR USERS -------------------------------------------------------------------------------------------------

-- SET UP RM_VAR ENVIRONMENT -------------------------------------------------------------------------------------------

--  May need to create the missing user
--  CREATE LOGIN [ReportViewer] WITH PASSWORD=N'reeport1', DEFAULT_DATABASE=[RC], DEFAULT_LANGUAGE=[us_english], CHECK_EXPIRATION=OFF, CHECK_POLICY=OFF

-- Ensure there is a share on the server called E
master.dbo.xp_cmdshell 'net share E=E:\'

-- To point the VAR database to the correct location
--  SELECT * FROM LookupValue WHERE LOOKUPNAME = 'Live Server'
UPDATE LookupValue set value = 'KOALA' WHERE LOOKUPNAME = 'Live Server'

GO

UPDATE LookupValue SET Value = '\\KOALA\e\VaR\RMindex.fmt' WHERE LookupName = 'CDSIndexFormatFile'
UPDATE LookupValue SET Value = '\\KOALA\e\VaR\RMmip.fmt' WHERE LookupName = 'CDSMIPFormatFile'
UPDATE LookupValue SET Value = '\\KOALA\e\VaR\RegCap\DataFiles\Imports\' WHERE LookupName = 'RegCapFileImportLocation'
UPDATE LookupValue SET Value = '\\KOALA\e\VaR' WHERE LookupName = 'RMnameDataImportDir'
UPDATE LookupValue SET Value = '\\KOALA\e\Export\data\RiskMetrics\Reports\Repo Var\' WHERE LookupName = 'RMrepoVarReportLocation'
UPDATE LookupValue SET Value = '\\KOALA\e\Export\data\RiskMetrics' WHERE LookupName = 'RMresultsDir'
UPDATE LookupValue SET Value = '\\KOALA\e\Export\data\RiskMetrics' WHERE LookupName = 'RMresultsDir'
UPDATE LookupValue SET Value = '\\KOALA\e\Export\data\RiskMetrics\UAT' WHERE LookupName = 'RMresultsDirDebug'
UPDATE LookupValue SET Value = '\\KOALA\e\Export\data\RiskMetrics\UAT' WHERE LookupName = 'RMresultsDirDev'

GO

-- /SET UP RM_VAR ENVIRONMENT ------------------------------------------------------------------------------------------

-- FIXUP RC USERS ------------------------------------------------------------------------------------------------------

USE RC
GO

EXEC sp_change_users_login @Action='Report';
-- fix these orphaned users. 

-- See comment at declaration of @DummyComplexPassword for explanation.
DECLARE @DummyComplexPassword varchar(50)
SELECT  @DummyComplexPassword = 'FOObarzajkshd8we0123456789abc:;-@'
EXEC sp_change_users_login @Action = 'Auto_Fix', @UserNamePattern = 'LinkedUser', @LoginName = NULL,
     @Password = @DummyComplexPassword
ALTER LOGIN LinkedUser WITH PASSWORD = 'linkacce55', CHECK_POLICY = OFF

GO

DECLARE @DummyComplexPassword varchar(50)
SELECT  @DummyComplexPassword = 'FOObarzajkshd8we0123456789abc:;-@'
EXEC sp_change_users_login @Action = 'Auto_Fix', @UserNamePattern = 'LondonViewer', @LoginName = NULL,
     @Password = @DummyComplexPassword
ALTER LOGIN [LondonViewer] WITH PASSWORD = 'ryder66', CHECK_POLICY = OFF

GO


-- See comment at declaration of @DummyComplexPassword for explanation.
DECLARE @DummyComplexPassword varchar(50)
SELECT  @DummyComplexPassword = 'FOObarzajkshd8we0123456789abc:;-@'
EXEC sp_change_users_login @Action = 'Auto_Fix', @UserNamePattern = 'NewJerseyViewer', @LoginName = NULL,
     @Password = @DummyComplexPassword
ALTER LOGIN [NewJerseyViewer] WITH PASSWORD = '5thAvenue', CHECK_POLICY = OFF

GO


-- See comment at declaration of @DummyComplexPassword for explanation.
DECLARE @DummyComplexPassword varchar(50)
SELECT  @DummyComplexPassword = 'FOObarzajkshd8we0123456789abc:;-@'
EXEC sp_change_users_login @Action = 'Auto_Fix', @UserNamePattern = 'ReportViewer', @LoginName = NULL,
     @Password = @DummyComplexPassword
ALTER LOGIN [ReportViewer] WITH PASSWORD = 'reeport1', CHECK_POLICY = OFF

GO

DECLARE @DummyComplexPassword varchar(50)
SELECT  @DummyComplexPassword = 'FOObarzajkshd8we0123456789abc:;-@'
EXEC sp_change_users_login @Action = 'Auto_Fix', @UserNamePattern = 'RiskUser', @LoginName = NULL,
     @Password = @DummyComplexPassword
ALTER LOGIN RiskUser WITH PASSWORD = 'DefaultPassword', CHECK_POLICY = OFF

GO

-- See comment at declaration of @DummyComplexPassword for explanation.
DECLARE @DummyComplexPassword varchar(50)
SELECT  @DummyComplexPassword = 'FOObarzajkshd8we0123456789abc:;-@'
EXEC sp_change_users_login @Action = 'Auto_Fix', @UserNamePattern = 'TorontoViewer', @LoginName = NULL,
     @Password = @DummyComplexPassword
ALTER LOGIN [TorontoViewer] WITH PASSWORD = 'acce55', CHECK_POLICY = OFF

GO

-- See comment at declaration of @DummyComplexPassword for explanation.
DECLARE @DummyComplexPassword varchar(50)
SELECT  @DummyComplexPassword = 'FOObarzajkshd8we0123456789abc:;-@'
EXEC sp_change_users_login @Action = 'Auto_Fix', @UserNamePattern = 'VarReaderUS', @LoginName = NULL,
     @Password = @DummyComplexPassword
ALTER LOGIN [VarReaderUS] WITH PASSWORD = 'View2Risk', CHECK_POLICY = OFF

GO

-- /FIXUP RC USERS ------------------------------------------------------------------------------------------------------

-- SET UP PUBSUB TO POINT AT KOALA ---------------------------------------------------------------------------------------

-- IMPORTANT NOTE!! PLEASE ADJUST TO IP ADDRESS OF APPROPRIATE SERVER IF YOU ARE NOT USING DRFS1. IF YOU NEED TO DETERMINE
-- THE IP ADDRESS OF A NAMED SERVER, USE ping [server name].
--  '172.26.64.58' is the IP address of DRFS1, but you should check this.

USE FMOP2
UPDATE tblUserConfig SET Data = '172.26.64.58' WHERE [Application] = 'Message' and Component = 'Server'

-- /SET UP PUBSUB TO POINT AT DRFS1 --------------------------------------------------------------------------------------





