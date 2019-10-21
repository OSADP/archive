# DFW-FRATIS
For Dallas Forthworth FRATIS project

Readme
============================
Alternate Optimization Program 
1.0
The alternate optimization program is a linear program written with the 
objective of minimizing empty (bobtail) moves for Southwest Freight, a 
participant in the Freight Advanced Traveler Information System (FRATIS) 
prototype test in Dallas-Fort Worth, Texas. The AOP was one application 
within the prototype system. 
The AOP requires 2 Excel or comma separate input file containing 30 columns; 
these input files were generated from Southwest Freight’s dispatching 
software [vendor: Trinium Technologies]. One file contains loaded orders, and 
the second file contained empty orders, but both files contained the same 
column structure. A blank template is included within the uploaded zip file. 
The user must select the input file from any location on their desktop. To 
account for Southwest Freight requirements, the program limits the number of 
orders that can be assigned to a single driver (route) to four, although this 
can be altered in the code. In addition, the program offers the user the 
flexibility to determine the number of drivers/routes that should be included 
in the daily plan. For Southwest Freight, they wanted to make sure a certain 
number of drivers would have work each day. This can also reduce the number 
of orders per route/driver. As described above, both the user interface and 
code were written in Java.
There is no installer included with this package. Only Source code is 
provided.

License information
-------------------

Licensed under the Apache License, Version 2.0 (the "License"); you may not 
use this
file except in compliance with the License.
You may obtain a copy of the License at 
http://www.apache.org/licenses/LICENSE-2.0
Unless required by applicable law or agreed to in writing, software 
distributed under
the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
CONDITIONS OF ANY
KIND, either express or implied. See the License for the specific language 
governing
permissions and limitations under the License.

System Requirements
-------------------------
Tools and language: Eclipse, Java 1.6
Java Runtime Environment 1.6 or higher, Microsoft Excel)
Connectivity (N/A)
Running the Alternate Optimization program: 
From DOS prompt, navigate to the location of optimizer.jar enter the command 
java –jar optimizer.jar
A user may also double click the optimizer.jar file
