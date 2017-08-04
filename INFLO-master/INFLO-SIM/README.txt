Open Source Overview
-------------------
INFLO SPHARM Scenario Evaluation Package, including the VISSIM input files and the VISSIM COM, are designed to evaluate the effectiveness of the INFLO SPHARM Prototype based on the US-101 testbed. Different response rates were tested in the simulation model with the implicit assumption that the connected vehicle combined response rate used in the simulation model runs is the market penetration rate depreciated for communication loss, and driver compliance effects. Three different response rates were tested in the simulation model, 50%, 25%, and 10% of the total passenger-vehicle fleet. Six possible scenarios combining three possible traffic demand levels, three possible severity levels of incidents and two possible weather types for the corridor were evaluated for each response rate. The simulations and scripts used to analyze the data are included in this folder and briefly described below.

Web sites
-----------------

INFLO SPHARM Scenario Evaluation Package is distributed through the USDOT's JPO Open Source Application Development Portal (OSADP)
http://itsforge.net/ 

-----------------
Application Name:
CV-XX

Version Number:
2.0

Installation Instructions:
Refer to the VISSIM user guide.

Three sets of VISSIM input files are included. CV10, CV25, and CV50. These represent three scenarios of connected vehicle (CV) market share for the San Mateo, CA test bed. The simulations were used to assess the effectiveness of SPDHARM and QWARN (see INFLO-PRO readme).

-----------------
Application Name:
qTmProc

Version Number: 
v1.0 

Installation Instructions:
There is no installer included with this package. Only Source code is provided. To run the script, load a CSV version of the VISSIM vehicle record. This folder includes a sample input file with the column structure the script needs.


One of the performance measures used to assess the effectiveness of SPDHARM and QWARN (see INFLO-PRO readme) is vehicle-hours in queue. 
The VISSIM vehicle record output includes this variable, but it reports it on a cumulative basis for each vehicle. The project team needed this variable on an hour-by-hour basis. A Python script, called qTMProc, was developed to disaggregate the data into 5-minute (300-second) buckets. This tool allowed the team to monitor the effect of the simulated incidents on queueing.

