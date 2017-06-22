@echo off

IF /I (%1)==(/u) goto :Unst

echo Installing Maple SWift Importer Service...
echo ---------------------------------------------------
InstallUtil /i bin\Debug\SwiftImporterService.exe
echo ---------------------------------------------------
echo Done.
goto :eof

:Unst
echo Uninstalling Maple SWift Importer Service...
echo ---------------------------------------------------
InstallUtil /u bin\Debug\SwiftImporterService.exe
echo ---------------------------------------------------
echo Done.
goto :eof
