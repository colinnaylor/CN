SELECT TOP 1 *
FROM   [dbo].[BloombergDataWarehouse]
WHERE  [BERG_MONIKER] = '{Berg_Moniker}'
       AND downloaded = 1
	   and effective_date ='{date}' ; 
