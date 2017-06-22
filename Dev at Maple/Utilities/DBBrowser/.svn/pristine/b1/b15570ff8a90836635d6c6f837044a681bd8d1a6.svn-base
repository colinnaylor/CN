--http://bretstateham.com/extracting-ssrs-report-rdl-xml-from-the-reportserver-database/
--The first CTE gets the content as a varbinary(max)
--as well as the other important columns for all reports,
--data sources and shared datasets.
WITH ItemContentBinaries
     AS (SELECT ItemID,
                NAME,
                [Type],
                CASE Type
                  WHEN 2 THEN 'Report'
                  WHEN 5 THEN 'Data Source'
                  WHEN 7 THEN 'Report Part'
                  WHEN 8 THEN 'Shared Dataset'
                  ELSE 'Other'
                END                              AS TypeDescription,
                CONVERT(VARBINARY(max), Content) AS Content
         FROM   ReportServer.dbo.Catalog
         WHERE  Type IN ( 2, 5, 7, 8 )),
     --The second CTE strips off the BOM if it exists...
     ItemContentNoBOM
     AS (SELECT ItemID,
                NAME,
                [Type],
                TypeDescription,
                CASE
                  WHEN LEFT(Content, 3) = 0xEFBBBF THEN CONVERT(VARBINARY(max), Substring(Content, 4, Len(Content)))
                  ELSE Content
                END AS Content
         FROM   ItemContentBinaries)
--The old outer query is now a CTE to get the content in its xml form only...
,
     ItemContentXML
     AS (SELECT ItemID,
                NAME,
                [Type],
                TypeDescription,
                CONVERT(XML, Content) AS ContentXML
         FROM   ItemContentNoBOM where content like '%{0}%')
--now use the XML data type to extract the queries, and their command types and text....
SELECT  distinct 
				ItemID,
                NAME,
                [Type],
                 TypeDescription
                
FROM   ItemContentXML
       --Get all the Query elements (The "*:" ignores any xml namespaces)
       CROSS APPLY ItemContentXML.ContentXML.nodes('//*:Query') Queries(Query) 
