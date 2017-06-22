@ECHO OFF

REM ----------------------------------------------------------------------------
REM DR workstation configuration script
REM Last modified by Lorenzo Stoakes on 2009-07-09
REM ----------------------------------------------------------------------------

REM At the outset, you should decide which databases need to be run on which
REM servers. Refer to the SQL Script to determine which servers have
REM been set up.

REM Set alias for sql servers --------------------------------------------------

    REM Edit alias.reg if you need to alias server names to a server other than
    REM KOALA.

    Regedit /s alias.reg

REM /Set alias for sql servers -------------------------------------------------


REM Re-run login script --------------------------------------------------------

    REM Ensure paths are correctly set up by re-running login script.

    CALL C:\LoginScripts\login.cmd

REM /Re-run login script -------------------------------------------------------


REM Copy Maple DSN to local folder ---------------------------------------------

    IF NOT EXIST "C:\%HOMEPATH%\Local Settings\Maple\" MKDIR "C:\%HOMEPATH%\Local Settings\Maple\"

    COPY /Y MapleDSN.txt "C:\%HOMEPATH%\Local Settings\Maple\"

REM /Copy Maple DSN to local folder --------------------------------------------

REM Set Excel macro security level to low -------------------------------------

    Regedit /s ExcelMacroSecurityLevel.reg

REM /Set Excel macro security level to low ------------------------------------


REM Set Access macro security level to low ------------------------------------

    Regedit /s AccessMacroSecurityLevel.reg

REM /Set Access macro security level to low -----------------------------------


REM Point Excel at Vision add-in ----------------------------------------------

    CALL Regedit /s VisionAddIn.reg

REM /Point Excel at Vision add-in ---------------------------------------------

REM Register TradeReporter DLLs -----------------------------------------------

    CALL RegisterTradereporterDLLs.bat

REM /Register TradeReporter DLLs ----------------------------------------------