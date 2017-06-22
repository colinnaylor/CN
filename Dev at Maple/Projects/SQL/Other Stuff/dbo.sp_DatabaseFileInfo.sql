SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROC sp_DatabaseFileInfo
as
Create table #res(ID int, Name varchar(50),FileID int, Size real, Filename varchar(50),FilePath varchar(500) )

declare @Name varchar(50), @ID int, @sql varchar(500) 

DECLARE mc CURSOR for
SELECT name, database_id
--  SELECT * 
FROM sys.databases d
WHERE name not in ('master','msdb','model','tempdb')
	AND state = 0
	
open mc

Fetch next from mc into @name, @id

while @@fetch_status = 0
begin
	SET @sql = 'SELECT ' + convert(varchar(50),@id) + ',''' + @name + ''',fileid, size, name, filename FROM ' + @name + '.dbo.sysfiles'
	
	print @sql
	INSERT #res
	EXEC(@sql)

	Fetch next from mc into @name, @id
end
close mc
deallocate mc

Update #res SET Size = Size / 1024

SELECT ID, Name, FileID, convert(varchar(50), convert(money,Size), 1) [Size MB],Filename, Filepath
FROM #res
Order by Name,FileID

DROP TABLE #res

GO
