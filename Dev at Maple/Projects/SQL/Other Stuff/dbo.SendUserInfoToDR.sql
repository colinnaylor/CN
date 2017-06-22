SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
Create Proc [dbo].[SendUserInfoToDR]
as

INSERT DR.Admin.dbo.LoginDetail(Source,Name,isNTname,dbName)
SELECT @@ServerName, l.Name, l.isntname, l.dbname
FROM master.dbo.syslogins l
LEFT JOIN DR.Admin.dbo.LoginDetail d ON d.Name = l.Name
Where l.Name not like '##%'
  AND d.Name is null

--  SELECT * FROM DR.Admin.dbo.LoginDetail

GO
