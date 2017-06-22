@echo off

echo.
xcopy InstallUtil.exe bin\debug\*.*
xcopy InstallUtil.exe.config bin\debug\*.*
xcopy InstallUtilLib.dll bin\debug\*.*

xcopy bin\Debug\*.* S:\dev\Test\SwiftImporterService\*.* /s /y
echo.
echo.
dir S:\dev\Test\SwiftImporterService\*.* /od
echo.

