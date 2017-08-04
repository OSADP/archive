TITLE                                             Vehicle Volume / Day / Direction
## REPORT_DIRECTORY
REPORT_FILE                                       //---- [report_directory]filename[_partition][.prn]
REPORT_FLAG                                       FALSE                   //---- TRUE/FALSE, YES/NO, 1/0, T/F, Y/N
PAGE_LENGTH                                       65                      //---- >= 0
## PROJECT_DIRECTORY
DEFAULT_FILE_FORMAT                               TAB_DELIMITED
TIME_OF_DAY_FORMAT                                DAY_TIME                //---- SECONDS, MINUTES, HOURS, HOUR_CLOCK, DAY_TIME, TIME_CODE

MODEL_START_TIME                                  0:00                    //---- >= 0 [seconds], 0.0 [hours], 0:00
MODEL_END_TIME                                    60:00                   //---- > [model_start_time]
MODEL_TIME_INCREMENT                              15 minutes              // FIFTEEN MINUTES

UNITS_OF_MEASURE                                  METRIC                 //---- METRIC, ENGLISH
DRIVE_SIDE_OF_ROAD                                RIGHT_SIDE              //---- RIGHT_SIDE, LEFT_SIDE
RANDOM_NUMBER_SEED                                0                       //---- 0 = computer clock, > 0 = fixed
MAX_WARNING_MESSAGES                              100000                  //---- >= 0
MAX_WARNING_EXIT_FLAG                             TRUE                    //---- TRUE/FALSE, YES/NO, 1/0, T/F, Y/N
MAX_PROBLEM_COUNT                                 0                       //---- >= 0
NUMBER_OF_THREADS                                 1                       //---- 1..128

#---- System File Keys ----

VEHICLE_TYPE_FILE                                 ..\..\..\Common_Inputs\1_Network_v6\ASCIIFiles\Vehicle_Type.csv

NODE_FILE                                         ..\..\..\Common_Inputs\1_Network_v6\ASCIIFiles\Node.csv
LINK_FILE                                         ..\..\..\Common_Inputs\1_Network_v6\ASCIIFiles\Link.csv
SHAPE_FILE                                        ..\..\..\Common_Inputs\1_Network_v6\ASCIIFiles\Shape.csv
CONNECTION_FILE                                   ..\..\..\Common_Inputs\1_Network_v6\ASCIIFiles\Connection.csv

NOTES_AND_NAME_FIELDS                             FALSE                   //---- TRUE/FALSE, YES/NO, 1/0, T/F, Y/N
SAVE_LANE_USE_FLOWS                               FALSE                   //---- TRUE/FALSE, YES/NO, 1/0, T/F, Y/N

#---- Data Service Keys ----

DAILY_WRAP_FLAG                                   FALSE                   //---- TRUE/FALSE, YES/NO, 1/0, T/F, Y/N
SUMMARY_TIME_RANGES                               6:00..12:00, 12:00..18:00, 1@6:00..1@12:00, 1@12:00..1@18:00, 0:00..60:00
SUMMARY_TIME_INCREMENT                            0 minutes

#---- Select Service Keys ----

SELECT_FACILITY_TYPES                             ALL                     //---- FREEWAY..EXTERNAL
SELECT_LINKS_1                                    ALL                     //---- e.g., 1, 2, 4..10, 100..200, 300

#---- Draw Service Keys ----

DRAW_NETWORK_LANES                                FALSE                   //---- TRUE/FALSE, YES/NO, 1/0, T/F, Y/N
DRAW_VEHICLE_SHAPES                               FALSE                   //---- TRUE/FALSE, YES/NO, 1/0, T/F, Y/N
LANE_WIDTH                                        12.0 feet               //---- 0..150 feet
CENTER_ONEWAY_LINKS                               FALSE                   //---- TRUE/FALSE, YES/NO, 1/0, T/F, Y/N
LINK_DIRECTION_OFFSET                             0.0 feet                //---- 0..200 feet


DRAW_AB_DIRECTION                                 TRUE                    //---- TRUE/FALSE, YES/NO, 1/0, T/F, Y/N


MAXIMUM_SHAPE_ANGLE                               0.0 feet                //---- 0..200 feet
MINIMUM_SHAPE_LENGTH                              0.0 feet                //---- 0..200 feet
DRAW_ONEWAY_ARROWS                                FALSE                   //---- TRUE/FALSE, YES/NO, 1/0, T/F, Y/N
ONEWAY_ARROW_LENGTH                               25.0 feet               //---- 0.3..2200 feet
ONEWAY_ARROW_SIDE_OFFSET                          6 feet                  //---- 0.3..600 feet


BANDWIDTH_FIELD                                   D000_6000
BANDWIDTH_SCALING_FACTOR                          5 units/meter           //---- 0.01..100000 units/foot
MINIMUM_BANDWIDTH_VALUE                           0                       //---- 0..100000
MINIMUM_BANDWIDTH_SIZE                            0.0 feet                //---- 0.0..35 feet
MAXIMUM_BANDWIDTH_SIZE                            35000.0 feet            //---- 1..35000 feet


#---- ArcDelay Control Keys ----

LINK_DIRECTION_FILE                               Performance_Measures_Shapefiles\LinkSum_LinkDIR_VolDiff.csv
NEW_ARC_LINK_DIR_FILE                             Performance_Measures_Shapefiles\PerfMeas_LinkDIR_VehicleVolumeDIFF_Bandwidth.shp

ADD_LINK_DIRECTION_INDEX                          FALSE                   //---- TRUE/FALSE, YES/NO, 1/0, T/F, Y/N
IGNORE_TIME_RANGE_FIELDS                          FALSE                   //---- TRUE/FALSE, YES/NO, 1/0, T/F, Y/N

#---- Coordinate Projection Keys ----

OUTPUT_COORDINATE_SYSTEM                          UTM,15N,METERS  //---- LATLONG, DEGREES/MILLION_DEGREES or STATEPLANE/UTM, code, FEET/METERS/MILES/KILOMETERS
