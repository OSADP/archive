'''
Created on Sep 9, 2014

@author: jbarrios
This script analyzes the vehicle queue time from VISSIM
'''
from __future__ import print_function

import csv
import sys
import easygui as eg
import math

programTitle = "Queue Time Processor v0.1"
selected_csv = 'QueueTime.csv'
selected_csv = eg.fileopenbox("Choose CSV", programTitle, "*", "*.csv")
#filterColumn = 6
startTime = 3600
endTime = 25200
reportInterval = 300
#numRows = (endTime - (startTime + reportInterval)) / reportInterval
numRows = (endTime - startTime) / reportInterval
vissimInterval = 20
numSims = 10
outputMatrix = [[((startTime + (reportInterval * i)) + (j * reportInterval)) for j in [0,1]] 
                for i in range(numRows+1)]
for k in xrange(0,len(outputMatrix)):
    for z in range(numSims): outputMatrix[k].append(0)#Filling the matrix with zeros in case input is not sorted
print(outputMatrix)

qTmThusFar = [] #This 3D matrix will hold the iteration, the vehicle ID and the latest queue time for that vehicle
for z in range(numSims):
    qTmThusFar.append([])
#Assuming that the following columns are part of the CSV, in this order:
#Iteration, VehNr, t, and QTm.
#Link numbers are not included as they were filtered in Access

with open(selected_csv,'rb') as ifile:
    reader = csv.reader(ifile)
    header = reader.next()    
    output_csv = eg.filesavebox("Select name to save file with: ", programTitle, "output.csv", "*.csv")
    #output_csv = 'output.csv'
    #saveColumns = sorted(stationingColumns + latLongColumns)
    rownum = 0
    for row in reader:
        print("Reading line number: " + str(rownum))
        rownum = rownum + 1 #go onto the next vehicle record
        if (float(row[3])<=0.0 and (int(float(row[2])) % reportInterval) != 0):
            pass #skipping non report intervals, but better done in Access
        else:
            if int(float(row[2])) <= startTime + reportInterval: 
                intervalNum = 0 #For the first time slice
            else:
                intervalNum = int(math.floor(((int(float(row[2])) - startTime) / reportInterval))) - 1 #-1 only when we only have end times
            if intervalNum > len(outputMatrix)-1: 
                intervalNum = len(outputMatrix) - 1 #For the last time slice
            
            iter = int(float(row[0])) #iteration number
            vehID = row[1] #vehicle ID
            cmlDelay = float(row[3]) #cumulative delay
            index = 0
            for v in xrange(0, len(qTmThusFar[iter-1])):
                if qTmThusFar[iter-1][v][0] == vehID: index = v
                v = v + 1
            #if vehID in qTmThusFar[iter-1][:][0]:
            if index <> 0:
                #index = qTmThusFar[iter-1][:][0].index(vehID)
                outputMatrix[intervalNum][iter + 1] = outputMatrix[intervalNum][iter + 1] + (cmlDelay - qTmThusFar[iter-1][index][1]) #Add the increment to the tally
                qTmThusFar[iter-1][index][1] = cmlDelay #update the entry for this vehID for future use
            else: 
                outputMatrix[intervalNum][iter + 1] = outputMatrix[intervalNum][iter + 1] + cmlDelay #First time this vehicle shows up, and it has delay
                qTmThusFar[iter-1].append([vehID, cmlDelay]) #Add the vehicle to the list for future use
    try:
        with open(output_csv, 'wb') as ofile:
            csv_writer = csv.writer(ofile)
            for w in xrange(0, len(outputMatrix)-1):
                csv_writer.writerow(outputMatrix[w])

    except IOError:
        eg.msgbox("Error. Ensure output file is not currently in use.", programTitle)
            #print(written)
eg.msgbox("Done", programTitle)