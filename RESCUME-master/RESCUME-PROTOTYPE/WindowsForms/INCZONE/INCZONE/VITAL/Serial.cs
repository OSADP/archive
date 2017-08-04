using System;
using System.Windows.Forms;

using System.Collections.Generic;
using System.Text;
using System.IO.Ports;
using System.Threading;

namespace INCZONE.VITAL
{
    public class MessageEventArgs : EventArgs
    {
        public string incominMessage;
        public MessageEventArgs(string inString)
        {
            incominMessage = inString;
        }
    }

    public delegate void MessageEvent(object sender, MessageEventArgs e);

    class Serial
    {
        public event MessageEvent messEvent;

        private SerialPort port;
        String comPort = "COM1";
        int buadRate = 19200;

        private static Mutex mut = new Mutex();
        private static string currentMessage = "";
        private static string finishedMessage = "";
        private int timeOut = 10000;
        private int waitPeriod = 0;
        private bool open = false;

        String timeFormat = "HH:mm:ss.fff ";

        public bool isConnected()
        {
            return open;
        }

        // Constructors
        public Serial()
        {
        }

        public bool Open(string port, int rate)
        {
            comPort = port;
            buadRate = rate;
            return Open();
        }

        public bool Open()
        {
            try
            {
                Console.WriteLine("Opening Serial Port " + comPort + "  " + buadRate);
                port = new SerialPort(comPort, buadRate);
                port.Open();
//                port.DataReceived += new SerialDataReceivedEventHandler(port_DataReceived);
                open = true;
            }
            catch (Exception oex)
            {
                Console.Out.WriteLine("Error Opening Port:\r\n" + oex);
                return false;
            }
            return true;
        }


        private void port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            int bytesToRead = port.BytesToRead;
            // Return if no data to read
            if (bytesToRead == 0) return;

            byte[] tempArray = new byte[bytesToRead];
            port.Read(tempArray, 0, bytesToRead);

            string outText = "";
            for (int i = 0; i < tempArray.Length; i++)
            {
                outText += Convert.ToChar(tempArray[i]);
            }
            Console.Out.WriteLine("Async Received:" + outText);
            currentMessage += outText;
            if (currentMessage.Contains("\r"))
            {
                
                String[] split = currentMessage.Split('\r');
                foreach (String msg in split)
                {
                    if (msg.Length > 0)
                    {
                        MessageEventArgs me = new MessageEventArgs(msg);
                        // Signal event that a new message has been received
                        Console.Out.WriteLine("Async Message:" + msg);
                        OnMessage(me);
                        currentMessage = msg;
                    }
                    else currentMessage = "";
                }
            }
        }

            
        public bool Close()
        {
            Console.WriteLine("Closing Port");
            try
            {
                port.Close();
            }
            catch
            {
                return false;
            }
            Console.WriteLine("Port Closed");
            open = false;
            return true;
        }

        public string sendMsg(string message)
        {
            if (!open) return "Error - Port not open\r\n";
            
            mut.WaitOne();
            try
            {
                // Send Message
//                message += "\r\n";
                message += "\r";
                port.DiscardInBuffer();
                port.Write(message);

                DateTime start = DateTime.Now;
                start = start.AddMilliseconds(timeOut);
                // wait for response
                while (start.CompareTo(DateTime.Now) >= 0)
                {
                    Thread.Sleep(waitPeriod);
                    if (checkData())
                    {
                        mut.ReleaseMutex();
                        return finishedMessage;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception in serial send - " + ex.Message);
            }
            mut.ReleaseMutex();
            currentMessage = "";
            return "Error - Time Out\r\n";
        }

        public string sendRawMsg(string message)
        {
            if (!open) return "Error - Port not open\r\n";

            Console.Out.WriteLine("Sending Raw:" + message);
            mut.WaitOne();
            try
            {
                // Send Message
                port.DiscardInBuffer();
                port.Write(message);

            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception in serial send - " + ex.Message);
            }
            mut.ReleaseMutex();
            return "Complete\r\n";
        }


        private bool checkData() 
        {
            string thisRead = "";
            try
            {
                if (port.BytesToRead > 0)
                {
                    thisRead = port.ReadExisting();
                    if (thisRead.Contains(">"))
                       {
                        currentMessage += thisRead;
                        finishedMessage = currentMessage;
                        currentMessage = "";
                        Console.WriteLine("Returned Data - " + finishedMessage);
                        return true;
                    }
                    else
                    {
                        currentMessage += thisRead;
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("CheckData error - " + ex.Message);
            }
            return false;
        }

        public void OnMessage(MessageEventArgs e)
        {
            if (messEvent != null)
            {
                messEvent(this, e);
            }
        }
    }
}
