TITLE                                   ArcNet Default Control Keys
REPORT_DIRECTORY                        
REPORT_FILE                                                     //---- [report_directory]filename[_partition][.prn]
REPORT_FLAG                             FALSE                   //---- TRUE/FALSE, YES/NO, 1/0, T/F, Y/N
PAGE_LENGTH                             65                      //---- >= 0
PROJECT_DIRECTORY                       
DEFAULT_FILE_FORMAT                     TAB_DELIMITED           //---- TEXT, BINARY, FIXED_COLUMN, COMMA_DELIMITED, SPACE_DELIMITED, TAB_DELIMITED, CSV_DELIMITED, DBASE, ARCVIEW, SQLITE3, VERSION3, TPPLUS, CUBE
TIME_OF_DAY_FORMAT                      DAY_TIME                //---- SECONDS, MINUTES, HOURS, HOUR_CLOCK, DAY_TIME, TIME_CODE, HOUR_MINUTE
MODEL_START_TIME                        0:00                    //---- >= 0 [seconds], 0.0 [hours], 0:00
MODEL_END_TIME                          24:00                   //---- > [model_start_time]
MODEL_TIME_INCREMENT                    15 minutes              //---- 0, 2..240 minutes
UNITS_OF_MEASURE                        METRIC                 //---- METRIC, ENGLISH
DRIVE_SIDE_OF_ROAD                      RIGHT_SIDE              //---- RIGHT_SIDE, LEFT_SIDE
RANDOM_NUMBER_SEED                      0                       //---- 0 = computer clock, > 0 = fixed
MAX_WARNING_MESSAGES                    100000                  //---- >= 0
MAX_WARNING_EXIT_FLAG                   TRUE                    //---- TRUE/FALSE, YES/NO, 1/0, T/F, Y/N
MAX_PROBLEM_COUNT                       0                       //---- >= 0
NUMBER_OF_THREADS                       1                       //---- 1..128

#---- System File Keys ----

NODE_FILE                               ASCIIFiles\Node.csv                        //---- [project_directory]filename
NODE_FORMAT                             TAB_DELIMITED           //---- TEXT, BINARY, FIXED_COLUMN, COMMA_DELIMITED, SPACE_DELIMITED, TAB_DELIMITED, CSV_DELIMITED, DBASE, ARCVIEW, SQLITE3, VERSION3, TPPLUS, CUBE
ZONE_FILE                               ASCIIFiles\Zone.csv                        //---- [project_directory]filename
ZONE_FORMAT                             TAB_DELIMITED           //---- TEXT, BINARY, FIXED_COLUMN, COMMA_DELIMITED, SPACE_DELIMITED, TAB_DELIMITED, CSV_DELIMITED, DBASE, ARCVIEW, SQLITE3, VERSION3, TPPLUS, CUBE
SHAPE_FILE                              ASCIIFiles\Shape.csv                        //---- [project_directory]filename
SHAPE_FORMAT                            TAB_DELIMITED           //---- TEXT, BINARY, FIXED_COLUMN, COMMA_DELIMITED, SPACE_DELIMITED, TAB_DELIMITED, CSV_DELIMITED, DBASE, ARCVIEW, SQLITE3, VERSION3, TPPLUS, CUBE
LINK_FILE                               ASCIIFiles\Link.csv                        //---- [project_directory]filename
LINK_FORMAT                             TAB_DELIMITED           //---- TEXT, BINARY, FIXED_COLUMN, COMMA_DELIMITED, SPACE_DELIMITED, TAB_DELIMITED, CSV_DELIMITED, DBASE, ARCVIEW, SQLITE3, VERSION3, TPPLUS, CUBE
POCKET_FILE                             ASCIIFiles\Pocket.csv                        //---- [project_directory]filename
POCKET_FORMAT                           TAB_DELIMITED           //---- TEXT, BINARY, FIXED_COLUMN, COMMA_DELIMITED, SPACE_DELIMITED, TAB_DELIMITED, CSV_DELIMITED, DBASE, ARCVIEW, SQLITE3, VERSION3, TPPLUS, CUBE
LANE_USE_FILE                           ASCIIFiles\LaneUse.csv                        //---- [project_directory]filename
LANE_USE_FORMAT                         TAB_DELIMITED           //---- TEXT, BINARY, FIXED_COLUMN, COMMA_DELIMITED, SPACE_DELIMITED, TAB_DELIMITED, CSV_DELIMITED, DBASE, ARCVIEW, SQLITE3, VERSION3, TPPLUS, CUBE
LOCATION_FILE                           ASCIIFiles\Location.csv                        //---- [project_directory]filename
LOCATION_FORMAT                         TAB_DELIMITED           //---- TEXT, BINARY, FIXED_COLUMN, COMMA_DELIMITED, SPACE_DELIMITED, TAB_DELIMITED, CSV_DELIMITED, DBASE, ARCVIEW, SQLITE3, VERSION3, TPPLUS, CUBE
PARKING_FILE                            ASCIIFiles\Parking.csv                        //---- [project_directory]filename
PARKING_FORMAT                          TAB_DELIMITED           //---- TEXT, BINARY, FIXED_COLUMN, COMMA_DELIMITED, SPACE_DELIMITED, TAB_DELIMITED, CSV_DELIMITED, DBASE, ARCVIEW, SQLITE3, VERSION3, TPPLUS, CUBE
ACCESS_FILE                             ASCIIFiles\Access.csv                        //---- [project_directory]filename
ACCESS_FORMAT                           TAB_DELIMITED           //---- TEXT, BINARY, FIXED_COLUMN, COMMA_DELIMITED, SPACE_DELIMITED, TAB_DELIMITED, CSV_DELIMITED, DBASE, ARCVIEW, SQLITE3, VERSION3, TPPLUS, CUBE
CONNECTION_FILE                         ASCIIFiles\Connection.csv                        //---- [project_directory]filename
CONNECTION_FORMAT                       TAB_DELIMITED           //---- TEXT, BINARY, FIXED_COLUMN, COMMA_DELIMITED, SPACE_DELIMITED, TAB_DELIMITED, CSV_DELIMITED, DBASE, ARCVIEW, SQLITE3, VERSION3, TPPLUS, CUBE
TURN_PENALTY_FILE                                               //---- [project_directory]filename
TURN_PENALTY_FORMAT                     TAB_DELIMITED           //---- TEXT, BINARY, FIXED_COLUMN, COMMA_DELIMITED, SPACE_DELIMITED, TAB_DELIMITED, CSV_DELIMITED, DBASE, ARCVIEW, SQLITE3, VERSION3, TPPLUS, CUBE
SIGN_FILE                               ASCIIFiles\Sign.csv                        //---- [project_directory]filename
SIGN_FORMAT                             TAB_DELIMITED           //---- TEXT, BINARY, FIXED_COLUMN, COMMA_DELIMITED, SPACE_DELIMITED, TAB_DELIMITED, CSV_DELIMITED, DBASE, ARCVIEW, SQLITE3, VERSION3, TPPLUS, CUBE
SIGNAL_FILE                             ASCIIFiles\Signal.csv                        //---- [project_directory]filename
SIGNAL_FORMAT                           TAB_DELIMITED           //---- TEXT, BINARY, FIXED_COLUMN, COMMA_DELIMITED, SPACE_DELIMITED, TAB_DELIMITED, CSV_DELIMITED, DBASE, ARCVIEW, SQLITE3, VERSION3, TPPLUS, CUBE
TIMING_PLAN_FILE                                                //---- [project_directory]filename
TIMING_PLAN_FORMAT                      TAB_DELIMITED           //---- TEXT, BINARY, FIXED_COLUMN, COMMA_DELIMITED, SPACE_DELIMITED, TAB_DELIMITED, CSV_DELIMITED, DBASE, ARCVIEW, SQLITE3, VERSION3, TPPLUS, CUBE
PHASING_PLAN_FILE                                               //---- [project_directory]filename
PHASING_PLAN_FORMAT                     TAB_DELIMITED           //---- TEXT, BINARY, FIXED_COLUMN, COMMA_DELIMITED, SPACE_DELIMITED, TAB_DELIMITED, CSV_DELIMITED, DBASE, ARCVIEW, SQLITE3, VERSION3, TPPLUS, CUBE
DETECTOR_FILE                                                   //---- [project_directory]filename
DETECTOR_FORMAT                         TAB_DELIMITED           //---- TEXT, BINARY, FIXED_COLUMN, COMMA_DELIMITED, SPACE_DELIMITED, TAB_DELIMITED, CSV_DELIMITED, DBASE, ARCVIEW, SQLITE3, VERSION3, TPPLUS, CUBE
TRANSIT_STOP_FILE                       ASCIIFiles\Transit_Stop.csv                        //---- [project_directory]filename
TRANSIT_STOP_FORMAT                     TAB_DELIMITED           //---- TEXT, BINARY, FIXED_COLUMN, COMMA_DELIMITED, SPACE_DELIMITED, TAB_DELIMITED, CSV_DELIMITED, DBASE, ARCVIEW, SQLITE3, VERSION3, TPPLUS, CUBE
TRANSIT_ROUTE_FILE                      ASCIIFiles\Transit_Route.csv                        //---- [project_directory]filename
TRANSIT_ROUTE_FORMAT                    TAB_DELIMITED           //---- TEXT, BINARY, FIXED_COLUMN, COMMA_DELIMITED, SPACE_DELIMITED, TAB_DELIMITED, CSV_DELIMITED, DBASE, ARCVIEW, SQLITE3, VERSION3, TPPLUS, CUBE
TRANSIT_SCHEDULE_FILE                   ASCIIFiles\Transit_Schedule.csv                        //---- [project_directory]filename
TRANSIT_SCHEDULE_FORMAT                 TAB_DELIMITED           //---- TEXT, BINARY, FIXED_COLUMN, COMMA_DELIMITED, SPACE_DELIMITED, TAB_DELIMITED, CSV_DELIMITED, DBASE, ARCVIEW, SQLITE3, VERSION3, TPPLUS, CUBE
TRANSIT_DRIVER_FILE                     ASCIIFiles\Transit_Driver.csv                       //---- [project_directory]filename
TRANSIT_DRIVER_FORMAT                   TAB_DELIMITED           //---- TEXT, BINARY, FIXED_COLUMN, COMMA_DELIMITED, SPACE_DELIMITED, TAB_DELIMITED, CSV_DELIMITED, DBASE, ARCVIEW, SQLITE3, VERSION3, TPPLUS, CUBE
ROUTE_NODES_FILE                                                //---- [project_directory]filename
ROUTE_NODES_FORMAT                      TAB_DELIMITED           //---- TEXT, BINARY, FIXED_COLUMN, COMMA_DELIMITED, SPACE_DELIMITED, TAB_DELIMITED, CSV_DELIMITED, DBASE, ARCVIEW, SQLITE3, VERSION3, TPPLUS, CUBE
VEHICLE_TYPE_FILE                       ASCIIFiles\Vehicle_Type.csv                        //---- [project_directory]filename
VEHICLE_TYPE_FORMAT                     COMMA_DELIMITED           //---- TEXT, BINARY, FIXED_COLUMN, COMMA_DELIMITED, SPACE_DELIMITED, TAB_DELIMITED, CSV_DELIMITED, DBASE, ARCVIEW, SQLITE3, VERSION3, TPPLUS, CUBE
NOTES_AND_NAME_FIELDS                   TRUE                   //---- TRUE/FALSE, YES/NO, 1/0, T/F, Y/N

#---- Draw Service Keys ----

DRAW_NETWORK_LANES                      TRUE                   //---- TRUE/FALSE, YES/NO, 1/0, T/F, Y/N
LANE_WIDTH                              12.0 feet               //---- 0..150 feet
CENTER_ONEWAY_LINKS                     FALSE                   //---- TRUE/FALSE, YES/NO, 1/0, T/F, Y/N
LINK_DIRECTION_OFFSET                   10.0 feet                //---- 0..200 feet
DRAW_AB_DIRECTION                       TRUE                   //---- TRUE/FALSE, YES/NO, 1/0, T/F, Y/N
POCKET_SIDE_OFFSET                      7.0 feet                //---- 0..300 feet
PARKING_SIDE_OFFSET                     10.0 feet               //---- 0..600 feet
LOCATION_SIDE_OFFSET                    35.0 feet               //---- 0..1200 feet
SIGN_SIDE_OFFSET                        7.0 feet                //---- 0..600 feet
SIGN_SETBACK                            7.0 feet                //---- 0..1200 feet
TRANSIT_STOP_SIDE_OFFSET                7.0 feet                //---- 0..600 feet
TRANSIT_DIRECTION_OFFSET                0.0 feet                //---- 0..200 feet
TRANSIT_OVERLAP_FLAG                    TRUE                    //---- TRUE/FALSE, YES/NO, 1/0, T/F, Y/N
DRAW_ONEWAY_ARROWS                      TRUE                   //---- TRUE/FALSE, YES/NO, 1/0, T/F, Y/N
ONEWAY_ARROW_LENGTH                     25.0 feet               //---- 0.3..2200 feet
ONEWAY_ARROW_SIDE_OFFSET                1.5 feet                  //---- 0.3..600 feet
CURVED_CONNECTION_FLAG                  TRUE                   //---- TRUE/FALSE, YES/NO, 1/0, T/F, Y/N

#---- ArcNet Control Keys ----

SUBZONE_DATA_FILE                                               //---- [project_directory]filename
NEW_ARC_NODE_FILE                       ShapeFiles\Node.shp                        //---- [project_directory]filename
NEW_ARC_ZONE_FILE                       ShapeFiles\Zone.shp                        //---- [project_directory]filename
NEW_ARC_LINK_FILE                       ShapeFiles\Link.shp                        //---- [project_directory]filename
NEW_ARC_CENTERLINE_FILE                 ShapeFiles\Centerline.shp                        //---- [project_directory]filename
NEW_ARC_POCKET_FILE                     ShapeFiles\Pocket.shp                        //---- [project_directory]filename
NEW_ARC_LANE_USE_FILE                   ShapeFiles\LaneUse.shp                        //---- [project_directory]filename
NEW_ARC_LOCATION_FILE                   ShapeFiles\Location.shp                        //---- [project_directory]filename
NEW_ARC_PARKING_FILE                    ShapeFiles\Parking.shp                        //---- [project_directory]filename
NEW_ARC_ACCESS_FILE                     ShapeFiles\Access.shp                        //---- [project_directory]filename
NEW_ARC_CONNECTION_FILE                 ShapeFiles\Connection.shp                        //---- [project_directory]filename
NEW_ARC_TURN_PENALTY_FILE                                       //---- [project_directory]filename
NEW_ARC_SIGN_FILE                       ShapeFiles\Sign.shp                        //---- [project_directory]filename
NEW_ARC_SIGNAL_FILE                     ShapeFiles\Signal.shp                        //---- [project_directory]filename
NEW_ARC_TIMING_PLAN_FILE                                        //---- [project_directory]filename
NEW_ARC_PHASING_PLAN_FILE                                       //---- [project_directory]filename
NEW_ARC_DETECTOR_FILE                                           //---- [project_directory]filename
NEW_ARC_TRANSIT_STOP_FILE               ShapeFiles\Transit_Stop.shp                        //---- [project_directory]filename
NEW_ARC_TRANSIT_ROUTE_FILE              ShapeFiles\Transit_Route.shp                        //---- [project_directory]filename
NEW_ARC_TRANSIT_DRIVER_FILE             ShapeFiles\Transit_Driver.shp                        //---- [project_directory]filename
NEW_ARC_STOP_SERVICE_FILE                                       //---- [project_directory]filename
NEW_ARC_ROUTE_NODES_FILE                                        //---- [project_directory]filename
NEW_ARC_SUBZONE_DATA_FILE                                       //---- [project_directory]filename
SELECT_TIME                             0:00                    //---- 0:00..24:00
TRANSIT_TIME_PERIODS                    0:00                    //---- 0:00..24:00

#---- Coordinate Projection Keys ----

INPUT_COORDINATE_SYSTEM                                         //---- LATLONG, DEGREES/MILLION_DEGREES or STATEPLANE/UTM, code, FEET/METERS/MILES/KILOMETERS
INPUT_COORDINATE_ADJUSTMENT                                     //---- X Offset, Y Offset, X Factor, Y Factor
OUTPUT_COORDINATE_SYSTEM                UTM,15N,METERS                        //---- LATLONG, DEGREES/MILLION_DEGREES or STATEPLANE/UTM, code, FEET/METERS/MILES/KILOMETERS
OUTPUT_COORDINATE_ADJUSTMENT                                    //---- X Offset, Y Offset, X Factor, Y Factor
OUTPUT_XYZ_SHAPES                       FALSE                   //---- TRUE/FALSE, YES/NO, 1/0, T/F, Y/N
OUTPUT_XYM_SHAPES                       FALSE                   //---- TRUE/FALSE, YES/NO, 1/0, T/F, Y/N
