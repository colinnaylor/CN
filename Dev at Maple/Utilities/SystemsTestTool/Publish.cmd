@echo off

echo.
echo.
dir bin\release\*.* 

pause

echo.
echo.
xcopy bin\release\*.* S:\Dev\Test\SystemsTestTool\*.* /s /y /d

echo.
echo.
dir S:\Dev\Test\SystemsTestTool\*.* /od
echo.
echo.

