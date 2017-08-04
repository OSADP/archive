using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace NTRIP
{
    public class NTRIPConnection
    {

        public static string DoSocketGet(string IP, int conPort, string username, string password)
        {
            //Set up variables and String to write to the server.
            Encoding ASCII = Encoding.ASCII;
            string Get = "GET /FLEN0 HTTP/1.1\r\nHost: " + IP +
                         "\r\nConnection: Close\r\n\r\n";
            Byte[] ByteGet = ASCII.GetBytes(Get);
            Byte[] RecvBytes = new Byte[256];
            String strRetPage = null;


            // IPAddress and IPEndPoint represent the endpoint that will 
            //   receive the request. 
            // Get first IPAddress in list return by DNS. 


            try
            {


                // Define those variables to be evaluated in the next for loop and  
                // then used to connect to the server. These variables are defined 
                // outside the for loop to make them accessible there after.
                Socket s = null;
                IPEndPoint hostEndPoint;
                IPAddress hostAddress = IPAddress.Parse(IP);
                hostEndPoint = new IPEndPoint(hostAddress, conPort);


                // Creates the Socket to send data over a TCP connection.
                s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);



                // Connect to the host using its IPEndPoint.
                s.Connect(hostEndPoint);

                if (!s.Connected)
                {
                    // Connection failed, try next IPaddress.
                    strRetPage = "Unable to connect to host";
                    s = null;
                }


                string auth = ToBase64(username + ":" + password); 
                string msg = "GET /FLEN0 HTTP/1.1\r\n"; 
                msg += "User-Agent: NTRIP iter.dk\r\n"; 
                msg += "Authorization: Basic " + auth + "\r\n"; 
                msg += "Accept: */*\r\nConnection: close\r\n"; 
                msg += "\r\n";

                byte[] data = System.Text.Encoding.ASCII.GetBytes(msg); 
                s.Send(data);

                // Receive the host home page content and loop until all the data is received.
                Int32 bytes = s.Receive(RecvBytes, RecvBytes.Length, 0);
                strRetPage = "Default HTML page on " + IP + ":\r\n";
                strRetPage = strRetPage + ASCII.GetString(RecvBytes, 0, bytes);

                while (bytes > 0)
                {
                    bytes = s.Receive(RecvBytes, RecvBytes.Length, 0);
                    strRetPage = strRetPage + ASCII.GetString(RecvBytes, 0, bytes);
                }  

                s.Shutdown(SocketShutdown.Both); 
                s.Close(); 



            } // End of the try block. 

            catch (SocketException e)
            {
                Console.WriteLine("SocketException caught!!!");
                Console.WriteLine("Source : " + e.Source);
                Console.WriteLine("Message : " + e.Message);
            }
            catch (ArgumentNullException e)
            {
                Console.WriteLine("ArgumentNullException caught!!!");
                Console.WriteLine("Source : " + e.Source);
                Console.WriteLine("Message : " + e.Message);
            }
            catch (NullReferenceException e)
            {
                Console.WriteLine("NullReferenceException caught!!!");
                Console.WriteLine("Source : " + e.Source);
                Console.WriteLine("Message : " + e.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception caught!!!");
                Console.WriteLine("Source : " + e.Source);
                Console.WriteLine("Message : " + e.Message);
            }

            return strRetPage;
        }

        private static string ToBase64(string str)
        {
            Encoding asciiEncoding = Encoding.ASCII;
            byte[] byteArray = new byte[asciiEncoding.GetByteCount(str)];
            byteArray = asciiEncoding.GetBytes(str);
            return Convert.ToBase64String(byteArray, 0, byteArray.Length);
        } 
    }
}
