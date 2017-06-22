@echo off

IF not exist C:\Maple md C:\Maple
xcopy s:\Dev\Adb3.dll C:\Maple\*.* /y

