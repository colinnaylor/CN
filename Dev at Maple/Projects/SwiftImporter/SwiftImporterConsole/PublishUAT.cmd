@echo off

xcopy bin\UAT\*.* s:\dev\test\SwiftImporterConsole\*.* /y /s

echo.
echo.
dir s:\dev\test\SwiftImporterConsole\*.*
echo.
echo.
