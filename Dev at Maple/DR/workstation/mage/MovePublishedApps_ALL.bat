@echo off

rem To do just the directory level you can use the following but some of the folders
rem have app nested beneath them.
rem    for /D %%a in (*.*) do for %%f in (%%a\*.application) do MovePublishedApps.bat %%a

rem So we call the 'for' recursively so that it updates all .application files found.
rem This will result in it updating all of them, even beneath the main setup folder
rem but this does not cause any problems.

for /R %%a in (*.*) do MovePublishedApps.bat %%a

