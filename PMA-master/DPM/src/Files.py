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

from sys import exit

import unittest
from string import split


class ConditionFile():
    """
    Class that is used for managing a individual ICPM condition file
    information.  It can read in both trajectory and trip based condition files
    """

    def __init__(self, filename, condition_id, db, time_slice, trip_data=0):
        """
        Sets the baseline paramaters for the Condition file

        @param filename - string value for the filename with or without path information
        @param condition_id - integer value with the condition_id assigned by the program
        @param db - Sqllite_DB object used to load the data into the database
        @param time_slice - time_slice object used to determine the time slice for a given start time
        @param trip_data - trip_data integer value read from the control file stating if the file is trip data or
                           trajectory data.  0=trip data and 1=trajectory data

        @return Nothing
        """
        self.filename = filename
        self.db = db
        self.condition_id = condition_id
        self.time_slice = time_slice
        self.trip_data = trip_data

    def line_trajectory_validation(self, line):
        """
        Checks a single line of trajectory data from condition file for errors

        @param line - string value of a given line of the condition file

        @return msg - string value with any error messages
        """

        msg = ''

        for data in line:
            #check to make sure they are all numbers
            if not data.replace(".","").isdigit():
                msg = "Error: one of the data values is not a number\n"

        return msg

    def line_trip_validation(self, line):
        """
        Checks a single line of trip data from condition file for errors

        @param line - list of string value of a given line of the condition file

        @return msg - string value with any error messages
        """

        msg = ''

        for data in line:
            #check to make sure they are all numbers
            if not data.replace(".","").isdigit():
                msg = "Error: one of the data values is not a number\n"


        return msg


    def load_file(self):
        """
        Main class procedure that loads the whole condition file into the database. 

        @param None

        @return None
        """

        from csv import reader

        try:
            f = open(self.filename)
            csvreader = reader(f)
        except:
            print "Can not read/load file: {}".format(self.filename)
            exit(0)


        print "Reading Conditional File: {}".format(self.filename)


        #read header line
        row = csvreader.next()

        #if trip data in file
        if int(self.trip_data)==1:

            #read each line
            for row in csvreader:

                errormsg = self.line_trip_validation(row)
                if errormsg =='':
                     #read line of data
                     trip_ID, O, D, trip_start, trip_distance, trip_TT, mode, passengers  = \
                        row[0], row[1], row[2], row[3], row[4], row[5], row[6], row[7]
                else:
                    print "{} Row: {}".format(errormsg,csvreader.line_num)
                    exit(0)

                #Determine time period and add to database
                if self.time_slice.bin_num(trip_start)!=None:
                    self.db.add_trip(self.condition_id, trip_ID, O, D, mode,  self.time_slice.bin_num(trip_start),
                                                 trip_start, trip_distance, trip_TT, passengers)
            #submit data to database
            self.db.commit()

        #if trajectory data
        else:
            #set starting stats holders
            curID = ''
            curlink = ''
            time_list = []
            distance_list = []

            at_start = True
            trip_distance = 0

            for row in csvreader:
                #read line of data
                errormsg = self.line_trajectory_validation(row)
                if errormsg =='':
                    vid, O, D, time, link, spd, acc, mode, passengers, distance = \
                    row[0], row[1], row[2], row[3], row[4], row[5], row[6], row[7], row[8], row[9]
                else:
                    print errormsg
                    print csvreader.line_num
                    print row
                    exit(0)

                #changed Vehicle ID
                if curID != vid and not at_start:

                            if len(distance_list) > 1:
                                trip_distance = distance_list[-1] - distance_list[0]
                            else:
                                trip_distance = distance_list[0]

                            #Add Trip
                            trip_start = time_list[0]
                            trip_TT = time_list[-1] - time_list[0]

                            if self.time_slice.bin_num(trip_start)!=None:
                                self.db.add_trip(self.condition_id, lr_vid, lr_O, lr_D, lr_mode,
                                                 self.time_slice.bin_num(trip_start),
                                                 trip_start, trip_distance, trip_TT, lr_passengers)


                            #reset lists for next vehicle
                            time_list = [float(time)]
                            link_dis_list = [float(distance)]
                            link_time_link = [float(time)]
                            link_spds_list = [float(spd)]
                            curID = vid
                            curlink = link
                            trip_distance = 0
                            lr_vid, lr_O, lr_D, lr_time, lr_link, lr_spd, lr_acc, lr_mode, lr_passengers, lr_distance = \
                            row[0], row[1], row[2], row[3], row[4], row[5], row[6], row[7], row[8], row[9]

                else:
                    if at_start:
                        at_start = False
                        curID = vid
                        curlink = link
                    time_list.append(float(time))
                    distance_list.append(float(distance))



                    lr_vid, lr_O, lr_D, lr_time, lr_link, lr_spd, lr_acc, lr_mode, lr_passengers, lr_distance = \
                    row[0], row[1], row[2], row[3], row[4], row[5], row[6], row[7], row[8], row[9]

                if csvreader.line_num % 10000 == 0:
                    pass
                    #print "Completed {} Lines".format(reader.line_num)



            #finalize last trip information
            if len(distance_list) > 1:
                trip_distance = distance_list[-1] - distance_list[0]
            else:
                trip_distance = distance_list[0]

            #Add Trip
            trip_start = time_list[0]
            trip_TT = time_list[-1] - time_list[0]

            if self.time_slice.bin_num(trip_start)!=None:
                self.db.add_trip(self.condition_id, lr_vid, lr_O, lr_D, lr_mode,
                                 self.time_slice.bin_num(trip_start),
                                 trip_start, trip_distance, trip_TT, lr_passengers)

            self.db.commit()



class ControlFile():
    """
    Class that is used for loading in the control file for the ICPM Program
    """

    def __init__(self, filename):
        """
        Creates local class variables and sets filename value.  Also starts class Load_file method

        @param filename - string value for the filename with or without path information

        @return None
        """
        self.filename = filename
        self.conditions_data = [] #array of all condition data objects
        self.load_file()

    def print_values(self):
        """
        Debugging procedure that prints to screen all of the values read in from the control file

        @param None

        @return None
        """

        for att in self.__dict__:
           print  '{}: {}'.format(att, self.__dict__[att])


    def load_file(self):
        """
        Method that reads in the control file and puts all values into local class variables

        @param None

        @return None
        """

        print "Reading Control File: {}".format(self.filename)
        try:
            infile = open(self.filename)
        except:
            print "Error cannot read file {}".format(self.filename)
            exit(0)

        self.start_time, self.end_time = infile.readline().split()
        self.time_period_length = infile.readline().strip()
        self.percentage = float(infile.readline().strip())
        self.database_file= infile.readline().strip()
        self.free_flow_file = infile.readline().strip()

        #Load Condition files
        line = infile.readline().strip()
        cond_id=0
        #Continue until there are no more files
        while True:
            filename, percentage, file_type = line.split()
            self.conditions_data.append((cond_id, filename, percentage, file_type))
            cond_id +=1
            line = infile.readline().strip()
            if not line:
                break





    def validate(self):
       """
       Does basic validation on the control file adding .s3db ending for the database file and
       making sure all condition file probability sum to 1.0

        @param None

        @return None
       """

       if self.database_file.find('.')!=-1:
           self.database_file = split(self.database_file,'.')[0] + ".s3db"
           print "Output file must be sqlite format changing name to {}".format( self.database_file)
       else:
           self.database_file = self.database_file + ".s3db"

       sum_value = sum([float(x[2]) for x in self.conditions_data])
       if sum_value == 1.0:
           return 1
       else:
           print self.conditions_data
           print "Error the sum of the conditions {} do not sum to 1.0".format(sum_value)
           return 0




class Free_Flow_File():
    """
    Class that is used to load the free flow file into the ICPM Program
    """


    def __init__(self, filename):
        """
        Creates local class variables and sets filename value.

        @param filename - string value for the filename with or without path information

        @return None
        """
        self.filename = filename
        self.free_flow_list = {}

    def line_validation(self, line):
        """
        Checks a single line of free flow data from free flow file to see if all of the values are numbers

        @param line - string value of a given line of the condition file

        @return msg - string value with any error messages
        """

        msg = ''

        for data in line.split(','):
            #check to make sure they are all numbers
            if not data.replace(".","").isdigit():
                msg = "Error: one of the data values is not a number\n"


        return msg


    def load_file(self):
        """
        Method that reads in the free flow file and puts all values into local class variables

        @param None

        @return None
        """

        print "Reading Free flow File: {}".format(self.filename)
        try:
            infile = open(self.filename)
        except:
            print "Error cannot read file {}".format(self.filename)
            exit(0)


        #Read Header
        line = infile.readline().strip()

        rownum = 1
        
        #Read Data
        while True:
           line = infile.readline().strip()
           rownum = rownum+1
           if not line:
                break

           errormsg = self.line_validation(line)
           if errormsg =='':
               origin, destination, free_flow_time = line.split(',')
               self.free_flow_list[(origin,destination)] = free_flow_time
           else:
               print "{} File: {} Row: {}".format(errormsg,self.filename, rownum)
               print "Line: {}".format(line)
               exit(0)



