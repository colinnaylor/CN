--http://bretstateham.com/extracting-ssrs-report-rdl-xml-from-the-reportserver-database/
SELECT 
                NAME,
                [Type] as OriginalType,
                CASE Type
                  WHEN 2 THEN 'Report'
                  WHEN 5 THEN 'Data Source'
                  WHEN 7 THEN 'Report Part'
                  WHEN 8 THEN 'Shared Dataset'
                  ELSE 'Other'
                END                              AS [Type],
                CONVERT(VARBINARY(max), Content) AS Content
         FROM   ReportServer.dbo.Catalog
         WHERE  Type IN ( 2, 5, 7, 8 )