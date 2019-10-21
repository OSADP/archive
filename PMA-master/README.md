
PMA 1.0 Overview
============================
The DMA performance measurement application estimates mode-independent, trip-based system productivity and traveler mobility performance measures.  The application was developed by making use of trip-based system performance measure algorithms developed as part of the USDOT’s Integrated Corridor Management (ICM) Program [1] and adapting them for use with observed data to measure impact in mobility and productivity.  The algorithms developed under ICM, estimate key measures of corridor performance (delay, travel time reliability, and throughput) from time-variant traffic simulation outputs.

The DMA performance measurement application estimates mode-independent, trip-specific mobility meaures.  The three key measures that the application estimates are travel time reliability, delay and throughput.  The basic unit of observation for each measure described below is a trip that starts within a specific time interval (t) from a given origin (o) and ends at given destination (d) using a specific mode (m).

The output data are stored in a SQLite Database.  The database includes both the input as well as the output information.  The input tables are an echo of the user inputs to allow easy access to the input data in the database.  For detailed information on the input and output files, please refer sections on Input Data and Output Data.


Release Notes
------------------------------------
Initial release version 1.0 
The DMA performance measurement application estimates mode-independent, trip-based system productivity and traveler mobility performance measures.  The application was developed by making use of trip-based system performance measure algorithms developed as part of the USDOT’s Integrated Corridor Management (ICM) Program [1] and adapting them for use with observed data to measure impact in mobility and productivity.  The algorithms developed under ICM, estimate key measures of corridor performance (delay, travel time reliability, and throughput) from time-variant traffic simulation outputs.


License information
-------------------
Licensed under the Apache License, Version 2.0 (the "License"); you may not use this
file except in compliance with the License.
You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
Unless required by applicable law or agreed to in writing, software distributed under
the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
KIND, either express or implied. See the License for the specific language governing
permissions and limitations under the License.

System Requirements
-------------------------
The DMA performance measurement application was developed using the open source programming language Python 2.7 (www.python.org).  The application requires Python 2.7 or higher to run.  Note that Python 3.0 is a change from Python 2.X language and will not run the DMA performance measurement application.  Python versions 2.7.X – 2.9.X will work.  

The application can be run on Windows, Linux, or Mac operating systems.  Python is installed by default on many Linux and Mac operating systems.

For Detailed information about running and installing the software please reference 'Chapter 3 Installion Guide' 
in User Guide (PerformanceMeasuresPrototypeAppGuidelines_v1.0)


Documentation
-------------

The DMA performance measurement application software is packaged with Word based User Guide
"PerformanceMeasuresPrototypeAppGuidelines_v1.0" that contains all information about background
purpose, benefits, objectives, inputs/outputs, how to run the software and requirements for the software.

Support Contact
------------------------------------
Key contributors to the development of the Preformance Measures Prototype software 1.0 include:
- Jim Larkin, Meenakshy Vasudevan, and Karl Wunderlich of Noblis.

Code.gov Info
----------------
Agency: DOT

Short Description: The DMA performance measurement application estimates mode-independent, trip-based system productivity and traveler mobility performance measures.

Status: Beta

Tags: transporation, connected vehicles, cost planning

Labor Hours: 0

Contact Name: Jim Larkin

Contact Phone: 202-863-2978