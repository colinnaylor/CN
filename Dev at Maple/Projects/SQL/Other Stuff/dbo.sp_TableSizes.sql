use master
go
alter proc sp_TableSizes
as
declare MC cursor FOR
select s.name, o.name
--  SELECT * 
from sys.Objects o
Join sys.schemas s on s.schema_id=o.schema_id
WHERE type = 'u' and o.name not like 'dt%'

SET NOCOUNT ON 

DECLARE @Schema varchar(150),@Table varchar(80), @Sql varchar(1000)
open MC

Create table #Results(Name varchar(80),Rows money, Reserved varchar(20),DataSize varchar(20),IndexSize varchar(20),Unused varchar(20))
Create table #Results2(Name varchar(80),Rows money, Reserved money,DataSize money,IndexSize money,Unused money)

fetch next from mc into @Schema, @Table
while @@Fetch_Status <> -1
begin
	select @Sql = 'INSERT #Results EXEC sp_spaceused [' + @Schema + '.' + @Table  + ']'
--	print @Sql	
	EXEC (@Sql)

	fetch next from mc into @Schema, @Table
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
