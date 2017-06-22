@echo off

echo.
echo    Copying any newer files to the Release folder.
echo.

xcopy *.* ..\..\Release\Desktop\*.* /EXCLUDE:PublishExclude.txt /d /y

echo.
pause

