# South Florida Freight Advanced Traveler Information System
Background--
The South Florida FRATIS demonstration project is focused on:
- Improving  communications  and  sharing  intermodal  logistics  information  between  the truck drayage industry and marine terminals such that terminals are less congested during peak hours;
- Improving traveler information available to intermodal truck drayage fleets so that they can more effectively plan around traffic; and
- Employment of an optimization algorithm which will allow for the technologies to work together in a way which optimizes the drayage fleet deliveries and movements based on several key constraints (e.g., time of day).

Together, these three areas of focus can result in significant improvements in intermodal efficiency, including reductions in truck trips and reductions in travel times.  These benefits, in turn, will directly result in the public-sector benefits of congestion reduction an improved air quality.

The two primary private-sector participants in the South Florida FRATIS demonstration project are Florida East Coast (FEC) Railway and FEC Highway Services (regional drayage fleets of over 50 trucks).

The optimization algorithm used in this project is an extension on the algorithm used in the Cross Town Improvement Project (C-TIP) Drayage Optimization Proof of Concept Application. Due to the complexity of the drayage problem, the proposed optimization algorithm was developed through multiple iterations. In each iteration, the performance of the algorithm was validated and tested to ensure that the algorithm was working properly. The algorithm performance has been tested over a set of different problems ranging from well-known benchmark problems to specially customized ones.   

Primary Functions:
- Minimizing total time for execution of drayage operations
- Minimizing total distance for execution of drayage operations
- Traffic delay consideration when generating route solutions for travel time estimates

Release Notes
------------------------------------
Version NUmber: v1.0

The Drayage Optimizer will produce a solution set that represents the best determined sequence of stops, minimizing the total time for execution based upon Locations, Divers, Jobs, Routes, and Traffic Delays provided to the algorithm. Traffic delays can be provided by any third party external system that estimates travel times considering traffic within two locations.  

The solution is built leveraging v4.5 of the .NET Framework.  The optimization class library has a small foot

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


Hardware Supported:
Intel/AMD based 64-bit processor architecture 

Installation Instructions:
There is no installer included with this package. Only Source code is provided.

Size of Package:
5.73 MB (6,015,944 bytes)

Suggested System requirements:
Memory:
4GB of RAM

Processing Power:
Any processor capable of running the Microsoft Windows 7 and higher operation system

Operating system:
Microsoft Windows Operating system supporting the .NET Framework v4.5 

Hard Drive Requirements:
50MB of HD

Additional to that, there are no hardware requirements for the algorithm to work. For the purpose of usnig the algorithm to generate a solution and send it to an On-Board device to a driver, any system with an API can be used to send the jobs.

Installation
------------------------------------ 
No installer included for this application

Support Contact
------------------------------------
From Cambridge Systematics, Inc.
	Mark A. Jensen (Project Manager)
	Roger Schiller (Deputy Project Manager)
	Michael Williamson (Project Manager, South Florida FRATIS)
	Erin Kersh (Deputy Project Manager, South Florida FRATIS)
From Productivity Apex, Inc.
	Sam Fayez (Director, Transportation and Supply Chain Solution)
	Fabio Zavagnini (Transportation and Business Analyst)
	Ahmed El-Nashar (Operations Research Expert)
	Joseph Tapia (Lead Software Developer)
	Alex Hijab (Software Developer)
	Chris Strube (Software Developer)
	Daniel Leonard (Transportation and Business Analyst)