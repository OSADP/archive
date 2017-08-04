from random import randint
from string import strip


"""
Reads in fzp file and connection and links file from VISSIM Network Reader program and
creates an input file for ICPM main program
"""



class Tripline:
    pass



def time_covert(time):
    from datetime import timedelta

    start_time = timedelta(hours = 6, minutes = 30, seconds = 0)
    time_parts = time.split(":")
    end_time = timedelta(hours = int(time_parts[0]), minutes = int(time_parts[1]), seconds = int(float(time_parts[2])))
    diff = end_time - start_time

    return diff.total_seconds()



def read_fzp(filename, outfile, links, C):

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

            if int(fields[2]) > 1000:
                link = C[fields[2]][0]
            else:
                link = fields[2]

            O = links[link][0][0]

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

            #ID,O,D,Time,link,spd,acc,mode,passengers,distance
            lineval = "%s,%s,%s,%s,%s,%s,%s,%s,%s,%s" % (id,O,"D",time,link,spd,"3.0",mode,passengers,'dis')

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

        #get last link for last record for ID
        last_link = newlines[ID][-1].split(",")[4]
        destination = links[last_link][1][0]

        for lineval in newlines[ID]:
            fout.write(lineval.replace('D',str(destination)) + "\n")

    fout.close()


def Load_Link_File(Link_file):

    f= open(Link_file)

    #skip header
    line = f.readline()
    line = f.readline()

    links = {}
    #Create internal Node numbers
    n = 1

    while line:
        fields = [strip(x) for x in line.split(",")]
        links[fields[0]] = [(n,fields[1], fields[2])]
        n +=1
        links[fields[0]].append((n,fields[3], fields[4]))
        n +=1
        links[fields[0]].append(fields[5])
        line = f.readline()


    return links

def Load_Connection_File(Connection_file):

    f= open(Connection_file)

    #skip header
    line = f.readline()
    line = f.readline()

    C = {}

    while line:
        fields = line.split(",")
        C[fields[0]] = [fields[1]]
        C[fields[0]].append(strip(fields[2]))
        line = f.readline()


    return C



def create_OD_file(links):
    import operator

    fout = open('Nodes_links.csv', "w")
    fout.write("Link,Node1,X,Y,Node2,X,Y\n")

    link_sorted = sorted(links.iteritems(), key = operator.itemgetter(1))


    for link in link_sorted:
        s = link[0] + ","
        s += "{},{},{},".format(link[1][0][0],link[1][0][1],link[1][0][2])
        s += "{},{},{}".format(link[1][1][0],link[1][1][1],link[1][1][2])
        fout.write(s + "\n")
    fout.close()



if __name__ == '__main__':

    lfile = r"C:\Code\workspace\python\ICPM\VISSIM_Network_Reader\data\links.csv"

    links = Load_Link_File(lfile)
    create_OD_file(links)

    cfile = r"C:\Code\workspace\python\ICPM\VISSIM_Network_Reader\data\connectors.csv"
    C = Load_Connection_File(cfile)

    file = r"C:\Users\m28050\Documents\Data\St_louis\StLouis_VISSIM\am_existing_df_casec_incident.fzp"
    outfile = r"C:\Users\m28050\Documents\Data\St_louis\incident_new.csv"
    read_fzp(file,outfile, links,C)

    file = r"C:\Users\m28050\Documents\Data\St_louis\StLouis_VISSIM\AM_existing_DF_CaseC.fzp"
    outfile = r"C:\Users\m28050\Documents\Data\St_louis\no_incident_new.csv"
    read_fzp(file,outfile, links,C)

    print "Program Complete"

  