:: LS 2011-09-13: Remove logging as taking up too much space on drive.

:: rem   Note the the formatting of the date depends on the format that is output by the Date command
:: rem SET AccLogFile=%date:~6,4%%date:~3,2%%date:~0,2%-%time:~0,2%%time:~3,2%%time:~6,2%.TXT
:: SET AccLogFile=%date:~10,4%%date:~7,2%%date:~4,2%-%time:~0,2%%time:~3,2%%time:~6,2%.TXT

rem #################################
:: echo Accounts copy starting at %date:~6,4%%date:~3,2%%date:~0,2% %time% > "Accounts\%AccLogFile%"

call MapleRobocopy "\\mpuk\dfs\data\Accounts" "\\DRFS3\D$\export\data\Accounts" 
::>> "Accounts\%AccLogFile%"

::echo Accounts copy completed at %date:~6,4%%date:~3,2%%date:~0,2% %time% >> "Accounts\%AccLogFile%"

rem Add the filenmae to file.txt and then import it into the db
rem echo \\DRFS3\D$\Scripts\Accounts\, %AccLogFile% > AccountsFile.txt
rem CALL bcp Process.dbo.SystemLogImportFile in AccountsFile.txt -S Lynx -U "SystemLogger" -P "logman09:" -f bcp.fmt
rem del AccountsFile.txt 
