TITLE                                             Person Volume / Day / Direction
DEFAULT_FILE_FORMAT                               TAB_DELIMITED
TIME_OF_DAY_FORMAT                                DAY_TIME                //---- SECONDS, MINUTES, HOURS, HOUR_CLOCK, DAY_TIME, TIME_CODE

MODEL_START_TIME                                  0:00                    //---- >= 0 [seconds], 0.0 [hours], 0:00
MODEL_END_TIME                                    60:00                   //---- > [model_start_time]
MODEL_TIME_INCREMENT                              15 minutes              // FIFTEEN MINUTES
UNITS_OF_MEASURE                                  METRIC                 //---- METRIC, ENGLISH

VEHICLE_TYPE_FILE                                 ..\..\..\Common_Inputs\6_Network_Merged\ASCIIFiles\Vehicle_Type.csv

NODE_FILE                                        ..\..\..\Common_Inputs\6_Network_Merged\ASCIIFiles\Node.csv
LINK_FILE                                        ..\..\..\Common_Inputs\6_Network_Merged\ASCIIFiles\Link.csv
SHAPE_FILE                                       ..\..\..\Common_Inputs\6_Network_Merged\ASCIIFiles\Shape.csv

PERFORMANCE_FILE                                  ..\Results\Router_Hwy_LinkPerformance_BASE ##   Results\Router_Hwy_Performance_PLANSUM.txt
NEW_ARC_PERFORMANCE_FILE                          Performance_Measures_ShapeFiles\Performance_Volume_Bandwidth_BASE.shp ##   Performance_Measures\Performance_Data_Bandwidth_PLANSUM.shp

NOTES_AND_NAME_FIELDS                             TRUE                   //---- TRUE/FALSE, YES/NO, 1/0, T/F, Y/N
SAVE_LANE_USE_FLOWS                               FALSE                   //---- TRUE/FALSE, YES/NO, 1/0, T/F, Y/N

DAILY_WRAP_FLAG                                   FALSE                   //---- TRUE/FALSE, YES/NO, 1/0, T/F, Y/N
SUMMARY_TIME_RANGES                               0:00..60:00
SUMMARY_TIME_INCREMENT                            0 minutes

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

BANDWIDTH_FIELD                                   VOLUME                  //  FLOW is based on VHT and link length and indicates congestion
BANDWIDTH_SCALING_FACTOR                          30.0 units/foot        //---- 0.01..100000 units/foot
MINIMUM_BANDWIDTH_VALUE                           0                       //---- 0..100000
MINIMUM_BANDWIDTH_SIZE                            0.0 feet                //---- 0.0..35 feet
MAXIMUM_BANDWIDTH_SIZE                            35000.0 feet            //---- 1..35000 feet

#---- Coordinate Projection Keys ----

OUTPUT_COORDINATE_SYSTEM                          UTM, 15N, Meters  //---- LATLONG, DEGREES/MILLION_DEGREES or STATEPLANE/UTM, code, FEET/METERS/MILES/KILOMETERS
