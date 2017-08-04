@echo off

echo Enter IP Address of Arada Radio: 
set /p IpAddress=
echo Enter Application Mode (responder/oncoming):
set /p AppMode=

echo Killing application...
@echo on
plink.exe -ssh root@%IpAddress% -pw password "killall rescume; rm -rf /var/bin/*"
@echo off

IF %AppMode% == responder ( 
	echo Installing In Responder Mode
	@echo on
	pscp.exe -r -scp -pw password ./responder/* root@%IpAddress%:/var/bin/.
	@echo off
) ELSE (
	IF %AppMode% == oncoming (
		echo Installing In Oncoming Mode
		@echo on
		pscp.exe -r -scp -pw password ./oncoming/* root@%IpAddress%:/var/bin/.
		@echo off
	) ELSE ( 
		echo ERR: Select "responder" or "oncoming"
	)
) 

echo Changing permissions of executable...
plink.exe -ssh root@%IpAddress% -pw password "chmod +x /var/bin/rescume"

pause