@echo off

xcopy bin\release\*.* S:\dev\ApplicationServer\BloombergFieldValueRetriever\*.* /y

echo.
echo.

dir S:\dev\ApplicationServer\BloombergFieldValueRetriever /od
