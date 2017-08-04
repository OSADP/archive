TITLE                                   PlanSelect Default Control Keys
REPORT_DIRECTORY                        
REPORT_FILE                                                     //---- [report_directory]filename[_partition][.prn]
REPORT_FLAG                             FALSE                   //---- TRUE/FALSE, YES/NO, 1/0, T/F, Y/N
PAGE_LENGTH                             50000                      //---- >= 0
PROJECT_DIRECTORY                       
DEFAULT_FILE_FORMAT                     TAB_DELIMITED           //---- TEXT, BINARY, FIXED_COLUMN, COMMA_DELIMITED, SPACE_DELIMITED, TAB_DELIMITED, CSV_DELIMITED, DBASE, ARCVIEW, SQLITE3, VERSION3, TPPLUS, CUBE, TRANSCAD
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

PLAN_FILE                               Demand\Plans_TRN_SCEN                        //---- [project_directory]filename.*
PLAN_FORMAT                             TAB_DELIMITED           //---- TEXT, BINARY, FIXED_COLUMN, COMMA_DELIMITED, SPACE_DELIMITED, TAB_DELIMITED, CSV_DELIMITED, DBASE, ARCVIEW, SQLITE3, VERSION3, TPPLUS, CUBE, TRANSCAD
NEW_SELECTION_FILE                                              //---- [project_directory]filename.*
NEW_SELECTION_FORMAT                    TAB_DELIMITED           //---- TEXT, BINARY, FIXED_COLUMN, COMMA_DELIMITED, SPACE_DELIMITED, TAB_DELIMITED, CSV_DELIMITED, DBASE, ARCVIEW, SQLITE3, VERSION3, TPPLUS, CUBE, TRANSCAD
NODE_FILE                               ..\..\Common_Inputs\1_Network_v6\ASCIIFiles\Node.csv                        //---- [project_directory]filename
NODE_FORMAT                             TAB_DELIMITED           //---- TEXT, BINARY, FIXED_COLUMN, COMMA_DELIMITED, SPACE_DELIMITED, TAB_DELIMITED, CSV_DELIMITED, DBASE, ARCVIEW, SQLITE3, VERSION3, TPPLUS, CUBE, TRANSCAD
LINK_FILE                               ..\..\Common_Inputs\1_Network_v6\ASCIIFiles\Link.csv                        //---- [project_directory]filename
LINK_FORMAT                             TAB_DELIMITED           //---- TEXT, BINARY, FIXED_COLUMN, COMMA_DELIMITED, SPACE_DELIMITED, TAB_DELIMITED, CSV_DELIMITED, DBASE, ARCVIEW, SQLITE3, VERSION3, TPPLUS, CUBE, TRANSCAD
LANE_USE_FILE                                                   //---- [project_directory]filename
LANE_USE_FORMAT                         TAB_DELIMITED           //---- TEXT, BINARY, FIXED_COLUMN, COMMA_DELIMITED, SPACE_DELIMITED, TAB_DELIMITED, CSV_DELIMITED, DBASE, ARCVIEW, SQLITE3, VERSION3, TPPLUS, CUBE, TRANSCAD
CONNECTION_FILE                         ..\..\Common_Inputs\1_Network_v6\ASCIIFiles\Connection.csv                        //---- [project_directory]filename
CONNECTION_FORMAT                       TAB_DELIMITED           //---- TEXT, BINARY, FIXED_COLUMN, COMMA_DELIMITED, SPACE_DELIMITED, TAB_DELIMITED, CSV_DELIMITED, DBASE, ARCVIEW, SQLITE3, VERSION3, TPPLUS, CUBE, TRANSCAD
LOCATION_FILE                           ..\..\Common_Inputs\1_Network_v6\ASCIIFiles\Connection.csv                        //---- [project_directory]filename
LOCATION_FORMAT                         TAB_DELIMITED           //---- TEXT, BINARY, FIXED_COLUMN, COMMA_DELIMITED, SPACE_DELIMITED, TAB_DELIMITED, CSV_DELIMITED, DBASE, ARCVIEW, SQLITE3, VERSION3, TPPLUS, CUBE, TRANSCAD
VEHICLE_TYPE_FILE                       ..\..\Common_Inputs\1_Network_v6\ASCIIFiles\Vehicle_Type.csv                        //---- [project_directory]filename
VEHICLE_TYPE_FORMAT                     TAB_DELIMITED           //---- TEXT, BINARY, FIXED_COLUMN, COMMA_DELIMITED, SPACE_DELIMITED, TAB_DELIMITED, CSV_DELIMITED, DBASE, ARCVIEW, SQLITE3, VERSION3, TPPLUS, CUBE, TRANSCAD
PERFORMANCE_FILE                                                //---- [project_directory]filename
PERFORMANCE_FORMAT                      TAB_DELIMITED           //---- TEXT, BINARY, FIXED_COLUMN, COMMA_DELIMITED, SPACE_DELIMITED, TAB_DELIMITED, CSV_DELIMITED, DBASE, ARCVIEW, SQLITE3, VERSION3, TPPLUS, CUBE, TRANSCAD
NEW_PLAN_FILE                           Demand\Plans_TRN_SCEN_NonEVAC                        //---- [project_directory]filename.*
NEW_PLAN_FORMAT                         TAB_DELIMITED           //---- TEXT, BINARY, FIXED_COLUMN, COMMA_DELIMITED, SPACE_DELIMITED, TAB_DELIMITED, CSV_DELIMITED, DBASE, ARCVIEW, SQLITE3, VERSION3, TPPLUS, CUBE, TRANSCAD
SAVE_LANE_USE_FLOWS                     FALSE                   //---- TRUE/FALSE, YES/NO, 1/0, T/F, Y/N

#---- Data Service Keys ----

DAILY_WRAP_FLAG                         FALSE                   //---- TRUE/FALSE, YES/NO, 1/0, T/F, Y/N
SUMMARY_TIME_RANGES                     ALL                     //---- e.g., ALL, 0..97200 seconds, 0.0..27.0 hours, 0:00..27:00
SUMMARY_TIME_INCREMENT                  15 minutes              //---- 0, 2..240 minutes

#---- Select Service Keys ----

SELECT_HOUSEHOLDS                       ALL                     //---- e.g., 1, 2, 4..10, 100..200, 300
SELECT_MODES                            ALL                     //---- e.g., ALL or 1, 12..14 or WALK, HOV2..HOV4
SELECT_PURPOSES                         1,2,3,4                     //---- e.g., 1, 2, 4..10, 100..200, 300
SELECT_PRIORITIES                       ALL                     //---- ALL, or 0..4, or LOW, MEDIUM, HIGH, CRITICAL
SELECT_START_TIMES                      ALL                     //---- e.g., ALL, 0..97200 seconds, 0.0..27.0 hours, 0:00..27:00
SELECT_END_TIMES                        ALL                     //---- e.g., ALL, 0..97200 seconds, 0.0..27.0 hours, 0:00..27:00
SELECT_ORIGINS                          ALL                     //---- e.g., 1, 2, 4..10, 100..200, 300
SELECT_DESTINATIONS                     ALL                     //---- e.g., 1, 2, 4..10, 100..200, 300
SELECT_VEHICLE_TYPES                    ALL                     //---- e.g., 1, 2, 4..10, 100..200, 300
SELECT_TRAVELER_TYPES                   5                     //---- e.g., 1, 2, 4..10, 100..200, 300
SELECT_FACILITY_TYPES                   ALL                     //---- FREEWAY..EXTERNAL
SELECT_PARKING_LOTS                     ALL                     //---- e.g., 1, 2, 4..10, 100..200, 300
SELECT_VC_RATIOS                        0.0                     //---- 0.0, >1.0
SELECT_TIME_RATIOS                      0.0                     //---- 0.0, >1.0
SELECT_LINKS_1                          ALL                     //---- e.g., 1, 2, 4..10, 100..200, 300
SELECT_NODES_1                          ALL                     //---- e.g., 1, 2, 4..10, 100..200, 300
SELECT_SUBAREAS                                                 //---- e.g., 1, 2, 4..10, 100..200, 300
SELECT_ORIGIN_ZONES                     ALL                     //---- e.g., 1, 2, 4..10, 100..200, 300
SELECT_DESTINATION_ZONES                ALL                     //---- e.g., 1, 2, 4..10, 100..200, 300
SELECTION_POLYGON                                               //---- [project_directory]filename
PERCENT_TIME_DIFFERENCE                 0.0 percent             //---- 0.0..100.0 percent
MINIMUM_TIME_DIFFERENCE                 1 minutes               //---- 0..120 minutes
MAXIMUM_TIME_DIFFERENCE                 60 minutes              //---- 0..1440 minutes
SELECTION_PERCENTAGE                    100.0 percent           //---- 0.01..100.0 percent
MAXIMUM_PERCENT_SELECTED                100.0 percent           //---- 0.1..100.0 percent
DELETION_FILE                                                   //---- [project_directory]filename
DELETION_FORMAT                         TAB_DELIMITED           //---- TEXT, BINARY, FIXED_COLUMN, COMMA_DELIMITED, SPACE_DELIMITED, TAB_DELIMITED, CSV_DELIMITED, DBASE, ARCVIEW, SQLITE3, VERSION3, TPPLUS, CUBE, TRANSCAD
DELETE_HOUSEHOLDS                       NONE                    //---- e.g., 1, 2, 4..10, 100..200, 300
DELETE_MODES                            NONE                    //---- e.g., ALL or 1, 12..14 or WALK, HOV2..HOV4
DELETE_TRAVELER_TYPES                   NONE                    //---- e.g., 1, 2, 4..10, 100..200, 300
