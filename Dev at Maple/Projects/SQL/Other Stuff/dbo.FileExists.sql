SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE
PROC [dbo].[FileExists](@Path varchar(200), @Filename varchar(50))
as
-- Thus returns 1 for existing and 0 for non existing. If you pass an invalid filename and 
-- the filename happens to be in the output of 'dir c:\winnt\system\rubbish'
-- the procedure will incorrectly report the file as being in existance.
SET NOCOUNT ON

declare @cmd varchar(300)
select @cmd = @Path + @Filename
if ascii(substring(@cmd,1,1)) != 34 SET @cmd = '"' + @cmd
if ascii(substring(@cmd,len(@cmd),1)) != 34 SET @cmd = @cmd + '"'
select @cmd = 'dir ' + @cmd

create table #output(data varchar(200))
insert #output
EXEC master..xp_cmdshell @cmd
IF (select count(*) from #output where data like '%' + @FileName + '%') > 0
begin
	Print 'Exists'
	return 1
end
ELSE
begin
	Print 'Does not exists'
	return 0
end
drop table #output


GO
