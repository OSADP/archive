
# INFLO SPHARM
INFLO SPHARM Scenario Evaluation Package, including the VISSIM input files and the VISSIM COM, are designed to evaluate the effectiveness of the INFLO SPHARM Prototype based on the US-101 testbed. Different response rates were tested in the simulation model with the implicit assumption that the connected vehicle combined response rate used in the simulation model runs is the market penetration rate depreciated for communication loss, and driver compliance effects. Three different response rates were tested in the simulation model, 50%, 25%, and 10% of the total passenger-vehicle fleet. Six possible scenarios combining three possible traffic demand levels, three possible severity levels of incidents and two possible weather types for the corridor were evaluated for each response rate. The simulations and scripts used to analyze the data are included in this folder and briefly described below.


 # Release Notes
  INFLO SPHARM Scenario Evaluation Package v 2.0
  06/11/2015

    - What's New
    VISSIM input files of different scenarios
    VISSIM COM (connecting VISSIM and INFLO SPHARM Prototype)
    Outputs of different scenarios

    - Outstanding issues
        The vehicle output files of VISSIM cannot exceed 2 GB due to the limit of Microsoft ACCESS

    - What's New
        VISSIM input files of different scenarios
        VISSIM COM (connecting VISSIM and INFLO SPHARM Prototype)
        Outputs of different scenarios

    
    - Outstanding issues
    The vehicle output files of VISSIM cannot exceed 2 GB due to the limit of Microsoft ACCESS

# License
Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.

# System Requirements
  Recommended Configuration:
  Intel Core i7-3720QM CPU @2.60GHz 2.60 GHz
  16 GM RAM
  64-bit Operating System


#  Installation
  Required packages include:
  VISSIM 5.40
  Microsoft EXCEL
  Microsoft ACCESS

  VISSIM COM (SPHM_COM.xlsx) need be put into the scenario folder (for example, "1_US101_Implementation")


    - Troubleshooting
    If the VISSIM COM cannot run properly, make sure the following libraries checked under the References of VBA project.
    1. Visual Basic for Applications
    2. Microsoft Excel 14.0 Object Library
    3. OLE Automation
    4. Microsoft Office 14.0 Object Library
    5. VISSIM_COMServer 5.40 Type Library
    6. Microsoft ActiveX Data Object 6.1 Library 
    


Support Contact
------------------------------------
Meenakshy Vasudevan of Noblis