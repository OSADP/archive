
# Los Angeles-Gateway Freight Advanced Traveler Information System (FRATIS)
The LA-Gateway FRATIS demonstration project is focused on:
- Improving  communications  and  sharing  intermodal  logistics  information  between  the truck drayage industry and port terminals such that terminals are less congested during peak hours;
- Improving traveler information available to intermodal truck drayage fleets so that they can more effectively plan around traffic and port congestion; and
 - Employment of an optimization algorithm which will allow for the technologies to work together in a way which optimizes the drayage fleet deliveries and movements based on several key constraints (e.g., time of day, PIERPASS restrictions, terminal queue status, etc.).

Together, these three areas of focus can result in significant improvements in intermodal efficiency, including reductions in truck trips, reductions in travel times, and improved terminal gate and processing efficiency.  These benefits, in turn, will directly result in the public-sector benefits of congestion reduction an improved air quality.

The two primary private-sector participants in the FRATIS LA demonstration project are Yusen Terminals, Inc. (Port of LA Terminal) and Port Logistics Group (regional drayage fleets of 50 trucks). The  primary  regional  public-sector  agencies  that  are  supporting  the  test  are  LA  Metro,  The Gateway Cities Council of Governments, and the Port of LA.

Primary Functions:
- Minimizing total time for execution of drayage operations
- Minimizing total distance for execution of drayage operations
- Queue delay consideration when generating route solutions for Terminal waiting times 
- Traffic delay consideration when generating route solutions for travel time estimates


Release Notes
------------------------------------
Version Number: v1.0 
The Drayage Optimizer will produce a solution set that represents the best determined sequence of stops, minimizing the total time for execution based upon Locations, Divers, Jobs, Routes, Traffic Delays, and Queue Delays provided to the algorithm. Traffic delays can be provided by any third party external system that estimates travel times considering traffic within two locations. Queue delays can be provided by any third party external system that estimates waiting times to get into a location (for example, the case of waiting in queues at the gates of Marine Terminals)  
The solution is built leveraging v4.5 of the .NET Framework.  The optimization class library has a small footprint, and should run successfully on any computer capable of supporting .NET v4.5.  


License Information
------------------------------------
    Copyright 2014 Productivity Apex Inc.
        http:www.productivityapex.com/
 
    Licensed under the Apache License, Version 2.0 (the "License");
    you may not use this file except in compliance with the License.
    You may obtain a copy of the License at
 
        http:www.apache.org/licenses/LICENSE-2.0
 
    Unless required by applicable law or agreed to in writing, software
    distributed under the License is distributed on an "AS IS" BASIS,
    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
    See the License for the specific language governing permissions and
    limitations under the License.	

Configuration
------------------------------------ 

Hardware Supported:
Intel/AMD based 64-bit processor architecture 

Suggested System requirements:
Memory:
4GB of RAM

Processing Power:
Any processor capable of running the Microsoft Windows 7 and higher operation system

Operating system:
Microsoft Windows Operating system supporting the .NET Framework v4.5 

Hard Drive Requirements:
50MB of HD
Additional to that, there are no hardware requirements for the algorithm to work. For the purpose of using the algorithm to generate a solution and send it to an On-Board device to a driver, any system with an API can be used to send the jobs.

Size of Package:
29.4 MB (30,846,660 bytes)

rojects Included In Solution:

Core	
PAI.Core	- Common interfaces used throughout solution
Defines common elements that are composed of objects defined in either the System or System.Runtime.Serializaiton namespaces and a common IEntity interface which underpins a common means for identifying similar objects.

PAI.FRATIS.Data	- Data Access Tier: Entity Framework implementation
Defines three namespaces, PAI.FRATIS.Data, PAI.FRATIS.Data.Mappings, and PAI.FRATIS.Data.Migrations.  PAI.FRATIS.Data comprises the EntityFramework implementation of the domain objects used by the Drayage Route Optimizer, the Mappings namespace defines any unique mappings necessary from the persistence repository objects to the domain objects.  The Migrations namespace is used to create or modify seed data and contains method hooks for implementing run once data conversion methods.

PAI.FRATIS.DateTimeService	- Utility methods for UTC / Local time conversions
Defines a common interface to facilitate handling data of different or uncertain time zoning.  Allows for easy time comparison and manipulation of time and date objects across time zones 

PAI.FRATIS.Domain	- Business / Domain Objects
Comprises several subnamespaces: Congifuration, Equipment, Geography, Geography.NokiaMaps, Geography.NokiaMapos.TrafficItems, Information, Logging, Messaging, Orders, Planning, Times, and Users
The Configuration namespace contains configuration and preference objects; the Equipment namespace defines typically amortizable physical goods represented in the optimizer; Geography defines objects that represent either a physical location and situational effects that are geographically bound, such as traffic and weather; the NokiaMaps and NokiaMaps.TrafficItems namespaces further define traffic and location specific objects that are specific to users that are integrating with a Nokia Data Provider.  Information defines those objects necessary for real time positioning; Logging provides facilities for outputting debugging and information messages to the file system or screen; Messaging provides an Alert object which can route a message or event from a user to another; Orders defines the objects that are necessary to instantiate an order to process through the optimization algorithm, such as drivers, jobs, jobs status updates, and which actions are available to undertake during a stop; Planning segregates thoseelements uses to set up and generate an optimization plan for a number of drivers with some number of jobs; Times supplies an access point for fine tuning actions by the day of th week; and Users defines the objects necessary for user management.  

PAI.FRATIS.Infrastructure	- Project Infrastructure, Ninject Inversion of Control (IoC)
Infrastructure segregates the Infrastructure; Infrastructure.Data; Infrastructure.Engine; and Infrastructure.Threading objects that are used to implement the Dependency Injection pattern and support multi-threaded generation of the optimal route. 

	
Services	
PAI.FRATIS.DataServices	- Services for Domain Objects, Persistence
Provides an access point for application access of persistence repositories during execution of the optimization algorithm. 

PAI.FRATIS.DataServices.Core	- Entity Services base implementations
Instantiates low level objects and services used by the PAI.FRATIS.DataServices.Core objects to manage cache and repository access.

	
External Services	
PAI.FRATIS.Services.ExternalDistanceService	- Navteq implementation of pluggable external traffic / distance service
Provides an access point, plug-in framework for interfacing with external data sources.

PAI.FRATIS.Wrappers.NokiaMaps	- Nokia Here / Maps external service implementation
An example implementation of the Nokia Mapping Service

PAI.FRATIS.Wrappers.QueueTimes	- Acyclica external service implementation for Terminal Queue reporting
An example implementation of an external Terminal Queue Reporting Service

PAI.FRATIS.Wrappers.WebFleet	- TomTom WEBFLEET external service implementation
An example implementation of the TomTom Mapping 

PAI.FRATIS.Wrappers.YahooWeather	- Yahoo Weather external service implementation
An example implementation of the Yahoo Weather Service 

PAI.FRATIS.YusenTerminalService	- Yusen Marine Terminal external service implementation
An example implementation of the Yusen Terminal Service

	
Optimization Algorithm	
PAI.Drayage.Optimization	- Optimization Algorithm
The optimization algorithm, wherein jobs and drivers are matched on user provided selection criteria.  Typically this is minimizes either time or distance traveled while servicing as many jobs as possible.

PAI.Optimization.Adapter	- �Adapter� intermediary between domain objects and algorithm
Access point for further configuration and adjustment of the optimization algorithm or supporting object after initialization and during run-time.

PAI.Drayage.Optimization.Reporting	- Reporting and statistical service for optimization algorithm solutions
A collection of objects and methods that will examine generated routes and schedules to extract meaningful data over the generated route solutions.

	
Executable Processes	
PAI.FRATIS.BackgroundProcesses	- Background process wrappers for container availability, traffic/travel time
Supplimental process that run in worker threads supporting the creation and maintenance of relatively static and parallelizable data sources and elements

PAI.FRATIS.ConsoleDemo	- Demo application 
A simple console application that demonstrates how to provide data to and consume the results of the optimization algorithm.  Uses a random subset of a static data set to create drivers and orders to feed into the algorithm.  Additionally illustrates how to consume the results of the optimizer.

	
Tests	
PAI.Drayage.Tests	Test base classes and Extension methods
PAI.FRATIS.ExternalServices.Tests	Integration tests for external data source connections
PAI.FRATIS.Tests	Unit tests to ensure the correct behavior of the component objects of the project whole.
The Drayage Optimizer will produce a solution set that represents the best determined sequence of stops, minimizing the total time for execution based upon Locations, Divers, Jobs, Routes, Traffic Delays, and Queue Delays provided to the algorithm. Traffic delays can be provided by any third party external system that estimates travel times considering traffic within two locations. Queue delays can be provided by any third party external system that estimates waiting times to get into a location (for example, the case of waiting in queues at the gates of Marine Terminals)  
The solution is built leveraging v4.5 of the .NET Framework.  The optimization class library has a small footprint, and should run successfully on any computer capable of supporting .NET v4.5.  



Installation
------------------------------------ 
There is no installer included with this package. Only Source code is provided.

Support Contact
------------------------------------
Key contributors to the development of theLos Angeles-Gateway Freight Advanced Traveler Information System, and development of the documentation include:

From Cambridge Systematics, Inc.
	Mark A. Jensen (Project Manager)
	Roger Schiller (Deputy Project Manager)
From Productivity Apex, Inc.
	Sam Fayez (Director, Transportation and Supply Chain Solution)
	Fabio Zavagnini (Transportation and Business Analyst)
	Ahmed El-Nashar (Operations Research Expert)
	Joseph Tapia (Lead Software Developer)
	Alex Hijab (Software Developer)
	Chris Strube (Software Developer)