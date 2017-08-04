SET _rootname_=Capacity_Constraints_BASE
SET _starthr_=0
SET _endhr_=55

IF EXIST %_rootname_%_Combined.txt DEL %_rootname_%_Combined.txt ELSE (
	ECHO. >> %_rootname_%_Combined.txt
)

FOR /L %%i IN (%_starthr_%, 1, %_endhr_%) DO (

	COPY %_rootname_%_Combined.txt + %_rootname_%_%%i.txt %_rootname_%_Combined.txt

)

