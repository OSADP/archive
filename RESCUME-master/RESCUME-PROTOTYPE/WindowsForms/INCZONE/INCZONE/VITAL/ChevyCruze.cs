using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace INCZONE.VITAL
{
    class ChevyCruze : vitalVehicleInterface
    {

        private static String timeFormat = "HH:mm:ss.fff ";

        // CAN Commands
        private static String LEFT_HEADLIGHT_ON = "07AE06017FFF0000";
        private static String RIGHT_HEADLIGHT_ON = "07AE060200007FFF";
        private static String BOTH_HEADLIGHTS_ON = "07AE06037FFF7FFF";
        private static String BOTH_HEADLIGHTS_OFF = "07AE060300000000";

        private static String TURN_SIGNALS_ON = "07AE02F0F0000000";
        private static String TURN_SIGNALS_OFF = "07AE02F000000000";
        private static String BRAKE_ON = "07AE1A03FFFFFFFF";
        private static String BRAKE_OFF = "07AE1A0300000000";

        
        private static String HORN_ON = "07AE100303000000";
        private static String HORN_OFF = "07AE100300000000";
        private static String TESTER_PRESENT = "013E000000000000";
        private static String RELEASE_CONTROL = "02AE000000000000";

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


        public ChevyCruze()
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
            if (!connected)
            {
                sp = new Serial();
                bool opened = sp.Open(port, buadRate);
                if (opened)
                {
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
            if (alarmActive)
            {
                // Deactivate Alarms
                alarmActive = false;
            }
            if (connected)
            {
                // Close Serial Port
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
                        sp.sendMsg(RIGHT_HEADLIGHT_ON);
//                        sp.sendMsg(TURN_SIGNALS_ON);
                        sp.sendMsg(BRAKE_ON);
                        leftHeadlightOn = false;
                        rightHeadlightOn = true;
                    }
                    else
                    {
                        Console.WriteLine(DateTime.Now.ToString(timeFormat) + " Sending Left Headlight On Command");
                        sp.sendMsg(LEFT_HEADLIGHT_ON);
//                        sp.sendMsg(TURN_SIGNALS_OFF);
                        sp.sendMsg(BRAKE_OFF);
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
                sp.sendMsg(BOTH_HEADLIGHTS_OFF);
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
