TITLE                                   TripSum Default Control Keys
REPORT_DIRECTORY                        
REPORT_FILE                                                     //---- [report_directory]filename[_partition][.prn]
REPORT_FLAG                             FALSE                   //---- TRUE/FALSE, YES/NO, 1/0, T/F, Y/N
PAGE_LENGTH                             65                      //---- >= 0
PROJECT_DIRECTORY                       
DEFAULT_FILE_FORMAT                     TAB_DELIMITED           //---- TEXT, BINARY, FIXED_COLUMN, COMMA_DELIMITED, SPACE_DELIMITED, TAB_DELIMITED, CSV_DELIMITED, DBASE, ARCVIEW, SQLITE3, VERSION3, TPPLUS, CUBE
TIME_OF_DAY_FORMAT                      DAY_TIME                //---- SECONDS, MINUTES, HOURS, HOUR_CLOCK, DAY_TIME, TIME_CODE, HOUR_MINUTE
MODEL_START_TIME                        0:00                    //---- >= 0 [seconds], 0.0 [hours], 0:00
MODEL_END_TIME                          60:00                   //---- > [model_start_time]
MODEL_TIME_INCREMENT                    15 minutes              //---- 0, 2..240 minutes
UNITS_OF_MEASURE                        ENGLISH                 //---- METRIC, ENGLISH
DRIVE_SIDE_OF_ROAD                      RIGHT_SIDE              //---- RIGHT_SIDE, LEFT_SIDE
RANDOM_NUMBER_SEED                      0                       //---- 0 = computer clock, > 0 = fixed
MAX_WARNING_MESSAGES                    100000                  //---- >= 0
MAX_WARNING_EXIT_FLAG                   TRUE                    //---- TRUE/FALSE, YES/NO, 1/0, T/F, Y/N
MAX_PROBLEM_COUNT                       0                       //---- >= 0
NUMBER_OF_THREADS                       1                       //---- 1..128

#---- System File Keys ----

TRIP_FILE                               Demand\Trips_TRN.csv                        //---- [project_directory]filename.*
TRIP_FORMAT                             TAB_DELIMITED           //---- TEXT, BINARY, FIXED_COLUMN, COMMA_DELIMITED, SPACE_DELIMITED, TAB_DELIMITED, CSV_DELIMITED, DBASE, ARCVIEW, SQLITE3, VERSION3, TPPLUS, CUBE
SELECTION_FILE                                                  //---- [project_directory]filename.*
SELECTION_FORMAT                        TAB_DELIMITED           //---- TEXT, BINARY, FIXED_COLUMN, COMMA_DELIMITED, SPACE_DELIMITED, TAB_DELIMITED, CSV_DELIMITED, DBASE, ARCVIEW, SQLITE3, VERSION3, TPPLUS, CUBE
NODE_FILE                               ..\..\Common_Inputs\1_Network_v6\ASCIIFiles\Node.csv                        //---- [project_directory]filename
NODE_FORMAT                             TAB_DELIMITED           //---- TEXT, BINARY, FIXED_COLUMN, COMMA_DELIMITED, SPACE_DELIMITED, TAB_DELIMITED, CSV_DELIMITED, DBASE, ARCVIEW, SQLITE3, VERSION3, TPPLUS, CUBE
LINK_FILE                               ..\..\Common_Inputs\1_Network_v6\ASCIIFiles\Link.csv                        //---- [project_directory]filename
LINK_FORMAT                             TAB_DELIMITED           //---- TEXT, BINARY, FIXED_COLUMN, COMMA_DELIMITED, SPACE_DELIMITED, TAB_DELIMITED, CSV_DELIMITED, DBASE, ARCVIEW, SQLITE3, VERSION3, TPPLUS, CUBE
SHAPE_FILE                                                      //---- [project_directory]filename
SHAPE_FORMAT                            TAB_DELIMITED           //---- TEXT, BINARY, FIXED_COLUMN, COMMA_DELIMITED, SPACE_DELIMITED, TAB_DELIMITED, CSV_DELIMITED, DBASE, ARCVIEW, SQLITE3, VERSION3, TPPLUS, CUBE
LOCATION_FILE                                                   //---- [project_directory]filename
LOCATION_FORMAT                         TAB_DELIMITED           //---- TEXT, BINARY, FIXED_COLUMN, COMMA_DELIMITED, SPACE_DELIMITED, TAB_DELIMITED, CSV_DELIMITED, DBASE, ARCVIEW, SQLITE3, VERSION3, TPPLUS, CUBE
ZONE_FILE                               ..\..\Common_Inputs\1_Network_v6\ASCIIFiles\Zone.csv                        //---- [project_directory]filename
ZONE_FORMAT                             TAB_DELIMITED           //---- TEXT, BINARY, FIXED_COLUMN, COMMA_DELIMITED, SPACE_DELIMITED, TAB_DELIMITED, CSV_DELIMITED, DBASE, ARCVIEW, SQLITE3, VERSION3, TPPLUS, CUBE
NOTES_AND_NAME_FIELDS                   FALSE                   //---- TRUE/FALSE, YES/NO, 1/0, T/F, Y/N
ZONE_EQUIVALENCE_FILE                                           //---- [project_directory]filename
TIME_EQUIVALENCE_FILE                                           //---- [project_directory]filename

#---- Data Service Keys ----

DAILY_WRAP_FLAG                         FALSE                   //---- TRUE/FALSE, YES/NO, 1/0, T/F, Y/N
SUMMARY_TIME_RANGES                     ALL                     //---- e.g., ALL, 0..97200 seconds, 0.0..27.0 hours, 0:00..27:00
SUMMARY_TIME_INCREMENT                  15 minutes              //---- 0, 2..240 minutes

#---- Select Service Keys ----

SELECT_HOUSEHOLDS                       ALL                     //---- e.g., 1, 2, 4..10, 100..200, 300
SELECT_MODES                            ALL                     //---- e.g., ALL or 1, 12..14 or WALK, HOV2..HOV4
SELECT_PURPOSES                         ALL                     //---- e.g., 1, 2, 4..10, 100..200, 300
SELECT_START_TIMES                      ALL                     //---- e.g., ALL, 0..97200 seconds, 0.0..27.0 hours, 0:00..27:00
SELECT_END_TIMES                        ALL                     //---- e.g., ALL, 0..97200 seconds, 0.0..27.0 hours, 0:00..27:00
SELECT_ORIGINS                          ALL                     //---- e.g., 1, 2, 4..10, 100..200, 300
SELECT_DESTINATIONS                     ALL                     //---- e.g., 1, 2, 4..10, 100..200, 300
SELECT_TRAVELER_TYPES                   ALL                     //---- e.g., 1, 2, 4..10, 100..200, 300
SELECT_VEHICLE_TYPES                    ALL                     //---- e.g., 1, 2, 4..10, 100..200, 300
SELECT_SUBAREAS                                                 //---- e.g., 1, 2, 4..10, 100..200, 300
SELECTION_POLYGON                                               //---- [project_directory]filename
SELECT_ORIGIN_ZONES                     ALL                     //---- e.g., 1, 2, 4..10, 100..200, 300
SELECT_DESTINATION_ZONES                ALL                     //---- e.g., 1, 2, 4..10, 100..200, 300
SELECTION_PERCENTAGE                    100.0 percent           //---- 0.01..100.0 percent

#---- TripSum Control Keys ----

NEW_TIME_DISTRIBUTION_FILE              Outputs\TripDistribution_TRN.csv                        //---- [project_directory]filename
NEW_TIME_DISTRIBUTION_FORMAT            COMMA_DELIMITED                        //---- [project_directory]filename
NEW_TRIP_TIME_FILE                                              //---- [project_directory]filename
NEW_TRIP_LENGTH_FILE                                            //---- [project_directory]filename
NEW_LINK_TRIP_END_FILE                                          //---- [project_directory]filename
NEW_LINK_TRIP_END_FORMAT                TAB_DELIMITED           //---- TEXT, BINARY, FIXED_COLUMN, COMMA_DELIMITED, SPACE_DELIMITED, TAB_DELIMITED, CSV_DELIMITED, DBASE, ARCVIEW, SQLITE3, VERSION3, TPPLUS, CUBE
NEW_LOCATION_TRIP_END_FILE                                      //---- [project_directory]filename
NEW_LOCATION_TRIP_END_FORMAT            TAB_DELIMITED           //---- TEXT, BINARY, FIXED_COLUMN, COMMA_DELIMITED, SPACE_DELIMITED, TAB_DELIMITED, CSV_DELIMITED, DBASE, ARCVIEW, SQLITE3, VERSION3, TPPLUS, CUBE
NEW_ZONE_TRIP_END_FILE                                          //---- [project_directory]filename
NEW_ZONE_TRIP_END_FORMAT                COMMA_DELIMITED           //---- TEXT, BINARY, FIXED_COLUMN, COMMA_DELIMITED, SPACE_DELIMITED, TAB_DELIMITED, CSV_DELIMITED, DBASE, ARCVIEW, SQLITE3, VERSION3, TPPLUS, CUBE
NEW_TRIP_TABLE_FILE                                             //---- [project_directory]filename
NEW_TRIP_TABLE_FORMAT                   TAB_DELIMITED           //---- TEXT, BINARY, FIXED_COLUMN, COMMA_DELIMITED, SPACE_DELIMITED, TAB_DELIMITED, CSV_DELIMITED, DBASE, ARCVIEW, SQLITE3, VERSION3, TPPLUS, CUBE
TRIP_TIME_INCREMENT                     15 minutes              //---- 0, 2..240 minutes
TRIP_LENGTH_INCREMENT                   1.0 miles               //---- 0.0, 0.1..10.0 miles
DISTANCE_CALCULATION                    STRAIGHT_LINE           //---- STRAIGHT_LINE, RIGHT_ANGLE, SIMPLE_AVERAGE, WEIGHTED_AVERAGE

#---- Program Report Keys ----

TRIPSUM_REPORT_1   TRIP_TIME_REPORT                                             //---- program report name
TRIPSUM_REPORT_2   TIME_DISTRIBUTION                                                             //---- TOP_100_LINK_TRIP_ENDS
                                                                //---- TOP_100_LANE_TRIP_ENDS
                                                                //---- TOP_100_TRIP/CAPACITY_RATIOS
                                                                //---- ZONE_EQUIVALENCE
                                                                //---- TIME_EQUIVALENCE
                                                                //---- TIME_DISTRIBUTION
                                                                //---- TRIP_TIME_REPORT
                                                                //---- TRIP_LENGTH_SUMMARY
                                                                //---- TRIP_PURPOSE_SUMMARY
                                                                //---- MODE_LENGTH_SUMMARY
                                                                //---- MODE_PURPOSE_SUMMARY
