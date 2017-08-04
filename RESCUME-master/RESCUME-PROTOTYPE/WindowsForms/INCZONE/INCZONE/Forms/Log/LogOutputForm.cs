using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using INCZONE.Common;
using INCZONE.Managers;
using InTheHand.Net;
using InTheHand.Net.Sockets;
using log4net;

namespace INCZONE.Forms.Log
{
    public partial class LogOutputForm : Form
    {
        private static readonly ILog log = LogManager.GetLogger(SystemConstants.Logger_Ref);
        private NetworkStream peer;

        public LogOutputForm(IncZoneMDIParent form)
        {
            this.MdiParent = form;
            InitializeComponent();
            textBox1.Text = "847aa758-a9b5-460a-8656-c115f23a2858";
            form.RequestDIAChange += RequestDIAChange;
        }

        private void RequestDIAChange(string newText)
        {
            DIATb.Text = "";
            DIATb.Text = newText;
        }

        private void button1_Click(object sender, EventArgs e)
        {

            /*
             * TODO:
             * -Speed should use a different frame alert type (speed not advisory)
             * -Need to add elevation to ShapePointSet ValidRegion's AnchorPoint.  Need the J2735 standards to implement, and I'm not in the lab right now.
             */

            List<DsrcTkDataFrame> frames = new List<DsrcTkDataFrame>();

            {
                /*Building first frame... Incident Zone
                 *
                 *Each node takes arguments: 
                 *      new DsrcTkAbsoluteNode(Latitude, Longitude, *Elevation in Meters, *Lane Width in Meters);
                 *      
                 * Note that Elevation and Lane Width is in meters.  With no elevation it won't build it into the TIM's.  Without lane width it inserts a default of 3.5 meters.
                 * 
                 */
                List<DsrcTkAbsoluteNode> region1 = new List<DsrcTkAbsoluteNode>();
                region1.Add(new DsrcTkAbsoluteNode(40.40785938372655, -83.07382527751572));
                region1.Add(new DsrcTkAbsoluteNode(40.40835848714723, -83.07382979579883));
                DsrcTkShapePointSet shapePointRegion1 = new DsrcTkShapePointSet(region1.ToArray(), 0);

                List<DsrcTkAbsoluteNode> region2 = new List<DsrcTkAbsoluteNode>();
                region2.Add(new DsrcTkAbsoluteNode(40.40774138277119, -83.07378078022363));
                region2.Add(new DsrcTkAbsoluteNode(40.40835440558949, -83.07378321224917));
                DsrcTkShapePointSet shapePointRegion2 = new DsrcTkShapePointSet(region2.ToArray(), 0);

                DsrcTkDataFrame frame = new DsrcTkDataFrame(new DsrcTkDataFrameOptions()
                    .setStartTime(DateTime.UtcNow)
                    .setDuration(3200)
                    .setPriority(7)
                    .addItisCode(531)
                    .setShapePointRegions(new DsrcTkShapePointSet[] { shapePointRegion1, shapePointRegion2 }));

                frames.Add(frame);
            }

            {
                //Building second frame... Closed to traffic Zone
                List<DsrcTkAbsoluteNode> region1 = new List<DsrcTkAbsoluteNode>();
                region1.Add(new DsrcTkAbsoluteNode(40.40145756846896, -83.0737567280343));
                region1.Add(new DsrcTkAbsoluteNode(40.40269882438896, -83.07377367318519));
                region1.Add(new DsrcTkAbsoluteNode(40.40418632851885, -83.07378557377811));
                region1.Add(new DsrcTkAbsoluteNode(40.40618005761478, -83.07381195126868));
                region1.Add(new DsrcTkAbsoluteNode(40.40729318976822, -83.07381778412292));
                region1.Add(new DsrcTkAbsoluteNode(40.40835579847381, -83.0738294934288));
                DsrcTkShapePointSet shapePointRegion1 = new DsrcTkShapePointSet(region1.ToArray(), 0);

                List<DsrcTkAbsoluteNode> region2 = new List<DsrcTkAbsoluteNode>();
                region2.Add(new DsrcTkAbsoluteNode(40.40145969496375, -83.07371182134263));
                region2.Add(new DsrcTkAbsoluteNode(40.40269765855564, -83.07372189401404));
                region2.Add(new DsrcTkAbsoluteNode(40.40418603136077, -83.07373586923599));
                region2.Add(new DsrcTkAbsoluteNode(40.40618301646926, -83.07376151017525));
                region2.Add(new DsrcTkAbsoluteNode(40.40729735658523, -83.07377413826308));
                region2.Add(new DsrcTkAbsoluteNode(40.40835460775712, -83.07378549490464));
                DsrcTkShapePointSet shapePointRegion2 = new DsrcTkShapePointSet(region2.ToArray(), 0);

                DsrcTkDataFrame frame = new DsrcTkDataFrame(new DsrcTkDataFrameOptions()
                    .setStartTime(DateTime.UtcNow)
                    .setDuration(3200)
                    .setPriority(7)
                    .addItisCode(769)
                    .setShapePointRegions(new DsrcTkShapePointSet[] { shapePointRegion1, shapePointRegion2 }));

                frames.Add(frame);
            }

            {
                //Building third frame... Restricted speed Zone
                List<DsrcTkAbsoluteNode> region1 = new List<DsrcTkAbsoluteNode>();
                region1.Add(new DsrcTkAbsoluteNode(40.40146112293053, -83.073800023415));
                region1.Add(new DsrcTkAbsoluteNode(40.40249771391922, -83.07380646975433));
                region1.Add(new DsrcTkAbsoluteNode(40.4035627894772, -83.07382193117491));
                region1.Add(new DsrcTkAbsoluteNode(40.40523612936645, -83.07384309409638));
                region1.Add(new DsrcTkAbsoluteNode(40.4066418247807, -83.07386117598256));
                region1.Add(new DsrcTkAbsoluteNode(40.40836025276567, -83.07388022815114));

                DsrcTkShapePointSet shapePointRegion1 = new DsrcTkShapePointSet(region1.ToArray(), 0);

                DsrcTkDataFrame frame = new DsrcTkDataFrame(new DsrcTkDataFrameOptions()
                    .setStartTime(DateTime.UtcNow)
                    .setDuration(3200)
                    .setPriority(7)
                    .addItisCode(6933)
                    .setShapePointRegions(new DsrcTkShapePointSet[] { shapePointRegion1 }));

                frames.Add(frame);
            }

            DsrcTkTIM tim = new DsrcTkTIM(new DsrcTkTIMOptions()
                .setFrames(frames.ToArray()));

            var encodedTim = tim.generateAsn();
            string str = string.Concat(encodedTim.Select(b => b.ToString("X2")).ToArray());

        }

        private async void button2_Click(object sender, EventArgs e)
        {
            try
            {
                CapWINConfig _CapWinConfig = ((IncZoneMDIParent)this.MdiParent)._CapWinConfig;
                Coordinate _ResponderLocation = IncZoneMDIParent._ResponderLocationOverRide;
                Guid _SelectedMapSet = IncZoneMDIParent._SelectedMapSet;

                CapWINManager CapWINManager = new CapWINManager(_CapWinConfig);
                MessageManager mm = new MessageManager((IncZoneMDIParent)this.MdiParent);

                CapWINIncidentListType1 CapWINIncidentListType = await CapWINManager.GetCapWINIncidentsList();
                string TIMBits = await mm.CreateTIMMessage(_ResponderLocation, CapWINIncidentListType, _CapWinConfig.DistanceToIncident, _CapWinConfig.LaneData, IncZoneMDIParent.MapNodeList, IncZoneMDIParent.MapLinkList);
                byte[] timmy = MessageManager.CreatOutgoingMessage(TypeId.TIM, TIMBits);

                if (peer == null)
                {
                    peer = BluetoothConnect();
                }
                peer.Flush();
                await peer.WriteAsync(timmy, 0, timmy.Length);

                byte[] outbuf = { };
                byte[] temp = new byte[4000];

                if (peer.DataAvailable)
                {
                    int x = await peer.ReadAsync(temp, 0, temp.Length);
                    //IAsyncResult result = peer.BeginRead(temp, 0, temp.Length, new AsyncCallback(_CallBack), null);

                    outbuf = new byte[x];
                    Array.Copy(temp, outbuf, x);

                    string str = Encoding.ASCII.GetString(outbuf);
                    Console.WriteLine("Bing " + str);
                }
            }
            catch (IOException ex)
            {
                log.Error("", ex);
                if (peer != null)
                    peer.Close();
            }
            catch (Exception ex)
            {
                log.Error("",ex);
            }
        }

        private void _CallBack(IAsyncResult ar)
        {
            //NetworkStream myNetworkStream = (NetworkStream)ar.AsyncState;
            byte[] myReadBuffer = new byte[4000];
            String myCompleteMessage = "";
            int numberOfBytesRead;

            numberOfBytesRead = peer.EndRead(ar);
            myCompleteMessage =
                String.Concat(myCompleteMessage, Encoding.ASCII.GetString(myReadBuffer, 0, numberOfBytesRead));

            // message received may be larger than buffer size so loop through until you have it all.
            while (peer.DataAvailable)
            {

                peer.BeginRead(myReadBuffer, 0, myReadBuffer.Length,
                                                       new AsyncCallback(_CallBack),
                                                           peer);

            }

            // Print out the received message to the console.
            Console.WriteLine("You received the following message : " +
                                        myCompleteMessage);
        }


        public NetworkStream BluetoothConnect()
        {
            var cli = new BluetoothClient();
            NetworkStream peer = null;

            try
            {
                cli.Connect(((IncZoneMDIParent)this.MdiParent)._AradaAddress, IncZoneMDIParent.uid);
                peer = cli.GetStream();
            }
            catch (SocketException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return peer;
        }

        private void LogOutputForm_Load(object sender, EventArgs e)
        {

        }
    }
}
