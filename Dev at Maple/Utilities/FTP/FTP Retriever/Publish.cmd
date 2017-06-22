@echo off
dir bin\release\*.*
echo.
pause    Ready to copy to S:\dev\ApplicationServer\FTPretriever
echo.
echo.
echo    Copying to  S:\dev\ApplicationServer\FTPretriever
xcopy bin\release\*.*		S:\dev\ApplicationServer\FTPretriever\*.* /y

pause

