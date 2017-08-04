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

import sys
import sqlite3 as sql


def median(nums):
  """
  Returns the median value from list of numbers

  @param nums - list of numerical values
  @return float value of the median value of the list
  """

  if len(nums)>0:
      theValues = sorted(nums)
      if len(theValues) % 2 == 1:
        return theValues[(len(theValues)+1)/2-1]
      else:
        lower = theValues[len(theValues)/2-1]
        upper = theValues[len(theValues)/2]
        return (float(lower + upper)) / 2
  else:
      return 0


def percentile_nonweighted(L, p):
    """
    Returns the percentile value from a list of non-weighted values

    @param L - list of numerical values
    @param p - float of the probability value
    @return float value of the percentile value of the list
    """
    if len(L)>0:
        return L[int(len(L) * p)]
    else:
        return 0



def percentile_weighted(L, p):
    """
    Returns the weighted percentile value from a list of values and their probabilities

    @param L - list of numerical values and their probabilities
    @param p - float of the probability value
    @return float value of the percentile value of the list
    """
    sum_probability=0
    if len(L)>0:
        for value, probability in L:
            if (sum_probability + probability)>=p:
                return value
            sum_probability += probability
        return value
    else:
        return 0





class Sqllite_DB:
    """
    Class that controls all SQLlite database calls and procedures
    """

    def create_db(self, dbname=None):
        """
        Creates database connection either in memory or to a file.  If no file name is given then database is
        placed in memory.  Note if the memory option is used once the ICPM ends all information in the database is
        deleted.

        @param dbname - optional string field that is used for the path and name of the database file

        @return None
        """

        import os

        if dbname == None:
            try:
                self.conn = sql.connect(':memory:')
                self.cur = self.conn.cursor()
                self.cur.arraysize = 1000000
            except:
                print "Error: could not create database in memory"
        else:
            if os.path.isfile(dbname):
                os.remove(dbname)
            try:
                self.conn = sql.connect(dbname)
                self.cur = self.conn.cursor()
                self.cur.arraysize = 1000000
            except:
                print "Error: could not create database file {}".format(dbname)
                exit(0)

    def open_db(self, dbname):
        """
        Debugging procedure to open a SQLlite database that is already made

        @param dbname - string field that is used for the path and name of the database file

        @return None
        """

        try:
                self.conn = sql.connect(dbname)
                self.cur = self.conn.cursor()
                self.cur.arraysize = 1000000
                print "Opened %s " % dbname
        except:
                print "Error: could not open database file {}".format(dbname)


    def commit(self):
        """
        Commit all current SQL statements to the database

        @param None

        @return None
        """
        self.conn.commit()

    def close(self):
        """
        Closes database connection

        @param None

        @return None
        """
        self.conn.close()

    def table_list(self):
        """
        Debugging method that lists all current tables in database

        @param None

        @return None
        """

        self.cur.execute("SELECT name FROM sqlite_master WHERE type='table'")
        rows = self.cur.fetchmany()

        for row in rows:
            print row[0]




    def create_trips_table(self):
        """
        Creates a blank trips table in the database that is used to store all input trip information coming in from
        the conditional input files

        @param None

        @return None
        """

        self.cur.execute("""
                        CREATE TABLE [trips] (
                        [Condition_ID] INTEGER NOT NULL,
                        [Trip_ID] INTEGER  NOT NULL,
                        [Origin] INTEGER NOT NULL,
                        [Destination] INTEGER NOT NULL,
                        [Mode] INTEGER NOT NULL,
                        [Time_period] INTEGER NOT NULL,
                        [Travel_time] FLOAT  NOT NULL,
                        [Start_Time] FLOAT  NOT NULL,
                        [Distance] FLOAT  NOT NULL,
                        [Passengers] FLOAT  NOT NULL
                         )""")

        self.commit()


    def create_free_flow_table(self, free_flow_file):
        """
        Creates and loads the free flow travel time from the freeflow file into free_flow table in the database
        that is used to store all free flow input information

        @param free_flow_file - dictionary of all the free flow travel times using Origin, Destination key

        @return None
        """

        self.cur.execute("""
                        CREATE TABLE [free_flow] (
                        [Origin] INTEGER NOT NULL,
                        [Destination] INTEGER NOT NULL,
                        [Free_flow_travel_time] FLOAT  NOT NULL
                         )""")

        self.commit()

        #load the data
        for O, D in free_flow_file.free_flow_list:
            sql = """INSERT INTO [free_flow] VALUES(%d,%d,%f)""" % \
                  (int(O), int(D), float(free_flow_file.free_flow_list[O,D]))

            self.cur.execute(sql)

        self.commit()



    def create_condition_table(self, files):
        """
        Creates and loads the condition file metadata from the a control file into conditions table in the database
        that is used to store all condition metadata input information.  This table in includes the Condition_ID,
        Condition_name (the name of the Condition file) and the probability of the condition.

        @param None

        @return None
        """

        self.cur.execute("""
                        CREATE TABLE conditions (
                        [Condition_ID] INTEGER NOT NULL,
                        [Condition_name] TEXT NOT NULL,
                        [probability] FLOAT  NOT NULL)
                        """)

        #Populates the table with ID, name, and probability
        for ID, name, probability, file_type in files:
            self.cur.execute("""insert into conditions values ({},'{}',{})""".format(ID, str(name), float(probability)))

        self.commit()

    def list_conditions(self):
        """
        Returns all Condition IDs in a List format

        @param None

        @return None
        """
        self.cur.execute('Select distinct Condition_ID from trips')
        return [item for sublist in self.cur.fetchmany() for item in sublist]

    def list_ods(self, condition=-1):
        """
        Returns all Origin, Destination Pairs in order

        @param Optional condition integer value used if you want to pull the origin, destinations by only one condition

        @return None
        """

        if condition == -1:
            self.cur.execute('Select distinct Origin, Destination from trips order by Origin, Destination')
        else:
            sql = 'Select distinct Origin, Destination from trips where Condition_ID = %s order by Origin,' \
                  ' Destination' % (condition)
            self.cur.execute(sql)
        return self.cur.fetchmany()

    def list_timeperiod(self):
        """
        Returns all Time_Periods in order in a list format

        @param None

        @return None
        """
        self.cur.execute('Select distinct Time_Period from trips order by Time_Period')
        return [item for sublist in self.cur.fetchmany() for item in sublist]

    def list_mode(self):
        """
        Returns all Mode value in a list format

        @param None

        @return None
        """
        self.cur.execute('Select distinct Mode from trips')
        return [item for sublist in self.cur.fetchmany() for item in sublist]



    def create_condition_results_table(self):
        """
        Creates the condition_results table and populates it with data based on SQL statements.  The
        condition_results table is the aggregation of trip information and condition information taken from
        he trips and conditions tables.

        @param None

        @return None
        """

        #check to make sure table is not already created
        self.cur.execute("SELECT name FROM sqlite_master WHERE type='table' AND name='condition_results'")
        rows = self.cur.fetchmany()

        #If not created
        if len(rows)==0:

            #Create base table for condition_results
            #equations 1 and 7,
            self.cur.execute("""CREATE TABLE condition_results as
                                select t.Condition_ID as 'Condition_ID',
                                c.probability as 'Probability',
                                t.Origin as 'Origin',
                                t.Destination as 'Destination',
                                t.Time_period as 'Time_period',
                                t.Mode as 'Mode',
                                count(*) as 'Trip_count' ,
                                avg(t.travel_time) as 'Avg_TT',
                                max((avg(t.Travel_time) - d.Free_flow_travel_time),0) as 'Delay',
                                d.Free_flow_travel_time as 'Free_flow',
                                sum(t.Distance * t.Passengers)/count(*) as 'PMT',
                                null as 'PTD',
                                null as 'PMD',
                                null as 'Median_TT',
                                null as 'Reliable_trips',
                                null as 'Reliable_passenger_trips',
                                null as 'Reliable_TT_Threshold'
                                from trips as t, conditions as c,
                                (select Origin, Destination, Free_flow_travel_time from free_flow) as d
                                where t.Origin =  d.Origin
                                and t.Destination = d.Destination
                                and t.Condition_ID= c.Condition_ID
                                group by t.Condition_ID, t.Origin, t.Destination, t.Time_period, t.Mode
                                order by t.Condition_ID, t.Origin, t.Destination, t.Time_period, t.Mode""")
            self.commit()


            #By Condition ,Origin,Destination ,time period
            self.cur.execute("""insert into condition_results
                                select t.Condition_ID as 'Condition_ID',
                                c.probability as 'Probability',
                                t.Origin as 'Origin',
                                t.Destination as 'Destination',
                                t.Time_period as 'Time_period',
                                null as 'Mode',
                                count(*) as 'Trip_count' ,
                                avg(t.travel_time) as 'Avg_TT',
                                max((avg(t.Travel_time) - d.Free_flow_travel_time),0) as 'Delay',
                                d.Free_flow_travel_time as 'Free_flow',
                                sum(t.Distance * t.Passengers)/count(*) as 'PMT',
                                null as 'PTD',
                                null as 'PMD',
                                null as 'Median_TT',
                                null as 'Reliable_trips',
                                null as 'Reliable_passenger_trips',
                                null as 'Reliable_TT_Threshold'
                                from trips as t, conditions as c,
                                (select Origin, Destination, Free_flow_travel_time from free_flow) as d
                                where t.Origin =  d.Origin
                                and t.Destination = d.Destination
                                and t.Condition_ID= c.Condition_ID
                                group by t.Condition_ID, t.Origin, t.Destination, t.Time_period
                                order by t.Condition_ID, t.Origin, t.Destination, t.Time_period""")


            self.commit()


    
    def update_condition_results(self, cond, O, D, tp, mode=''):
        """
        Completes the loading of data into the condition_results table by calculating the median, Reliable_time,
        Reliable_trips, Reliable_passenger_trips from data from the trips table and enters it into the
        condition_results table in the database.

        @param cond integer value of the condition id for the given condition
        @param O integer value for the Origin that values are being calculated for
        @param D integer value for the Destination that values are being calculated for
        @param tp integer value of the time period to add information for
        @param mode optional string value that is used if a mode values are being calculated

        @return None
        """

        #If no mode value given
        if mode=='':
            #set where statement
            sql_where = """where Condition_ID={} and Origin = {} and Destination = {} and Time_period =
                        {}""".format(cond,O,D,tp)
            #set where statement for update statement
            sql_where_update = """where Condition_ID={} and Origin = {} and Destination = {} and Time_period = {}
                                  and mode is null""".format(cond,O,D,tp)
        else:
            #set where statement
            sql_where = """where Condition_ID={} and Origin = {} and Destination = {} and Time_period = {} and mode={}
                         """.format(cond,O,D,tp,mode)
            #set where statement is the same for the update if mode is given
            sql_where_update = sql_where



        #select all the travel time for the condition, origin, destination, and time period and order by travel time
        sql_str = """select Travel_Time from trips {} Order by Travel_Time""".format(sql_where)
        self.cur.execute(sql_str)

        tt_list = self.cur.fetchall()


        # if there is data
        if len(tt_list) > 0:
                #calculate the median travel time
                median_tt = median( [item for sublist in tt_list for item in sublist])
                #calculate the 95% travel time for the list
                reliable_time = percentile_nonweighted( [item for sublist in tt_list for item in sublist], 0.95)

                #calculate the trip count, passenger sum, and passenger * distance summ for all trips that are
                #less then or equal to the reliable travel time
                self.cur.execute("""Select count(*), sum(Passengers),  sum(Passengers * Distance)  from trips
                                {} and travel_time<= {}""".format(sql_where,reliable_time))

                data_rows = self.cur.fetchall()[0]

                #If there is data
                if data_rows:
                    #set data into variables
                    reliable_trips = data_rows[0]
                    reliable_passenger_trips = data_rows[1]
                    reliable_passenger_distance = data_rows[2]

                #find the total number of trips
                self.cur.execute("""select count(*) from trips
                                  {}
                                 """.format(sql_where))

                #store the total number trips
                all_trips = self.cur.fetchone()[0]

                #calculate the passenger trips delivered
                PTD = reliable_passenger_trips / all_trips

                #calculate the passenger distance  delivered
                PMD = reliable_passenger_distance / all_trips

                sql_str = """UPDATE condition_results set Reliable_trips = {}, Reliable_TT_Threshold = {},
                             Median_TT = {}, Reliable_passenger_trips={}, PMD = {}, PTD={} {}
                                """.format(reliable_trips, reliable_time, median_tt, reliable_passenger_trips,
                                            PMD, PTD, sql_where_update)


                #add the values into the database
                self.cur.execute(sql_str)

                self.commit()

    def create_weighted_table(self):
        """
        Creates a blank weighted table in the database to be used to store all the weighted results across all
        conditional files.

        @param None

        @returns None
        """


        #Determine if the table is already created.  Debugging line of code
        self.cur.execute("SELECT name FROM sqlite_master WHERE type='table' AND name='weighted_results'")
        rows = self.cur.fetchmany()

        sql_str = ''

        if not rows:

            sql_str = """
                CREATE TABLE weighted_results(
                  Origin INT,
                  Destination INT,
                  Time_period INT,
                  Mode INT,
                  Avg_trip_count,
                  Avg_TT,
                  Avg_delay,
                  Avg_PMT,
                  Avg_Median_TT,
                  Avg_PTD,
                  Avg_PMD,
                  Avg_Reliable_trips,
                  Avg_Reliable_passenger_trips,
                  Planning_index
                )
            """

            self.cur.execute(sql_str)



    def create_weighted_results(self, O, D, tp, mode=''):
        """
        Calculates the weighted results for each origin, destination, time period, and mode pairing from each
        of the individual conditions and their probabilities and stores the data is the weighted_results table
        in database

        @param O integer value for the Origin that values are being calculated for
        @param D integer value for the Destination that values are being calculated for
        @param tp integer value of the time period to add information for
        @param mode optional string value that is used if a mode values are being calculated

        @returns None
        """
        
        #If no mode value given
        if mode=='':
            #set where statement
            sql_where = """where Origin = {} and Destination = {} and Time_period = {} and mode is null
                         """.format(O,D,tp)
        else:
            #set where statement
            sql_where = """where Origin = {} and Destination = {} and Time_period = {} and mode={}
                         """.format(O,D,tp,mode)


        sql_str = """
             select
             mode,
             Probability,
            (Trip_count * Probability) as 'Avg_trip_count',
            (Avg_TT * Probability)  as 'Avg_TT',
            (delay * Probability)  as 'Avg_delay',
            (PMT * Probability) as 'Avg_PMT',
            (Median_TT * Probability) as 'Avg_Median_TT',
            (PTD * Probability) as 'Avg_PTD',
            (PMD * Probability)  as 'Avg_PMD',
            (Reliable_trips * Probability)  as 'Avg_Reliable_trips',
            (Reliable_passenger_trips * Probability)  as 'Avg_Reliable_passenger_trips'
            from condition_results
            %s
            """ % (sql_where)

        self.cur.execute(sql_str)

        rows = self.cur.fetchmany()

        if rows:

            #sum value to 0 to start
            sum = [0 for x in range(0,10)]

            #sum columns
            for row in rows:

                for col in range(1,11):
                    sum[col-1] = sum[col-1] + row[col]

            #divide sum by total probability
            sum = [float(x/sum[0]) for x in sum]

            if mode == '':
                sql_insert = """
                INSERT INTO weighted_results VALUES(%s,%s,%s,%s,%s,%s,%s,%s,%s,%s,%s,%s,%s,%s)
                """ % (O,D,tp,'null',sum[1],sum[2],sum[3],sum[4],sum[5],sum[6],sum[7],sum[8],sum[9],'null')
            else:
                sql_insert = """
                INSERT INTO weighted_results VALUES(%s,%s,%s,%s,%s,%s,%s,%s,%s,%s,%s,%s,%s,%s)
                """ % (O,D,tp,mode,sum[1],sum[2],sum[3],sum[4],sum[5],sum[6],sum[7],sum[8],sum[9],'null')


            #if 1==1:
            try:
                #add the values into the database
                self.cur.execute(sql_insert)

                self.commit()

            except :
                print "ERROR"
                print sql_str
                print sql_insert
                sys.exit(1)



    def add_planning_index_weighted_results(self, O, D, tp, mode=''):
        """
        Calculates the planning index for each origin, destination, time period, and mode
        and adds it to the weighted_results database table

        @param O integer value for the Origin that values are being calculated for
        @param D integer value for the Destination that values are being calculated for
        @param tp integer value of the time period to add information for
        @param mode optional string value that is used if a mode values are being calculated

        @returns None
        """

        if mode=='':
            sql_where = """where Origin = {} and Destination = {} and Time_period = {}
                         """.format(O,D,tp)
            sql_where_update = """where Origin = {} and Destination = {} and Time_period = {}
                          and mode is null""".format(O,D,tp)
        else:
            sql_where = """where  Origin = {} and Destination = {} and Time_period = {} and mode={}
                         """.format(O,D,tp,mode)
            sql_where_update = sql_where

        #Find Planning index (Equation 11)
        self.cur.execute("""select avg(a.Travel_time), b.Probability
                            from trips as a, conditions as b
                             {} and a.Condition_ID = b.Condition_ID
                             group by a.Condition_ID
                             order by avg(a.Travel_time)""".format(sql_where))


        TT_list = self.cur.fetchall()
        if TT_list:
            #find the reliable time based on weighted probabilities
            reliable_time = percentile_weighted(TT_list,0.95)

            #get Free_flow value
            self.cur.execute("""select Free_flow from condition_results
                                {}""".format(sql_where))

            freeflow_speed = self.cur.fetchone()[0]

            #equation #12
            planning_index = reliable_time/freeflow_speed

            #Add planning index to database
            self.cur.execute("""UPDATE weighted_results set  Planning_index = {}
                                {}
                                """.format(planning_index,sql_where_update))
            self.commit()



    def create_system_table(self):
        """
        Creates and populates the system_results table by aggregate information in the weighted_results table across
        all condition, origins, destinations, time periods, and modes.  It also prints out the overall system results
        to the screen.

        @param None

        @returns None
        """

        #check to make sure table is not already created
        self.cur.execute("SELECT name FROM sqlite_master WHERE type='table' AND name='system_results'")
        rows = self.cur.fetchmany()

        if not rows:

            #Create the Table
            self.cur.execute("""
                CREATE TABLE  system_results(
                [System_Avg_TT],
                [System_Avg_delay],
                [System_Total_delay],
                [System_Planning],
                [System_PTD],
                [System_PDT],
                [System_PDD]
            )
            """)
            self.commit()

        #equation 5, 9,  10, 17, 20, 23
        self.cur.execute("""select
                    (sum(avg_TT * avg_trip_count) / sum(avg_trip_count))  as 'TT',
                    (sum(avg_delay * avg_trip_count) / sum(avg_trip_count))  as 'AVG_Delay',
                     sum(avg_delay * avg_trip_count) as 'System_Delay',
                    (sum(Planning_index * avg_trip_count) / sum(avg_trip_count))  as 'Planning Index',
                    sum(avg_PMT * avg_trip_count) as 'PMT',
                    sum(avg_PTD * avg_trip_count) as 'PTD',
                    sum(avg_PMD * avg_trip_count) as 'PMD'
                    from weighted_results
                    where mode is null""")
        rows =  self.cur.fetchmany()[0]

        #print rows
        if rows:
            TT_avg = rows[0]
            system_delay = rows[1]
            total_delay = rows[2]
            system_planning = rows[3]
            system_PMT = rows[4]
            system_PTD = rows[5]
            system_PMD = rows[6]


            self.cur.execute("""INSERT INTO system_results Values({},{},{},{},{},{},{})
                            """.format(TT_avg, system_delay, total_delay, system_planning,
                                       system_PMT, system_PTD, system_PMD))

            self.commit()

            print "System Wide Average Travel Time = {}".format(TT_avg)
            print "System Wide Average Delay = {}".format(system_delay)
            print "System Wide Total Delay = {}".format(total_delay)
            print "System Wide Planning Index = {}".format(system_planning)
            print "System Wide Passenger Miles Traveled = {}".format(system_PDT)
            print "System Wide Passenger Travel Delivered = {}".format(system_PTD)
            print "System Wide Passenger Miles Delivered = {}".format(system_PDD)



    def add_trip(self, condition_ID, trip_ID, O, D, mode, time_period, start_time, distance, travel_time, passengers):
        """
        Adds a new single trip information to the database table trip

        @param O integer value for the Origin that values are being calculated for
        @param D integer value for the Destination that values are being calculated for
        @param tp integer value of the time period to add information for
        @param mode optional string value that is used if a mode values are being calculated

        @returns None
        """
        self.cur.execute("""insert into trips values ({},{},{},{},{},{},{},{},{},{})"""
                    .format(condition_ID, trip_ID, O, D, mode, time_period, travel_time, start_time, distance, passengers))





