TITLE                                   LinkSum BASE
REPORT_DIRECTORY                        
REPORT_FILE                                                     //---- [report_directory]filename[_partition][.prn]
REPORT_FLAG                             FALSE                   //---- TRUE/FALSE, YES/NO, 1/0, T/F, Y/N
PAGE_LENGTH                             99999                   //---- >= 0
PROJECT_DIRECTORY                       
DEFAULT_FILE_FORMAT                     BINARY                  //---- TEXT, BINARY, FIXED_COLUMN, COMMA_DELIMITED, SPACE_DELIMITED, TAB_DELIMITED, CSV_DELIMITED, DBASE, ARCVIEW, SQLITE3, VERSION3, TPPLUS, CUBE, TRANSCAD
TIME_OF_DAY_FORMAT                      DAY_TIME                //---- SECONDS, MINUTES, HOURS, HOUR_CLOCK, DAY_TIME, TIME_CODE, HOUR_MINUTE
MODEL_START_TIME                        0:00                    //---- >= 0 [seconds], 0.0 [hours], 0:00
MODEL_END_TIME                          60:00                   //---- > [model_start_time]
MODEL_TIME_INCREMENT                    15 minutes              //---- 0, 2..240 minutes
UNITS_OF_MEASURE                        METRIC                  //---- METRIC, ENGLISH
DRIVE_SIDE_OF_ROAD                      RIGHT_SIDE              //---- RIGHT_SIDE, LEFT_SIDE
RANDOM_NUMBER_SEED                      123456789               //---- 0 = computer clock, > 0 = fixed
MAX_WARNING_MESSAGES                    99999                   //---- >= 0
MAX_WARNING_EXIT_FLAG                   FALSE                   //---- TRUE/FALSE, YES/NO, 1/0, T/F, Y/N
MAX_PROBLEM_COUNT                       0                       //---- >= 0
NUMBER_OF_THREADS                       1                       //---- 1..128

#---- System File Keys ----

NODE_FILE                               ..\..\..\..\Common_Inputs\1_Network_v6\ASCIIFiles\Node.csv
NODE_FORMAT                             TAB_DELIMITED           //---- TEXT, BINARY, FIXED_COLUMN, COMMA_DELIMITED, SPACE_DELIMITED, TAB_DELIMITED, CSV_DELIMITED, DBASE, ARCVIEW, SQLITE3, VERSION3, TPPLUS, CUBE, TRANSCAD
LINK_FILE                               ..\..\..\..\Common_Inputs\1_Network_v6\ASCIIFiles\Link.csv
LINK_FORMAT                             TAB_DELIMITED           //---- TEXT, BINARY, FIXED_COLUMN, COMMA_DELIMITED, SPACE_DELIMITED, TAB_DELIMITED, CSV_DELIMITED, DBASE, ARCVIEW, SQLITE3, VERSION3, TPPLUS, CUBE, TRANSCAD
PERFORMANCE_FILE                        Sim_LinkPerformance_15min.csv
PERFORMANCE_FORMAT                      TAB_DELIMITED           //---- TEXT, BINARY, FIXED_COLUMN, COMMA_DELIMITED, SPACE_DELIMITED, TAB_DELIMITED, CSV_DELIMITED, DBASE, ARCVIEW, SQLITE3, VERSION3, TPPLUS, CUBE, TRANSCAD
CONNECTION_FILE                                                 //---- [project_directory]filename
CONNECTION_FORMAT                       TAB_DELIMITED           //---- TEXT, BINARY, FIXED_COLUMN, COMMA_DELIMITED, SPACE_DELIMITED, TAB_DELIMITED, CSV_DELIMITED, DBASE, ARCVIEW, SQLITE3, VERSION3, TPPLUS, CUBE, TRANSCAD
LANE_USE_FILE                                                   //---- [project_directory]filename
LANE_USE_FORMAT                         TAB_DELIMITED           //---- TEXT, BINARY, FIXED_COLUMN, COMMA_DELIMITED, SPACE_DELIMITED, TAB_DELIMITED, CSV_DELIMITED, DBASE, ARCVIEW, SQLITE3, VERSION3, TPPLUS, CUBE, TRANSCAD
LOCATION_FILE                                                   //---- [project_directory]filename
LOCATION_FORMAT                         TAB_DELIMITED           //---- TEXT, BINARY, FIXED_COLUMN, COMMA_DELIMITED, SPACE_DELIMITED, TAB_DELIMITED, CSV_DELIMITED, DBASE, ARCVIEW, SQLITE3, VERSION3, TPPLUS, CUBE, TRANSCAD
TURN_DELAY_FILE                         Sim_TurnPerformance_15min.csv
TURN_DELAY_FORMAT                       TAB_DELIMITED           //---- TEXT, BINARY, FIXED_COLUMN, COMMA_DELIMITED, SPACE_DELIMITED, TAB_DELIMITED, CSV_DELIMITED, DBASE, ARCVIEW, SQLITE3, VERSION3, TPPLUS, CUBE, TRANSCAD
NEW_PERFORMANCE_FILE                                            //---- [project_directory]filename
NEW_PERFORMANCE_FORMAT                  TAB_DELIMITED           //---- TEXT, BINARY, FIXED_COLUMN, COMMA_DELIMITED, SPACE_DELIMITED, TAB_DELIMITED, CSV_DELIMITED, DBASE, ARCVIEW, SQLITE3, VERSION3, TPPLUS, CUBE, TRANSCAD
NEW_TURN_DELAY_FILE                                             //---- [project_directory]filename
NEW_TURN_DELAY_FORMAT                   TAB_DELIMITED           //---- TEXT, BINARY, FIXED_COLUMN, COMMA_DELIMITED, SPACE_DELIMITED, TAB_DELIMITED, CSV_DELIMITED, DBASE, ARCVIEW, SQLITE3, VERSION3, TPPLUS, CUBE, TRANSCAD
SAVE_LANE_USE_FLOWS                     FALSE                   //---- TRUE/FALSE, YES/NO, 1/0, T/F, Y/N
LINK_EQUIVALENCE_FILE                                           //---- [project_directory]filename
ZONE_EQUIVALENCE_FILE                                           //---- [project_directory]filename

#---- Data Service Keys ----

DAILY_WRAP_FLAG                         FALSE                   //---- TRUE/FALSE, YES/NO, 1/0, T/F, Y/N
SUMMARY_TIME_RANGES                     0:00..50:00             //---- e.g., ALL, 0..97200 seconds, 0.0..27.0 hours, 0:00..27:00
SUMMARY_TIME_INCREMENT                  60 minutes              //---- 0, 2..240 minutes
CONGESTED_TIME_RATIO                    2.0                     //---- 1.0..5.0
MAXIMUM_TIME_RATIO                      20000.0                 //---- 2.0..20000.0

#---- Select Service Keys ----

SELECT_FACILITY_TYPES                   FREEWAY                 //---- FREEWAY..EXTERNAL
SELECT_VC_RATIOS                        0.0                     //---- 0.0, >1.0
SELECT_TIME_RATIOS                      0.0                     //---- 0.0, >1.0
SELECT_SUBAREAS                                                 //---- e.g., 1, 2, 4..10, 100..200, 300
SELECTION_POLYGON                                               //---- [project_directory]filename

#---- LinkSum Control Keys ----

COMPARE_PERFORMANCE_FILE                                        //---- [project_directory]filename
COMPARE_PERFORMANCE_FORMAT              TAB_DELIMITED           //---- TEXT, BINARY, FIXED_COLUMN, COMMA_DELIMITED, SPACE_DELIMITED, TAB_DELIMITED, CSV_DELIMITED, DBASE, ARCVIEW, SQLITE3, VERSION3, TPPLUS, CUBE, TRANSCAD
COMPARE_LINK_MAP_FILE                                           //---- [project_directory]filename
COMPARE_LINK_MAP_FORMAT                 TAB_DELIMITED           //---- TEXT, BINARY, FIXED_COLUMN, COMMA_DELIMITED, SPACE_DELIMITED, TAB_DELIMITED, CSV_DELIMITED, DBASE, ARCVIEW, SQLITE3, VERSION3, TPPLUS, CUBE, TRANSCAD
MINIMUM_LINK_VOLUME                     0.0                     //---- >= 0
PERSON_BASED_STATISTICS                 TRUE                    //---- TRUE/FALSE, YES/NO, 1/0, T/F, Y/N
SELECT_BY_LINK_GROUP                    FALSE                   //---- TRUE/FALSE, YES/NO, 1/0, T/F, Y/N
COMPARE_TURN_DELAY_FILE                                         //---- [project_directory]filename
COMPARE_TURN_DELAY_FORMAT               TAB_DELIMITED           //---- TEXT, BINARY, FIXED_COLUMN, COMMA_DELIMITED, SPACE_DELIMITED, TAB_DELIMITED, CSV_DELIMITED, DBASE, ARCVIEW, SQLITE3, VERSION3, TPPLUS, CUBE, TRANSCAD
TURN_NODE_RANGE                         ALL                     //---- e.g., 1, 2, 4..10, 100..200, 300
NEW_LINK_ACTIVITY_FILE                                          //---- [project_directory]filename
NEW_LINK_ACTIVITY_FORMAT                TAB_DELIMITED           //---- TEXT, BINARY, FIXED_COLUMN, COMMA_DELIMITED, SPACE_DELIMITED, TAB_DELIMITED, CSV_DELIMITED, DBASE, ARCVIEW, SQLITE3, VERSION3, TPPLUS, CUBE, TRANSCAD
COPY_LOCATION_FIELDS                    
NEW_ZONE_TRAVEL_FILE                                            //---- [project_directory]filename
NEW_ZONE_TRAVEL_FORMAT                  TAB_DELIMITED           //---- TEXT, BINARY, FIXED_COLUMN, COMMA_DELIMITED, SPACE_DELIMITED, TAB_DELIMITED, CSV_DELIMITED, DBASE, ARCVIEW, SQLITE3, VERSION3, TPPLUS, CUBE, TRANSCAD
NEW_GROUP_TRAVEL_FILE                                           //---- [project_directory]filename
NEW_GROUP_TRAVEL_FORMAT                 TAB_DELIMTED            //---- TEXT, BINARY, FIXED_COLUMN, COMMA_DELIMITED, SPACE_DELIMITED, TAB_DELIMITED, CSV_DELIMITED, DBASE, ARCVIEW, SQLITE3, VERSION3, TPPLUS, CUBE, TRANSCAD
NEW_LINK_DIRECTION_FILE_1                                       //---- [project_directory]filename
NEW_LINK_DIRECTION_FORMAT_1             COMMA_DELIMITED         //---- TEXT, BINARY, FIXED_COLUMN, COMMA_DELIMITED, SPACE_DELIMITED, TAB_DELIMITED, CSV_DELIMITED, DBASE, ARCVIEW, SQLITE3, VERSION3, TPPLUS, CUBE, TRANSCAD
NEW_LINK_DIRECTION_FORMAT_2             COMMA_DELIMITED         //---- TEXT, BINARY, FIXED_COLUMN, COMMA_DELIMITED, SPACE_DELIMITED, TAB_DELIMITED, CSV_DELIMITED, DBASE, ARCVIEW, SQLITE3, VERSION3, TPPLUS, CUBE, TRANSCAD
NEW_LINK_DIRECTION_FORMAT_3             COMMA_DELIMITED         //---- TEXT, BINARY, FIXED_COLUMN, COMMA_DELIMITED, SPACE_DELIMITED, TAB_DELIMITED, CSV_DELIMITED, DBASE, ARCVIEW, SQLITE3, VERSION3, TPPLUS, CUBE, TRANSCAD
NEW_LINK_DIRECTION_FORMAT_4             COMMA_DELIMITED         //---- TEXT, BINARY, FIXED_COLUMN, COMMA_DELIMITED, SPACE_DELIMITED, TAB_DELIMITED, CSV_DELIMITED, DBASE, ARCVIEW, SQLITE3, VERSION3, TPPLUS, CUBE, TRANSCAD
NEW_LINK_DIRECTION_FORMAT_5             COMMA_DELIMITED         //---- TEXT, BINARY, FIXED_COLUMN, COMMA_DELIMITED, SPACE_DELIMITED, TAB_DELIMITED, CSV_DELIMITED, DBASE, ARCVIEW, SQLITE3, VERSION3, TPPLUS, CUBE, TRANSCAD
NEW_LINK_DIRECTION_FIELD_1              EXIT                    //---- TRAVEL_TIME, PERSONS, VOLUME, ENTER, EXIT, FLOW, SPEED, TIME_RATIO, DELAY, DENSITY, MAX_DENSITY, QUEUE, MAX_QUEUE, CYCLE_FAILURE, VC_RATIO,  VMT, VHT, VHD, CONGESTED_TIME, CONGESTED_VMT, CONGESTED_VHT
NEW_LINK_DIRECTION_FIELD_2              VOLUME                  //---- TRAVEL_TIME, PERSONS, VOLUME, ENTER, EXIT, FLOW, SPEED, TIME_RATIO, DELAY, DENSITY, MAX_DENSITY, QUEUE, MAX_QUEUE, CYCLE_FAILURE, VC_RATIO,  VMT, VHT, VHD, CONGESTED_TIME, CONGESTED_VMT, CONGESTED_VHT
NEW_LINK_DIRECTION_FIELD_3              TIME_RATIO              //---- TRAVEL_TIME, PERSONS, VOLUME, ENTER, EXIT, FLOW, SPEED, TIME_RATIO, DELAY, DENSITY, MAX_DENSITY, QUEUE, MAX_QUEUE, CYCLE_FAILURE, VC_RATIO,  VMT, VHT, VHD, CONGESTED_TIME, CONGESTED_VMT, CONGESTED_VHT
NEW_LINK_DIRECTION_FIELD_4              CONGESTED_PHT           //---- TRAVEL_TIME, PERSONS, VOLUME, ENTER, EXIT, FLOW, SPEED, TIME_RATIO, DELAY, DENSITY, MAX_DENSITY, QUEUE, MAX_QUEUE, CYCLE_FAILURE, VC_RATIO,  VMT, VHT, VHD, CONGESTED_TIME, CONGESTED_VMT, CONGESTED_VHT
NEW_LINK_DIRECTION_FIELD_5              CONGESTED_TIME          //---- TRAVEL_TIME, PERSONS, VOLUME, ENTER, EXIT, FLOW, SPEED, TIME_RATIO, DELAY, DENSITY, MAX_DENSITY, QUEUE, MAX_QUEUE, CYCLE_FAILURE, VC_RATIO,  VMT, VHT, VHD, CONGESTED_TIME, CONGESTED_VMT, CONGESTED_VHT
NEW_LINK_DIRECTION_INDEX_1              FALSE                   //---- TRUE/FALSE, YES/NO, 1/0, T/F, Y/N
NEW_LINK_DIRECTION_FLIP_1               FALSE                   //---- TRUE/FALSE, YES/NO, 1/0, T/F, Y/N
NEW_LINK_DATA_FILE_1                    LinkSum_LinkDATA_TRatio_Sim_Freeway.csv
NEW_LINK_DATA_FILE_2                    LinkSum_LinkDATA_VCRatio_Sim_Freeway.csv
NEW_LINK_DATA_FORMAT_1                  COMMA_DELIMITED         //---- TEXT, BINARY, FIXED_COLUMN, COMMA_DELIMITED, SPACE_DELIMITED, TAB_DELIMITED, CSV_DELIMITED, DBASE, ARCVIEW, SQLITE3, VERSION3, TPPLUS, CUBE, TRANSCAD
NEW_LINK_DATA_FORMAT_2                  COMMA_DELIMITED         //---- TEXT, BINARY, FIXED_COLUMN, COMMA_DELIMITED, SPACE_DELIMITED, TAB_DELIMITED, CSV_DELIMITED, DBASE, ARCVIEW, SQLITE3, VERSION3, TPPLUS, CUBE, TRANSCAD
NEW_LINK_DATA_FIELD_1                   TIME_RATIO              //---- TRAVEL_TIME, PERSONS, VOLUME, ENTER, EXIT, FLOW, SPEED, TIME_RATIO, DELAY, DENSITY, MAX_DENSITY, QUEUE, MAX_QUEUE, CYCLE_FAILURE, VC_RATIO,  VMT, VHT, VHD, CONGESTED_TIME, CONGESTED_VMT, CONGESTED_VHT
NEW_LINK_DATA_FIELD_2                   VC_RATIO                //---- TRAVEL_TIME, PERSONS, VOLUME, ENTER, EXIT, FLOW, SPEED, TIME_RATIO, DELAY, DENSITY, MAX_DENSITY, QUEUE, MAX_QUEUE, CYCLE_FAILURE, VC_RATIO,  VMT, VHT, VHD, CONGESTED_TIME, CONGESTED_VMT, CONGESTED_VHT
NEW_DATA_SUMMARY_FILE                                           //---- [project_directory]filename
NEW_DATA_SUMMARY_FORMAT                 COMMA_DELIMITED         //---- TEXT, BINARY, FIXED_COLUMN, COMMA_DELIMITED, SPACE_DELIMITED, TAB_DELIMITED, CSV_DELIMITED, DBASE, ARCVIEW, SQLITE3, VERSION3, TPPLUS, CUBE, TRANSCAD
NEW_DATA_SUMMARY_PERIODS                6:00..12:00, 12:00..18:00, 1@6:00..1@12:00, 1@12:00..1@18:00, 0:00..60:00	//---- e.g., ALL, 0..97200 seconds, 0.0..27.0 hours, 0:00..27:00
NEW_DATA_SUMMARY_RATIOS                 1.3, 2.0, 3.0           //---- 0.0, 1.0..5.0
NEW_GROUP_SUMMARY_FILE                                          //---- [project_directory]filename
NEW_GROUP_SUMMARY_FORMAT                TAB_DELIMITED           //---- TEXT, BINARY, FIXED_COLUMN, COMMA_DELIMITED, SPACE_DELIMITED, TAB_DELIMITED, CSV_DELIMITED, DBASE, ARCVIEW, SQLITE3, VERSION3, TPPLUS, CUBE, TRANSCAD

#---- Program Report Keys ----

LINKSUM_REPORT_1                                                //---- program report name
                                                                //---- TOP_100_LINK_VOLUME
                                                                //---- TOP_100_LANE_VOLUME
                                                                //---- TOP_100_PERIOD_VOLUME
                                                                //---- TOP_100_SPEED_REDUCTIONS
                                                                //---- TOP_100_TRAVEL_TIME_RATIOS
                                                                //---- TOP_100_VOLUME_CAPACITY_RATIOS
                                                                //---- TOP_100_TRAVEL_TIME_CHANGES
                                                                //---- TOP_100_VOLUME_CHANGES
                                                                //---- LINK_VOLUME_GREATER_THAN_*
                                                                //---- GROUP_VOLUME_GREATER_THAN_*
                                                                //---- LINK_EQUIVALENCE
                                                                //---- ZONE_EQUIVALENCE
                                                                //---- TRAVEL_TIME_DISTRIBUTION
                                                                //---- VOLUME_CAPACITY_RATIOS
                                                                //---- TRAVEL_TIME_CHANGES
                                                                //---- VOLUME_CHANGES
                                                                //---- LINK_GROUP_TRAVEL_TIME
                                                                //---- NETWORK_PERFORMANCE_SUMMARY
                                                                //---- NETWORK_PERFORMANCE_DETAILS
                                                                //---- GROUP_PERFORMANCE_SUMMARY
                                                                //---- GROUP_PERFORMANCE_DETAILS
                                                                //---- RELATIVE_GAP_REPORT
