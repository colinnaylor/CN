------------------------------------------------------------------------------------------------------------------------
-- DR Developer Checks Script
-- Last edited by Lorenzo Stoakes 2009-06-16
------------------------------------------------------------------------------------------------------------------------

-- LIST BOSS2000 LINKED SERVER SP's TO FIX -----------------------------------------------------------------------------

--  Need to replace all Linked server usage to MMA in Boss when MMA and boss are installed onto the same machine,
--  otherwise we will get an error when trying to retrieve data FROM the Linked server (which is on the same machine).
--  Need to programmatically do this but for now it is a manual job Change MMA.Fmop2.dbo.Table to Fmop2.dbo.Table The
--  following gets a list of procs that need to be changed: OR script all procedures and do an edit.replace.

'Easier to script all Procedures and perform a global replace'   Takes too long !!!!!!!!
--  Only two Views, one trigger and two IFs today that needed changing !

'Watch out for the synonym called MMAtrades'

use Boss2000
go
SELECT DISTINCT o.Type, o.type_Desc, o.Name FROM sys.objects o
JOIN syscomments c ON c.ID = o.object_id
WHERE (  TEXT LIKE '%MMA.%' OR  TEXT LIKE '%Mammoth.%'  )
	AND TEXT NOT LIKE '%EMMA.Potter%' and TYPE != 'p'

SELECT DISTINCT o.Type, o.type_Desc, o.Name FROM sys.objects o
JOIN syscomments c ON c.ID = o.object_id
WHERE TEXT LIKE '%Mammoth.%'
	and TYPE = 'p'

SELECT DISTINCT o.Type, o.type_Desc, o.Name FROM sys.objects o
JOIN syscomments c ON c.ID = o.object_id
WHERE TEXT LIKE '%MMA.%'
	and TYPE = 'p'

GO

