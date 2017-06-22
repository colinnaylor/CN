SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
--  ###################################################################################################
CREATE PROCEDURE dbo.sp_select(@table_name sysname, @spid int = NULL, @max_pages int = 1000)
AS
SET NOCOUNT ON

DECLARE @object_id int
    , @table     sysname
    , @db_name   sysname
    , @db_id     int
    , @file_name varchar(MAX)  
    , @status    int
    , @rowcount  int
  
IF PARSENAME(@table_name, 3) = 'tempdb'
begin
	SET @table = PARSENAME(@table_name, 1)

	IF (SELECT COUNT(*)  from tempdb.sys.tables where name like @table + '[_][_]%') > 1
	begin
		-- determine the default trace file
		SELECT @file_name = SUBSTRING(path, 0, LEN(path) - CHARINDEX('\', REVERSE(path)) + 1) + '\Log.trc'  
		FROM sys.traces   
		WHERE is_default = 1;  

		CREATE TABLE #objects (ObjectId sysname primary key)
	    
		-- Match the spid with db_id and object_id via the default trace file
		insert into #objects
		SELECT o.OBJECT_ID
		FROM sys.fn_trace_gettable(@file_name, DEFAULT) AS gt  
		JOIN tempdb.sys.objects AS o   
			 ON gt.ObjectID = o.OBJECT_ID  
		LEFT JOIN (SELECT distinct spid, dbid 
					 FROM master..sysprocesses 
					WHERE spid = @spid or @spid is null) dr ON dr.spid = gt.SPID
		WHERE gt.DatabaseID = 2 
		  AND gt.EventClass = 46 -- (Object:Created Event from sys.trace_events)  
		  AND o.create_date >= DATEADD(ms, -100, gt.StartTime)   
		  AND o.create_date <= DATEADD(ms, 100, gt.StartTime)
		  AND o.name like @table + '[_][_]%'
		  AND (gt.SPID = @spid or (@spid is null and dr.dbid = DB_ID()))
	      
		SET @rowcount = @@ROWCOUNT
	    
		IF @rowcount = 0 
		begin
			RAISERROR('Unable to figure out which temp table with name [%s] to select, please run the procedure on a specific database, or specify a @spid to filter on.', 16,1, @table_name)
			RETURN(-1)
		end
	    
		IF @rowcount > 1 and @spid is null
		begin
			RAISERROR('There are %d temp tables with the name [%s] active in your database. Please specify the @spid you wish to find it for.', 16, 1, @rowcount, @table_name)
			RETURN(-1)
		end   
	    
		IF @rowcount > 1
		begin
			RAISERROR('There are %d temp tables with the name [%s] active on the spid %d. There must be something wrong in this procedure. Showing the first one', 16, 1, @rowcount, @table_name, @spid)
		-- We'll continue with the first match.
		end

		SELECT TOP 1 @object_id = ObjectId 
		FROM #objects
		ORDER BY ObjectId
	end
	ELSE
	begin
	  SELECT @object_id = object_id FROM tempdb.sys.tables WHERE name LIKE @table + '[_][_]%'
	end
end
ELSE 
begin
	SET @object_id = OBJECT_ID(@table_name)
end

IF @object_id IS NULL
begin 
	RAISERROR('The table [%s] does not exist', 16, 1, @table_name)
	RETURN (-1)
end

SET @db_id = DB_ID(PARSENAME(@table_name, 3))

EXEC @status = master..sp_selectpages @object_id = @object_id, @db_id = @db_id, @max_pages = @max_pages

--SELECT 'For debugging only!' as Note

RETURN (@status)

GO
