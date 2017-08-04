TITLE                                   ArcPlan Default Control Keys
REPORT_DIRECTORY                        
REPORT_FILE                                                     //---- [report_directory]filename[_partition][.prn]
REPORT_FLAG                             FALSE                   //---- TRUE/FALSE, YES/NO, 1/0, T/F, Y/N
PAGE_LENGTH                             200                      //---- >= 0
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

NODE_FILE                               ..\..\..\Common_Inputs\1_Network_v6\ASCIIFiles\Node.csv                        //---- [project_directory]filename
NODE_FORMAT                             TAB_DELIMITED           //---- TEXT, BINARY, FIXED_COLUMN, COMMA_DELIMITED, SPACE_DELIMITED, TAB_DELIMITED, CSV_DELIMITED, DBASE, ARCVIEW, SQLITE3, VERSION3, TPPLUS, CUBE, TRANSCAD
LINK_FILE                               ..\..\..\Common_Inputs\1_Network_v6\ASCIIFiles\Link.csv                        //---- [project_directory]filename
LINK_FORMAT                             TAB_DELIMITED           //---- TEXT, BINARY, FIXED_COLUMN, COMMA_DELIMITED, SPACE_DELIMITED, TAB_DELIMITED, CSV_DELIMITED, DBASE, ARCVIEW, SQLITE3, VERSION3, TPPLUS, CUBE, TRANSCAD
LOCATION_FILE                           ..\..\..\Common_Inputs\1_Network_v6\ASCIIFiles\Location.csv                        //---- [project_directory]filename
LOCATION_FORMAT                         TAB_DELIMITED           //---- TEXT, BINARY, FIXED_COLUMN, COMMA_DELIMITED, SPACE_DELIMITED, TAB_DELIMITED, CSV_DELIMITED, DBASE, ARCVIEW, SQLITE3, VERSION3, TPPLUS, CUBE, TRANSCAD
PARKING_FILE                            ..\..\..\Common_Inputs\1_Network_v6\ASCIIFiles\Parking.csv                        //---- [project_directory]filename
PARKING_FORMAT                          TAB_DELIMITED           //---- TEXT, BINARY, FIXED_COLUMN, COMMA_DELIMITED, SPACE_DELIMITED, TAB_DELIMITED, CSV_DELIMITED, DBASE, ARCVIEW, SQLITE3, VERSION3, TPPLUS, CUBE, TRANSCAD
SHAPE_FILE                              ..\..\..\Common_Inputs\1_Network_v6\ASCIIFiles\Shape.csv                        //---- [project_directory]filename
SHAPE_FORMAT                            TAB_DELIMITED           //---- TEXT, BINARY, FIXED_COLUMN, COMMA_DELIMITED, SPACE_DELIMITED, TAB_DELIMITED, CSV_DELIMITED, DBASE, ARCVIEW, SQLITE3, VERSION3, TPPLUS, CUBE, TRANSCAD
ACCESS_FILE                             ..\..\..\Common_Inputs\1_Network_v6\ASCIIFiles\Access.csv                        //---- [project_directory]filename
ACCESS_FORMAT                           TAB_DELIMITED           //---- TEXT, BINARY, FIXED_COLUMN, COMMA_DELIMITED, SPACE_DELIMITED, TAB_DELIMITED, CSV_DELIMITED, DBASE, ARCVIEW, SQLITE3, VERSION3, TPPLUS, CUBE, TRANSCAD
TRANSIT_STOP_FILE                       ..\..\..\Common_Inputs\1_Network_v6\ASCIIFiles\Transit_Stop.csv                        //---- [project_directory]filename
TRANSIT_STOP_FORMAT                     TAB_DELIMITED           //---- TEXT, BINARY, FIXED_COLUMN, COMMA_DELIMITED, SPACE_DELIMITED, TAB_DELIMITED, CSV_DELIMITED, DBASE, ARCVIEW, SQLITE3, VERSION3, TPPLUS, CUBE
TRANSIT_FARE_FILE                                               //---- [project_directory]filename
TRANSIT_FARE_FORMAT                     TAB_DELIMITED           //---- TEXT, BINARY, FIXED_COLUMN, COMMA_DELIMITED, SPACE_DELIMITED, TAB_DELIMITED, CSV_DELIMITED, DBASE, ARCVIEW, SQLITE3, VERSION3, TPPLUS, CUBE
TRANSIT_ROUTE_FILE                      ..\..\..\Common_Inputs\1_Network_v6\ASCIIFiles\Transit_Route.csv                        //---- [project_directory]filename
TRANSIT_ROUTE_FORMAT                    TAB_DELIMITED           //---- TEXT, BINARY, FIXED_COLUMN, COMMA_DELIMITED, SPACE_DELIMITED, TAB_DELIMITED, CSV_DELIMITED, DBASE, ARCVIEW, SQLITE3, VERSION3, TPPLUS, CUBE
TRANSIT_SCHEDULE_FILE                   ..\..\..\Common_Inputs\1_Network_v6\ASCIIFiles\Transit_Schedule.csv                        //---- [project_directory]filename
TRANSIT_SCHEDULE_FORMAT                 TAB_DELIMITED           //---- TEXT, BINARY, FIXED_COLUMN, COMMA_DELIMITED, SPACE_DELIMITED, TAB_DELIMITED, CSV_DELIMITED, DBASE, ARCVIEW, SQLITE3, VERSION3, TPPLUS, CUBE
TRANSIT_DRIVER_FILE                     ..\..\..\Common_Inputs\1_Network_v6\ASCIIFiles\Transit_Driver.csv                        //---- [project_directory]filename
TRANSIT_DRIVER_FORMAT                   TAB_DELIMITED           //---- TEXT, BINARY, FIXED_COLUMN, COMMA_DELIMITED, SPACE_DELIMITED, TAB_DELIMITED, CSV_DELIMITED, DBASE, ARCVIEW, SQLITE3, VERSION3, TPPLUS, CUBE
VEHICLE_TYPE_FILE                       ..\..\..\Common_Inputs\1_Network_v6\ASCIIFiles\Vehicle_Type.csv                        //---- [project_directory]filename
VEHICLE_TYPE_FORMAT                     TAB_DELIMITED           //---- TEXT, BINARY, FIXED_COLUMN, COMMA_DELIMITED, SPACE_DELIMITED, TAB_DELIMITED, CSV_DELIMITED, DBASE, ARCVIEW, SQLITE3, VERSION3, TPPLUS, CUBE, TRANSCAD
## PLAN_FILE                               ..\Demand\Save_Plans_BASE.txt                        //---- [project_directory]filename.*
## PLAN_FILE                               ..\Outputs\Plans_BASE_Select                       //---- [project_directory]filename.*
PLAN_FILE                               ..\Demand\Plans_TRN_BASE                       //---- [project_directory]filename.*
PLAN_FORMAT                             BINARY           //---- TEXT, BINARY, FIXED_COLUMN, COMMA_DELIMITED, SPACE_DELIMITED, TAB_DELIMITED, CSV_DELIMITED, DBASE, ARCVIEW, SQLITE3, VERSION3, TPPLUS, CUBE, TRANSCAD
PROBLEM_FILE                                                    //---- [project_directory]filename.*
PROBLEM_FORMAT                          TAB_DELIMITED           //---- TEXT, BINARY, FIXED_COLUMN, COMMA_DELIMITED, SPACE_DELIMITED, TAB_DELIMITED, CSV_DELIMITED, DBASE, ARCVIEW, SQLITE3, VERSION3, TPPLUS, CUBE, TRANSCAD
SELECTION_FILE                                                  //---- [project_directory]filename.*
SELECTION_FORMAT                        TAB_DELIMITED           //---- TEXT, BINARY, FIXED_COLUMN, COMMA_DELIMITED, SPACE_DELIMITED, TAB_DELIMITED, CSV_DELIMITED, DBASE, ARCVIEW, SQLITE3, VERSION3, TPPLUS, CUBE, TRANSCAD
NOTES_AND_NAME_FIELDS                   FALSE                   //---- TRUE/FALSE, YES/NO, 1/0, T/F, Y/N
SAVE_LANE_USE_FLOWS                     FALSE                   //---- TRUE/FALSE, YES/NO, 1/0, T/F, Y/N
STOP_EQUIVALENCE_FILE                                           //---- [project_directory]filename

#---- Select Service Keys ----

SELECT_HOUSEHOLDS                       16678 // 361150 //  // 34881 // 386913 (EVAC Bad) // 81454  // 336556 (EVAC Bad) // 309870 (Non-EVAC Bad) // 298070 (GOOD) // 361325                     //---- e.g., 1, 2, 4..10, 100..200, 300
SELECT_MODES                            ALL                     //---- e.g., ALL or 1, 12..14 or WALK, HOV2..HOV4
SELECT_PURPOSES                         ALL                     //---- e.g., 1, 2, 4..10, 100..200, 300
SELECT_PRIORITIES                       ALL                     //---- ALL, or 0..4, or LOW, MEDIUM, HIGH, CRITICAL
SELECT_START_TIMES                      ALL                     //---- e.g., ALL, 0..97200 seconds, 0.0..27.0 hours, 0:00..27:00
SELECT_END_TIMES                        ALL                     //---- e.g., ALL, 0..97200 seconds, 0.0..27.0 hours, 0:00..27:00
SELECT_ORIGINS                          ALL                     //---- e.g., 1, 2, 4..10, 100..200, 300
SELECT_DESTINATIONS                     ALL                     //---- e.g., 1, 2, 4..10, 100..200, 300
SELECT_TRAVELER_TYPES                   ALL                     //---- e.g., 1, 2, 4..10, 100..200, 300
SELECT_FACILITY_TYPES                   ALL                     //---- FREEWAY..EXTERNAL
SELECT_VEHICLE_TYPES                    ALL                     //---- e.g., 1, 2, 4..10, 100..200, 300
SELECT_PROBLEM_TYPES                    ALL                     //---- e.g., 1, 2, 4..10, 100..200, 300
SELECT_LINKS_1                          ALL                     //---- e.g., 1, 2, 4..10, 100..200, 300
SELECT_LINKS_2                          ALL                     //---- e.g., 1, 2, 4..10, 100..200, 300
SELECT_LINKS_3                          ALL                     //---- e.g., 1, 2, 4..10, 100..200, 300
SELECT_LINKS_4                          ALL                      //---- e.g., 1, 2, 4..10, 100..200, 300
SELECT_LINKS_5                          ALL                      //---- e.g., 1, 2, 4..10, 100..200, 300
SELECT_SUBAREAS                                                 //---- e.g., 1, 2, 4..10, 100..200, 300
SELECT_ORIGIN_ZONES                     ALL                     //---- e.g., 1, 2, 4..10, 100..200, 300
SELECT_DESTINATION_ZONES                ALL                     //---- e.g., 1, 2, 4..10, 100..200, 300
SELECTION_POLYGON                                               //---- [project_directory]filename
SELECTION_PERCENTAGE                    100.0 percent           //---- 0.01..100.0 percent

#---- Draw Service Keys ----

DRAW_NETWORK_LANES                      FALSE                   //---- TRUE/FALSE, YES/NO, 1/0, T/F, Y/N
LANE_WIDTH                              12.0 feet               //---- 0..150 feet
CENTER_ONEWAY_LINKS                     FALSE                   //---- TRUE/FALSE, YES/NO, 1/0, T/F, Y/N
LINK_DIRECTION_OFFSET                   0.0 feet                //---- 0..200 feet
PARKING_SIDE_OFFSET                     10.0 feet               //---- 0..600 feet
LOCATION_SIDE_OFFSET                    35.0 feet               //---- 0..1200 feet
TRANSIT_STOP_SIDE_OFFSET                7.0 feet                //---- 0..600 feet
TRANSIT_DIRECTION_OFFSET                0.0 feet                //---- 0..200 feet
MAXIMUM_SHAPE_ANGLE                     0.0 feet                //---- 0..200 feet
MINIMUM_SHAPE_LENGTH                    0.0 feet                //---- 0..200 feet
DRAW_ONEWAY_ARROWS                      FALSE                   //---- TRUE/FALSE, YES/NO, 1/0, T/F, Y/N
ONEWAY_ARROW_LENGTH                     25.0 feet               //---- 0.3..2200 feet
ONEWAY_ARROW_SIDE_OFFSET                6 feet                  //---- 0.3..600 feet
BANDWIDTH_FIELD                         
BANDWIDTH_SCALING_FACTOR                1.0 units/foot          //---- 0.01..100000 units/foot
MINIMUM_BANDWIDTH_VALUE                 0                       //---- 0..100000
MINIMUM_BANDWIDTH_SIZE                  3.5 feet                //---- 0.0..35 feet
MAXIMUM_BANDWIDTH_SIZE                  3500.0 feet             //---- 1..35000 feet

#---- ArcPlan Control Keys ----

NEW_ARC_PLAN_FILE                       Performance_Measures_Shapefiles\Plans_Select_BASE.shp                        //---- [project_directory]filename
NEW_ARC_PROBLEM_FILE                                            //---- [project_directory]filename
NEW_ARC_BANDWIDTH_FILE                                          //---- [project_directory]filename
NEW_ARC_TIME_CONTOUR_FILE                                       //---- [project_directory]filename
NEW_ARC_DISTANCE_CONTOUR_FILE                                   //---- [project_directory]filename
NEW_ARC_ACCESSIBILITY_FILE                                      //---- [project_directory]filename
NEW_ARC_RIDERSHIP_FILE                                          //---- [project_directory]filename
NEW_ARC_STOP_DEMAND_FILE                                        //---- [project_directory]filename
NEW_ARC_STOP_GROUP_FILE                                         //---- [project_directory]filename
NEW_ARC_PARKING_DEMAND_FILE                                     //---- [project_directory]filename
CONTOUR_TIME_INCREMENTS                 10.0 minutes            //---- 0..1000 minutes
CONTOUR_DISTANCE_INCREMENTS             1.0 miles               //---- 0..1000 miles
RIDERSHIP_SCALING_FACTOR                0.0 feet                //---- 0..160 feet
MINIMUM_RIDERSHIP_VALUE                 0.0 feet                //---- 0..160 feet
MINIMUM_RIDERSHIP_SIZE                  0.0 feet                //---- 0..160 feet
MAXIMUM_RIDERSHIP_SIZE                  0.0 feet                //---- 0..160 feet
PROBLEM_DISPLAY_METHOD                  ORG-DES                 //---- ORG-DES, ORIGIN, DESTINATION

#---- Program Report Keys ----

ARCPLAN_REPORT_1                                                //---- program report name
                                                                //---- STOP_EQUIVALENCE

#---- Coordinate Projection Keys ----

INPUT_COORDINATE_SYSTEM                                         //---- LATLONG, DEGREES/MILLION_DEGREES or STATEPLANE/UTM, code, FEET/METERS/MILES/KILOMETERS
INPUT_COORDINATE_ADJUSTMENT                                     //---- X Offset, Y Offset, X Factor, Y Factor
OUTPUT_COORDINATE_SYSTEM                UTM, 15N, METERS                        //---- LATLONG, DEGREES/MILLION_DEGREES or STATEPLANE/UTM, code, FEET/METERS/MILES/KILOMETERS
OUTPUT_COORDINATE_ADJUSTMENT                                    //---- X Offset, Y Offset, X Factor, Y Factor
OUTPUT_XYZ_SHAPES                       FALSE                   //---- TRUE/FALSE, YES/NO, 1/0, T/F, Y/N
OUTPUT_XYM_SHAPES                       FALSE                   //---- TRUE/FALSE, YES/NO, 1/0, T/F, Y/N
