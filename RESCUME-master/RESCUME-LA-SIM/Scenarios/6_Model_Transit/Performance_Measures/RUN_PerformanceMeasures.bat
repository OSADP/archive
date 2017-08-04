
:: LinkSum for individual link performance
REM CALL v6 bin64thread LinkSum.exe -k LinkSum_Perf_BASE.ctl
REM CALL v6 bin64thread LinkSum.exe -k LinkSum_Perf_SCEN.ctl
REM CALL v6 bin64thread LinkSum.exe -k LinkSum_PerfDiff.ctl

:: ArcPerf for Volume Bandwidth
REM CALL v6 bin64thread ArcPerf.exe -k ArcPerf_PerfMeas_Volume_Bandwidth_BASE.ctl
REM CALL v6 bin64thread ArcPerf.exe -k ArcPerf_PerfMeas_Volume_Bandwidth_SCEN.ctl

:: ArcPerf for performance difference bandwidth
REM CALL v6 bin64thread ArcPerf.exe -k ArcPerf_PerfMeas_VehicleVolumeDIFF_Bandwidth.ctl
REM CALL v6 bin64thread ArcPerf.exe -k ArcPerf_PerfMeas_DelayDIFF_Bandwidth.ctl

:: PathSkim for Travel time stats
REM CALL v6 bin64thread PathSkim.exe -k PathSkim_BASE.ctl
REM CALL v6 bin64thread PathSkim.exe -k PathSkim_SCEN.ctl
REM CALL v6 PathSkim.exe -k PathSkim_BASE.ctl


:: ArcPlan for Drawing Plans
CALL v6 bin64thread ArcPlan.exe -k ArcPlan_DrawPlans_BASE.ctl
CALL v6 bin64thread ArcPlan.exe -k ArcPlan_DrawPlans_SCEN.ctl

