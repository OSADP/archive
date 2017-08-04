:: BASE Routing, Use Existing Plans
CALL v6 bin64thread Router.exe -k 2_Router_Hwy_BASE.ctl
CALL v6 bin64thread Router.exe -k 2_Router_Hwy_BASE_Update.ctl

:: Scenario Routing
CALL v6 bin64thread Router.exe -k 2_Router_Hwy_SCEN.ctl
CALL v6 bin64thread Router.exe -k 2_Router_Hwy_SCEN_Update.ctl