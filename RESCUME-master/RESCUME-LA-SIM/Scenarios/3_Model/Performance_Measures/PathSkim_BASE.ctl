TITLE                           PathSkim Travel Time
MODEL_START_TIME                0:00                    //---- >= 0 [seconds], 0.0 [hours], 0:00
MODEL_END_TIME                  60:00                   //---- > [model_start_time]
MODEL_TIME_INCREMENT            15 minutes              //---- 0, 2..240 minutes
UNITS_OF_MEASURE                METRIC                  //---- METRIC, ENGLISH
NUMBER_OF_THREADS               1



NODE_FILE                       ..\..\..\Common_Inputs\6_Network_Merged\ASCIIFiles\Node.csv
NODE_FORMAT                     TAB_DELIMITED            
LINK_FILE                       ..\..\..\Common_Inputs\6_Network_Merged\ASCIIFiles\Link.csv
LINK_FORMAT                     TAB_DELIMITED            
CONNECTION_FILE                 ..\..\..\Common_Inputs\6_Network_Merged\ASCIIFiles\Connection.csv
CONNECTION_FORMAT               TAB_DELIMITED            
LOCATION_FILE                   ..\..\..\Common_Inputs\6_Network_Merged\ASCIIFiles\Location.csv
LOCATION_FORMAT                 TAB_DELIMITED            
PARKING_FILE					..\..\..\Common_Inputs\6_Network_Merged\ASCIIFiles\Parking.csv
ACCESS_FILE						..\..\..\Common_Inputs\6_Network_Merged\ASCIIFiles\Access.csv
ZONE_FILE                       ..\..\..\Common_Inputs\6_Network_Merged\ASCIIFiles\Zone.csv
VEHICLE_TYPE_FILE               ..\..\..\Common_Inputs\6_Network_Merged\ASCIIFiles\Vehicle_Type.csv

## PERFORMANCE_FILE                ..\Results\Router_Hwy_LinkPerformance_BASE.0
## TURN_DELAY_FILE                 ..\Results\Router_Hwy_TurnDelay_BASE.0

NEW_SKIM_FILE                   Performance_Measures_Shapefiles\Skim_MergeNetwork.csv
NEW_SKIM_FORMAT                 COMMA_DELIMITED

NEW_PLAN_FILE					Performance_Measures_Shapefiles\Skim_Plans_MergeNetwork.csv
NEW_PLAN_FORMAT                 COMMA_DELIMITED

## NEW_PROBLEM_FILE				Performance_Measures_Shapefiles\Skim_Problems_MergeNetwork.csv
NEW_PROBLEM_FORMAT				COMMA_DELIMITED
SKIM_OD_UNITS                   LOCATIONS
## SKIM_OD_UNITS                   ZONES
## SKIM_TIME_PERIODS               6:00..12:00, 12:00..18:00, 30:00..36:00, 36:00..42:00
SKIM_TIME_PERIODS               0:00..48:00
SKIM_TIME_INCREMENT             0
SKIM_TOTAL_TIME_FLAG            TRUE
SKIM_TRAVEL_TIME_FORMAT         MINUTES
SKIM_TRIP_LENGTH_FORMAT         KILOMETERS
NEAREST_NEIGHBOR_FACTOR         0.5

SAVE_LANE_USE_FLOWS             FALSE

## ROUTE_AT_SPECIFIED_TIMES        6:00..12:00, 12:00.18:00, 30:00..36:00, 36:00..42:00
## ROUTE_AT_SPECIFIED_TIMES        12:00..18:00
ROUTE_BY_TIME_INCREMENT         60 minutes
## ROUTE_WITH_TIME_CONSTRAINT      START_TIME
ROUTE_WITH_SPECIFIED_MODE       DRIVE
## ROUTE_FROM_SPECIFIED_ZONES      21670	 ## 15000..50000 21670
ROUTE_FROM_SPECIFIED_LOCATIONS      50005 // 7894	 ## 15000..50000 5473
## ROUTE_TO_SPECIFIED_ZONES        65201, 65202, 65203, 65204, 65205	## 100001		## 100001, 100002, 100003, 100004, 100010
ROUTE_TO_SPECIFIED_LOCATIONS    50006,50007    		## 10158, 100001, 100002, 100003, 100004, 100010
## ROUTE_TO_SPECIFIED_LOCATIONS    20001   		## 10158, 100001, 100002, 100003, 100004, 100010
## ORIGIN_LOCATIONS_PER_ZONE       1
## DESTINATION_LOCATIONS_PER_ZONE  3

## NEW_ORIGIN_LOCATION_FILE			Performance_Measures_Shapefiles\SkimOrigins_BASE
## NEW_DESTINATION_LOCATION_FILE		Performance_Measures_Shapefiles\SkimDestinations_BASE

## LOCATION_SELECTION_METHOD       CENTROID

## NEW_ACCESSIBILITY_FILE          Skim_Network/BASE_Accessibility_Ext.csv
## NEW_ACCESSIBILITY_FORMAT        COMMA_DELIMITED

## ORIGIN_WEIGHT_FIELD             HH
## DESTINATION_WEIGHT_FIELD        TOTEMP
## MAXIMUM_TRAVEL_TIME             45 minutes

RANDOM_NUMBER_SEED              1111

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

## VDOT
EQUATION_PARAMETERS_1                             CONICAL,   15, 50             //  1 => FREEWAY                 Conical VDF A=Alpha, B=MaxTTI
EQUATION_PARAMETERS_2                             CONICAL,    8, 50             //  2 => EXPRESSWAY              Conical VDF A=Alpha, B=MaxTTI
EQUATION_PARAMETERS_4                             CONICAL,    7, 50             //  4 => MAJOR                   Conical VDF A=Alpha, B=MaxTTI
EQUATION_PARAMETERS_5                             CONICAL,  5.5, 50             //  5 => MINOR                   Conical VDF A=Alpha, B=MaxTTI
EQUATION_PARAMETERS_6                             CONICAL,    3, 50             //  6 => COLLECTOR               Conical VDF A=Alpha, B=MaxTTI
EQUATION_PARAMETERS_10                            CONICAL,   15, 50             // 10 => RAMP                    Conical VDF A=Alpha, B=MaxTTI
EQUATION_PARAMETERS_11                            CONICAL,    7, 50             // 11 => BRIDGE  =  MAJOR        Conical VDF A=Alpha, B=MaxTTI
EQUATION_PARAMETERS_12                            CONICAL,    8, 50             // 12 => TUNNEL  =  EXPRESSWAY   Conical VDF A=Alpha, B=MaxTTI
EQUATION_PARAMETERS_13                            CONICAL,  5.5, 50             // 13 => OTHER   =  MINOR        Conical VDF A=Alpha, B=MaxTTI
EQUATION_PARAMETERS_20                            BPR,     0.01,  1, 1          // 20 => EXTERNAL                    BPR VDF A=Alpha, B=Beta,   C=Gamma,  D=MaxV/C

FREEWAY_BIAS_FACTORS_1                            0.8                       ## 1    SOV
EXPRESSWAY_BIAS_FACTORS_1                         0.8                       ## 1    SOV







