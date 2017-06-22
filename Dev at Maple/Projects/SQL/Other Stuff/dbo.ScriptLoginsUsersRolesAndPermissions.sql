SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
--  ScriptLoginsUsersRolesAndPermissions 'FMOP2', @Logins = 1, @Users = 0, @Permissions = 0, @Username = null

--  ###################################################################################################
CREATE Proc ScriptLoginsUsersRolesAndPermissions @Database varchar(50) = null, @Logins bit = 0, @Users bit = 0, @Permissions bit = 0, @Username VARCHAR(50) = NULL, 
	@NoExistCheck BIT = 0
AS
--  ###################################################################################################
IF @Logins = 1  --  Script all logins
	EXEC sp_help_revlogin

--  ###################################################################################################
IF @Users = 1
begin
	IF @NoExistCheck = 0
	BEGIN
		PRINT 'The output of the user section produces script that tests to see if the user has already been'
		PRINT 'added, thus avoiding an attempt to add a user when they already exist.'
		PRINT ''
	END

	--  Users for each individual database together with the roles they belong to.
	declare @dbName varchar(50),@User varchar(50) , @sql varchar(5000) 

	declare mc cursor for
	select name from sys.databases
	Where name not in ('master','tempdb','model','SystemInfo')
	  and ( name = @Database OR @Database is null )

	SET NOCOUNT ON

	open mc
	fetch next from mc into @dbName
	while @@fetch_status = 0
	begin
		IF @NoExistCheck = 0
		BEGIN
			--  ###########################################################################################################################################################
			SET @Sql = 'USE [' + @dbName + '];
	SELECT ''USE ' + REPLACE(@dbName, '_Copy','') + ';
	GO''
	SELECT ''IF not exists ( SELECT * FROM sys.database_principals WHERE name = '''''' + name + '''''' )
	begin
		PRINT ''''CREATE USER ['' + name + ''] FOR LOGIN ['' + name + '']''''
		CREATE USER ['' + name + ''] FOR LOGIN ['' + name + '']
	end'' 
	FROM sys.database_principals WHERE type != ''R'' AND name not in (''dbo'',''sys'',''guest'') and name not like ''##%''
			' + CASE WHEN @Username IS NULL THEN '' ELSE 'AND name = ''' + @Username + '''
		' END + '

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
			' + CASE WHEN @Username IS NULL THEN '' ELSE 'AND u.name = ''' + @Username + '''
		' END
			--  ###########################################################################################################################################################
		END
		ELSE
		BEGIN
			--  ###########################################################################################################################################################
			SET @Sql = 'USE [' + @dbName + '];
	SELECT ''USE ' + REPLACE(@dbName, '_Copy','') + ';
	GO''
	SELECT ''CREATE USER ['' + name + ''] FOR LOGIN ['' + name + '']
	'' 
	FROM sys.database_principals WHERE type != ''R'' AND name not in (''dbo'',''sys'',''guest'') and name not like ''##%''
			' + CASE WHEN @Username IS NULL THEN '' ELSE 'AND name = ''' + @Username + '''
		' END + '

	select ''ALTER ROLE ['' + r.name + ''] ADD MEMBER ['' + u.name + '']
	''
	--  select u.name, r.name
	from sys.database_principals u
	join sys.database_role_members m on m.member_principal_id = u.principal_id
	join sys.database_principals r on m.role_principal_id = r.principal_id
	where u.type_desc <> ''DATABASE_ROLE''
	AND u.name not in (''dbo'',''guest'',''INFORMATION_SCHEMA'',''sys'')
			' + CASE WHEN @Username IS NULL THEN '' ELSE 'AND u.name = ''' + @Username + '''
		' END
			--  ###########################################################################################################################################################
		END

		--Print @sql
		EXEC(@Sql)

		fetch next from mc into @dbName
	end
	close mc
	deallocate mc
end
--  ###################################################################################################
IF @Permissions = 1
BEGIN
	IF @NoExistCheck = 0
	BEGIN
		PRINT 'The output of the permission section produces script that tests to see if the permission has already been'
		PRINT 'granted, thus avoiding audit entries of permission grants or revokes when the permission is already set.'
		PRINT ''
	END

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
	BEGIN
		IF @NoExistCheck = 0
		BEGIN
		--  ###########################################################################################################################################################
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
			' + CASE WHEN @Username IS NULL THEN '' ELSE 'AND grantee_principal.name = ''' + @Username + '''
		' END
		--  ###########################################################################################################################################################
		END
		ELSE
		BEGIN --  NoExistCheck
		--  ###########################################################################################################################################################
			SET @Sql = 'USE [' + @dbName + '];
		SELECT ''USE ' + REPLACE(@dbName, '_Copy','') + ';
GO''
		SELECT '''' + prmssn.State_desc + '' '' + permission_name + '' ON ['' + obj.Name COLLATE LATIN1_GENERAL_CI_AS + ''] TO ['' + grantee_principal.name + '']''
		from sys.objects AS obj
		JOIN sys.database_permissions AS prmssn ON prmssn.major_id=obj.object_id AND prmssn.minor_id=0 AND prmssn.class=1
		JOIN sys.database_principals AS grantee_principal ON grantee_principal.principal_id = prmssn.grantee_principal_id
		WHERE prmssn.minor_id = 0
			' + CASE WHEN @Username IS NULL THEN '' ELSE 'AND grantee_principal.name = ''' + @Username + '''
		' END + ' ORDER BY obj.Name
		'

		--  ###########################################################################################################################################################
		END

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

GO
