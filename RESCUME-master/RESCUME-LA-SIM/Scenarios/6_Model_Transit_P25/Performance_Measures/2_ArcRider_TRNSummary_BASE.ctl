TITLE                                   ArcRider Default Control Keys
REPORT_DIRECTORY                        
REPORT_FILE                                                     //---- [report_directory]filename[_partition][.prn]
REPORT_FLAG                             FALSE                   //---- TRUE/FALSE, YES/NO, 1/0, T/F, Y/N
PAGE_LENGTH                             50000                      //---- >= 0
PROJECT_DIRECTORY                       
DEFAULT_FILE_FORMAT                     TAB_DELIMITED           //---- TEXT, BINARY, FIXED_COLUMN, COMMA_DELIMITED, SPACE_DELIMITED, TAB_DELIMITED, CSV_DELIMITED, DBASE, ARCVIEW, SQLITE3, VERSION3, TPPLUS, CUBE
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
NODE_FORMAT                             TAB_DELIMITED           //---- TEXT, BINARY, FIXED_COLUMN, COMMA_DELIMITED, SPACE_DELIMITED, TAB_DELIMITED, CSV_DELIMITED, DBASE, ARCVIEW, SQLITE3, VERSION3, TPPLUS, CUBE
LINK_FILE                               ..\..\..\Common_Inputs\1_Network_v6\ASCIIFiles\Link.csv                        //---- [project_directory]filename
LINK_FORMAT                             TAB_DELIMITED           //---- TEXT, BINARY, FIXED_COLUMN, COMMA_DELIMITED, SPACE_DELIMITED, TAB_DELIMITED, CSV_DELIMITED, DBASE, ARCVIEW, SQLITE3, VERSION3, TPPLUS, CUBE
TRANSIT_STOP_FILE                       ..\..\..\Common_Inputs\1_Network_v6\ASCIIFiles\Transit_Stop.csv                        //---- [project_directory]filename
TRANSIT_STOP_FORMAT                     TAB_DELIMITED           //---- TEXT, BINARY, FIXED_COLUMN, COMMA_DELIMITED, SPACE_DELIMITED, TAB_DELIMITED, CSV_DELIMITED, DBASE, ARCVIEW, SQLITE3, VERSION3, TPPLUS, CUBE
TRANSIT_ROUTE_FILE                      ..\..\..\Common_Inputs\1_Network_v6\ASCIIFiles\Transit_Route.csv                        //---- [project_directory]filename
TRANSIT_ROUTE_FORMAT                    TAB_DELIMITED           //---- TEXT, BINARY, FIXED_COLUMN, COMMA_DELIMITED, SPACE_DELIMITED, TAB_DELIMITED, CSV_DELIMITED, DBASE, ARCVIEW, SQLITE3, VERSION3, TPPLUS, CUBE
TRANSIT_SCHEDULE_FILE                   ..\..\..\Common_Inputs\1_Network_v6\ASCIIFiles\Transit_Schedule.csv                        //---- [project_directory]filename
TRANSIT_SCHEDULE_FORMAT                 TAB_DELIMITED           //---- TEXT, BINARY, FIXED_COLUMN, COMMA_DELIMITED, SPACE_DELIMITED, TAB_DELIMITED, CSV_DELIMITED, DBASE, ARCVIEW, SQLITE3, VERSION3, TPPLUS, CUBE
TRANSIT_DRIVER_FILE                     ..\..\..\Common_Inputs\1_Network_v6\ASCIIFiles\Transit_Driver.csv                        //---- [project_directory]filename
TRANSIT_DRIVER_FORMAT                   TAB_DELIMITED           //---- TEXT, BINARY, FIXED_COLUMN, COMMA_DELIMITED, SPACE_DELIMITED, TAB_DELIMITED, CSV_DELIMITED, DBASE, ARCVIEW, SQLITE3, VERSION3, TPPLUS, CUBE
VEHICLE_TYPE_FILE                       ..\..\..\Common_Inputs\1_Network_v6\ASCIIFiles\Vehicle_Type.csv                        //---- [project_directory]filename
VEHICLE_TYPE_FORMAT                     TAB_DELIMITED           //---- TEXT, BINARY, FIXED_COLUMN, COMMA_DELIMITED, SPACE_DELIMITED, TAB_DELIMITED, CSV_DELIMITED, DBASE, ARCVIEW, SQLITE3, VERSION3, TPPLUS, CUBE
RIDERSHIP_FILE                          ..\Results\Ridership.csv                        //---- [project_directory]filename
RIDERSHIP_FORMAT                        TAB_DELIMITED           //---- TEXT, BINARY, FIXED_COLUMN, COMMA_DELIMITED, SPACE_DELIMITED, TAB_DELIMITED, CSV_DELIMITED, DBASE, ARCVIEW, SQLITE3, VERSION3, TPPLUS, CUBE
SHAPE_FILE                              ..\..\..\Common_Inputs\1_Network_v6\ASCIIFiles\Shape.csv                        //---- [project_directory]filename
SHAPE_FORMAT                            TAB_DELIMITED           //---- TEXT, BINARY, FIXED_COLUMN, COMMA_DELIMITED, SPACE_DELIMITED, TAB_DELIMITED, CSV_DELIMITED, DBASE, ARCVIEW, SQLITE3, VERSION3, TPPLUS, CUBE
NOTES_AND_NAME_FIELDS                   FALSE                   //---- TRUE/FALSE, YES/NO, 1/0, T/F, Y/N
STOP_EQUIVALENCE_FILE                                           //---- [project_directory]filename
LINE_EQUIVALENCE_FILE                                           //---- [project_directory]filename

#---- Select Service Keys ----

SELECT_MODES                            ALL                     //---- e.g., ALL or 1, 12..14 or WALK, HOV2..HOV4
SELECT_TIME_OF_DAY                      ALL                     //---- 0:00..24:00
SELECT_START_TIMES                      ALL                     //---- e.g., ALL, 0..97200 seconds, 0.0..27.0 hours, 0:00..27:00
SELECT_END_TIMES                        ALL                     //---- e.g., ALL, 0..97200 seconds, 0.0..27.0 hours, 0:00..27:00
SELECT_LINKS_1                          ALL                     //---- e.g., 1, 2, 4..10, 100..200, 300
SELECT_ROUTES                           ALL                     //---- e.g., 1, 2, 4..10, 100..200, 300
SELECT_STOPS                            ALL                     //---- e.g., 1, 2, 4..10, 100..200, 300

#---- Draw Service Keys ----

TRANSIT_STOP_SIDE_OFFSET                7.0 feet                //---- 0..600 feet
TRANSIT_DIRECTION_OFFSET                0.0 feet                //---- 0..200 feet
TRANSIT_OVERLAP_FLAG                    TRUE                    //---- TRUE/FALSE, YES/NO, 1/0, T/F, Y/N
MAXIMUM_SHAPE_ANGLE                     0.0 feet                //---- 0..200 feet
MINIMUM_SHAPE_LENGTH                    0.0 feet                //---- 0..200 feet
BANDWIDTH_FIELD                         
BANDWIDTH_SCALING_FACTOR                1.0 units/foot          //---- 0.01..100000 units/foot
MINIMUM_BANDWIDTH_VALUE                 0                       //---- 0..100000
MINIMUM_BANDWIDTH_SIZE                  3.5 feet                //---- 0.0..35 feet
MAXIMUM_BANDWIDTH_SIZE                  3500.0 feet             //---- 1..35000 feet

#---- ArcRider Control Keys ----

NEW_ARC_LINE_DEMAND_FILE                ..\Outputs\ArcPlan_Outputs\LineDemand_TRN_BASE.shp                        //---- [project_directory]filename
NEW_ARC_LINE_GROUP_FILE                                         //---- [project_directory]filename
NEW_ARC_RIDERSHIP_FILE                  ..\Outputs\ArcPlan_Outputs\Ridership_TRN_BASE.shp                        //---- [project_directory]filename
NEW_ARC_STOP_DEMAND_FILE                ..\Outputs\ArcPlan_Outputs\StopDemand_TRN_BASE.shp                        //---- [project_directory]filename
NEW_ARC_STOP_GROUP_FILE                                         //---- [project_directory]filename
NEW_ARC_RUN_CAPACITY_FILE                                       //---- [project_directory]filename

#---- Program Report Keys ----

ARCRIDER_REPORT_1                                               //---- program report name
                                                                //---- STOP_EQUIVALENCE
                                                                //---- LINE_EQUIVALENCE

#---- Coordinate Projection Keys ----

INPUT_COORDINATE_SYSTEM                                         //---- LATLONG, DEGREES/MILLION_DEGREES or STATEPLANE/UTM, code, FEET/METERS/MILES/KILOMETERS
INPUT_COORDINATE_ADJUSTMENT                                     //---- X Offset, Y Offset, X Factor, Y Factor
OUTPUT_COORDINATE_SYSTEM                UTM, 15N, Meters                        //---- LATLONG, DEGREES/MILLION_DEGREES or STATEPLANE/UTM, code, FEET/METERS/MILES/KILOMETERS
OUTPUT_COORDINATE_ADJUSTMENT                                    //---- X Offset, Y Offset, X Factor, Y Factor
OUTPUT_XYZ_SHAPES                       FALSE                   //---- TRUE/FALSE, YES/NO, 1/0, T/F, Y/N
OUTPUT_XYM_SHAPES                       FALSE                   //---- TRUE/FALSE, YES/NO, 1/0, T/F, Y/N
