SET NOCOUNT ON

select 'CREATE LOGIN [' + name + '] FROM WINDOWS WITH DEFAULT_DATABASE=[' + dbname + '], DEFAULT_LANGUAGE=[' + language + ']' 
from syslogins 
where isntname = 1 and name != 'NT AUTHORITY\SYSTEM'
and name not like @@servername + '%'

select 'CREATE LOGIN [' + name + '] WITH PASSWORD='''', DEFAULT_DATABASE=[' + dbname + '], DEFAULT_LANGUAGE=[' + language 
	+ '], CHECK_EXPIRATION=OFF, CHECK_POLICY=OFF'
from syslogins 
where isntname = 0 and loginname != 'sa' and hasaccess = 1
and name not like '##%'



