CO-PILOT 1.0
====================================
The Cost Overview for Planning Ideas and Logical Organization Tool ("CO-PILOT") is designed to support 
stakeholders considering connected vehicle pilot deployments. CO-PILOT is a web-based tool that allows 
users to estimate the costs of proposed pilot deployments at a high level. The tool aims to refine 
deployment plans and to reduce the risk that the requested Federal cost share will exceed the pilot 
deployment grant ceiling.  

The software's intuitive and user friendly interface helps walk users through the process of selecting 
their desired application(s), inputting their building block quantities, and assigning groups of 
building blocks to the appropriate applications. CO-PILOT allows users to generate deployment cost 
estimates for 56 applications, and provides flexibility so that users can modify assumptions to suit 
their needs. CO-PILOT uses a simulation approach to account for uncertainty in both unit and overall 
costs, and provides outputs including the estimated probability distribution of total costs and a 
detailed spreadsheet.   

The tool can be found at https://co-pilot.noblis.org/CVP_CET/

Release Notes
------------------------------------
This version is the initial release. Any bugs found will be addressed in future releases. 


License Information
------------------------------------
Licensed under the Apache License, Version 2.0 (the "License"); you may not use this

file except in compliance with the License.

You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software distributed under

the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY

KIND, either express or implied. See the License for the specific language governing

permissions and limitations under the License.

Configuration
------------------------------------ 
CO-PILOT can run on all operating systems and supports all major web browsers. 


Required Software:

1.	An updated version of Java (language it was developed in) 
	Available for download at https://www.java.com/en/

2.	Apache Tomcat 8.0 (server) 
	Available for download at  http://tomcat.apache.org/

3.	PostgreSQL (database)
	Available for download at http://www.postgresql.org/download/
	
	
	
Recommended Software:
	
1.	pgAdmin (database client/viewer) 
	Available for download at http://www.pgadmin.org/download/
	
 
Installation
------------------------------------ 
Within the CO-PILOT project folder:

A User Guide for the tool titled "CO-PILOT_UserGuide_1_15.pptx" can be found in the csv folder (Co-Pilot\web\resources\noblis\csv)
All source code (java and configuration files) is located in the "src" folder.
All web files (HTML, CSS, javascript as well as the jquery and highcharts packages) are located in the "web" folder. 

Data Input/Output
------------------------------------

Users are asked to individually select and assign application(s) to user-input quantities of relevant building blocks.
CO-PILOT then produces three valuable outputs: 

1. An Excel spreadsheet providing a line-item breakdown of deployment costs.
2. A pie chart displaying the percentages of costs associated with each deployment building block.
3. A cost probability distribution graph displaying the projected deployment costs.

Additional Features
------------------------------------  
-Users have the ability to upload an existing CO-PILOT output spreadsheet to expedite data entry for future runs.

-Users are given the opportunity to edit the Quantities or Default Unit Costs presented in the line-item breakdown.

-You can also add to your line-item breakdown the name, quantity and cost of additional component(s)
(i.e for components not listed in the database)  

Note: User-input costs added with the above two features will be fixed rather than simulated.


Documentation
------------------------------------

The CO-PILOT software features a "Help" page containing a thorough cost estimation walk-through. The page also 
includes links for several reference materials, including: 
-CO-PILOT Frequently Asked Questions
-CO-PILOT Cost Estimator Webinar Recording – January 7, 2015
-CO-PILOT Cost Estimator Webinar Slide Deck
-Connected Vehicle Cost Components detail
-CO-PILOT Background, Assumptions, and User Instructions


The documents can be retrieved from your downloaded copy from the following path: Co-Pilot\web\resources\noblis\csv. Links to the webinar 
recording and webinar slide deck are embedded in the help.html page in the web folder. You can also access the Help page 
directly at https://co-pilot.noblis.org/CVP_CET/help.html and download the resources from there.  

Web Sites
------------------------------------
URL to tool: https://co-pilot.noblis.org/CVP_CET/

Disclaimer
------------------------------------
CO-PILOT is intended for high-level, preliminary planning purposes to support Connected Vehicle Pilot Deployment 
cost estimation. Outputs are intended to support long-range budget planning and do not replace detailed cost 
proposals required for Concept Development (Phase 1), Design/Build/Test (Phase 2), or Maintain and Operate (Phase 3). 


Support Contact
------------------------------------
For any technical support or inquiries regarding CO-PILOT, please e-mail co-pilot@noblis.org

Code.gov Info
----------------
Agency: DOT

Short Description: The Cost Overview for Planning Ideas and Logical Organization Tool ("CO-PILOT") is designed to support 
stakeholders considering connected vehicle pilot deployments. CO-PILOT is a web-based tool that allows 
users to estimate the costs of proposed pilot deployments at a high level. 

Status: Beta

Tags: transporation, connected vehicles, cost planning

Labor Hours: 0

Contact Name: James O'Hara

Contact Phone: 703-610-1632

