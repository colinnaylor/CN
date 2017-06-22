@echo off

echo.
echo    Copying any newer files to the Release folder.
echo.

xcopy *.* ..\..\..\Release\*.* /EXCLUDE:PublishExclude.txt /d /y /s

echo.
pause

