CALL v6 FileFormat.exe -k 5_FileFormat_PlanCompareExcludeSet.ctl

COPY Results\Selection_BASE_Problems.csv + Results\Selection_SCEN_Problems.csv Results\Deletion_Problems.csv
DEL Results\Selection_BASE_Problems.csv
DEL Results\Selection_SCEN_Problems.csv
