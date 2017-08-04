TITLE                                             Reschedule Transit Routes using base link performance
TIME_OF_DAY_FORMAT                                DAY_TIME                //---- SECONDS, MINUTES, HOURS, HOUR_CLOCK, DAY_TIME, TIME_CODE, HOUR_MINUTE
MODEL_START_TIME                                  0:00                    //---- >= 0 [seconds], 0.0 [hours], 0:00
MODEL_END_TIME                                    60:00                   //---- > [model_start_time]
MODEL_TIME_INCREMENT                              15 minutes              //---- 0, 2..240 minutes

#---- System File Keys ----

NODE_FILE                                         ..\1_Network\ASCIIFiles\Node.csv
LINK_FILE                                         ..\1_Network\ASCIIFiles\Link.csv
LANE_USE_FILE                                     ..\1_Network\ASCIIFiles\LaneUse.csv
TRANSIT_STOP_FILE                                 ..\1_Network\ASCIIFiles\Transit_Stop.csv
TRANSIT_ROUTE_FILE                                ..\1_Network\ASCIIFiles\Transit_Route.csv
TRANSIT_SCHEDULE_FILE                             ..\1_Network\ASCIIFiles\Transit_Schedule.csv
TRANSIT_DRIVER_FILE                               ..\1_Network\ASCIIFiles\Transit_Driver.csv

NEW_TRANSIT_SCHEDULE_FILE                         Network\Transit_Schedule_Updated.csv

PERFORMANCE_FILE                           		  
## PERFORMANCE_UPDATE_FILE                           Results\Router_Hwy_LinkPerformance_BASE    // the superior performance file
PERFORMANCE_UPDATE_FILE                           ..\3_Model\Results\Router_Hwy_LinkPerformance_BASE    // the superior performance file

VEHICLE_TYPE_FILE                                 ..\1_Network\ASCIIFiles\Vehicle_Type.csv

NOTES_AND_NAME_FIELDS                             TRUE
SAVE_LANE_USE_FLOWS                               TRUE
