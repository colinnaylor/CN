@echo off

REM LS 2009-01-05 13:00 - VS might not be installed. Use local copy of mage.
REM Set the enviroment
REM call "C:\Program Files\Microsoft Visual Studio 9.0\VC\vcvarsall.bat"

REM Update the deployment provider URL
C:\Mage\mage -Update %1 -pu %1

REM Sign the manifest with our key
c:\Mage\mage -Sign %1 -CertFile S:\Dev\MapleKeyLong.pfx -Password Ryder14
