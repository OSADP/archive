using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using INCZONE.Common;
using INCZONE.NMEA;
using log4net;

namespace INCZONE.Managers
{
    class NTripManager : IDisposable
    {
        private static readonly ILog log = LogManager.GetLogger(SystemConstants.Logger_Ref);

        private string _username;
        private string _password;
        private Socket _socket;

        /// <summary>
        /// NTRIP server Username
        /// </summary>
        public string UserName
        {
            get { return _username; }
            set { _username = value; }
        }

        /// <summary>
        /// NTRIP server password
        /// </summary>
        public string Password
        {
            get { return _password; }
            set { _password = value; }
        }

        /// <summary>
        /// NTRIP Server
        /// </summary>
        public IPEndPoint BroadCaster { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="server"></param>
        public NTripManager(IPEndPoint server)
        {
            //Initialization...
            BroadCaster = server;
            //InitializeSocket();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="server"></param>
        /// <param name="strUserName"></param>
        /// <param name="strPassword"></param>
        public NTripManager(IPEndPoint server, string strUserName, string strPassword)
        {
            BroadCaster = server;
            _username = strUserName;
            _password = strPassword;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_DGPSConfig"></param>
        public NTripManager(DGPSConfig _DGPSConfig)
        {
            BroadCaster = new IPEndPoint(IPAddress.Parse(_DGPSConfig.HostIP), Convert.ToInt32(_DGPSConfig.HostPort));
            _username = _DGPSConfig.Username;
            _password = _DGPSConfig.Password;

        }

        /// <summary>
        /// 
        /// </summary>
        private void InitializeSocket()
        {
            //log.Debug("In InitializeSocket");

            try
            {
                _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            }
            catch (Exception ex)
            {
                log.Error("InitializeSocket Exception", ex);
            }            
        }

        /// <summary>
        /// Creates request to NTRIP server
        /// </summary>
        /// <param name="strRequest">Resource to request. Leave blank to get NTRIP service data</param>
        private byte[] CreateRequest(string responderLocation)
        {
            //log.Debug("In CreateRequest");
            string msg = string.Empty;

            try
            {
                string auth = ToBase64(_username + ":" + _password);

                msg = "GET /" + UIConstants.NTRIP_MOUNT_POINT + " HTTP/1.0\r\n";
				msg += "Accept: */*\r\n";
				msg += "Connection: close\r\n";
				msg += "Authorization: Basic " + auth;
                msg += "\r\n\r\n";
            }
            catch (Exception ex)
            {
                log.Error("CreateRequest Exception", ex);
            }

            return Encoding.ASCII.GetBytes(msg);
        }

        /// <summary>
        /// 
        /// </summary>
        private void Connect()
        {
            //log.Debug("In Connect");

            try
            {
                if (!_socket.Connected)
                    _socket.Connect(BroadCaster);
            }
            catch (Exception ex)
            {
                log.Error("Connect Exception", ex);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void Close()
        {
            //log.Debug("In Close");

            try
            {
                _socket.Shutdown(SocketShutdown.Both);
                _socket.Close();
            }
            catch (Exception ex)
            {
                log.Error("Close Exception", ex);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>bool</returns>
        public bool IsDGPSConfiguredCorrectly(IPEndPoint endPoint, string username, string password)
        {
            //log.Debug("In IsDGPSConfiguredCorrectly");

            SourceTable result = new SourceTable();
            Socket testSocket = null;

            try
            {
                try
                {
                    testSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                }
                catch (Exception ex)
                {
                    log.Error("InitializeSocket Exception", ex);
                }
                testSocket.Blocking = true;
                testSocket.Connect(endPoint);
                testSocket.Send(CreateRequest(""));
                string responseData = "";
                System.Threading.Thread.Sleep(1000); //Wait for response
                while (testSocket.Available > 0)
                {
                    byte[] returndata = new byte[testSocket.Available];
                    testSocket.Receive(returndata); //Get response
                    responseData += Encoding.ASCII.GetString(returndata, 0, returndata.Length);
                    System.Threading.Thread.Sleep(100); //Wait for response
                }
                testSocket.Shutdown(SocketShutdown.Both);
                testSocket.Close();

                if (responseData.StartsWith(UIConstants.ICY_200_OK))
                    return true;
            }
            catch (Exception ex)
            {
                log.Error("IsDGPSConfiguredCorrectly Exception", ex);
            }

            return false;
        }

        /// <summary>
        /// Apply AsciiEncoding to user name and password to obtain it as an array of bytes
        /// </summary>
        /// <param name="str">username:password</param>
        /// <returns>Base64 encoded username/password</returns>
        private static string ToBase64(string str)
        {
            //log.Debug("In ToBase64");

            byte[] byteArray = null;

            try
            {
                Encoding asciiEncoding = Encoding.ASCII;
                byteArray = asciiEncoding.GetBytes(str);
                
            } catch(Exception ex)
            {
                log.Error("ToBase64 Exception", ex);
            }

            return Convert.ToBase64String(byteArray, 0, byteArray.Length);
        }

        #region IDisposable Members

        public void Dispose()
        {
            Close();
        }

        #endregion

        public async Task<string> GetDGPS(byte[] request, Socket NTRIPSocket, Coordinate responderLocation)
        {
            string DGPS = await Task.Run(() =>
            {
                string responseData = "";

                if (request != null)
                {
                   // NTRIPSocket.Send(Encoding.ASCII.GetBytes("GET /" + UIConstants.NTRIP_MOUNT_POINT + " HTTP/1.0\r\n"));
                    NTRIPSocket.Send(request);
                    Thread.Sleep(100);
                    while (NTRIPSocket.Available > 0)
                    {
                        byte[] returndata = new byte[NTRIPSocket.Available];
                        NTRIPSocket.Receive(returndata); //Get response
                        responseData += Encoding.ASCII.GetString(returndata, 0, returndata.Length);
                        System.Threading.Thread.Sleep(100); //Wait for response
                    }
                }
                else
                {
                    try
                    {
                        while (NTRIPSocket.Available > 0)
                        {
                            byte[] returndata = new byte[NTRIPSocket.Available];
                            NTRIPSocket.Receive(returndata);
                            responseData = Encoding.ASCII.GetString(returndata, 0, returndata.Length);
                            Thread.Sleep(100); //Wait for response
                        }

                        if (NTRIPSocket.Available <= 0)
                        {
                            string gpgga = GPGGA.GenerateGPGGAcode(responderLocation);
                            NTRIPSocket.Send(Encoding.ASCII.GetBytes(gpgga + "\r\n"));
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Error("Recieve Exception ", ex);
                    }
                }


                return responseData;
            });


            return DGPS;
        }
    }
}
