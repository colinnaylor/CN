:connect Lynx
GO
use master
GO
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

