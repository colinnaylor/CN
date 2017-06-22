@echo off

echo.
xcopy bin\release\*.* S:\dev\ApplicationServer\SupportRequestNotify\*.* /Y
echo.
echo.

dir S:\dev\ApplicationServer\SupportRequestNotify\*.* /od

echo.
echo.
