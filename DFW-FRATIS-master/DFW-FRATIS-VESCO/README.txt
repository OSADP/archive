# DFW-Fratis Vesco
For Dallas Forthworth FRATIS project

# Readme
The Vesco system is a Windows client application developed to generate optimized plans from Trinium daily files for the drayage companies, Southwest Freight (SWF) and Associated Carriers (AC).  The Trinium files consist of one (for AC) or two (for SWF) Excel files that enumerate all of the shipments that are planned for the day.  The program parses the file(s) to convert move codes, assign geocodes for locations, and imports drivers from templates.  It then runs the parsed data through an optimization engine developed by Productivity Apex Inc (PAI).  The optimization engine utilizes an ant colony optimization algorithm to determine an optimized plan.  Since this algorithm employs the use of metaheuristic optimizations, the results for the optimized plan will vary even though the input files remain the same.  The optimized plan file format is a comma separated value (CSV) test file which can be opened by most text editors and spreadsheet programs.

# License information
Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0 Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.

# System Requirements
.NET Framework 4.5
Microsoft Excel