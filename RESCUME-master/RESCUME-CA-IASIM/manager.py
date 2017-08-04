######################################################################################################################
# INC-ZONE Simulation Application
#
# What it does: The application simulates the INC-ZONE and RESP-STG DMA Application Functionalities in VISSIM 6 or 7
# using vehicle commands such as changing desired lanes and changing desired speed distribution of vehicles.
#
# The application is coded under USDOT Contract by Booz Allen Hamilton, Inc.
#
#
######################################################################################################################

######################################################################################################################
#Import Statements ~ DO NOT MODIFY
import win32com.client as com
import os
import time
from random import randrange


######################################################################################################################
#Functions are defined below ~ DO NOT MODIFY
def IncidentCreate():
	for i in range (0, Blocked_Lanes):
		Vissim.Net.Vehicles.AddVehicleAtLinkPosition(Crasher_type,Crash_Link, i+1, Crash_Loc, 0, False)
	print "\nincident initialized"
			
			
def IncidentRemove():
	all_veh_attributes = Vissim.Net.Vehicles.GetMultipleAttributes(('No','VehType'))
	for cnt in range(len(all_veh_attributes)):
		if all_veh_attributes[cnt][1] == Crasher_type:
			print "crash-vehicle detected"
			Vissim.Net.Vehicles.RemoveVehicle(all_veh_attributes[cnt][0])
	print "incident cleared"

def IncZone():
	divisor = 100/Pen_Rate						
	all_veh_attributes = Vissim.Net.Vehicles.GetMultipleAttributes(('No','Speed','Lane', 'DesSpeed', 'Pos', 'DesLane', 'NextLink','VehType'))
	for cnt in range(len(all_veh_attributes)):
		if all_veh_attributes[cnt][0] % divisor == 0 and all_veh_attributes[cnt][7] != ResponseType:
			#Zone 1 commands
			if all_veh_attributes[cnt][2].split('-')[0] == Crash_Link:
				if all_veh_attributes[cnt][4] > (Crash_Loc - dist_one) and all_veh_attributes[cnt][4] < (Crash_Loc - dist_two):
					#Lanechange Commands
					if all_veh_attributes[cnt][2].split('-')[1] == "1":
						Veh = Vissim.Net.Vehicles.ItemByKey(all_veh_attributes[cnt][0])
						Veh.SetAttValue('DesLane', 3)
					#Speed Commands
					if all_veh_attributes[cnt][3] != zone_1_speed:
						Veh = Vissim.Net.Vehicles.ItemByKey(all_veh_attributes[cnt][0])
						Veh.SetAttValue('DesSpeed', zone_1_speed)
						Veh.SetAttValue('Color1', "ff0000ff") #Blue
					
			#Zone 2 commands
			if all_veh_attributes[cnt][2].split('-')[0] == Crash_Link:
				if all_veh_attributes[cnt][4] > (Crash_Loc - dist_two) and all_veh_attributes[cnt][4] < (Crash_Loc - dist_three):
					if Blocked_Lanes == 2:
						if all_veh_attributes[cnt][2].split('-')[1] == "1" or all_veh_attributes[cnt][2].split('-')[1] == "2":
							Veh = Vissim.Net.Vehicles.ItemByKey(all_veh_attributes[cnt][0])
							Veh.SetAttValue('DesLane', 3)
					if Blocked_Lanes == 1:
						if all_veh_attributes[cnt][2].split('-')[1] == "1":
							Veh = Vissim.Net.Vehicles.ItemByKey(all_veh_attributes[cnt][0])
							Veh.SetAttValue('DesLane', 2)
					if all_veh_attributes[cnt][3] != zone_2_speed:
						Veh = Vissim.Net.Vehicles.ItemByKey(all_veh_attributes[cnt][0])
						Veh.SetAttValue('DesSpeed', zone_2_speed)
						Veh.SetAttValue('Color1', "ffffff00") #Yellow
					#Zone 3 commands
			if all_veh_attributes[cnt][2].split('-')[0] == Crash_Link:
				if all_veh_attributes[cnt][4] >= (Crash_Loc + 75):
					if all_veh_attributes[cnt][3] != zone_3_speed:
						Veh = Vissim.Net.Vehicles.ItemByKey(all_veh_attributes[cnt][0])
						Veh.SetAttValue('DesSpeed', zone_3_speed)
						Veh.SetAttValue('DesLane', 9999)
						Veh.SetAttValue('Color1', "ffffffff") #White
						
def IncZone2():
	divisor = 100/Pen_Rate						
	all_veh_attributes = Vissim.Net.Vehicles.GetMultipleAttributes(('No','Speed','Lane', 'DesSpeed', 'Pos', 'DesLane', 'NextLink', 'VehType'))
	for cnt in range(len(all_veh_attributes)):
		if all_veh_attributes[cnt][0] % divisor != 0 and all_veh_attributes[cnt][7] != ResponseType:
			#Zone 1 commands
			if all_veh_attributes[cnt][2].split('-')[0] == Crash_Link:
				if all_veh_attributes[cnt][4] > (Crash_Loc - dist_one) and all_veh_attributes[cnt][4] < (Crash_Loc - dist_two):
					#Lanechange Commands
					if all_veh_attributes[cnt][2].split('-')[1] == "1":
						Veh = Vissim.Net.Vehicles.ItemByKey(all_veh_attributes[cnt][0])
						Veh.SetAttValue('DesLane', 3)
					#Speed Commands
					if all_veh_attributes[cnt][3] != zone_1_speed:
						Veh = Vissim.Net.Vehicles.ItemByKey(all_veh_attributes[cnt][0])
						Veh.SetAttValue('DesSpeed', zone_1_speed)
					
			#Zone 2 commands
			if all_veh_attributes[cnt][2].split('-')[0] == Crash_Link:
				if all_veh_attributes[cnt][4] > (Crash_Loc - dist_two) and all_veh_attributes[cnt][4] < (Crash_Loc - dist_three):
					if Blocked_Lanes == 2:
						if all_veh_attributes[cnt][2].split('-')[1] == "1" or all_veh_attributes[cnt][2].split('-')[1] == "2":
							Veh = Vissim.Net.Vehicles.ItemByKey(all_veh_attributes[cnt][0])
							Veh.SetAttValue('DesLane', 3)
					if Blocked_Lanes == 1:
						if all_veh_attributes[cnt][2].split('-')[1] == "1":
							Veh = Vissim.Net.Vehicles.ItemByKey(all_veh_attributes[cnt][0])
							Veh.SetAttValue('DesLane', 2)
					if all_veh_attributes[cnt][3] != zone_2_speed:
						Veh = Vissim.Net.Vehicles.ItemByKey(all_veh_attributes[cnt][0])
						Veh.SetAttValue('DesSpeed', zone_2_speed)
					#Zone 3 commands
			if all_veh_attributes[cnt][2].split('-')[0] == Crash_Link:
				if all_veh_attributes[cnt][4] >= (Crash_Loc + 75):
					if all_veh_attributes[cnt][3] != zone_3_speed:
						Veh = Vissim.Net.Vehicles.ItemByKey(all_veh_attributes[cnt][0])
						Veh.SetAttValue('DesSpeed', zone_3_speed)
						Veh.SetAttValue('DesLane', 9999)

def ClearIncZone():
		all_veh_attributes = Vissim.Net.Vehicles.GetMultipleAttributes(('No','DesSpeed','Lane'))
		for cnt in range(len(all_veh_attributes)):
			if all_veh_attributes[cnt][2].split('-')[0] == Crash_Link:
				Veh = Vissim.Net.Vehicles.ItemByKey(all_veh_attributes[cnt][0])
				Veh.SetAttValue('DesLane', 9999)
				Veh.SetAttValue('DesSpeed', zone_3_speed)

def RespStgDispatch():
	for i in range (0, Blocked_Lanes):
		Vissim.Net.Vehicles.AddVehicleAtLinkPosition(ResponseType,Crash_Link,i+1,0,Speed_Limit,True)
		print "\nresponse initialized"
	all_veh_attributes = Vissim.Net.Vehicles.GetMultipleAttributes(('No','VehType'))
	for cnt in range(len(all_veh_attributes)):
		if all_veh_attributes[cnt][1] == ResponseType:
			Veh = Vissim.Net.Vehicles.ItemByKey(all_veh_attributes[cnt][0])
			Veh.SetAttValue('Color1', "ffff0000") #Red
		
def RespStgRouting():
	all_veh_attributes = Vissim.Net.Vehicles.GetMultipleAttributes(('No','VehType','Lane','Pos', 'DesLane','Speed'))
	for cnt in range(len(all_veh_attributes)):
		if all_veh_attributes[cnt][1] == ResponseType:
			if all_veh_attributes[cnt][3] <= (Crash_Loc - 500):
				Veh = Vissim.Net.Vehicles.ItemByKey(all_veh_attributes[cnt][0])
				Veh.SetAttValue('DesLane', 1)
			if all_veh_attributes[cnt][3] > (Crash_Loc - 400):
				Veh = Vissim.Net.Vehicles.ItemByKey(all_veh_attributes[cnt][0])
				Veh.SetAttValue('DesLane',1)
				Veh.SetAttValue('DesSpeed',30)
			if all_veh_attributes[cnt][3] >= (Crash_Loc):
				Veh = Vissim.Net.Vehicles.ItemByKey(all_veh_attributes[cnt][0])
				Veh.SetAttValue('DesLane',1)
				Veh.SetAttValue('DesSpeed',0)
				Veh.SetAttValue('Color1', "ffff0000") #Red
				
def ClearRespStg():
	all_veh_attributes = Vissim.Net.Vehicles.GetMultipleAttributes(('No','VehType'))
	for cnt in range(len(all_veh_attributes)):
		if all_veh_attributes[cnt][1] == ResponseType:
			Veh = Vissim.Net.Vehicles.ItemByKey(all_veh_attributes[cnt][0])
			Veh.SetAttValue('DesSpeed',45)
			"\nemergency vehicle cleared"
				
######################################################################################################################
#Code for Simulation Manager ~ DO NOT MODIFY				
def runSimulation():
	Vissim.Simulation.SetAttValue('RandSeed', Random_Seed)
	Vissim.Simulation.SetAttValue('SimPeriod', End_of_Simulation)
	Vissim.Simulation.SetAttValue('SimRes',Sim_Resolution)
	
	CurrDir = os.getcwd() #current directory
	Filename = os.path.join(CurrDir, 'network.inpx')
	flag_read_additionally = False
	
	Vissim.LoadNet(Filename, flag_read_additionally)
	Filename = os.path.join(CurrDir, 'network.layx')
	Vissim.LoadLayout(Filename)
	tot_steps = Sim_Resolution * End_of_Simulation
	
	for i in range(tot_steps):
		Vissim.Simulation.RunSingleStep()
		MsgFreq = IZFreq * Sim_Resolution

		if i == Crash_Start * Sim_Resolution and Incident == True:
			IncidentCreate()
		
		if INC_ZONE == True and Incident == True:
			if i == Sim_Resolution * (Crash_Start + IZDelay):
				print "\nINC-ZONE Application Started"
			
			if i > Sim_Resolution * (Crash_Start + IZDelay) and i < Sim_Resolution * Crash_End:
				if i % MsgFreq == 0:
					if Pen_Rate == 75:
						IncZone2()
					else:
						IncZone()
					
			if i == Sim_Resolution * Crash_End:
				ClearIncZone()
				print "\nINC-ZONE Application Stopped"
				
		if RESP_STG == True and Incident == True:
			if i == Sim_Resolution * (Crash_Start + RespDelay):
				RespStgDispatch()
			
			if i > Sim_Resolution * (Crash_Start + RespDelay) and i < Sim_Resolution * Crash_End:
				RespStgRouting()
					
			if i == Sim_Resolution * Crash_End:
				ClearRespStg()
			
		if i == Crash_End * Sim_Resolution and Incident == True:
			IncidentRemove()


######################################################################################################################
#Defining all the variables. Please define all the variables here.

def startup():
	global Random_Seed, End_of_Simulation, Sim_Resolution, Speed_Limit
	global Incident, INC_ZONE, Pen_Rate, IZDelay, IZFreq
	global RESP_STG, RespDelay, ResponseType
	global Crash_Start, Crash_End, Crash_Link, Crasher_type, Blocked_Lanes, Crash_Loc
	global advWarningDist, bufferLength, taperLength
	global zone_1_speed, zone_2_speed, zone_3_speed
	global dist_one, dist_two, dist_three
	
	#INPUT ALL VARIABLES BETWEEN THIS LINE:
	
	#Simulation Variables
	Random_Seed = randrange(1,1000)
	End_of_Simulation = 3600
	Sim_Resolution = 5
	Speed_Limit = 55
	
	#Incident Variables
	Incident = True
	Crash_Start = 900 		#Start time of the crash in seconds
	Crash_End = 1800 		#End time of the crash in seconds
	Crash_Link = "2" 		#Link ID on which crash will take place
	Crasher_type = "101"	#Crash Vehicle Type
	Blocked_Lanes = 1 		#choose 1 or 2
	Crash_Loc = 2500		#Location of crash on the link
	
	#INC-ZONE Variables
	INC_ZONE = True		#Whether INC-ZONE application is enabled.
	Pen_Rate = 100			#Percentage Market Penetration
	IZDelay = 0 			#Seconds after which the INC-ZONE application should start
	IZFreq = 1 				#Messaging Frequency in as a multiple of Simulation Resolution
	zone_1_speed = 50 		#mph
	zone_2_speed = 45 		#mph
	zone_3_speed = 65 		#mph
	
	#RESP-STG Variables
	RESP_STG = True
	RespDelay = 30
	ResponseType = "102"
	
	#AND THIS LINE
	
	advWarningDist = 1000 						#feet
	bufferLength = 13.3*Speed_Limit - 284 		#feet
	taperLength = 12*Blocked_Lanes*Speed_Limit 	#feet
	
	dist_one = advWarningDist + bufferLength + taperLength
	dist_two = bufferLength + taperLength
	dist_three = taperLength
		
	runSimulation()


######################################################################################################################
#Simulation Initiation
if __name__ == "__main__":	
	#Defining number of runs
	runs = 10
	for i in range(runs):
		Vissim = com.Dispatch("Vissim.Vissim-64.600")
		startup()
		Vissim = None
		time.sleep(15)
######################################################################################################################

