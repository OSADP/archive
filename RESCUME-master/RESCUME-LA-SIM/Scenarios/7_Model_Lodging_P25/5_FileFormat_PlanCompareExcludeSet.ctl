TITLE                                   FileFormat Default Control Keys
REPORT_DIRECTORY                        
REPORT_FILE                                                     //---- [report_directory]filename[_partition][.prn]
REPORT_FLAG                             FALSE                   //---- TRUE/FALSE, YES/NO, 1/0, T/F, Y/N
PAGE_LENGTH                             50000                      //---- >= 0
PROJECT_DIRECTORY                       
DEFAULT_FILE_FORMAT                     TAB_DELIMITED           //---- TEXT, BINARY, FIXED_COLUMN, COMMA_DELIMITED, SPACE_DELIMITED, TAB_DELIMITED, CSV_DELIMITED, DBASE, ARCVIEW, SQLITE3, VERSION3, TPPLUS, CUBE, TRANSCAD
TIME_OF_DAY_FORMAT                      DAY_TIME                //---- SECONDS, MINUTES, HOURS, HOUR_CLOCK, DAY_TIME, TIME_CODE, HOUR_MINUTE
UNITS_OF_MEASURE                        METRIC                 //---- METRIC, ENGLISH
DRIVE_SIDE_OF_ROAD                      RIGHT_SIDE              //---- RIGHT_SIDE, LEFT_SIDE
RANDOM_NUMBER_SEED                      0                       //---- 0 = computer clock, > 0 = fixed
USER_FUNCTIONS_1                                                //---- TYPE, A, B, C, D

#---- FileFormat Control Keys ----

DATA_FILE_1                             Results\Router_Hwy_Problems_BASE_ProblemSet.csv                        //---- [project_directory]filename
DATA_FORMAT_1                           TAB_DELIMITED           //---- TEXT, BINARY, FIXED_COLUMN, COMMA_DELIMITED, SPACE_DELIMITED, TAB_DELIMITED, CSV_DELIMITED, DBASE, ARCVIEW, SQLITE3, VERSION3, TPPLUS, CUBE, TRANSCAD

DATA_FILE_2                             Results\Router_Hwy_Problems_SCEN_ProblemSet.csv                        //---- [project_directory]filename


NEW_DATA_FILE_1                         Results\Selection_BASE_Problems.csv                        //---- [project_directory]filename
NEW_DATA_FORMAT_1                       COMMA_DELIMITED           //---- TEXT, BINARY, FIXED_COLUMN, COMMA_DELIMITED, SPACE_DELIMITED, TAB_DELIMITED, CSV_DELIMITED, DBASE, ARCVIEW, SQLITE3, VERSION3, TPPLUS, CUBE, TRANSCAD
COPY_EXISTING_FIELDS_1                  FALSE                    //---- TRUE/FALSE, YES/NO, 1/0, T/F, Y/N				
NEW_FILE_HEADER_1                       TRUE                    //---- TRUE/FALSE, YES/NO, 1/0, T/F, Y/N
NEW_DATA_FIELD_1_1                      HHOLD                        //---- NAME, INTEGER, 10
NEW_DATA_FIELD_1_2                      PERSON                        //---- NAME, INTEGER, 10
NEW_DATA_FIELD_1_3                      TOUR                        //---- NAME, INTEGER, 10
NEW_DATA_FIELD_1_4                      TRIP                        //---- NAME, INTEGER, 10

NEW_DATA_FILE_2                         Results\Selection_SCEN_Problems.csv                        //---- [project_directory]filename
NEW_DATA_FORMAT_2                       COMMA_DELIMITED           //---- TEXT, BINARY, FIXED_COLUMN, COMMA_DELIMITED, SPACE_DELIMITED, TAB_DELIMITED, CSV_DELIMITED, DBASE, ARCVIEW, SQLITE3, VERSION3, TPPLUS, CUBE, TRANSCAD
COPY_EXISTING_FIELDS_2                  FALSE                    //---- TRUE/FALSE, YES/NO, 1/0, T/F, Y/N				
NEW_FILE_HEADER_2                       FALSE                    //---- TRUE/FALSE, YES/NO, 1/0, T/F, Y/N
NEW_DATA_FIELD_2_1                      HHOLD                        //---- NAME, INTEGER, 10
NEW_DATA_FIELD_2_2                      PERSON                        //---- NAME, INTEGER, 10
NEW_DATA_FIELD_2_3                      TOUR                        //---- NAME, INTEGER, 10
NEW_DATA_FIELD_2_4                      TRIP                        //---- NAME, INTEGER, 10


SORT_BY_FIELDS_1                        
DATA_FIELD_MAP_1_1                                              //---- DATA_FIELD = COMBINE_FIELD
## DATA_INDEX_FIELD_1                      HHOLD
## DATA_INDEX_FIELD_2                      PERSON
## DATA_INDEX_FIELD_3                      TOUR
## DATA_INDEX_FIELD_4                      TRIP
## NEW_MERGE_DATA_FILE                     Results\test.csv                        //---- [project_directory]filename
NEW_MERGE_DATA_FORMAT                   TAB_DELIMITED           //---- TEXT, BINARY, FIXED_COLUMN, COMMA_DELIMITED, SPACE_DELIMITED, TAB_DELIMITED, CSV_DELIMITED, DBASE, ARCVIEW, SQLITE3, VERSION3, TPPLUS, CUBE, TRANSCAD
NEW_COMBINE_FIELDS_FILE                                         //---- [project_directory]filename
NEW_COMBINE_FIELDS_FORMAT               TAB_DELIMITED           //---- TEXT, BINARY, FIXED_COLUMN, COMMA_DELIMITED, SPACE_DELIMITED, TAB_DELIMITED, CSV_DELIMITED, DBASE, ARCVIEW, SQLITE3, VERSION3, TPPLUS, CUBE, TRANSCAD
MATRIX_FILE_1                                                   //---- [project_directory]filename
MATRIX_FORMAT_1                         TAB_DELIMITED           //---- TRANSCAD, CUBE, TPPLUS, TRANPLAN, BINARY, FIXED_COLUMN, COMMA_DELIMITED, SPACE_DELIMITED, TAB_DELIMITED, CSV_DELIMITED, DBASE, SQLITE3
NEW_MATRIX_FILE_1                                               //---- [project_directory]filename
NEW_MATRIX_FORMAT_1                     TAB_DELIMITED           //---- TRANSCAD, CUBE, TPPLUS, TRANPLAN, BINARY, FIXED_COLUMN, COMMA_DELIMITED, SPACE_DELIMITED, TAB_DELIMITED, CSV_DELIMITED, DBASE, SQLITE3
SELECT_TABLES_1                         ALL
CONVERSION_SCRIPT                                               //---- [project_directory]filename

#---- Program Report Keys ----

FILEFORMAT_REPORT_1                                             //---- program report name
                                                                //---- CONVERSION_SCRIPT
                                                                //---- CONVERSION_STACK
                                                                //---- FIELD_STATISTICS
