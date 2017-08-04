TITLE                                   Router Assignments
REPORT_DIRECTORY
REPORT_FILE                                                     //---- [report_directory]filename[_partition][.prn]
REPORT_FLAG                             FALSE                   //---- TRUE/FALSE, YES/NO, 1/0, T/F, Y/N
PAGE_LENGTH                             99999                   //---- >= 0
PROJECT_DIRECTORY
DEFAULT_FILE_FORMAT                     BINARY                  //---- TEXT, BINARY, FIXED_COLUMN, COMMA_DELIMITED, SPACE_DELIMITED, TAB_DELIMITED, CSV_DELIMITED, DBASE, ARCVIEW, SQLITE3, VERSION3, TPPLUS, CUBE
TIME_OF_DAY_FORMAT                      DAY_TIME                //---- SECONDS, MINUTES, HOURS, HOUR_CLOCK, DAY_TIME, TIME_CODE, HOUR_MINUTE
MODEL_START_TIME                        0:00                    //---- >= 0 [seconds], 0.0 [hours], 0:00
MODEL_END_TIME                          60:00                   //---- > [model_start_time]
MODEL_TIME_INCREMENT                    15 minutes              //---- 0, 2..240 minutes
UNITS_OF_MEASURE                        METRIC                 //---- METRIC, ENGLISH
DRIVE_SIDE_OF_ROAD                      RIGHT_SIDE              //---- RIGHT_SIDE, LEFT_SIDE
RANDOM_NUMBER_SEED                      123456789               //---- 0 = computer clock, > 0 = fixed
MAX_WARNING_MESSAGES                    999                     //---- >= 0
MAX_WARNING_EXIT_FLAG                   FALSE                   //---- TRUE/FALSE, YES/NO, 1/0, T/F, Y/N
MAX_PROBLEM_COUNT                       0                       //---- >= 0
NUMBER_OF_THREADS                       20                      //---- 1..128

#---- System File Keys ----

PLAN_FILE                               ..\Demand\Plans_BASE_ProblemSet
PLAN_FORMAT
NODE_FILE                               ..\..\..\Common_Inputs\6_Network_Merged\ASCIIFiles\Node.csv
NODE_FORMAT
LINK_FILE                               ..\..\..\Common_Inputs\6_Network_Merged\ASCIIFiles\Link.csv
LINK_FORMAT
ZONE_FILE
ZONE_FORMAT
CONNECTION_FILE                         ..\..\..\Common_Inputs\6_Network_Merged\ASCIIFiles\Connection.csv
CONNECTION_FORMAT
LANE_USE_FILE                           ..\..\..\Common_Inputs\6_Network_Merged\ASCIIFiles\LaneUse.csv
LANE_USE_FORMAT
LOCATION_FILE                           ..\..\..\Common_Inputs\6_Network_Merged\ASCIIFiles\Location.csv
LOCATION_FORMAT
PARKING_FILE                            ..\..\..\Common_Inputs\6_Network_Merged\ASCIIFiles\Parking.csv
PARKING_FORMAT
ACCESS_FILE                             ..\..\..\Common_Inputs\6_Network_Merged\ASCIIFiles\Access.csv
ACCESS_FORMAT
TRANSIT_STOP_FILE
TRANSIT_STOP_FORMAT
TRANSIT_ROUTE_FILE
TRANSIT_ROUTE_FORMAT
TRANSIT_SCHEDULE_FILE
TRANSIT_SCHEDULE_FORMAT
TRANSIT_DRIVER_FILE
TRANSIT_DRIVER_FORMAT
SELECTION_FILE
SELECTION_FORMAT
VEHICLE_TYPE_FILE                       ..\..\..\Common_Inputs\6_Network_Merged\ASCIIFiles\Vehicle_Type.csv
VEHICLE_TYPE_FORMAT
PERFORMANCE_FILE                       
PERFORMANCE_FORMAT
TURN_DELAY_FILE                         
TURN_DELAY_FORMAT
NEW_PERFORMANCE_FILE                    ..\Results\Router_Hwy_LinkPerformance_BASE_ProblemSet_PlanSum
NEW_PERFORMANCE_FORMAT					TAB_DELIMITED
NEW_TURN_DELAY_FILE                     
NEW_TURN_DELAY_FORMAT
NEW_RIDERSHIP_FILE
NEW_RIDERSHIP_FORMAT
NOTES_AND_NAME_FIELDS                   TRUE
SAVE_LANE_USE_FLOWS                     TRUE
ZONE_EQUIVALENCE_FILE
LINK_EQUIVALENCE_FILE
STOP_EQUIVALENCE_FILE
LINE_EQUIVALENCE_FILE

#---- Data Service Keys ----

DAILY_WRAP_FLAG                         FALSE                   //---- TRUE/FALSE, YES/NO, 1/0, T/F, Y/N
SUMMARY_TIME_RANGES                     ALL                     //---- e.g., ALL, 0..97200 seconds, 0.0..27.0 hours, 0:00..27:00
SUMMARY_TIME_INCREMENT                  15                      //---- 0, 2..240 minutes
PERIOD_CONTROL_POINT                    MID-TRIP                //---- START, END, MID-TRIP

#---- Select Service Keys ----

SELECT_HOUSEHOLDS                       ALL                     //---- e.g., 1, 2, 4..10, 100..200, 300
SELECT_MODES                            ALL                     //---- e.g., ALL or 1, 12..14 or WALK, HOV2..HOV4
SELECT_PURPOSES                         ALL                     //---- e.g., 1, 2, 4..10, 100..200, 300
SELECT_START_TIMES                      ALL                     //---- e.g., ALL, 0..97200 seconds, 0.0..27.0 hours, 0:00..27:00
SELECT_END_TIMES                        ALL                     //---- e.g., ALL, 0..97200 seconds, 0.0..27.0 hours, 0:00..27:00
SELECT_ORIGINS                          ALL                     //---- e.g., 1, 2, 4..10, 100..200, 300
SELECT_DESTINATIONS                     ALL                     //---- e.g., 1, 2, 4..10, 100..200, 300
SELECT_VEHICLE_TYPES                    ALL                     //---- e.g., 1, 2, 4..10, 100..200, 300
SELECT_TRAVELER_TYPES                   ALL                     //---- e.g., 1, 2, 4..10, 100..200, 300
SELECT_LINKS_1                          ALL                     //---- e.g., 1, 2, 4..10, 100..200, 300
SELECT_NODES_1                          ALL                     //---- e.g., 1, 2, 4..10, 100..200, 300
SELECT_SUBAREAS                                                 //---- e.g., 1, 2, 4..10, 100..200, 300
SELECT_STOPS                            ALL                     //---- e.g., 1, 2, 4..10, 100..200, 300
SELECT_ROUTES                           ALL                     //---- e.g., 1, 2, 4..10, 100..200, 300
SELECTION_POLYGON                                               //---- [project_directory]filename
SELECTION_PERCENTAGE                    100 percent             //---- 0.01..100.0 percent

#---- Flow-Time Service Keys ----

UPDATE_FLOW_RATES                       TRUE                    //---- TRUE/FALSE, YES/NO, 1/0, T/F, Y/N
CLEAR_INPUT_FLOW_RATES                  TRUE                    //---- TRUE/FALSE, YES/NO, 1/0, T/F, Y/N
UPDATE_TURNING_MOVEMENTS                TRUE                    //---- TRUE/FALSE, YES/NO, 1/0, T/F, Y/N
UPDATE_TRAVEL_TIMES                     TRUE                    //---- TRUE/FALSE, YES/NO, 1/0, T/F, Y/N
LINK_FLOW_FACTOR                        1.0                     //---- 1..100000


## VDOT
EQUATION_PARAMETERS_1                             CONICAL,   2, 50             //  1 => FREEWAY                 Conical VDF A=Alpha, B=MaxTTI
EQUATION_PARAMETERS_2                             CONICAL,    2, 50             //  2 => EXPRESSWAY              Conical VDF A=Alpha, B=MaxTTI
EQUATION_PARAMETERS_4                             CONICAL,    2, 50             //  4 => MAJOR                   Conical VDF A=Alpha, B=MaxTTI
EQUATION_PARAMETERS_5                             CONICAL,  5.5, 50             //  5 => MINOR                   Conical VDF A=Alpha, B=MaxTTI
EQUATION_PARAMETERS_6                             CONICAL,    3, 50             //  6 => COLLECTOR               Conical VDF A=Alpha, B=MaxTTI
EQUATION_PARAMETERS_10                            CONICAL,   4, 50             // 10 => RAMP                    Conical VDF A=Alpha, B=MaxTTI
EQUATION_PARAMETERS_11                            CONICAL,    4, 50             // 11 => BRIDGE  =  MAJOR        Conical VDF A=Alpha, B=MaxTTI
EQUATION_PARAMETERS_12                            CONICAL,    8, 50             // 12 => TUNNEL  =  EXPRESSWAY   Conical VDF A=Alpha, B=MaxTTI
EQUATION_PARAMETERS_13                            CONICAL,  5.5, 50             // 13 => OTHER   =  MINOR        Conical VDF A=Alpha, B=MaxTTI
EQUATION_PARAMETERS_20                            BPR,     0.01,  1, 1          // 20 => EXTERNAL                    BPR VDF A=Alpha, B=Beta,   C=Gamma,  D=MaxV/C

#---- PlanSum Control Keys ----

NEW_TRIP_TIME_FILE                                              //---- [project_directory]filename
NEW_LINK_VOLUME_FILE                                            //---- [project_directory]filename
NEW_ACCESS_DETAIL_FILE                                          //---- [project_directory]filename
NEW_ACCESS_GROUP_FILE                                           //---- [project_directory]filename
NEW_STOP_DIURNAL_FILE                                           //---- [project_directory]filename
NEW_LINE_ON_OFF_FILE                                            //---- [project_directory]filename
NEW_STOP_BOARDING_FILE                                          //---- [project_directory]filename

#---- Program Report Keys ----

PLANSUM_REPORT_1                                                //---- program report name
                                                                //---- TOP_100_V/C_RATIOS
                                                                //---- ALL_V/C_RATIOS_GREATER_THAN_*
                                                                //---- LINK_GROUP_V/C_RATIOS_*
                                                                //---- ZONE_EQUIVALENCE
                                                                //---- LINK_EQUIVALENCE
                                                                //---- STOP_EQUIVALENCE
                                                                //---- LINE_EQUIVALENCE
                                                                //---- TRANSIT_RIDERSHIP_SUMMARY
                                                                //---- TRANSIT_STOP_SUMMARY
                                                                //---- TRANSIT_TRANSFER_SUMMARY
                                                                //---- TRANSIT_TRANSFER_DETAILS
                                                                //---- TRANSIT_STOP_GROUP_SUMMARY
                                                                //---- TRANSIT_STOP_GROUP_DETAILS
                                                                //---- TRANSIT_LINE_GROUP_SUMMARY
                                                                //---- TRANSIT_LINE_GROUP_DETAILS
                                                                //---- TRANSIT_PASSENGER_SUMMARY
                                                                //---- TRANSIT_LINK_GROUP_SUMMARY
                                                                //---- LINE_TO_LINE_TRANSFERS
                                                                //---- STOP_GROUP_ACCESS_DETAILS
                                                                //---- TRIP_TIME_REPORT
                                                                //---- TRAVEL_SUMMARY_REPORT
