#FRATIS Emergency Response Mobile Application / Server Component

The Emergency Response component of the South Florida FRATIS project is a comprehensive set of tools designed to help emergency response crews, freight drivers/operators and civillians gather and display critical data during emergency situations where standard technologies fail to represent a real world sittuation such as during a hurricane when power outages and widespread area closures may occur. 

This is an early rapid prototyping project which was designed to illustrate how such as system may work so the majority of the display component was designed using the web version of Google APIs such as Maps, Places and Directions.

This project contains two parts: 
*An Android Mobile Application - designed to collect data in the field such as emergency responder reports, business status (for business owners and civillians who try to visit the business), civillian reports for damages and gps points which can help identify areas that are accesssible.

*Server Component - a standard php/MySQL web interface responsible for storing and displaying data. All report templates and data are stored in the web interface and downloaded by the mobile application when changes occur. The server receives completed reports from the mobile devices in JSON/XML formats and process them into a format that is suitable for storing in a MySql Database.

## Demonstration

A live demonstration server can be viewed at this location:
http://fratis.trac.washington.edu/

## Installation
The system has two components. There are two sets of installation instructions: one for each of the components.

####Server Component
because the system was programmed to work with php/MySQL it should run on any server which supports it. The demonstration server runs on Microsoft Windows Server 2012 R2 inside an IIS page with a PHP extension and a standalone installation of MySQL. We have also tested inside WAMP (a bundled software based on Apache/php/MySQL) for development purposes.

#####Installation
1. Install/Configure an Apache/MySQL web server environment
2. Upload/Copy contents of wwwroot directory to the appropropriate home directory on server
3. Create a MySQL username and database to use for project.
4. Grant user at least CREATE, DROP, ALTER, DELETE, INSERT, SELECT, UPDATE privileges
5. Edit config.php and configure database settings at the top to reflect actions in step 3:
	$dbhost = "localhost"; //Database Server
	$username = ""; //Username
	$password = ""; //Password
	$database = ""; //FRATIS Database
6. For complete integration you will need to modify the RemoteServer attribute in Mobile Application source code and copy the compiled App to wwwroot\App folder

####Mobile Application
The mobile application requires the latest version of Google Play Services. Please install/update the Google Play Store which should automatically update Google Play Services to latest version. Please note that Google Play Store and Services are not available on Kindle OS or some other types of customized android devices.

#####Installation
1. Install Eclipse / Android SDK
2. Download Android SDK packages for target Android version (for example 4.2 or 4.4)
3. Download SDK package for Google Play Services for that version
4. Point Eclipse to the Mobile App folder in the repository
5. If you wish to use your own Server version edit MyApplication.java and change the following line to point to the home directory where you installed the Server Component (for example your.server.com/fratis):
    public String RemoteHost 	= "140.142.198.59"; //Change to your location
6. Build the Android project and copy EmergencyResponse.apk to the App directory where you uploaded the Server Component

To install the compiled App you must allow installation from unknown sources. More info view the last section on this page:
http://developer.android.com/distribute/tools/open-distribution.html

You may install the app on your Android device by navigating to the URL of the server you installed the Server Component and clicking the Green Android icon at the top of the page.

## Usage

Upon installation of the server component you will be able to register new users and take reports through the web interface for the Server Component. After the mobile application is installed and compiled, you will be able to log in using those user accounts and take more comprehensive reports.

Please note the rapid prototype nature of this project. It is mostly used to display what the future of this project may represent and extended security features are not in place to make this a real-world ready application. There are no restrictions on who can create usernames and take ownership of businesses or what types of data are entered into the system. 

## Credits

Dmitri Zyuzin - dzyuzin@uw.edu
Mark Hallenbeck - tracmark@uw.edu

## License
Copyright 2015 University of Washington TRAC Cost Center

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.