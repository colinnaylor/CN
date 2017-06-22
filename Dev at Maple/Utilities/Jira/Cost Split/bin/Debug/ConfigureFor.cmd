@echo off

SET AppName=Cost Split.exe.config

IF (%1)==() goto noargs
IF /I (%1)==(Debug) goto Dev
IF /I (%1)==(UAT) goto Uat
IF /I (%1)==(Release) goto Release
goto noargs

:Dev
echo    Copy Debug file to %AppName%
xcopy Config\Debug_app.config "%AppName%" /y
echo.
goto :eof


:Uat
echo    Copy Uat file to %AppName%
xcopy Config\Uat_app.config "%AppName%" /y
echo.
goto :eof


:Release
echo    Copy Release file to %AppName%
xcopy Config\Release_app.config "%AppName%" /y
echo.
goto :eof

:noargs
echo.
echo    You must provide an argument of Dev, Uat or Release
echo.

