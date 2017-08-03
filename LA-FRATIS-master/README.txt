Application Name:
Los Angeles-Gateway Freight Advanced Traveler Information System

Version Number: 
v1.0 

Primary Functions:
Minimizing total time for execution of drayage operations
Minimizing total distance for execution of drayage operations
Queue delay consideration when generating route solutions for Terminal waiting times 
Traffic delay consideration when generating route solutions for travel time estimates


Hardware Supported:
Intel/AMD based 64-bit processor architecture 

Installation Instructions:
There is no installer included with this package. Only Source code is provided.


Size of Package:
29.4 MB (30,846,660 bytes)

Description of the applciation:
Background--
The LA-Gateway FRATIS demonstration project is focused on:
	Improving  communications  and  sharing  intermodal  logistics  information  between  the truck drayage industry and port terminals such that terminals are less congested during peak hours;
	Improving traveler information available to intermodal truck drayage fleets so that they can more effectively plan around traffic and port congestion; and
	Employment of an optimization algorithm which will allow for the technologies to work together in a way which optimizes the drayage fleet deliveries and movements based on several key constraints (e.g., time of day, PIERPASS restrictions, terminal queue status, etc.).
Together, these three areas of focus can result in significant improvements in intermodal efficiency, including reductions in truck trips, reductions in travel times, and improved terminal gate and processing efficiency.  These benefits, in turn, will directly result in the public-sector benefits of congestion reduction an improved air quality.
The two primary private-sector participants in the FRATIS LA demonstration project are Yusen Terminals, Inc. (Port of LA Terminal) and Port Logistics Group (regional drayage fleets of 50 trucks). The  primary  regional  public-sector  agencies  that  are  supporting  the  test  are  LA  Metro,  The Gateway Cities Council of Governments, and the Port of LA.

The optimization algorithm used in this project is an extension on the algorithm used in the Cross Town Improvement Project (C-TIP) Drayage Optimization Proof of Concept Application. Due to the complexity of the drayage problem, the proposed optimization algorithm was developed through multiple iterations. In each iteration, the performance of the algorithm was validated and tested to ensure that the algorithm was working properly. The algorithm performance has been tested over a set of different problems ranging from well-known benchmark problems to specially customized ones.   

Release notes--The Drayage Optimizer will produce a solution set that represents the best determined sequence of stops, minimizing the total time for execution based upon Locations, Divers, Jobs, Routes, Traffic Delays, and Queue Delays provided to the algorithm. Traffic delays can be provided by any third party external system that estimates travel times considering traffic within two locations. Queue delays can be provided by any third party external system that estimates waiting times to get into a location (for example, the case of waiting in queues at the gates of Marine Terminals)  
The solution is built leveraging v4.5 of the .NET Framework.  The optimization class library has a small footprint, and should run successfully on any computer capable of supporting .NET v4.5.  



Category:
Algorithm

Subcategory:
Transportation

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