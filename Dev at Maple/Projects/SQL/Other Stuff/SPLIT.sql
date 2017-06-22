CREATE FUNCTION [dbo].[split](@delimited varchar(MAX),@delimiter varchar(100)) 
RETURNS @t TABLE (ID INT IDENTITY(1,1), Val varchar(MAX))
AS
begin
    DECLARE @xml XML
    SET @xml = N'<t>' + REPLACE(@delimited,@delimiter,'</t><t>') + '</t>'

    INSERT INTO @t(val)
    SELECT  r.value('.','varchar(MAX)') as item
    FROM  @xml.nodes('/t') as records(r)

    RETURN
end

