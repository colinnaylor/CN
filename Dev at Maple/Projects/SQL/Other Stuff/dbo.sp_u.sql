SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
--  sp_u 2
CREATE PROC sp_u   @Active varchar(40) = '0'
AS
--          sp_u
--        dbcc inputbuffer(53)
--        dbcc outputbuffer(53)
SET NOCOUNT ON
declare @machine varchar(40)
IF @Active not in ('0','1','2') SET @machine = @Active

SELECT * FROM master.dbo.cn_info
WHERE cmd NOT IN ('AWAITING COMMAND','TASK MANAGER','SIGNAL HANDLER','LOCK MONITOR','LAZY WRITER','LOG WRITER','CHECKPOINT SLEEP',
				  'TRACE QUEUE TASK','BRKR TASK','CHECKPOINT','BRKR EVENT HNDLR','RESOURCE MONITOR','UNKNOWN TOKEN' )
AND waittime = 0
AND ( HostName = @machine  OR  @machine is null )

SELECT * FROM master.dbo.cn_info
WHERE cmd NOT IN ('AWAITING COMMAND','TASK MANAGER','SIGNAL HANDLER','LOCK MONITOR','LAZY WRITER','LOG WRITER','CHECKPOINT SLEEP',
				  'TRACE QUEUE TASK','BRKR TASK','CHECKPOINT','BRKR EVENT HNDLR')
AND waittime <> 0
AND ( HostName = @machine  OR  @machine is null )

IF @Active IN ('0','2')
begin
	SELECT * FROM master.dbo.cn_info
	WHERE ( @Active != 2 AND cmd IN ('AWAITING COMMAND','TASK MANAGER','SIGNAL HANDLER','LOCK MONITOR','LAZY WRITER','LOG WRITER','CHECKPOINT SLEEP',
				  'TRACE QUEUE TASK','BRKR TASK','CHECKPOINT','BRKR EVENT HNDLR','RESOURCE MONITOR','UNKNOWN TOKEN') )
		OR
		  ( @Active = 2 AND cmd IN ('AWAITING COMMAND' ) )

	IF @Active = '0'
	begin
		select 	DISTINCT
			convert (smallint, req_spid) As spid, 
			convert(varchar(10),db_name(rsc_dbid)) As dbid, 
			rsc_objid As ObjId,
			rsc_indid As IndId,
			substring (v.name, 1, 4) As Type,
			substring (u.name, 1, 8) As Mode,
			substring (x.name, 1, 5) As Status

		from 	master.dbo.syslockinfo,
			master.dbo.spt_values v,
			master.dbo.spt_values x,
			master.dbo.spt_values u

		where   master.dbo.syslockinfo.rsc_type = v.number
				and v.type = 'LR'
				and master.dbo.syslockinfo.req_status = x.number
				and x.type = 'LS'
				and master.dbo.syslockinfo.req_mode + 1 = u.number
				and u.type = 'L'
				AND rsc_objid <> 0
	end
end

IF @machine is not null
begin
	SELECT * FROM master.dbo.cn_info
	WHERE cmd IN ('AWAITING COMMAND','TASK MANAGER','SIGNAL HANDLER','LOCK MONITOR','LAZY WRITER','LOG WRITER','CHECKPOINT SLEEP',
					  'TRACE QUEUE TASK','BRKR TASK','CHECKPOINT','BRKR EVENT HNDLR')
	AND ( HostName = @machine  OR  @machine is null )
end


GO
