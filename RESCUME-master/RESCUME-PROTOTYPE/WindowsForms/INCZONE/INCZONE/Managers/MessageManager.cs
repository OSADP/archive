using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Xml;
using System.Xml.Serialization;
using INCZONE.Common;
using INCZONE.Managers;
using INCZONE.Model;
using INCZONE.Repositories;
using log4net;
using Newtonsoft.Json;
using INCZONE.Exceptions;

namespace INCZONE.Managers
{
    public class MessageManager
    {
        readonly IUnitOfWork _uow;
        private static readonly ILog log = LogManager.GetLogger(SystemConstants.Logger_Ref);
        IncZoneMDIParent parentForm;

        public MessageManager(IncZoneMDIParent form)
        {
            this.parentForm = form;
            string connectionString = ConfigurationManager.ConnectionStrings["IncZoneEntities"].ConnectionString;
            this._uow = new UnitOfWork(connectionString);
        }

        public static CapWINIncidentListType1 CapWINXMLDigester(XmlDocument capWINIncident)
        {
            //log.Debug("In CapWINXMLDigester");

            XmlSerializer serializer = new XmlSerializer(typeof(CapWINIncidentListType1));
            XmlReader xmlReader = new XmlNodeReader(capWINIncident);
            CapWINIncidentListType1 capWINList = null;

            if (serializer.CanDeserialize(xmlReader))
            {
                capWINList = (CapWINIncidentListType1)serializer.Deserialize(xmlReader);
            }
            return capWINList;
        }

        public void messageHandler() { }

        public async Task<string> CreateTIMMessage(Coordinate ResponderLocation, CapWINIncidentListType1 CapWINIncidents, int DistanceToIncident, int AmountOfLaneData, List<INCZONE.Common.MapNode> MapNodeList, List<INCZONE.Common.MapLink> MapLinkList)
        {
            //log.Debug("In CapWINXMLDigester");

            Segment Segment = null;
            List<INCZONE.Common.MapNode> nodesList = new List<Common.MapNode>();
            IncidentZone inczone = new IncidentZone();


            if (ResponderLocation == null)
                return null;
            try
            {
                string TIM = await Task.Run<string>(() =>
                {
                    string str = string.Empty;

                    try
                    {
                        Segment = CapWINManager.GetCapWINIncidents(CapWINIncidents, ResponderLocation, DistanceToIncident);
                        List<DsrcTkDataFrame> frames = new List<DsrcTkDataFrame>();
                        IncidentZone IncidentZone = null;

                        if (Segment != null)
                        {
                            IncidentZone = new IncidentZone();
                            int LaneNumber = 0;

                            IncidentZone.IncidentTime = (dateTime)Segment.TimeofIncident.Items[0];
                            //Incident Zone
                            foreach (LaneType lane in Segment.SegmentType.Items)
                            {
                                Lane Lane = new Lane(lane, LaneNumber);
                                parentForm.IncidentName = Segment.IncidentName;

                                var nodes = MapNodeList.Where(m => m.LaneDirection.ToLower().Equals(lane.direction.ToLower())).Where(m => m.laneOrder == LaneNumber).OrderBy(m => new Coordinate(m.longitude, m.latitude).Distance(ResponderLocation)).ToList();
                                var topTwoNodes = nodes.Take(2).ToList();
                                //var link = MapLinkList.Where(l => topTwoNodes.Any(n => n.Id.Equals(l.startMapNodeId)) && topTwoNodes.Any(n => n.Id.Equals(l.endMapNodeId))).FirstOrDefault();

                                var link = MapLinkList.Where(l => nodes.Any(n => n.Id.Equals(l.startMapNodeId))).OrderBy(l =>
                                    {
                                        var node1 = nodes.Find(n => n.Id.Equals(l.startMapNodeId));
                                        var node2 = nodes.Find(n => n.Id.Equals(l.endMapNodeId));
                                        if (node1 == null || node2 == null)
                                            return 99999999999.9;
                                        return new Coordinate(node1.longitude, node1.latitude).Distance(ResponderLocation) + new Coordinate(node2.longitude, node2.latitude).Distance(ResponderLocation);
                                    }).FirstOrDefault();

                                var currentNode = nodes.FirstOrDefault();
                                
                                if (link != null)
                                {
                                    currentNode = nodes.Find(n => n.Id.Equals(link.endMapNodeId));
                                }

                                if (currentNode == null)
                                {
                                    throw new Exception("Unable to find map node nearest to responder vehicle.");
                                }

                                IncidentZone.PostSpeed = currentNode.postedSpeed;

                                double totalDistance = 0;
                                Coordinate previousCoordinate = null;
                                List<INCZONE.Common.MapNode> laneNodes = new List<Common.MapNode>();

                                while (currentNode != null && totalDistance < UIConstants.MAX_LANE_DATA)
                                {
                                    laneNodes.Add(currentNode);

                                    Coordinate currentCoordinate = new Coordinate(currentNode.longitude, currentNode.latitude);
                                    if (previousCoordinate != null)
                                    {
                                        totalDistance += previousCoordinate.Distance(currentCoordinate);
                                    }
                                    previousCoordinate = currentCoordinate;

                                    var linkToPreviousNode = MapLinkList.Where(l => l.endMapNodeId.Equals(currentNode.Id)).FirstOrDefault();
                                    if (linkToPreviousNode == null || linkToPreviousNode.startMapNodeId == null)
                                        break;

                                    currentNode = nodes.Where(n => n.Id == linkToPreviousNode.startMapNodeId).FirstOrDefault();
                                }

                                laneNodes.Reverse();
                                Lane.MapNodes.AddRange(laneNodes);
                                IncidentZone.Lanes.Add(Lane);
                                LaneNumber++;
                            }


                            frames.Add(SetDsrcTkDataFrameAll(ResponderLocation, IncidentZone, AmountOfLaneData));
                            frames.Add(SetDsrcTkDataFrameClosed(IncidentZone));
                            frames.Add(SetDsrcTkDataFrameSpeed(IncidentZone));

                            DsrcTkTIM tim = new DsrcTkTIM(new DsrcTkTIMOptions()
                                .setFrames(frames.ToArray()));

                            var encodedTim = tim.generateAsn();
                            str = string.Concat(encodedTim.Select(b => b.ToString("X2")).ToArray());
                        }
                    }
                    catch (Exception ex)
                    {
                        parentForm._StopIncZone(true);
                        log.Error("Node and DSRC Create Exception", ex);
                    }

                    if (string.IsNullOrEmpty(str))
                    {
                        str = null;
                    }

                    return str;
                }
                );
                return TIM;
            }
            catch (Exception ex)
            {
                log.Error("CreateTIMMessage Exception", ex);

            }

            return null;
        }

        private DsrcTkDataFrame SetDsrcTkDataFrameSpeed(IncidentZone IncidentZone)
        {
            DsrcTkDataFrame frame = null;
            int NumberOfShapePointRegions = 0;
            int openLane = 0;

            var lanes = IncidentZone.Lanes.Where(l => l.LanyType.status.Equals("open")).Where(l =>
            {
                foreach (Lane lc in IncidentZone.Lanes)
                {
                    if (lc.LanyType.status.Equals("closed") || lc.LanyType.status.Equals("blocked"))
                    {
                        if (Math.Abs(lc.LaneNumber - l.LaneNumber) == 1)
                            return true;
                    }
                }
                return false;
            });

            try
            {
                foreach (Lane Lane in lanes)
                {
                    if (Lane.LanyType.status == "open")
                    {
                        NumberOfShapePointRegions++;
                    }
                }

                DsrcTkShapePointSet[] ShapePointRegions = new DsrcTkShapePointSet[NumberOfShapePointRegions];

                openLane = 0;
                foreach (Lane Lane in lanes)
                {
                    if (Lane.LanyType.status == "open")
                    {
                        List<DsrcTkAbsoluteNode> regions = new List<DsrcTkAbsoluteNode>();

                        foreach (INCZONE.Common.MapNode mapNode in Lane.MapNodes)
                        {
                            regions.Add(new DsrcTkAbsoluteNode(mapNode.latitude, mapNode.longitude,870));
                        }

                        ShapePointRegions[openLane] = new DsrcTkShapePointSet(regions.ToArray(), 0);
                        openLane++;
                    }
                }

                frame = new DsrcTkDataFrame(new DsrcTkDataFrameOptions()
                                        .setStartTime(IncidentZone.IncidentTime.Value)
                                        .setDuration(3200)
                                        .setPriority(7)
                                        .addItisCode(6933)
                                        .addItisText(String.Format("mph:{0}", IncidentZone.PostSpeed))
                                        .setShapePointRegions(ShapePointRegions));
            }
            catch (Exception ex)
            {
                log.Debug("SetDsrcTkDataFrameAll exception ", ex);

            }
            return frame;
        }

        private DsrcTkDataFrame SetDsrcTkDataFrameClosed(IncidentZone IncidentZone)
        {
            DsrcTkDataFrame frame = null;
            int NumberOfShapePointRegions = 0;

            try
            {
                foreach (Lane Lane in IncidentZone.Lanes)
                {
                    if (Lane.LanyType.status == "closed" || Lane.LanyType.status == "blocked")
                    {
                        NumberOfShapePointRegions++;
                    }
                }

                DsrcTkShapePointSet[] ShapePointRegions = new DsrcTkShapePointSet[NumberOfShapePointRegions];
                NumberOfShapePointRegions = 0;
                foreach (Lane Lane in IncidentZone.Lanes)
                {
                    if (Lane.LanyType.status == "closed" || Lane.LanyType.status == "blocked")
                    {
                        List<DsrcTkAbsoluteNode> region = new List<DsrcTkAbsoluteNode>();

                        foreach (INCZONE.Common.MapNode mapNode in Lane.MapNodes)
                        {
                            region.Add(new DsrcTkAbsoluteNode(mapNode.latitude, mapNode.longitude, 870));
                        }

                        ShapePointRegions[NumberOfShapePointRegions] = new DsrcTkShapePointSet(region.ToArray(), 0);
                        NumberOfShapePointRegions++;
                    }
                }

                frame = new DsrcTkDataFrame(new DsrcTkDataFrameOptions()
                                        .setStartTime(IncidentZone.IncidentTime.Value)
                                        .setDuration(3200)
                                        .setPriority(7)
                                        .addItisCode(769)
                                        .setShapePointRegions(ShapePointRegions));
            }
            catch (Exception ex)
            {
                log.Debug("SetDsrcTkDataFrameAll exception ", ex);

            }
            return frame;
        }

        private DsrcTkDataFrame SetDsrcTkDataFrameAll(Coordinate ResponderLocation, IncidentZone IncidentZone, int AmountOfLaneData)
        {
            DsrcTkDataFrame frame = null;

            int NumberOfShapePointRegions = 0;

            try
            {
                foreach (Lane Lane in IncidentZone.Lanes)
                {
                    if (Lane.LanyType.status == "closed" || Lane.LanyType.status == "blocked")
                    {
                        NumberOfShapePointRegions++;
                    }
                }

                DsrcTkShapePointSet[] ShapePointRegions = new DsrcTkShapePointSet[NumberOfShapePointRegions];

                NumberOfShapePointRegions = 0;
                foreach (Lane Lane in IncidentZone.Lanes)
                {
                    if (Lane.LanyType.status == "closed" || Lane.LanyType.status == "blocked")
                    {
                        List<DsrcTkAbsoluteNode> regions = new List<DsrcTkAbsoluteNode>();
                        List<INCZONE.Common.MapNode> EmaergancyVehicleZoneList = GetEmaergancyVehicleZone(ResponderLocation, Lane.MapNodes, AmountOfLaneData);
                        foreach (INCZONE.Common.MapNode mapNode in EmaergancyVehicleZoneList)
                        {
                            regions.Add(new DsrcTkAbsoluteNode(mapNode.latitude, mapNode.longitude,870));
                        }

                        ShapePointRegions[NumberOfShapePointRegions] = new DsrcTkShapePointSet(regions.ToArray(), 0);
                        NumberOfShapePointRegions++;
                    }
                }

                frame = new DsrcTkDataFrame(new DsrcTkDataFrameOptions()
                                        .setStartTime(IncidentZone.IncidentTime.Value)
                                        .setDuration(3200)
                                        .setPriority(7)
                                        .addItisCode(531)
                                        .setShapePointRegions(ShapePointRegions));
            }
            catch (Exception ex)
            {
                log.Debug("SetDsrcTkDataFrameAll exception ", ex);

            }
            return frame;
        }

        private List<Common.MapNode> GetEmaergancyVehicleZone(Coordinate ResponderLocation, List<Common.MapNode> nodesList, int AmountOfLaneData)
        {
            List<Common.MapNode> nodesListCopy = new List<Common.MapNode>(nodesList);
            nodesListCopy.Reverse();
            List<Common.MapNode> returnList = new List<Common.MapNode>();

            double totalDistance = 0;
            Coordinate previousCoordinate = null;

            for (int i = 0; i < nodesListCopy.Count(); i++)
            {
                var mapNode = nodesListCopy.ElementAt(i);
                Coordinate currentCoordinate = new Coordinate(mapNode.longitude, mapNode.latitude);
                returnList.Add(mapNode);

                if (previousCoordinate != null)
                    totalDistance += previousCoordinate.Distance(currentCoordinate);

                previousCoordinate = currentCoordinate;

                if (totalDistance >= AmountOfLaneData)
                    break;
            }

            returnList.Reverse();
            return returnList;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="TypeId"></param>
        /// <param name="Message"></param>
        /// <returns>byte[]</returns>
        /// <exception cref="TIMException"></exception>
        public static byte[] CreatOutgoingMessage(TypeId TypeId, Object Message)
        {
            //log.Debug("In CreatOutgoingMessage");

            byte[] retrunByteArray = null;

            try
            {
                switch (TypeId)
                {
                    case TypeId.RTCM:
                        string RTCM = (string)Message;
                        RTCM = RTCM.Replace("ICY 200 OK\r\n\r\n", "");
                        byte[] temp = Encoding.ASCII.GetBytes(RTCM);
                        RTCM = string.Concat(temp.Select(b => b.ToString("X2")).ToArray());
                        retrunByteArray = Encoding.ASCII.GetBytes((char)2 + "{\"typeid\":\"RTCM\",\"2.3\":\"" + RTCM + "\"}" + (char)3);
                        break;
                    case TypeId.EVA:
                        string itis = (string)Message;
                        retrunByteArray = Encoding.ASCII.GetBytes((char)2 + "{\"typeid\":\"EVA\",\"enabled\":true,\"itis\":" + itis + "}" + (char)3);
                        break;
                    case TypeId.TIM:
                        string TIMBits = (string)Message;
                        retrunByteArray = Encoding.ASCII.GetBytes((char)2 + "{\"typeid\":\"TIM\",\"enabled\":true,\"payload\":\"" + TIMBits + "\"}" + (char)3);
                        break;
                    case TypeId.THREAT:
                        JsonTHREAT THREAT = (JsonTHREAT)Message;
                        retrunByteArray = Encoding.ASCII.GetBytes((char)2 + "{\"typeid\":\"THREAT\",\"tlevel0count\":" + THREAT.tlevel0count + ",\"tlevel1count\":" + THREAT.tlevel1count + ",\"tlevel2count\":" + THREAT.tlevel2count + "}" + (char)3);
                        break;
                    case TypeId.STOPTIM:
                        retrunByteArray = Encoding.ASCII.GetBytes((char)2 + "{\"typeid\":\"TIM\",\"enabled\":false}" + (char)3);
                        break;
                    case TypeId.STOPEVA:
                        retrunByteArray = Encoding.ASCII.GetBytes((char)2 + "{\"typeid\":\"EVA\",\"enabled\":false}" + (char)3);
                        break;
                }
            }
            catch (Exception)
            {
                throw new TIMException("Could not create Outgoing Message");
            }

            //log.Debug(retrunByteArray);
            return retrunByteArray;
        }
    }
}
