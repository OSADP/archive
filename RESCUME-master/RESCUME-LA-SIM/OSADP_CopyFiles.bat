@ECHO OFF

PAUSE
SET _sourceroot_=R:\EVAC_NewOrleans\Scen_2A_00_Base
SET _destroot_=R:\EVAC_NewOrleans\OSADP\Scenarios

:: Copy Folder Directory
::XCOPY /t /e %_sourceroot_% %_destroot_%


:: This command Copies the entire folder structure and all files in sub-directory
 REM XCOPY /d /s /e %_sourceroot_%\*.ctl %_destroot_%\*.ctl
  XCOPY /d /s /e %_sourceroot_%\*.bat %_destroot_%\*.bat


REM FOR /f %%i IN ('DIR /s /ad /b %_sourceroot_%') DO (

REM ECHO  %%~ni >> test.txt

REM CD /d %%i 

REM XCOPY /d %_sourceroot_%\%%i\*.ctl %_destroot_%\%%i\*.ctl
REM XCOPY /d %_sourceroot_%\%%i\*.bat %_destroot_%\%%i\*.bat

REM ECHO %_sourceroot_%\%%i\

)

PAUSE