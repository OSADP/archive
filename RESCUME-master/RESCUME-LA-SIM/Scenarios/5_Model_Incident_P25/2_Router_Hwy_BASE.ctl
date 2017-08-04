TITLE                                   Router Default Control Keys
REPORT_DIRECTORY                        
REPORT_FILE                                                     //---- [report_directory]filename[_partition][.prn]
REPORT_FLAG                             FALSE                   //---- TRUE/FALSE, YES/NO, 1/0, T/F, Y/N
PAGE_LENGTH                             200                      //---- >= 0
PROJECT_DIRECTORY                       
DEFAULT_FILE_FORMAT                     TAB_DELIMITED           //---- TEXT, BINARY, FIXED_COLUMN, COMMA_DELIMITED, SPACE_DELIMITED, TAB_DELIMITED, CSV_DELIMITED, DBASE, ARCVIEW, SQLITE3, VERSION3, TPPLUS, CUBE
TIME_OF_DAY_FORMAT                      DAY_TIME                //---- SECONDS, MINUTES, HOURS, HOUR_CLOCK, DAY_TIME, TIME_CODE, HOUR_MINUTE
MODEL_START_TIME                        0:00                    //---- >= 0 [seconds], 0.0 [hours], 0:00
MODEL_END_TIME                          60:00                   //---- > [model_start_time]
MODEL_TIME_INCREMENT                    15 minutes              //---- 0, 2..240 minutes
UNITS_OF_MEASURE                        METRIC                  //---- METRIC, ENGLISH
DRIVE_SIDE_OF_ROAD                      RIGHT_SIDE              //---- RIGHT_SIDE, LEFT_SIDE
RANDOM_NUMBER_SEED                      0                       //---- 0 = computer clock, > 0 = fixed
MAX_WARNING_MESSAGES                    100000                  //---- >= 0
MAX_WARNING_EXIT_FLAG                   TRUE                    //---- TRUE/FALSE, YES/NO, 1/0, T/F, Y/N
MAX_PROBLEM_COUNT                       0                       //---- >= 0
NUMBER_OF_THREADS                       4                      //---- 1..128

#---- System File Keys ----

NODE_FILE                               ..\..\Common_Inputs\1_Network_v6\ASCIIFiles\Node.csv
NODE_FORMAT                             TAB_DELIMITED           //---- TEXT, BINARY, FIXED_COLUMN, COMMA_DELIMITED, SPACE_DELIMITED, TAB_DELIMITED, CSV_DELIMITED, DBASE, ARCVIEW, SQLITE3, VERSION3, TPPLUS, CUBE
LINK_FILE                               ..\..\Common_Inputs\1_Network_v6\ASCIIFiles\Link.csv
LINK_FORMAT                             TAB_DELIMITED           //---- TEXT, BINARY, FIXED_COLUMN, COMMA_DELIMITED, SPACE_DELIMITED, TAB_DELIMITED, CSV_DELIMITED, DBASE, ARCVIEW, SQLITE3, VERSION3, TPPLUS, CUBE
CONNECTION_FILE                         ..\..\Common_Inputs\1_Network_v6\ASCIIFiles\Connection.csv
CONNECTION_FORMAT                       TAB_DELIMITED           //---- TEXT, BINARY, FIXED_COLUMN, COMMA_DELIMITED, SPACE_DELIMITED, TAB_DELIMITED, CSV_DELIMITED, DBASE, ARCVIEW, SQLITE3, VERSION3, TPPLUS, CUBE
LOCATION_FILE                           ..\..\Common_Inputs\1_Network_v6\ASCIIFiles\Location.csv
LOCATION_FORMAT                         TAB_DELIMITED           //---- TEXT, BINARY, FIXED_COLUMN, COMMA_DELIMITED, SPACE_DELIMITED, TAB_DELIMITED, CSV_DELIMITED, DBASE, ARCVIEW, SQLITE3, VERSION3, TPPLUS, CUBE
POCKET_FILE                             ..\..\Common_Inputs\1_Network_v6\ASCIIFiles\Pocket.csv
POCKET_FORMAT                           TAB_DELIMITED           //---- TEXT, BINARY, FIXED_COLUMN, COMMA_DELIMITED, SPACE_DELIMITED, TAB_DELIMITED, CSV_DELIMITED, DBASE, ARCVIEW, SQLITE3, VERSION3, TPPLUS, CUBE
LANE_USE_FILE                           Network\LaneUse.csv
LANE_USE_FORMAT                         TAB_DELIMITED           //---- TEXT, BINARY, FIXED_COLUMN, COMMA_DELIMITED, SPACE_DELIMITED, TAB_DELIMITED, CSV_DELIMITED, DBASE, ARCVIEW, SQLITE3, VERSION3, TPPLUS, CUBE
TURN_PENALTY_FILE                                               //---- [project_directory]filename
TURN_PENALTY_FORMAT                     TAB_DELIMITED           //---- TEXT, BINARY, FIXED_COLUMN, COMMA_DELIMITED, SPACE_DELIMITED, TAB_DELIMITED, CSV_DELIMITED, DBASE, ARCVIEW, SQLITE3, VERSION3, TPPLUS, CUBE
PARKING_FILE                            ..\..\Common_Inputs\1_Network_v6\ASCIIFiles\Parking.csv
PARKING_FORMAT                          TAB_DELIMITED           //---- TEXT, BINARY, FIXED_COLUMN, COMMA_DELIMITED, SPACE_DELIMITED, TAB_DELIMITED, CSV_DELIMITED, DBASE, ARCVIEW, SQLITE3, VERSION3, TPPLUS, CUBE
ACCESS_FILE                             ..\..\Common_Inputs\1_Network_v6\ASCIIFiles\Access.csv
ACCESS_FORMAT                           TAB_DELIMITED           //---- TEXT, BINARY, FIXED_COLUMN, COMMA_DELIMITED, SPACE_DELIMITED, TAB_DELIMITED, CSV_DELIMITED, DBASE, ARCVIEW, SQLITE3, VERSION3, TPPLUS, CUBE
TRANSIT_STOP_FILE                       ..\..\Common_Inputs\1_Network_v6\ASCIIFiles\Transit_Stop.csv
TRANSIT_STOP_FORMAT                     TAB_DELIMITED           //---- TEXT, BINARY, FIXED_COLUMN, COMMA_DELIMITED, SPACE_DELIMITED, TAB_DELIMITED, CSV_DELIMITED, DBASE, ARCVIEW, SQLITE3, VERSION3, TPPLUS, CUBE
TRANSIT_FARE_FILE                                               //---- [project_directory]filename
TRANSIT_FARE_FORMAT                     TAB_DELIMITED           //---- TEXT, BINARY, FIXED_COLUMN, COMMA_DELIMITED, SPACE_DELIMITED, TAB_DELIMITED, CSV_DELIMITED, DBASE, ARCVIEW, SQLITE3, VERSION3, TPPLUS, CUBE
TRANSIT_ROUTE_FILE                      ..\..\Common_Inputs\1_Network_v6\ASCIIFiles\Transit_Route.csv
TRANSIT_ROUTE_FORMAT                    TAB_DELIMITED           //---- TEXT, BINARY, FIXED_COLUMN, COMMA_DELIMITED, SPACE_DELIMITED, TAB_DELIMITED, CSV_DELIMITED, DBASE, ARCVIEW, SQLITE3, VERSION3, TPPLUS, CUBE
TRANSIT_SCHEDULE_FILE                   ..\..\Common_Inputs\1_Network_v6\ASCIIFiles\Transit_Schedule.csv
TRANSIT_SCHEDULE_FORMAT                 TAB_DELIMITED           //---- TEXT, BINARY, FIXED_COLUMN, COMMA_DELIMITED, SPACE_DELIMITED, TAB_DELIMITED, CSV_DELIMITED, DBASE, ARCVIEW, SQLITE3, VERSION3, TPPLUS, CUBE
TRANSIT_DRIVER_FILE                     ..\..\Common_Inputs\1_Network_v6\ASCIIFiles\Transit_Driver.csv
TRANSIT_DRIVER_FORMAT                   TAB_DELIMITED           //---- TEXT, BINARY, FIXED_COLUMN, COMMA_DELIMITED, SPACE_DELIMITED, TAB_DELIMITED, CSV_DELIMITED, DBASE, ARCVIEW, SQLITE3, VERSION3, TPPLUS, CUBE
HOUSEHOLD_FILE                                                  //---- [project_directory]filename.*
HOUSEHOLD_FORMAT                        TAB_DELIMITED           //---- TEXT, BINARY, FIXED_COLUMN, COMMA_DELIMITED, SPACE_DELIMITED, TAB_DELIMITED, CSV_DELIMITED, DBASE, ARCVIEW, SQLITE3, VERSION3, TPPLUS, CUBE
SELECTION_FILE                              					//---- [project_directory]filename.*
SELECTION_FORMAT                        TAB_DELIMITED           //---- TEXT, BINARY, FIXED_COLUMN, COMMA_DELIMITED, SPACE_DELIMITED, TAB_DELIMITED, CSV_DELIMITED, DBASE, ARCVIEW, SQLITE3, VERSION3, TPPLUS, CUBE
PERFORMANCE_FILE                                                //---- [project_directory]filename
PERFORMANCE_FORMAT                      TAB_DELIMITED           //---- TEXT, BINARY, FIXED_COLUMN, COMMA_DELIMITED, SPACE_DELIMITED, TAB_DELIMITED, CSV_DELIMITED, DBASE, ARCVIEW, SQLITE3, VERSION3, TPPLUS, CUBE
TURN_DELAY_FILE                                                 //---- [project_directory]filename
TURN_DELAY_FORMAT                       TAB_DELIMITED           //---- TEXT, BINARY, FIXED_COLUMN, COMMA_DELIMITED, SPACE_DELIMITED, TAB_DELIMITED, CSV_DELIMITED, DBASE, ARCVIEW, SQLITE3, VERSION3, TPPLUS, CUBE
VEHICLE_TYPE_FILE                       ..\..\Common_Inputs\1_Network_v6\ASCIIFiles\Vehicle_Type.csv
VEHICLE_TYPE_FORMAT                     TAB_DELIMITED           //---- TEXT, BINARY, FIXED_COLUMN, COMMA_DELIMITED, SPACE_DELIMITED, TAB_DELIMITED, CSV_DELIMITED, DBASE, ARCVIEW, SQLITE3, VERSION3, TPPLUS, CUBE
RIDERSHIP_FILE                                                  //---- [project_directory]filename
RIDERSHIP_FORMAT                        TAB_DELIMITED           //---- TEXT, BINARY, FIXED_COLUMN, COMMA_DELIMITED, SPACE_DELIMITED, TAB_DELIMITED, CSV_DELIMITED, DBASE, ARCVIEW, SQLITE3, VERSION3, TPPLUS, CUBE
NEW_PLAN_FILE                           Demand\Plans_Hwy_BASE						//---- [project_directory]filename.*
NEW_PLAN_FORMAT                         BINARY                  //---- TEXT, BINARY, FIXED_COLUMN, COMMA_DELIMITED, SPACE_DELIMITED, TAB_DELIMITED, CSV_DELIMITED, DBASE, ARCVIEW, SQLITE3, VERSION3, TPPLUS, CUBE
NEW_PROBLEM_FILE                        						//---- [project_directory]filename.*
NEW_PROBLEM_FORMAT                      COMMA_DELIMITED         //---- TEXT, BINARY, FIXED_COLUMN, COMMA_DELIMITED, SPACE_DELIMITED, TAB_DELIMITED, CSV_DELIMITED, DBASE, ARCVIEW, SQLITE3, VERSION3, TPPLUS, CUBE
NEW_PERFORMANCE_FILE                    Results\Router_Hwy_LinkPerformance_BASE
NEW_PERFORMANCE_FORMAT                  TAB_DELIMITED                  //---- TEXT, BINARY, FIXED_COLUMN, COMMA_DELIMITED, SPACE_DELIMITED, TAB_DELIMITED, CSV_DELIMITED, DBASE, ARCVIEW, SQLITE3, VERSION3, TPPLUS, CUBE
TRIP_FILE                               ..\..\Common_Inputs\3_Demand_v6\Trips_Hwy_ExitUpdated_P25.csv    //---- [project_directory]filename.*
TRIP_FORMAT                             COMMA_DELIMITED           //---- TEXT, BINARY, FIXED_COLUMN, COMMA_DELIMITED, SPACE_DELIMITED, TAB_DELIMITED, CSV_DELIMITED, DBASE, ARCVIEW, SQLITE3, VERSION3, TPPLUS, CUBE
PLAN_FILE                               ..\3_Model_P25\Demand\Plans_BASE_Sorted                        //---- [project_directory]filename.*
PLAN_FORMAT                             BINARY                  //---- TEXT, BINARY, FIXED_COLUMN, COMMA_DELIMITED, SPACE_DELIMITED, TAB_DELIMITED, CSV_DELIMITED, DBASE, ARCVIEW, SQLITE3, VERSION3, TPPLUS, CUBE
SIGN_FILE                               ..\..\Common_Inputs\1_Network_v6\ASCIIFiles\Sign.csv
SIGN_FORMAT                             TAB_DELIMITED           //---- TEXT, BINARY, FIXED_COLUMN, COMMA_DELIMITED, SPACE_DELIMITED, TAB_DELIMITED, CSV_DELIMITED, DBASE, ARCVIEW, SQLITE3, VERSION3, TPPLUS, CUBE
SIGNAL_FILE                             ..\..\Common_Inputs\1_Network_v6\ASCIIFiles\Signal.csv
SIGNAL_FORMAT                           TAB_DELIMITED           //---- TEXT, BINARY, FIXED_COLUMN, COMMA_DELIMITED, SPACE_DELIMITED, TAB_DELIMITED, CSV_DELIMITED, DBASE, ARCVIEW, SQLITE3, VERSION3, TPPLUS, CUBE
TIMING_PLAN_FILE                        ..\..\Common_Inputs\1_Network_v6\ASCIIFiles\Timing_Plan.csv
TIMING_PLAN_FORMAT                      TAB_DELIMITED           //---- TEXT, BINARY, FIXED_COLUMN, COMMA_DELIMITED, SPACE_DELIMITED, TAB_DELIMITED, CSV_DELIMITED, DBASE, ARCVIEW, SQLITE3, VERSION3, TPPLUS, CUBE
PHASING_PLAN_FILE                       ..\..\Common_Inputs\1_Network_v6\ASCIIFiles\Phasing_Plan.csv
PHASING_PLAN_FORMAT                     TAB_DELIMITED           //---- TEXT, BINARY, FIXED_COLUMN, COMMA_DELIMITED, SPACE_DELIMITED, TAB_DELIMITED, CSV_DELIMITED, DBASE, ARCVIEW, SQLITE3, VERSION3, TPPLUS, CUBE
DETECTOR_FILE                           ..\..\Common_Inputs\1_Network_v6\ASCIIFiles\Detector.csv                        //---- [project_directory]filename
DETECTOR_FORMAT                         TAB_DELIMITED           //---- TEXT, BINARY, FIXED_COLUMN, COMMA_DELIMITED, SPACE_DELIMITED, TAB_DELIMITED, CSV_DELIMITED, DBASE, ARCVIEW, SQLITE3, VERSION3, TPPLUS, CUBE
NEW_TURN_DELAY_FILE                     Results\Router_Hwy_TurnDelay_BASE
NEW_TURN_DELAY_FORMAT                   TAB_DELIMITED           //---- TEXT, BINARY, FIXED_COLUMN, COMMA_DELIMITED, SPACE_DELIMITED, TAB_DELIMITED, CSV_DELIMITED, DBASE, ARCVIEW, SQLITE3, VERSION3, TPPLUS, CUBE
NEW_RIDERSHIP_FILE                                              //---- [project_directory]filename
NEW_RIDERSHIP_FORMAT                    TAB_DELIMITED           //---- TEXT, BINARY, FIXED_COLUMN, COMMA_DELIMITED, SPACE_DELIMITED, TAB_DELIMITED, CSV_DELIMITED, DBASE, ARCVIEW, SQLITE3, VERSION3, TPPLUS, CUBE

#---- Select Service Keys ----

SELECT_HOUSEHOLDS                       ALL                     //---- e.g., 1, 2, 4..10, 100..200, 300
SELECT_MODES                            ALL                     //---- e.g., ALL or 1, 12..14 or WALK, HOV2..HOV4
SELECT_PURPOSES                         ALL                     //---- e.g., 1, 2, 4..10, 100..200, 300
SELECT_PRIORITIES                       ALL // HIGH, CRITICAL          //---- ALL, or 0..4, or LOW, MEDIUM, HIGH, CRITICAL
SELECT_START_TIMES                      ALL                     //---- e.g., ALL, 0..97200 seconds, 0.0..27.0 hours, 0:00..27:00
SELECT_END_TIMES                        ALL                     //---- e.g., ALL, 0..97200 seconds, 0.0..27.0 hours, 0:00..27:00
SELECT_ORIGINS                          ALL                     //---- e.g., 1, 2, 4..10, 100..200, 300
SELECT_DESTINATIONS                     ALL                     //---- e.g., 1, 2, 4..10, 100..200, 300
SELECT_TRAVELER_TYPES                   ALL                     //---- e.g., 1, 2, 4..10, 100..200, 300
SELECT_VEHICLE_TYPES                    ALL                     //---- e.g., 1, 2, 4..10, 100..200, 300
SELECT_ORIGIN_ZONES                     ALL                     //---- e.g., 1, 2, 4..10, 100..200, 300
SELECT_DESTINATION_ZONES                ALL                     //---- e.g., 1, 2, 4..10, 100..200, 300
DELETION_FILE                                                   //---- [project_directory]filename
DELETION_FORMAT                         TAB_DELIMITED           //---- TEXT, BINARY, FIXED_COLUMN, COMMA_DELIMITED, SPACE_DELIMITED, TAB_DELIMITED, CSV_DELIMITED, DBASE, ARCVIEW, SQLITE3, VERSION3, TPPLUS, CUBE
DELETE_HOUSEHOLDS                       NONE                    //---- e.g., 1, 2, 4..10, 100..200, 300
DELETE_MODES                            NONE                    //---- e.g., ALL or 1, 12..14 or WALK, HOV2..HOV4
DELETE_TRAVELER_TYPES                   NONE                    //---- e.g., 1, 2, 4..10, 100..200, 300

#---- Path Building Service Keys ----

IMPEDANCE_SORT_METHOD                   TRUE                    //---- TRUE/FALSE, YES/NO, 1/0, T/F, Y/N
SAVE_ONLY_SKIMS                         FALSE                   //---- TRUE/FALSE, YES/NO, 1/0, T/F, Y/N
WALK_PATH_DETAILS                       FALSE                   //---- TRUE/FALSE, YES/NO, 1/0, T/F, Y/N
IGNORE_VEHICLE_ID                       TRUE                   //---- TRUE/FALSE, YES/NO, 1/0, T/F, Y/N
LIMIT_PARKING_ACCESS                    TRUE                    //---- TRUE/FALSE, YES/NO, 1/0, T/F, Y/N
ADJUST_ACTIVITY_SCHEDULE                FALSE                   //---- TRUE/FALSE, YES/NO, 1/0, T/F, Y/N
IGNORE_ACTIVITY_DURATIONS               TRUE                    //---- TRUE/FALSE, YES/NO, 1/0, T/F, Y/N
IGNORE_TIME_CONSTRAINTS                 TRUE                    //---- TRUE/FALSE, YES/NO, 1/0, T/F, Y/N
END_TIME_CONSTRAINT                     0 minutes               //---- 0..360 minutes
IGNORE_ROUTING_PROBLEMS                 FALSE                   //---- TRUE/FALSE, YES/NO, 1/0, T/F, Y/N
TRANSIT_CAPACITY_PENALTY                FALSE                   //---- TRUE/FALSE, YES/NO, 1/0, T/F, Y/N
PERCENT_RANDOM_IMPEDANCE                0.0 percent             //---- 0.0..100.0 percent
TRAVELER_TYPE_SCRIPT                                            //---- [project_directory]filename
TRAVELER_PARAMETER_FILE                                         //---- [project_directory]filename
WALK_SPEED                              1.5 mps                 //---- 1.5..12.0 mph
BICYCLE_SPEED                           12.0 mph                //---- 3.0..30.0 mph
WALK_TIME_VALUES_1                      20.0 impedance/second	//---- 0.0..1000.0
BICYCLE_TIME_VALUES_1                   15.0 impedance/second	//---- 0.0..1000.0
FIRST_WAIT_VALUES_1                     20.0 impedance/second	//---- 0.0..1000.0
TRANSFER_WAIT_VALUES_1                  60.0 impedance/second	//---- 0.0..1000.0
PARKING_TIME_VALUES_1                   0.0 impedance/second    //---- 0.0..1000.0
VEHICLE_TIME_VALUES_1                   10.0 impedance/second	//---- 0.0..1000.0
DISTANCE_VALUES_1                       0.0 impedance/foot      //---- 0.0..1000.0
COST_VALUES_1                           0.0 impedance/cent      //---- 0.0..1000.0
FREEWAY_BIAS_FACTORS_1                  1.0                     //---- 0.5..2.0
EXPRESSWAY_BIAS_FACTORS_1               1.0                     //---- 0.5..2.0
LEFT_TURN_PENALTIES_1                   0 impedance             //---- 0..10000
RIGHT_TURN_PENALTIES_1                  0 impedance             //---- 0..10000
U_TURN_PENALTIES_1                      0 impedance             //---- 0..10000
TRANSFER_PENALTIES_1                    0 impedance             //---- 0..100000
STOP_WAITING_PENALTIES_1                0 impedance             //---- 0..100000
STATION_WAITING_PENALTIES_1             0 impedance             //---- 0..100000
BUS_BIAS_FACTORS_1                      1.0                     //---- 1.0..3.0
BUS_BIAS_CONSTANTS_1                    0 impedance             //---- 0..10000
BRT_BIAS_FACTORS_1                      1.0                     //---- 1.0..3.0
BRT_BIAS_CONSTANTS_1                    0 impedance             //---- 0..10000
RAIL_BIAS_FACTORS_1                     1.0                     //---- 0.1..1.0
RAIL_BIAS_CONSTANTS_1                   0 impedance             //---- -1000..0
MAX_WALK_DISTANCES_1                    3000 meters             //---- 300..60000 feet
WALK_PENALTY_DISTANCES_1                6000 feet               //---- 300..30000 feet
WALK_PENALTY_FACTORS_1                  0.0                     //---- 0.0..25.0
MAX_BICYCLE_DISTANCES_1                 30000 feet              //---- 3000..120000 feet
BIKE_PENALTY_DISTANCES_1                30000 feet              //---- 3000..60000 feet
BIKE_PENALTY_FACTORS_1                  0.0                     //---- 0.0..25.0
MAX_WAIT_TIMES_1                        180 minutes             //---- 5..400 minutes
WAIT_PENALTY_TIMES_1                    60 minutes              //---- 5..200 minutes
WAIT_PENALTY_FACTORS_1                  0.0                     //---- 0.0..25.0
MIN_WAIT_TIMES_1                        0 seconds               //---- 0..3600 seconds
MAX_NUMBER_OF_TRANSFERS_1               1                       //---- 0..10
MAX_PARK_RIDE_PERCENTS_1                50 percent              //---- 1..100 percent
MAX_KISS_RIDE_PERCENTS_1                35 percent              //---- 1..100 percent
KISS_RIDE_TIME_FACTORS_1                2.5                     //---- 1.0..4.4
KISS_RIDE_STOP_TYPES                    EXTERNAL
MAX_KISS_RIDE_DROPOFF_WALK              300 feet                //---- 30..1500 feet
TRANSIT_PENALTY_FILE                                            //---- [project_directory]filename
PARKING_PENALTY_FILE                                            //---- [project_directory]filename
DEFAULT_PARKING_DURATION                0.0 hours               //---- 0.0..24.0 hours
MAX_LEGS_PER_PATH                       1000                    //---- 10..10000
MAX_NUMBER_OF_PATHS                     4                       //---- 1..10
FARE_CLASS_DISTRIBUTION                 0
LOCAL_ACCESS_DISTANCE                   6000 feet               //---- 600..25000 meters
LOCAL_FACILITY_TYPE                     EXTERNAL                //---- MAJOR..LOCAL, EXTERNAL
LOCAL_IMPEDANCE_FACTOR                  0.0                     //---- 0.0..25.0
MAX_CIRCUITY_RATIO                      0.0                     //---- 0.0..10.0
MIN_CIRCUITY_DISTANCE                   6000 feet               //---- 0..30000 feet
MAX_CIRCUITY_DISTANCE                   60000 feet              //---- 0..300000 feet
MIN_DURATION_FACTORS                    0.1, 0.5, 0.8, 1.0      //---- 0.0..1.0
NOTES_AND_NAME_FIELDS                   FALSE                   //---- TRUE/FALSE, YES/NO, 1/0, T/F, Y/N
SAVE_LANE_USE_FLOWS                     FALSE                   //---- TRUE/FALSE, YES/NO, 1/0, T/F, Y/N

#---- Data Service Keys ----

DAILY_WRAP_FLAG                         FALSE                   //---- TRUE/FALSE, YES/NO, 1/0, T/F, Y/N
SUMMARY_TIME_RANGES                     ALL                     //---- e.g., ALL, 0..97200 seconds, 0.0..27.0 hours, 0:00..27:00
SUMMARY_TIME_INCREMENT                  15 minutes              //---- 0, 2..240 minutes
TRIP_SORT_TYPE                          DO_NOT_SORT             //---- DO_NOT_SORT, TRAVELER_SORT, TIME_SORT
PLAN_SORT_TYPE                          TIME_SORT               //---- DO_NOT_SORT, TRAVELER_SORT, TIME_SORT

#---- Flow-Time Service Keys ----

UPDATE_FLOW_RATES                       TRUE                    //---- TRUE/FALSE, YES/NO, 1/0, T/F, Y/N
UPDATE_TURNING_MOVEMENTS                TRUE                    //---- TRUE/FALSE, YES/NO, 1/0, T/F, Y/N
CLEAR_INPUT_FLOW_RATES                  TRUE                    //---- TRUE/FALSE, YES/NO, 1/0, T/F, Y/N
UPDATE_TRAVEL_TIMES                     TRUE                    //---- TRUE/FALSE, YES/NO, 1/0, T/F, Y/N
TIME_UPDATE_RATE                        0                       //---- -1..5000
AVERAGE_TRAVEL_TIMES                    FALSE                   //---- TRUE/FALSE, YES/NO, 1/0, T/F, Y/N
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

FREEWAY_BIAS_FACTORS_1                            0.8                       ## 1    SOV
EXPRESSWAY_BIAS_FACTORS_1                         1.0                       ## 1    SOV

#---- Draw Service Keys ----

MAXIMUM_NUMBER_OF_ITERATIONS            0 // 10                       //---- 0..1000
LINK_CONVERGENCE_CRITERIA               0.001                     //---- 0..10.0
TRIP_CONVERGENCE_CRITERIA               0.001                     //---- 0..10.0
TRANSIT_CAPACITY_CRITERIA               0.0                     //---- 0..10.0
INITIAL_WEIGHTING_FACTOR                1.0                     //---- >= 0.0
ITERATION_WEIGHTING_INCREMENT           1.0                     //---- 0.0..5.0
MAXIMUM_WEIGHTING_FACTOR                20.0                    //---- 0.0, >= 2.0
MINIMIZE_VEHICLE_HOURS                  TRUE                    //---- e.g., 1, 2, 4..10, 100..200, 300
MAXIMUM_RESKIM_ITERATIONS               0                       //---- 0..100
RESKIM_CONVERGENCE_CRITERIA             0.02                    //---- 0..10.0
SAVE_AFTER_ITERATIONS                                       //---- TRUE/FALSE, YES/NO, 1/0, T/F, Y/N
NEW_LINK_CONVERGENCE_FILE                                       //---- [project_directory]filename
NEW_TRIP_CONVERGENCE_FILE                                       //---- [project_directory]filename
PERCENT_TIME_DIFFERENCE                 2.0 percent             //---- 0.0..100.0 percent
MINIMUM_TIME_DIFFERENCE                 10 minutes               //---- 0..120 minutes
MAXIMUM_TIME_DIFFERENCE                 60 minutes              //---- 0..1440 minutes
PERCENT_COST_DIFFERENCE                 0.0 percent             //---- 0.0..100.0 percent
MINIMUM_COST_DIFFERENCE                 10 impedance            //---- 0..500 impedance
MAXIMUM_COST_DIFFERENCE                 1000 impedance          //---- 0..10000 impedance
PERCENT_TRIP_DIFFERENCE                 0.0 percent             //---- 0.0..100.0 percent
MINIMUM_TRIP_DIFFERENCE                 10 minutes              //---- 0..120 minutes
MAXIMUM_TRIP_DIFFERENCE                 60 minutes              //---- 0..1440 minutes
SELECTION_PERCENTAGE                    100.0 percent            //---- 0.01..100.0 percent
MAXIMUM_PERCENT_SELECTED                10.0 percent          //---- 0.1..100.0 percent

#---- Router Control Keys ----

APPLICATION_METHOD                      LINK_FLOWS // TRAVEL_PLANS            //---- TRAVEL_PLANS, LINK_FLOWS, DUE_PLANS, DTA_FLOWS
STORE_TRIPS_IN_MEMORY                   TRUE                    //---- TRUE/FALSE, YES/NO, 1/0, T/F, Y/N
STORE_PLANS_IN_MEMORY                   TRUE                    //---- TRUE/FALSE, YES/NO, 1/0, T/F, Y/N
INITIALIZE_TRIP_PRIORITY                NO // CRITICAL	// NO                //---- NO, LOW, MEDIUM, HIGH, CRITICAL
UPDATE_PLAN_RECORDS                     FALSE                   //---- TRUE/FALSE, YES/NO, 1/0, T/F, Y/N
REROUTE_FROM_TIME_POINT                 0:00
PRINT_UPDATE_WARNINGS                   TRUE                    //---- TRUE/FALSE, YES/NO, 1/0, T/F, Y/N
OUTPUT_ALL_RECORDS                      FALSE                   //---- TRUE/FALSE, YES/NO, 1/0, T/F, Y/N
PRELOAD_TRANSIT_VEHICLES                FALSE                   //---- TRUE/FALSE, YES/NO, 1/0, T/F, Y/N

#---- Program Report Keys ----

ROUTER_REPORT_1             LINK_GAP_REPORT                                    //---- program report name
ROUTER_REPORT_2             TRIP_GAP_REPORT                                    //---- program report name
                                                                //---- TRAVELER_TYPE_SCRIPT
                                                                //---- TRAVELER_TYPE_STACK
                                                                //---- LINK_GAP_REPORT
                                                                //---- TRIP_GAP_REPORT
                                                                //---- ITERATION_PROBLEMS
