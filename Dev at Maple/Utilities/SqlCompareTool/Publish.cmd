@echo off
xcopy bin\release\*.* C:\maple\SqlCompare\*.* /s /y
echo.
echo.
dir C:\maple\SqlCompare\*.*
echo.
echo.
