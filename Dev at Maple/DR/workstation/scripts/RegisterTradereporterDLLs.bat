@echo off

rem ----------------------------------------------------------------------------
rem TradeReporter DLL Registration Script
rem Last modified by Lorenzo Stoakes on 09/07/2009
rem ----------------------------------------------------------------------------

if not exist C:\Maple\TradeReporter goto :eof

cd C:\Maple\TradeReporter

rem register all DLLs in silent mode. Hackish but considerably easier than
rem working out which of this *ridiculous* number of DLLs we need to be
rem registered. You'd think they'd work being in the same directory an' all...

for %%i in (*.dll) do regsvr32 /s %%i