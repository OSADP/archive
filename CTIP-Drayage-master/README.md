# Cross Town Improvement Project 
Cross Town Improvement Project (C-TIP) Drayage Optimization Proof of Concept Application.

Background--The Cross-Town Improvement Project (C-TIP) has expanded under the Federal Highway Administration since it was introduced as a new project by the Intermodal Freight Technology Working Group (IFTWG). The IFTWG was seeking a solution to congestion, ineffectiveness, and increasing energy consumption that stemmed from the inefficiency of cross-town �rubber tire� interchanges (i.e., traffic from truck to rail, and rail to truck to rail) by developing and deploying an information sharing capability that enables the coordination of moves between parties to maximize loaded moves and minimize unproductive moves. In 2012, US DOT funded a project focusing on the Real-Time Traffic Monitoring and Dynamic Route Guidance components of C-TIP by incorporating optimization heuristics and a robust routing algorithm. The team selected by the US DOT partnered with a drayage company in Memphis, TN to be the basis for developing and deploying the optimization algorithm and other related technology.

Approach--To effectively handle the complexity of the drayage problem inherent in the Memphis C-TIP and ensure that the algorithm is working properly:

- The development and validation of the algorithm was divided into multiple iterations
- The algorithm performance was tested over a set of different problems ranging from well-known benchmark problems to specially customized ones 
- The performance assessment of the algorithm relied on sets of real data that were collected from the daily operations of the drayage company in Memphis, TN over six-month period 

To improve the capabilities of the optimization algorithm and to increase the robustness of its generated plans, data collection and testing was separated into two phases: one three-month Pre-Deployment Phase and one three-month Post-Deployment Phase. Each phase ensured that the algorithm design, development, and deployment were effective. Pre-Deployment and Post-Deployment phase issues were thoroughly examined, documenting all findings and response actions on a continual basis in order to minimize any impact to the project schedule, maintain user adherence to processes, and avoid similar issues in the future.

Description and objectives--The main objective of C-TIP project was the development, testing, and deployment of an Algorithm that utilizes powerful and intelligent optimization heuristics to improve drayage operations while considering all operational constraints and restrictions associated with drayage moves. The main concern of the project was to maximize the loaded moves and minimize the unproductive ones, which improves drayage companies� efficiency, reduces congestion on the roads, and impacts the environment by decreasing carbon footprint.

Due to the complexity of the drayage problem, the proposed optimization algorithm was developed through multiple iterations. In each iteration, the performance of the algorithm was validated and tested to ensure that the algorithm was working properly. The algorithm performance has been tested over a set of different problems ranging from well-known benchmark problems to specially customized ones. 

Primary Functions:
- Minimizing total time for execution of drayage operations
- Minimizing total distance for execution of drayage operations

# Release Notes
Version Number: 
v1.0

The Drayage Optimizer will produce a solution set that represents the best determined sequence of stops, minimizing the total time for execution based upon Locations, Divers, Jobs and Routes provided to the algorithm.

# License Information
Licensed under the Apache License, Version 2.0 (the "License"); you may not use this

file except in compliance with the License.

You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software distributed under

the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY

KIND, either express or implied. See the License for the specific language governing

permissions and limitations under the License.

The solution is built leveraging v4.5 of the .NET Framework.  The optimization class library has a small footprint, and should run successfully on any computer capable of supporting .NET v4.5.  

# Configuration

Hardware Supported:
Intel/AMD based 64-bit processor architecture 

Installation Instructions:
There is no installer included with this package. Only Source code is provided.

Size of Package:
6.53 MB (6,852,969 bytes)

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

