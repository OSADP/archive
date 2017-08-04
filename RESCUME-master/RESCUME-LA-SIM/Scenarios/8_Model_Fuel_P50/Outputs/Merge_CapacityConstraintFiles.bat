SET _rootname_=Fuel_Constraints_BASE
SET _starthr_=0
SET _endhr_=47

IF EXIST %_rootname_%_Combined.txt DEL %_rootname_%_Combined.txt ELSE (
	ECHO. >> %_rootname_%_Combined.txt
)

FOR /L %%i IN (%_starthr_%, 1, %_endhr_%) DO (

	COPY %_rootname_%_Combined.txt + %_rootname_%_%%i.txt %_rootname_%_Combined.txt

)

