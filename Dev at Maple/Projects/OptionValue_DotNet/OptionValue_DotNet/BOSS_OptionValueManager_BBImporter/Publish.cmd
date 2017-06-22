@echo off

echo.
xcopy bin\release\*.* S:\dev\ApplicationServer\OTCOptionValuation_BBImporter\*.* /s /y /d
echo.
echo.
dir S:\dev\ApplicationServer\OTCOptionValuation_BBImporter /od

echo.
