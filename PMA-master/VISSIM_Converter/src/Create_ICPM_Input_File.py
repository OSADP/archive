from random import randint
from string import strip


"""
Reads in fzp file and connection and links file from VISSIM Network Reader program and
creates an input file for ICPM main program
"""



class Tripline:
    pass



def read_connector_file(connector_file):
    """Reads in VISSIM Connector File created by VIS_Network.Reader.py program
    """

    f = open(connector_file)

    connector_paths = []

    #read header
    line = f.readline()


    for line in f.readlines():
        connector_dir = {}
        connector_dir["connector"], connector_dir["alink"], connector_dir["blink"] = [int(x) for x in  line.strip().split(",")]
        connector_paths.append(connector_dir)



    f.close()
    return connector_paths


def time_covert(time):
    from datetime import timedelta

    start_time = timedelta(hours = 6, minutes = 30, seconds = 0)
    time_parts = time.split(":")
    end_time = timedelta(hours = int(time_parts[0]), minutes = int(time_parts[1]), seconds = int(float(time_parts[2])))
    diff = end_time - start_time

    return diff.total_seconds()



def read_fzp(filename, outfile, links, connectors):

    f = open(filename)
    print "Reading %s.." % (filename)

    #skip headers
    for i in range(17):
        line = f.readline()

    c=1
    line = f.readline()
    newlines = {}
    vech_time = {}

    while line:
        c +=1
        fields = [strip(x) for x in line.split(";")]

        #only take trips that start after 6:30 and before 7:00
        if float(fields[6]) >= 1800.0 and float(fields[6])<=3600.0:

            link = fields[2]

            if fields[1] == "7":
                mode = 2
                passengers = randint(1,4)
            elif fields[1] == "8":
                mode = 3
                passengers = randint(1,49)
            else:
                mode = 1
                passengers=1

            id = fields[0]
            time = time_covert(fields[5])
            spd = fields[3]
            distance = fields[4]

            #ID,O,D,Time,link,spd,acc,mode,passengers,distance
            lineval = "%s,%s,%s,%s,%s,%s,%s,%s,%s,%s" % (id,"O","D",time,link,spd,"3.0",mode,passengers,distance)

            #if a new id then add list
            if not newlines.has_key(id):
                    newlines[id]= []

            newlines[id].append(lineval)

        line = f.readline()

        if c%500000 == 0:
            print "completed %f" % (c/ 24170196.0 * 100)


    print "Output data..%s" % outfile

    fout = open(outfile, "w")
    fout.write("ID,O,D,Time,link,spd,acc,mode,passengers,distance\n")

    for ID in newlines:

        #get first link for first record for ID
        first_link = newlines[ID][0].split(",")[4]
        #origin = links[first_link]['anode']
        origin = first_link

        #get last link for last record for ID
        last_link = newlines[ID][-1].split(",")[4]

        #do not end on a connector
        if len(last_link) == 5:
            for connector in connectors:
                if connector['connector'] == last_link:
                    last_link = connector['alink']

        destination = last_link

        for lineval in newlines[ID]:
            line = lineval.replace('O',str(origin))
            line = line.replace('D',str(destination))
            fout.write(line + "\n")

    fout.close()


def Load_Link_File(Link_file):

    f= open(Link_file)

    #skip header
    line = f.readline()

    links = {}
    #Create internal Node numbers
    n = 1

    while line:
        fields = map(strip,line.split(",") )
        #fields = [strip(x) for x in line.split(",")]
        links[fields[0]] = {}
        links[fields[0]]["anode"] = fields[1]
        links[fields[0]]["bnode"] = fields[2]
        links[fields[0]]["length"] = fields[3]
        links[fields[0]]["lanes"] = fields[4]
        line = f.readline()


    return links








if __name__ == '__main__':

    lfile = r"C:\Code\workspace\python\ICPM\VISSIM_Network_Reader\data\links_update.csv"

    links = Load_Link_File(lfile)

    connector_file = r"C:\Code\workspace\python\ICPM\VISSIM_path\connectors.csv"
    connectors = read_connector_file(connector_file)

    file = r"C:\Users\Public\Documents\St_louis\StLouis_VISSIM\am_existing_df_casec_incident.fzp"
    outfile = r"C:\Users\Public\Documents\St_louis\2011_08_05_incident.csv"
    read_fzp(file, outfile, links,connectors)

    file = r"C:\Users\Public\Documents\St_louis\StLouis_VISSIM\AM_existing_DF_CaseC.fzp"
    outfile = r"C:\Users\Public\Documents\St_louis\2011_08_05_no_incident.csv"
    read_fzp(file,outfile, links,connectors)

    print "Program Complete"

  