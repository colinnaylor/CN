-- DR Workstation Procedure -- sql
------------------------------------------------------------------------------------------------------------------------
  -- DR SQL Server reversal Script
  -- Last edited by Colin Naylor 2013 02 26
------------------------------------------------------------------------------------------------------------------------

--########################################################################################################
--Step --  Making the aliases file writable -- cmd
attrib Files\alias.reg -r

--########################################################################################################
--Step --  Renaming the server in the sql aliases file -- cmd
Replace Files\alias.reg %SQLserver% %SqlServerName%

--########################################################################################################
--Step --  Set sql aliases in the registry -- cmd --Admin

Regedit /s Files\alias.reg

--########################################################################################################
--Step --  Copy Maple DSN to local folder -- cmd

XCOPY Files\MapleDSN.txt "C:%HOMEPATH%\AppData\Local\Maple\*.*" /y

--########################################################################################################
--Step --  Set Excel macro security level to low -- cmd --Admin

Regedit /s Files\ExcelMacroSecurityLevel.reg

--########################################################################################################
--Step --  Set Access macro security level to low -- cmd --Admin

Regedit /s Files\AccessMacroSecurityLevel.reg

--########################################################################################################
--Step --  Point Excel at Vision addin -- cmd --Admin

Regedit /s Files\VisionAddIn.reg

--########################################################################################################
--Step --  Register TradeReporter DLLs -- cmd --Admin

Call Files\RegisterTradereporterDLLs.bat

--########################################################################################################
--Step --  Ensure Risk folder exists -- cmd

if not exist C:\Maple\Risk md C:\Maple\Risk

--########################################################################################################
--Step --  Update Risk Lookup Values -- sql
use rm_var
GO
UPDATE LookupValue SET Value = replace(Value,'\\LondonFS1\','\\%FileServerName%\') Where Value like '\\LondonFS1%'

--########################################################################################################
--Step --  Copy Desktop items to public Desktop -- cmd --Admin

XCOPY "Desktop\*.*" "%public%\Desktop\*.*" /y

