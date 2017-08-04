using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace INCZONE.Common
{
    public class MapNode
    {
        public Guid Id { get; set; }
        public int directionality { get; set; }
        public double distance { get; set; }
        public double elevation { get; set; }
        public int laneOrder { get; set; }
        public int laneWidth { get; set; }
        public double latitude { get; set; }
        public double longitude { get; set; }
        public Guid mapSetId { get; set; }
        public int positionalAccuracyP1 { get; set; }
        public int positionalAccuracyP2 { get; set; }
        public int positionalAccuracyP3 { get; set; }
        public int postedSpeed { get; set; }
        public int xOffset { get; set; }
        public int yOffset { get; set; }
        public int zOffset { get; set; }
        public string LaneDirection { get; set; }
        public string LaneType { get; set; }
        public INCZONE.Common.MapSet mapSet { get; set; }
    }
}

