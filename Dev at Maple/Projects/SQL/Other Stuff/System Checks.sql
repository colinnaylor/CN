use process
go
/*
	--  Insert a new check item
	insert SystemCheckItem(Name, Inserted, InsertedBy) select 'MilMmaApp', getdate(), system_user
	--  Insert a new problem description
	insert SystemProblem(Name, inserted, insertedby) select 'No access to Milan machines', getdate(), system_user

	delete systemcheck where day = '16 Jun 09'
	--  Current items to check
	select * from SystemCheckItem where Active = 1
	--  Current outcomes
	select * from SystemProblem
	select * from SystemCheck where Day > '23 dec 08'
	select Day, count(*) from SystemCheck where Day > '23 dec 08' group by Day
*/
-- #####################################################################################################################
-- #####################################################################################################################
SET NOCOUNT ON
declare @Day datetime		select @Day = convert(varchar(8), getdate(), 112)

--  OK
EXEC InsertSystemCheck @Day, 'Comms room'	,			'OK'
EXEC InsertSystemCheck @Day, 'Phones outgoing',			'OK'
EXEC InsertSystemCheck @Day, 'Phones incoming',			'OK'
EXEC InsertSystemCheck @Day, 'Phones internal',			'OK'  -- 2109
EXEC InsertSystemCheck @Day, 'Cougar',					'OK'
EXEC InsertSystemCheck @Day, 'Lynx',					'OK'
EXEC InsertSystemCheck @Day, 'MMA1',					'OK'
EXEC InsertSystemCheck @Day, 'MMA2',					'OK'
EXEC InsertSystemCheck @Day, 'MilMmaSql',				'OK'
EXEC InsertSystemCheck @Day, 'MilMmaApp',				'OK'
EXEC InsertSystemCheck @Day, 'Minky',					'OK'
EXEC InsertSystemCheck @Day, 'Koala',					'OK'
EXEC InsertSystemCheck @Day, 'DRFS1',					'OK'
EXEC InsertSystemCheck @Day, 'ORCA',					'OK'
EXEC InsertSystemCheck @Day, 'Check database backups',	'OK'
EXEC InsertSystemCheck @Day, 'Outside email',			'OK'
EXEC InsertSystemCheck @Day, 'Boss scheduler',			'OK' /*
EXEC InsertSystemCheck @Day, 'Boss scheduler',			'Restarted'
EXEC MSDB..sp_send_dbmail  @profile_name = 'sqlAdmin',@recipients = 'Trusha.Yadave@mpuk.com;mohammed.chehab@mpuk.com',@subject = 'Scheduler Has been restarted',@body = 'The Boss Scheduler was not responding this morning and has been restarted.'
--*/
--    Check scheduler:
--     select Status, * from Boss.Boss2000.dbo.tblPNLSchedule where TimeToRun > getdate() - 0.5 order by TimeToRun desc

--  delete  SystemCheck where Day = '16 Dec 08'
--  Failed
--EXEC InsertSystemCheck @Day, 'Boss scheduler',			'General Failure'
--EXEC InsertSystemCheck @Day, 'Outside email',			'General Failure'
--EXEC InsertSystemCheck @Day, 'Phones outgoing',			'General Failure'
--EXEC InsertSystemCheck @Day, 'Phones incoming',			'General Failure'
--EXEC InsertSystemCheck @Day, 'Phones internal',			'General Failure'
--EXEC InsertSystemCheck @Day, 'Cougar',					'General Failure'
--EXEC InsertSystemCheck @Day, 'Lynx',					'General Failure'
--EXEC InsertSystemCheck @Day, 'MMA1',					'General Failure'
--EXEC InsertSystemCheck @Day, 'MMA2',					'General Failure'
--EXEC InsertSystemCheck @Day, 'Koala',					'Unavailable'
--EXEC InsertSystemCheck @Day, 'Domiziano',				'Unavailable'
--EXEC InsertSystemCheck @Day, 'Check database backups',	'General Failure'
--EXEC InsertSystemCheck @Day, 'Check database backups',	'A few transaction failures only'
--EXEC InsertSystemCheck @Day, 'Check database backups',	'UKDR5 machine failure'
--EXEC InsertSystemCheck @Day, 'Check database backups',	'Some Restore failures'
--EXEC InsertSystemCheck @Day, 'Check database backups',	'Some Restore failures while copying across the network'
--EXEC InsertSystemCheck @Day, 'UKDR5',	'Some Restore failures due to DR test on previous day'
--EXEC InsertSystemCheck @Day, 'Rhino',	'Some Restore failures due to DR test on previous day'
--EXEC InsertSystemCheck @Day, 'UKDR5',	'Server had rebooted overnight'
--EXEC InsertSystemCheck @Day, 'UKDR5',	'Unavailable'

--EXEC InsertSystemCheck @Day, 'Check database backups',	'MMA2 machine failure'
--EXEC InsertSystemCheck @Day, 'Check database backups',	'Some network file copies failed'
--EXEC InsertSystemCheck @Day, 'Check database backups',	'Some network file copies failed due to permissions'
--EXEC InsertSystemCheck @Day, 'Check database backups',	'Some backup jobs failed as MMA2 was out of disk space'

select Day, i.Name [Check Item], p.Name Outcome 
--   SELECT  * 
from SystemCheck c
JOIN SystemProblem p on p.ProblemCode = c.ProblemCode
JOIN SystemCheckItem i on i.ID = c.CheckID
where Day = @Day

/*
create table SystemCheck(Day datetime not null, CheckID int not null, ProblemCode int not null, Inserted datetime not null, InsertedBy varchar(20) not null )
ALTER table SystemCheck add constraint pkSystemCheck primary key(Day,CheckID)

create table SystemProblem(ProblemCode int identity(0,1) not null, Name varchar(500) not null, Inserted datetime not null, InsertedBy varchar(20) not null )
ALTER table SystemProblem add constraint pkSystemProblems primary key(ProblemCode)

create table SystemCheckItem(ID int identity(1,1) not null, Name varchar(100) not null, Inserted datetime not null, InsertedBy varchar(20)  not null)
ALTER table SystemCheckItem add constraint pkSystemCheckItem primary key(ID)

drop table SystemCheck
drop table SystemProblem
drop table SystemCheckItem

-- #####################################################################################################################
-- #####################################################################################################################
alter proc InsertSystemCheck @Day datetime, @CheckName varchar(100), @ProblemName varchar(500)
as
declare @CheckID int, @ProblemCode int

select @CheckID = ID from SystemCheckItem where name = @CheckName
select @ProblemCode = ProblemCode from SystemProblem where Name = @ProblemName

IF (@CheckID is null) 
begin
	raiserror('Check name does not exist!',17,1)
	return 1
end
IF (@ProblemCode is null) 
begin
	raiserror('Problem description name does not exist!',17,1)
	return 1
end

Insert SystemCheck select @Day, @CheckID, @ProblemCode, Getdate(), system_user
*/
go

