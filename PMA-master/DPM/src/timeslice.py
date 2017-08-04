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

class Timeslice:
    """
    Class that is used for managing and determining the individual time slices 
    based on the trip starting  time.
    """

    def __init__(self, start, end, interval ):
        """
        Creation method that sets the baseline parameters of starting time, 
        ending time, and interval length

        @param start - integer value for starting time of evaluation in seconds
        @param end - integer value for ending time of evaluation in seconds
        @param interval - integer value for the time interval for each time 
                          slice in seconds

        @return Nothing
        """

        self.interval = interval
        self.start = start
        self.end = end

        self.interval_start = start
        self.interval_end = int(start) + int(interval)

    def bin_num(self, start_time):
        """
        Determines which bin (time slice) a trip should be placed in based on 
        the trip start time and returns the bin number.

        @param start_time - integer/float/str value for the starting time of 
                            the trip.

        @return integer value for the bin number for the trip
        """

        #if in range
        if float(self.start) <= float(start_time) <= float(self.end):
            value = int(abs(float(self.start) - float(start_time))/
                        float(self.interval))
            return value


    def __iter__(self):
        """
        Method that allows the Time slices to be iterated over

        @param None

        @return self
        """

        return self

    def reset(self):
        """
        Returns the interval values to their starting values

        @param None

        @return None
        """
        self.interval_start = self.start
        self.interval_end = self.start + self.interval


    def next(self):
        """
        Method that runs when the class is used in a loop to pull the next value

        @param None

        @return None
        """
        if (self.interval_start + self.interval) >= self.end:
            raise StopIteration
        else:
            self.interval_start += self.interval
            self.interval_end += self.interval


    def __str__(self):
        """
        Method that returns a string representation of the time interval

        @param None

        @return string with the starting and ending time for the current 
                interval
        """
        return "{} - {}".format(self.interval_start, self.interval_end)


