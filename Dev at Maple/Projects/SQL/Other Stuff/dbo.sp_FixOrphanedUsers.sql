SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
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
