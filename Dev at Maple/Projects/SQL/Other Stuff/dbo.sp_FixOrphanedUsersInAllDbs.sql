SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
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


GO
