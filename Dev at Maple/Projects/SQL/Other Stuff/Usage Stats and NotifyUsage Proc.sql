use FMOP2
go
Update UsageStats SET Name = OBJECT_NAME(ProcID) Where Name is null
SELECT * FROM UsageStats
DELETE UsageStats

go
create table UsageStats(ObjectType varchar(50), ProcID int, Name varchar(150), UsedWhen datetime default GetDate() )

go
create alter proc NotifyUsage @ObjectType varchar(50), @ProcID int
as

INSERT UsageStats(ObjectType, ProcID)
SELECT @ObjectType, @ProcID
go
grant exec on NotifyUsage to public 
go
create proc TestProc
as
EXEC NotifyUsage 'Proc', @@procid
go
drop proc TestProc

--  Add into procedures the following        Ensure SET NOCOUNT ON to avoid unwanted rows affected reports
EXEC NotifyUsage 'Proc', @@PROCID

