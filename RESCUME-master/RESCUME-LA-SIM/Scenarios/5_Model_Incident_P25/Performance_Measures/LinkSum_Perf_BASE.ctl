TITLE                                             LinkSum BASE
REPORT_FLAG                                       FALSE                   //---- TRUE/FALSE, YES/NO, 1/0, T/F, Y/N
PAGE_LENGTH                                       99999                   //---- >= 0
DEFAULT_FILE_FORMAT                               BINARY                  //---- TEXT, BINARY, FIXED_COLUMN, COMMA_DELIMITED, SPACE_DELIMITED, TAB_DELIMITED, CSV_DELIMITED, DBASE, ARCVIEW, SQLITE3, VERSION3, TPPLUS, CUBE
TIME_OF_DAY_FORMAT                                DAY_TIME                //---- SECONDS, MINUTES, HOURS, HOUR_CLOCK, DAY_TIME, TIME_CODE, HOUR_MINUTE
MODEL_START_TIME                                  0:00                    //---- >= 0 [seconds], 0.0 [hours], 0:00
MODEL_END_TIME                                    60:00                   //---- > [model_start_time]
MODEL_TIME_INCREMENT                              15 minutes              //---- 0, 2..240 minutes
UNITS_OF_MEASURE                                  METRIC                 //---- METRIC, ENGLISH
DRIVE_SIDE_OF_ROAD                                RIGHT_SIDE              //---- RIGHT_SIDE, LEFT_SIDE
RANDOM_NUMBER_SEED                                123456789               //---- 0 = computer clock, > 0 = fixed
MAX_WARNING_MESSAGES                              99999                   //---- >= 0
MAX_WARNING_EXIT_FLAG                             FALSE                   //---- TRUE/FALSE, YES/NO, 1/0, T/F, Y/N
MAX_PROBLEM_COUNT                                 0                       //---- >= 0
NUMBER_OF_THREADS                                 1                       //---- 1..128

#---- System File Keys ----

NODE_FILE                                         ..\..\..\Common_Inputs\6_Network_Merged\ASCIIFiles\Node.csv
LINK_FILE                                         ..\..\..\Common_Inputs\6_Network_Merged\ASCIIFiles\Link.csv
## CONNECTION_FILE                                   ..\..\1_Network\ASCIIFiles\Connection.csv
## LANE_USE_FILE                                     ..\..\1_Network\ASCIIFiles\LaneUse.csv

PERFORMANCE_FILE                                  ..\Results\Router_Hwy_LinkPerformance_BASE
TURN_DELAY_FILE                                   ..\Results\Router_Hwy_TurnDelay_BASE

SAVE_LANE_USE_FLOWS                               FALSE

#---- Data Service Keys ----

DAILY_WRAP_FLAG                                   FALSE                    //---- TRUE/FALSE, YES/NO, 1/0, T/F, Y/N
SUMMARY_TIME_RANGES                               0:00..60:00             //---- e.g., ALL, 0..97200 seconds, 0.0..27.0 hours, 0:00..27:00
SUMMARY_TIME_INCREMENT                            60 minutes               //---- 0, 2..240 minutes
CONGESTED_TIME_RATIO                              2.0                     //---- 1.0..5.0
PERSON_BASED_STATISTICS                           FALSE                    //---- TRUE/FALSE, YES/NO, 1/0, T/F, Y/N

SELECT_FACILITY_TYPES							  FREEWAY

#---- LinkSum Control Keys ----

MINIMUM_LINK_VOLUME                               0.0
SELECT_BY_LINK_GROUP                              FALSE

NEW_DATA_SUMMARY_FILE                             Performance_Measures_Shapefiles\LinkSum_Summary_BASE.csv
NEW_DATA_SUMMARY_FORMAT                           COMMA_DELIMITED
NEW_DATA_SUMMARY_PERIODS                          6:00..12:00, 12:00..18:00, 1@6:00..1@12:00, 1@12:00..1@18:00, 0:00..60:00
NEW_DATA_SUMMARY_RATIOS                           1.3, 2.0, 3.0


NEW_LINK_DATA_FILE_1                              Performance_Measures_Shapefiles\LinkSum_LinkDATA_Vol_BASE.csv
NEW_LINK_DATA_FORMAT_1                            COMMA_DELIMITED
NEW_LINK_DATA_FIELD_1                             VOLUME                      //---- TRAVEL_TIME, PERSONS, VOLUME, ENTER, EXIT, FLOW, SPEED, TIME_RATIO, DELAY, DENSITY, MAX_DENSITY, QUEUE, MAX_QUEUE, CYCLE_FAILURE, VC_RATIO,  VMT, VHT, VHD, CONGESTED_TIME, CONGESTED_VMT, CONGESTED_VHT

NEW_LINK_DATA_FILE_2                              Performance_Measures_Shapefiles\LinkSum_LinkDATA_EXIT_BASE.csv
NEW_LINK_DATA_FORMAT_2                            COMMA_DELIMITED
NEW_LINK_DATA_FIELD_2                             EXIT                      //---- TRAVEL_TIME, PERSONS, VOLUME, ENTER, EXIT, FLOW, SPEED, TIME_RATIO, DELAY, DENSITY, MAX_DENSITY, QUEUE, MAX_QUEUE, CYCLE_FAILURE, VC_RATIO,  VMT, VHT, VHD, CONGESTED_TIME, CONGESTED_VMT, CONGESTED_VHT

NEW_LINK_DATA_FILE_3                              Performance_Measures_Shapefiles\LinkSum_LinkDATA_FLOW_BASE.csv
NEW_LINK_DATA_FORMAT_3                            COMMA_DELIMITED
NEW_LINK_DATA_FIELD_3                             FLOW                      //---- TRAVEL_TIME, PERSONS, VOLUME, ENTER, EXIT, FLOW, SPEED, TIME_RATIO, DELAY, DENSITY, MAX_DENSITY, QUEUE, MAX_QUEUE, CYCLE_FAILURE, VC_RATIO,  VMT, VHT, VHD, CONGESTED_TIME, CONGESTED_VMT, CONGESTED_VHT


NEW_LINK_DIRECTION_FILE_2                         Performance_Measures_Shapefiles\LinkSum_LinkDIR_Vol_BASE.csv
NEW_LINK_DIRECTION_FORMAT_2                       COMMA_DELIMITED
NEW_LINK_DIRECTION_FIELD_2                        VOLUME                      //---- TRAVEL_TIME, PERSONS, VOLUME, ENTER, EXIT, FLOW, SPEED, TIME_RATIO, DELAY, DENSITY, MAX_DENSITY, QUEUE, MAX_QUEUE, CYCLE_FAILURE, VC_RATIO,  VMT, VHT, VHD, CONGESTED_TIME, CONGESTED_VMT, CONGESTED_VHT


NEW_LINK_DIRECTION_FILE_3                         Performance_Measures_Shapefiles\LinkSum_LinkDIR_Delay_BASE.csv
NEW_LINK_DIRECTION_FORMAT_3                       COMMA_DELIMITED
NEW_LINK_DIRECTION_FIELD_3                        DELAY                        //---- TRAVEL_TIME, PERSONS, VOLUME, ENTER, EXIT, FLOW, SPEED, TIME_RATIO, DELAY, DENSITY, MAX_DENSITY, QUEUE, MAX_QUEUE, CYCLE_FAILURE, VC_RATIO,  VMT, VHT, VHD, CONGESTED_TIME, CONGESTED_VMT, CONGESTED_VHT


NEW_LINK_DIRECTION_FILE_4                         Performance_Measures_Shapefiles\LinkSum_LinkDIR_CongPHT_BASE.csv
NEW_LINK_DIRECTION_FORMAT_4                       COMMA_DELIMITED
NEW_LINK_DIRECTION_FIELD_4                        CONGESTED_PHT                //---- TRAVEL_TIME, PERSONS, VOLUME, ENTER, EXIT, FLOW, SPEED, TIME_RATIO, DELAY, DENSITY, MAX_DENSITY, QUEUE, MAX_QUEUE, CYCLE_FAILURE, VC_RATIO,  VMT, VHT, VHD, CONGESTED_TIME, CONGESTED_VMT, CONGESTED_VHT


NEW_LINK_DIRECTION_FILE_5                         Performance_Measures_Shapefiles\LinkSum_LinkDIR_CongDur_BASE.csv
NEW_LINK_DIRECTION_FORMAT_5                       COMMA_DELIMITED
NEW_LINK_DIRECTION_FIELD_5                        CONGESTED_TIME              //---- TRAVEL_TIME, PERSONS, VOLUME, ENTER, EXIT, FLOW, SPEED, TIME_RATIO, DELAY, DENSITY, MAX_DENSITY, QUEUE, MAX_QUEUE, CYCLE_FAILURE, VC_RATIO,  VMT, VHT, VHD, CONGESTED_TIME, CONGESTED_VMT, CONGESTED_VHT


## NEW_DATA_SUMMARY_FILE
## NEW_DATA_SUMMARY_FORMAT


#---- Program Report Keys ----

## LINKSUM_REPORT_1                                  TOP_100_LINK_VOLUME
## LINKSUM_REPORT_2                                  TOP_100_LANE_VOLUME
## LINKSUM_REPORT_3                                  TOP_100_PERIOD_VOLUME
## LINKSUM_REPORT_4                                  TOP_100_SPEED_REDUCTIONS
LINKSUM_REPORT_5                                  TOP_100_TRAVEL_TIME_RATIOS
LINKSUM_REPORT_6                                  TOP_100_VOLUME_CAPACITY_RATIOS
## LINKSUM_REPORT_7                                  TOP_100_TRAVEL_TIME_CHANGES
## LINKSUM_REPORT_8                                  TOP_100_VOLUME_CHANGES
## LINKSUM_REPORT_9                                  LINK_VOLUME_GREATER_THAN_*
## LINKSUM_REPORT_10                                 GROUP_VOLUME_GREATER_THAN_*
## LINKSUM_REPORT_11                                 LINK_EQUIVALENCE
## LINKSUM_REPORT_12                                 ZONE_EQUIVALENCE
## LINKSUM_REPORT_13                                 TRAVEL_TIME_DISTRIBUTION
## LINKSUM_REPORT_14                                 VOLUME_CAPACITY_RATIOS
## LINKSUM_REPORT_15                                 TRAVEL_TIME_CHANGES
## LINKSUM_REPORT_16                                 VOLUME_CHANGES
## LINKSUM_REPORT_17                                 LINK_GROUP_TRAVEL_TIME
LINKSUM_REPORT_18                                 NETWORK_PERFORMANCE_SUMMARY
## LINKSUM_REPORT_20                                 NETWORK_PERFORMANCE_DETAILS
## LINKSUM_REPORT_21                                 GROUP_PERFORMANCE_SUMMARY
## LINKSUM_REPORT_22                                 GROUP_PERFORMANCE_DETAILS
## LINKSUM_REPORT_23                                 RELATIVE_GAP_REPORT

