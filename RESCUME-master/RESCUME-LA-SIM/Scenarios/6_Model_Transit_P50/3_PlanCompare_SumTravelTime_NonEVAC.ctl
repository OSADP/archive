TITLE                                   PlanCompare Default Control Keys
REPORT_DIRECTORY                        
REPORT_FILE                                                     //---- [report_directory]filename[_partition][.prn]
REPORT_FLAG                             FALSE                   //---- TRUE/FALSE, YES/NO, 1/0, T/F, Y/N
PAGE_LENGTH                             32767                      //---- >= 0
PROJECT_DIRECTORY                       
DEFAULT_FILE_FORMAT                     COMMA_DELIMITED           //---- TEXT, BINARY, FIXED_COLUMN, COMMA_DELIMITED, SPACE_DELIMITED, TAB_DELIMITED, CSV_DELIMITED, DBASE, ARCVIEW, SQLITE3, VERSION3, TPPLUS, CUBE, TRANSCAD
TIME_OF_DAY_FORMAT                      DAY_TIME                //---- SECONDS, MINUTES, HOURS, HOUR_CLOCK, DAY_TIME, TIME_CODE, HOUR_MINUTE
MODEL_START_TIME                        0:00                    //---- >= 0 [seconds], 0.0 [hours], 0:00
MODEL_END_TIME                          60:00                   //---- > [model_start_time]
MODEL_TIME_INCREMENT                    15 minutes              //---- 0, 2..240 minutes
UNITS_OF_MEASURE                        METRIC                 //---- METRIC, ENGLISH
DRIVE_SIDE_OF_ROAD                      RIGHT_SIDE              //---- RIGHT_SIDE, LEFT_SIDE
RANDOM_NUMBER_SEED                      0                       //---- 0 = computer clock, > 0 = fixed
MAX_WARNING_MESSAGES                    100000                  //---- >= 0
MAX_WARNING_EXIT_FLAG                   TRUE                    //---- TRUE/FALSE, YES/NO, 1/0, T/F, Y/N
MAX_PROBLEM_COUNT                       0                       //---- >= 0
NUMBER_OF_THREADS                       1                       //---- 1..128

#---- System File Keys ----

PLAN_FILE                               Demand\Plans_TRN_SCEN_Sorted                        //---- [project_directory]filename.*
PLAN_FORMAT                             TAB_DELIMITED           //---- TEXT, BINARY, FIXED_COLUMN, COMMA_DELIMITED, SPACE_DELIMITED, TAB_DELIMITED, CSV_DELIMITED, DBASE, ARCVIEW, SQLITE3, VERSION3, TPPLUS, CUBE, TRANSCAD
SELECTION_FILE                                                  //---- [project_directory]filename.*
SELECTION_FORMAT                        TAB_DELIMITED           //---- TEXT, BINARY, FIXED_COLUMN, COMMA_DELIMITED, SPACE_DELIMITED, TAB_DELIMITED, CSV_DELIMITED, DBASE, ARCVIEW, SQLITE3, VERSION3, TPPLUS, CUBE, TRANSCAD
NEW_SELECTION_FILE                                              //---- [project_directory]filename.*
NEW_SELECTION_FORMAT                    TAB_DELIMITED           //---- TEXT, BINARY, FIXED_COLUMN, COMMA_DELIMITED, SPACE_DELIMITED, TAB_DELIMITED, CSV_DELIMITED, DBASE, ARCVIEW, SQLITE3, VERSION3, TPPLUS, CUBE, TRANSCAD
NEW_PLAN_FILE                                                   //---- [project_directory]filename.*
NEW_PLAN_FORMAT                         TAB_DELIMITED           //---- TEXT, BINARY, FIXED_COLUMN, COMMA_DELIMITED, SPACE_DELIMITED, TAB_DELIMITED, CSV_DELIMITED, DBASE, ARCVIEW, SQLITE3, VERSION3, TPPLUS, CUBE, TRANSCAD

#---- Data Service Keys ----

DAILY_WRAP_FLAG                         FALSE                   //---- TRUE/FALSE, YES/NO, 1/0, T/F, Y/N
SUMMARY_TIME_RANGES                     ALL                     //---- e.g., ALL, 0..97200 seconds, 0.0..27.0 hours, 0:00..27:00
SUMMARY_TIME_INCREMENT                  0 minutes              //---- 0, 2..240 minutes
PERIOD_CONTROL_POINT                    START                //---- START, END, MID-TRIP

#---- Select Service Keys ----

SELECT_HOUSEHOLDS                       ALL                     //---- e.g., 1, 2, 4..10, 100..200, 300
SELECT_MODES                            ALL                     //---- e.g., ALL or 1, 12..14 or WALK, HOV2..HOV4
SELECT_PURPOSES                         ALL                     //---- e.g., 1, 2, 4..10, 100..200, 300
SELECT_START_TIMES                      ALL                     //---- e.g., ALL, 0..97200 seconds, 0.0..27.0 hours, 0:00..27:00
SELECT_END_TIMES                        ALL                     //---- e.g., ALL, 0..97200 seconds, 0.0..27.0 hours, 0:00..27:00
SELECT_ORIGINS                          ALL                     //---- e.g., 1, 2, 4..10, 100..200, 300
SELECT_DESTINATIONS                     ALL                     //---- e.g., 1, 2, 4..10, 100..200, 300
SELECT_VEHICLE_TYPES                    ALL                     //---- e.g., 1, 2, 4..10, 100..200, 300
SELECT_TRAVELER_TYPES                   5                     //---- e.g., 1, 2, 4..10, 100..200, 300
SELECT_LINKS_1                          ALL                     //---- e.g., 1, 2, 4..10, 100..200, 300
SELECT_NODES_1                          ALL                     //---- e.g., 1, 2, 4..10, 100..200, 300
SELECT_SUBAREAS                                                 //---- e.g., 1, 2, 4..10, 100..200, 300
SELECTION_POLYGON                                               //---- [project_directory]filename
PERCENT_TIME_DIFFERENCE                 0.0 percent             //---- 0.0..100.0 percent
MINIMUM_TIME_DIFFERENCE                 1 minutes               //---- 0..120 minutes
MAXIMUM_TIME_DIFFERENCE                 1440 minutes              //---- 0..1440 minutes
PERCENT_COST_DIFFERENCE                 0.0 percent             //---- 0.0..100.0 percent
MINIMUM_COST_DIFFERENCE                 10 impedance            //---- 0..500 impedance
MAXIMUM_COST_DIFFERENCE                 10000 impedance          //---- 0..10000 impedance
PERCENT_PATH_DIFFERENCE                 0.0 percent             //---- 0.0..100.0 percent
MINIMUM_PATH_DIFFERENCE                 1 link                  //---- 0..50 links
MAXIMUM_PATH_DIFFERENCE                 1000 links                //---- 0..1000 links
SELECTION_PERCENTAGE                    100.0 percent           //---- 0.01..100.0 percent
MAXIMUM_PERCENT_SELECTED                100.0 percent           //---- 0.1..100.0 percent
DELETION_FILE                                                   //---- [project_directory]filename
DELETION_FORMAT                         COMMA_DELIMITED           //---- TEXT, BINARY, FIXED_COLUMN, COMMA_DELIMITED, SPACE_DELIMITED, TAB_DELIMITED, CSV_DELIMITED, DBASE, ARCVIEW, SQLITE3, VERSION3, TPPLUS, CUBE, TRANSCAD
DELETE_HOUSEHOLDS                       NONE                    //---- e.g., 1, 2, 4..10, 100..200, 300
DELETE_MODES                            NONE                    //---- e.g., ALL or 1, 12..14 or WALK, HOV2..HOV4
DELETE_TRAVELER_TYPES                   NONE                    //---- e.g., 1, 2, 4..10, 100..200, 300

#---- PlanCompare Control Keys ----

COMPARE_PLAN_FILE                       Demand\Plans_TRN_BASE_Sorted                        //---- [project_directory]filename.*
COMPARE_PLAN_FORMAT                     TAB_DELIMITED           //---- TEXT, BINARY, FIXED_COLUMN, COMMA_DELIMITED, SPACE_DELIMITED, TAB_DELIMITED, CSV_DELIMITED, DBASE, ARCVIEW, SQLITE3, VERSION3, TPPLUS, CUBE, TRANSCAD
COMPARE_GENERALIZED_COSTS               FALSE                   //---- TRUE/FALSE, YES/NO, 1/0, T/F, Y/N
SELECTION_METHOD                        RANDOM                  //---- RANDOM, PERCENT_DIFFERENCE, RELATIVE_GAP
MERGE_PLAN_FILES                        FALSE                   //---- TRUE/FALSE, YES/NO, 1/0, T/F, Y/N
NEW_TIME_DISTRIBUTION_FILE              Outputs\Time_Distribution_NonEVAC.txt                        //---- [project_directory]filename
TIME_DISTRIBUTION_MINIMUM               -360 minutes             //---- -360..0
TIME_DISTRIBUTION_COUNT                 72                     //---- 50..500
TIME_DISTRIBUTION_INCREMENT             10 minutes                //---- 1..60 minutes
NEW_COST_DISTRIBUTION_FILE                                      //---- [project_directory]filename
## NEW_TRIP_TIME_GAP_FILE                  Outputs\Time_Gap_NonEVAC.txt                        //---- [project_directory]filename
NEW_TRIP_COST_GAP_FILE                                          //---- [project_directory]filename
## NEW_TRIP_MATCH_FILE                     Outputs\Trip_Match.txt                        //---- [project_directory]filename.*
NEW_TRIP_MATCH_FORMAT                   COMMA_DELIMITED           //---- TEXT, BINARY, FIXED_COLUMN, COMMA_DELIMITED, SPACE_DELIMITED, TAB_DELIMITED, CSV_DELIMITED, DBASE, ARCVIEW, SQLITE3, VERSION3, TPPLUS, CUBE, TRANSCAD

#---- Program Report Keys ----

PLANCOMPARE_REPORT_1   TOTAL_TIME_SUMMARY                                         //---- program report name
PLANCOMPARE_REPORT_2   TRIP_TIME_GAP_REPORT                                                                //---- TOTAL_TIME_DISTRIBUTION
                                                                //---- PERIOD_TIME_DISTRIBUTIONS
                                                                //---- TOTAL_TIME_SUMMARY
                                                                //---- PERIOD_TIME_SUMMARY
                                                                //---- TOTAL_COST_DISTRIBUTION
                                                                //---- PERIOD_COST_DISTRIBUTIONS
                                                                //---- TOTAL_COST_SUMMARY
                                                                //---- PERIOD_COST_SUMMARY
                                                                //---- TRIP_TIME_GAP_REPORT
                                                                //---- TRIP_COST_GAP_REPORT
