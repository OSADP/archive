
Commercial Vehicle (CV) Retrofit Safety Device (RSD) Kits Project Overview
============================
Retrofit Safety Device (RSD) kits were developed and deployed on commercial vehicles as part of the USDOT Connected Vehicle Safety Pilot to gain insight into the unique aspects of deploying connected vehicle technology in a commercial vehicle environment.  These kits enable communication with other in-vehicle (commercial and passenger) and infrastructure Direct Short Range Communication (DSRC) devices via a Basic Safety Message (BSM) and multiple infrastructure-oriented messages, and various messages related to device security credential management.  Implemented safety applications include Curve Speed Warnings (CSW), Emergency Electronic Brake Light (EEBL), and Forward Collision Warning (FCW).  Each of the RSD kits includes a DSRC radio and antenna(s), GPS receiver and antenna, embedded gyroscope, J1939 Controller Area Network (CAN) interface, human machine interface (HMI), and interface to a Data Acquisition System (DAS).

License information
-------------------
Licensed under the Apache License, Version 2.0 (the "License"); you may not use this
file except in compliance with the License.
You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
Unless required by applicable law or agreed to in writing, software distributed under
the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
KIND, either express or implied. See the License for the specific language governing
permissions and limitations under the License.

System Software Requirements
-------------------------
The Onboard Equipment (OBE) Development Environment:
VMWare virtual machine was provided by Cohda with API, radio specific libraries, and build tools. This is not avaliable with the OSADP package.

Display Development Environment:
Eclipse 3.7.2 (Indigo): IDE for Java Developers 
Android Dev Software Development Kit (SDK) 3.2 (API 13): Android libraries and framework 
Java JDK 1.6.0_32: Core Java libraries and framework 

OBE-DSRC Software:
mk2-3.5.img: Core operating system firmware and radio libraries 

Display Software 
AndroidServer_120: Tablet display application software 

For detailed information on setting up a new applciation or current working environment please refer to 'Plan to Accommodate Future Development Version 1.1.0 for the CV RSD' document.

Documentation
-------------

The Commercial Vehicle (CV) Retrofit Safety Device (RSD) Kits Project has several key documents that is high recommended to review before working with the software.  These documents include:

1.  Plan to Accommodate Future Development 
2.  Data Acquisition System (DAS) functions and Instructions for Programming 
3.  RSD Configurable Parameters 
4.  RSD Installation, Troubleshooting, and Initialization Guide 
5.  Architecture and Design Specification 
6.  Retrofit Safety Device Build, Test Plan and Procedure 

Along with these documents are several other system engineering documents that may also be helpful when working with the software.  These documents will all be avaliable on the website CV pilot website application page (http://www.its.dot.gov/pilots/cv_pilot_apps.htm)  in the near future.

File structure (all of these files are compressed using the free 7zip utility (http://www.7-zip.org/)) :
- 'safety_apps.7z' - AndroidServer Code for the safety applications.  
- 'gemimi-4945.7z' - Code for the safety applications.  


Web sites
---------
The Commercial Vehicle (CV) Retrofit Safety Device (RSD) Kits Project is distributed through the USDOT's JPO Open Source Application Development Portal (OSADP)
http://itsforge.net/ 

Southwest Research Institute Project Summary Page
http://www.swri.org/4org/d10/isd/ivs/rsd.htm
