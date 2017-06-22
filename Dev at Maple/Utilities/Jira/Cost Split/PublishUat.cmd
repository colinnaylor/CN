@echo off

echo.
xcopy bin\Debug\*.* "S:\Dev\Test\Jira\Cost Split\*.*" /s /y
echo.
echo.
S:
cd "S:\Dev\Test\Jira\Cost Split"
Call ConfigureFor Uat
C:

dir "S:\Dev\Test\Jira\Cost Split\*.*" /od
echo.
