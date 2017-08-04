using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace INCZONE.VITAL
{
    class ChevyCaprice : vitalVehicleInterface
    {

        private static String timeFormat = "HH:mm:ss.fff ";

        /*
BACKUP LAMPS: 07-AE-12-01-01-00-00-00 / 07-AE-12-01-00-00-00-00
DAYTIME RUNNING: 07-AE-12-08-08-00-00-00 / 07-AE-12-08-00-00-00-00
FOG LAMPS: 07-AE-12-10-10-00-00-00 / 07-AE-12-10-00-00-00-00
HIGH BEAMS: 07-AE-12-20-20-00-00-00 / 07-AE-12-20-00-00-00-00
HORN: 07-AE-1A-02-00-FF-00-00 / 07-AE-1A-02-00-00-00-00
LEFT FRONT TURN: 07-AE-13-04-04-00-00-00 / 07-AE-13-04-00-00-00-00
RIGHT FRONT TURN: 07-AE-13-10-10-00-00-00 / 07-AE-13-10-00-00-00-00
LEFT REAR TURN: 07-AE-13-40-40-00-00-00 / 07-AE-13-40-00-00-00-00
RIGHT REAR TURN: 07-AE-13-80-80-00-00-00 / 07-AE-13-80-00-00-00-00
LOW BEAMS: 07-AE-12-40-40-00-00-00 / 07-AE-12-40-00-00-00-00

DRIVER DOOR UNLOCK: 07-AE-10-00-00-02-02-00
PASSENGER DOOR UNLOCK: 07-AE-10-04-04-00-00-00
LOCK ALL: 07-AE-10-02-02-00-00-00

LEFT REAR WINDOW DOWN: 07-AE-17-22-22-00-00-00
LEFT REAR WINDOW UP: 07-AE-17-24-24-00-00-00
RIGHT REAR WIDNOW DOWN: 07-AE-17-09-09-00-00-00
RIGHT REAR WIDNOW UP: 07-AE-17-11-11-00-00-00
         
         */

        // CAN Commands
        private static String LOW_BEAMS_ON =    "07AE124040000000";
        private static String LOW_BEAMS_OFF =   "07AE124000000000";
        private static String HIGH_BEAMS_ON =   "07AE122020000000";
        private static String HIGH_BEAMS_OFF =  "07AE122000000000";

        private static String HORN_ON =         "07AE1A0200FF0000";
        private static String HORN_OFF =        "07AE1A0200000000";

        private static String BACKUP_ON =       "07AE120101000000";
        private static String BACKUP_OFF =          "07AE120100000000";
        private static String RIGHT_REAR_TURN_ON =  "07AE138080000000";
        private static String RIGHT_REAR_TURN_OFF = "07AE138000000000";
        private static String LEFT_REAR_TURN_ON =   "07AE134040000000";
        private static String LEFY_REAR_TURN_OFF =  "07AE134000000000";
        
        private static String TESTER_PRESENT =      "013E000000000000";
        private static String RELEASE_CONTROL =     "02AE000000000000";

        // Durations
        private static double headlightDuration_ms = 500;
        private static double hornDuration_ms = 500;
        private static double testerPresentDelay_ms = 2000;
        private static double settleDelay_ms = 10;

        private static DateTime sendHL = DateTime.Now;
        private static DateTime sendHorn = DateTime.Now;
        private static DateTime sendTesterPresent = DateTime.Now;

        // Alarm Thread
        Thread alarmThread = new Thread(startAlarm);
        private static bool alarmActive = false;
        private static bool leftHeadlightOn = false;
        private static bool rightHeadlightOn = false;
        private static bool headlightsOn = false;
        private static bool hornOn = false;

        public static bool connected = false;
        public static bool threadTerminated = true;

        public bool Connected
        {
            get { return connected; }
        }

        private static Serial sp;
        int buadRate = 115200;


        public ChevyCaprice()
        {

        }
        public bool isConnected()
        {
            return connected;
        }

        public bool activateAlarms()
        {
            if (connected && threadTerminated)
            {
                alarmThread = new Thread(startAlarm);
                alarmThread.Start();
                threadTerminated = false;
                return true;
            }
            else
            {
                if (!connected)
                    Console.WriteLine("Com Port Not Open - Not Starting Alarm Thread");
                else 
                    Console.WriteLine("Alarm already active - Not Starting Alarm Thread");
                return false;
            }
        }

        public void deactivateAlarms()
        {
            if (alarmActive)
            {
                alarmActive = false;
                alarmThread.Join();
            }
            else
            {
                Console.WriteLine("Alarm is not active");
            }
        }


        public void Connect(string port)
        {
            Console.WriteLine("Caprice Connect......................................................................................................................................");
            if (!connected)
            {
                sp = new Serial();
                bool opened = sp.Open(port, buadRate);
                if (opened)
                {
                    Console.WriteLine("Connected to VITAL Initializing. Port:" + port + " Baud:" + buadRate);
                    connected = true;
                    initializeAdapter();
                }
                else
                {
                    Console.WriteLine("Could not open port " + port + " " + buadRate);
                }
            }
            else Console.WriteLine("Port Already Open " + port + " " + buadRate);
        }
        public void Disconnect()
        {
            Console.WriteLine("Caprice Disconnect........................................................................................................................");
            if (alarmActive)
            {
                // Deactivate Alarms
                alarmActive = false;
            }
            if (connected)
            {
                // Close Serial Port
                Console.WriteLine("Caprice Closing Serial Port ........................................................................................................................");
                sp.Close();
                connected = false;
            }
        }
        private static void startAlarm(object obj)
        {
            alarmActive = true;
            while (alarmActive)
            {
                // Check Headlights
                if (sendHL.CompareTo(DateTime.Now) <= 0)
                {
                    if (leftHeadlightOn)
                    {
                        Console.WriteLine(DateTime.Now.ToString(timeFormat) + " Sending Right Headlight On Command");
                        sp.sendMsg(HIGH_BEAMS_ON);
                        sp.sendMsg(BACKUP_ON);
                        leftHeadlightOn = false;
                        rightHeadlightOn = true;
                    }
                    else
                    {
                        Console.WriteLine(DateTime.Now.ToString(timeFormat) + " Sending Left Headlight On Command");
                        sp.sendMsg(HIGH_BEAMS_OFF);
                        sp.sendMsg(BACKUP_OFF);
                        leftHeadlightOn = true;
                        rightHeadlightOn = false;
                    }
                    sendHL = DateTime.Now.AddMilliseconds(headlightDuration_ms);
                }
                
                if (alarmActive)
                {
                    if (sendHorn.CompareTo(DateTime.Now) <= 0)
                    {
                        if (hornOn)
                        {
                            Console.WriteLine(DateTime.Now.ToString(timeFormat) + " Sending Horn Off Command");
                            sp.sendMsg(HORN_OFF);
                            hornOn = false;
                        }
                        else
                        {
                            Console.WriteLine(DateTime.Now.ToString(timeFormat) + " Sending Horn On Command");
                            sp.sendMsg(HORN_ON);
                            hornOn = true;
                        }
                        sendHorn = DateTime.Now.AddMilliseconds(hornDuration_ms);
                    }
                }
                
                if (alarmActive)
                {
                    if (sendTesterPresent.CompareTo(DateTime.Now) <= 0)
                    {
                        Console.WriteLine(DateTime.Now.ToString(timeFormat) + " Sending Tester Present Command");
                        sp.sendMsg(TESTER_PRESENT);
                        sendTesterPresent = DateTime.Now.AddMilliseconds(testerPresentDelay_ms);
                    }
                }

                Thread.Sleep(10);

            }

            // Turn off horn and lights, release control and do not send tester present
            Console.WriteLine(DateTime.Now.ToString(timeFormat) + " Exiting Alarm Thread");
            if (leftHeadlightOn || rightHeadlightOn || headlightsOn)
            {
                Console.WriteLine(DateTime.Now.ToString(timeFormat) + " Sending Headlights Off Command");
                sp.sendMsg(HIGH_BEAMS_OFF);
                sp.sendMsg(BACKUP_OFF);
                leftHeadlightOn = false;
                rightHeadlightOn = false;
                headlightsOn = false;
            }
            if (hornOn)
            {
                Console.WriteLine(DateTime.Now.ToString(timeFormat) + " Sending Horn Off Command");
                sp.sendMsg(HORN_OFF);
                hornOn = false;
            }
            Console.WriteLine(DateTime.Now.ToString(timeFormat) + " Releasing Control");
            sp.sendMsg(RELEASE_CONTROL);
            threadTerminated = true;

        }

        private void initializeAdapter()
        {
            string command;
            string response;

            // Reset Device
            command = "ATZ";
            response = sp.sendMsg(command);
            Console.WriteLine(DateTime.Now.ToString(timeFormat) + "\rCommand:" + command + "\rResponse:" + response);
            // Warm Start
            command = "ATWS";
            response = sp.sendMsg(command);
            Console.WriteLine(DateTime.Now.ToString(timeFormat) + "\rCommand:" + command + "\rResponse:" + response);
            // Baud Rate Switch timeout 1
            command = "STBRT1";
            response = sp.sendMsg(command);
            Console.WriteLine(DateTime.Now.ToString(timeFormat) + "\rCommand:" + command + "\rResponse:" + response);
            // Baud Rate 115200
            command = "STBR115200";
            response = sp.sendMsg(command);
            Console.WriteLine(DateTime.Now.ToString(timeFormat) + "\rCommand:" + command + "\rResponse:" + response);
            // Print Spaces Off
            command = "ATS0";
            response = sp.sendMsg(command);
            Console.WriteLine(DateTime.Now.ToString(timeFormat) + "\rCommand:" + command + "\rResponse:" + response);
            // Automatic Formating
            command = "ATCAF0";
            response = sp.sendMsg(command);
            Console.WriteLine(DateTime.Now.ToString(timeFormat) + "\rCommand:" + command + "\rResponse:" + response);
            // Clear All CAN filters
            command = "STFCA";
            response = sp.sendMsg(command);
            Console.WriteLine(DateTime.Now.ToString(timeFormat) + "\rCommand:" + command + "\rResponse:" + response);
            // Pass Filter 641 - Mask 7FF
            command = "STFAP 641,7FF";
            response = sp.sendMsg(command);
            Console.WriteLine(DateTime.Now.ToString(timeFormat) + "\rCommand:" + command + "\rResponse:" + response);
            // Linefeeds Off
            command = "ATL0";
            response = sp.sendMsg(command);
            Console.WriteLine(DateTime.Now.ToString(timeFormat) + "\rCommand:" + command + "\rResponse:" + response);

            command = "ATSH 241";
            response = sp.sendMsg(command);
            Console.WriteLine(DateTime.Now.ToString(timeFormat) + "\rCommand:" + command + "\rResponse:" + response);
            // Current Protocol
            command = "ATDP";
            response = sp.sendMsg(command);
            Console.WriteLine(DateTime.Now.ToString(timeFormat) + "\rCommand:" + command + "\rResponse:" + response);

        }

    }
}
