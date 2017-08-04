using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace INCZONE.Common
{
    public class Lane
    {
        public Lane(LaneType LanyType, int LaneNumber)
        {
            this.LanyType = LanyType;
            this.LaneNumber = LaneNumber;
            this.MapNodes = new List<Common.MapNode>();
        }
        public int LaneNumber { get; set; }
        public LaneType LanyType { get; set; }
        public List<INCZONE.Common.MapNode> MapNodes { get; set; }
    }
}
