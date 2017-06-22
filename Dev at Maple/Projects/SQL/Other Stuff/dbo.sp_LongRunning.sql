SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
--  ###################################################################################################
CREATE proc sp_LongRunning
as
SELECT * FROM master.dbo.cn_info
WHERE cmd NOT IN ('AWAITING COMMAND','TASK MANAGER','SIGNAL HANDLER','LOCK MONITOR','LAZY WRITER','LOG WRITER','CHECKPOINT SLEEP',
				  'TRACE QUEUE TASK','BRKR TASK','CHECKPOINT','BRKR EVENT HNDLR','RESOURCE MONITOR','UNKNOWN TOKEN' )
AND waittime = 0
and DateDiff(minute, GetDate(), [Last batch]) > 30

GO
