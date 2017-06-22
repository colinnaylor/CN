SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE sp_help_revlogin @login_name sysname = NULL 
AS
SET NOCOUNT ON
CREATE TABLE #output(Row int identity(1,1), Line varchar(5000) )
--  sp_helptext sp_help_revlogin 'CreditTrader'

DECLARE @name sysname
DECLARE @type varchar (1)
DECLARE @hasaccess int
DECLARE @denylogin int
DECLARE @is_disabled int
DECLARE @PWD_varbinary  varbinary (256)
DECLARE @PWD_string  varchar (514)
DECLARE @SID_varbinary varbinary (85)
DECLARE @SID_string varchar (514)
DECLARE @tmpstr  varchar (max)
DECLARE @is_policy_checked varchar (3)
DECLARE @is_expiration_checked varchar (3)
DECLARE @defaultdb sysname
 
IF (@login_name IS NULL)
	DECLARE login_curs CURSOR FOR
    SELECT p.sid, p.name, p.type, p.is_disabled, p.default_database_name, l.hasaccess, l.denylogin 
	FROM sys.server_principals p 
	LEFT JOIN sys.syslogins l ON ( l.name = p.name ) 
	WHERE p.type IN ( 'S', 'G', 'U' ) AND p.name <> 'sa'
	Order by p.Name
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

SET @tmpstr = ''
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
