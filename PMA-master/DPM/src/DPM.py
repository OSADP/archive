"""
The purpose of the prototype DMA Performance Measurement application is to 
measures mode-independent trip-based traveler mobility and system productivity 
by taking trip or trajectory based vehicle input and aggregating that 
information into system wide performance measures.  The DMA Performance 
Measurement application uses trip-based system performance measure algorithms 
developed under the Integrated Corridor Management (ICM) Program and adapts 
them for use with observed data to measure travel time reliability, delay,and 
throughput.

The program has four(4) files:
DPM.py - Main program
Files.py - Classes used to read in all of the input files
sqlload.py - Classes to control the program's interface with the SQLlite 
             database
timeslice.py - Class that is used for managing and determining the individual 
                time slices based on the trip starting time

To run the program type the following from a command prompt:
>>python DPM.py -file [your control file name]

"""
__author__ = 'Jim Larkin (Noblis)'
__date__ = 'February 2012'
__credits__ = ["Meenakshy Vasudevan (Noblis)","Karl Wunderlich (Noblis)"]
__license__ = "GPL"
__version__ = "1.0"
__maintainer__ = "Jim Larkin (Noblis)"
__email__ = "jlarkin@noblis.org"
__status__ = "Prototype"

from Files import ControlFile, ConditionFile, Free_Flow_File
from  sqlload import Sqllite_DB
from timeslice import Timeslice


def run_stats(db):
    """
    Core procedure of the DMA Performance Measurement application that runs 
    through all of the statistics for generating the performance measures and 
    loads them into the SQLlite database.

    @param db - Sqllite_DB object that controls all calls to the the database.
    @return None
    """

    print "Calculating Performance Measures"


    #create and populate condition table
    db.create_condition_results_table()

    #get list count of each area
    conditions = db.list_conditions()
    ods = db.list_ods()

    timeslices = db.list_timeperiod()
    modes = db.list_mode()
    print "Conditions {} ODs {} Timeperiods {} Modes {}".format(len(conditions),
          len(ods), len(timeslices), len(modes))

    db.create_weighted_table()

    #Add Median, reliable trips by Mode and time period for all conditions, 
    #origins, destinations and time period
    for condition in conditions:
        ods = db.list_ods(condition)
        print "ODs {} for condition {}".format(len(ods), str(condition))
        for O, D in ods:
            for ts in timeslices:
                for mode in modes:
                    #Add median travel time and reliable trips to database by mode
                    db.update_condition_results(condition, O, D, ts, mode)
                    if condition == conditions[-1]:
                        db.create_weighted_results( O, D, ts, mode)

                #Add median travel time and reliable trips to database by time period
                db.update_condition_results(condition, O, D, ts)
                if condition == conditions[-1]:
                    db.create_weighted_results(O, D, ts)



    #Add Planning index for weighted results table
    for O, D in ods:
         for ts in timeslices:
             for mode in modes:
                 db.add_planning_index_weighted_results(O,D,ts,mode)
             db.commit()
             db.add_planning_index_weighted_results(O,D,ts)
         db.commit()


    db.commit()

    #create and populates the system table
    db.create_system_table()



def main():
    """
    Main procedure of the DMA Performance Measurement application  that reads 
    in all input files, creates the database and runs the run_stats procedure.  
    Note this procedure uses Python's argparse library which is only available 
    in Python 2.7 or higher.

    @param None
    @return None
    """

    import argparse

    parser = argparse.ArgumentParser(description="""
        'DMA Performance Measurement application was designed by Noblis. """)

    parser.add_argument('-file', help="""control file for program. if not given 
                        then file defaults to master.in""", default='master.in')

    args = parser.parse_args().__dict__

    #read control file
    if args['file'] =='master.in':
        print "No control file given using default control file: master.in"
    cf = ControlFile(args['file'])
    cf.validate()

    #create Time_slice Object
    ts = Timeslice(cf.start_time, cf.end_time, cf.time_period_length)
    files = cf.conditions_data

    #Create Database Object.
    db = Sqllite_DB()
    #Create Database
    db.create_db(cf.database_file)

    #Create Database trip table
    db.create_trips_table()
    #Create Database condition table
    db.create_condition_table(files)

    #create Free Flow File
    free_flow_file = Free_Flow_File(cf.free_flow_file)
    free_flow_file.load_file()
    #create and load Free_flow Table
    db.create_free_flow_table(free_flow_file)


    #load each condition file into trip table
    for id, f, pro, file_type in files:
        condition_file =  ConditionFile(f, id, db, ts, file_type)
        condition_file.load_file()

    #Run Stats on Data
    run_stats(db)

    print "Program Complete"


#Runs main procedure when the file is ran
if __name__ == "__main__":
       main()

