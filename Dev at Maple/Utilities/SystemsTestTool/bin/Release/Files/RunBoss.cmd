@echo off

S:\Apps\robocopy.exe S:\Apps\BossUI\Live C:\Maple\BossUI\Live /MIR /W:1
cd /d C:\Maple\BossUI\Live\
start C:\Maple\BossUI\Live\BossUI.exe %3
