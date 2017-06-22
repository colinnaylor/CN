-- DR Procedure Reversal -- sql
------------------------------------------------------------------------------------------------------------------------
  -- DR SQL Server reversal Script
  -- Last edited by Colin Naylor 2013 02 26
------------------------------------------------------------------------------------------------------------------------

--########################################################################################################
--Step -- Kill all user logins -- sql

declare @spid int, @Sql varchar(500) 
declare mc cursor for
SELECT spid FROM master..sysprocesses 
Where dbid > 0
	AND dbid not in ( SELECT database_id FROM sys.databases WHERE name = 'master' )
	AND Spid != @@Spid

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
--Step -- Rename dbs back to the Name_Copy style -- sql

declare mc cursor for
SELECT OldName, NewName FROM RenamedDatabase WHERE TestDay = convert(date,GetDate())

declare @old_name varchar(200), @new_name varchar(200), @sql varchar(300)

open mc
fetch next from mc into @old_name, @new_name

while @@fetch_status = 0
begin
	--  Attempt to change @new_name back to @old_name

	IF exists( SELECT * FROM sys.databases WHERE name = @old_name )
	begin	--  Old name already exists, 
		IF exists( SELECT * FROM sys.databases WHERE name = @new_name )
		begin
			raiserror('Both %s and %s already exist.',10,1,@new_name,@old_name) with nowait
		end
		ELSE
		begin
			raiserror('Looks like %s has already been renamed to %s.',10,1,@new_name,@old_name) with nowait
		end
	end
	ELSE
	begin
		raiserror('Renaming %s to %s.',10,1,@new_name,@old_name) with nowait
		SET @sql = 'sp_renamedb ' + @new_name + ', ' + @old_name
		--print @sql
		EXEC(@sql)
	end

	fetch next from mc into @old_name, @new_name
end
close mc
deallocate mc

--########################################################################################################
------ Commented this out as the BR app will have been failing all day whilst the DR test is done
------ so should need to add an entry at this point.
----Step -- Notify Backup/Restore app that DBs are delayed -- sql
--:Connect Lynx
--use Process;
--INSERT br_KnownIssues SELECT Target, getdate(), system_user, 'DRsql down',GetDate() + 0.5 From Br_process where target like 'DRsql%'
--INSERT br_KnownIssues SELECT Target, getdate(), system_user, 'DRsql down',GetDate() + 0.5 From Br_process where Source like 'DRsql%'

