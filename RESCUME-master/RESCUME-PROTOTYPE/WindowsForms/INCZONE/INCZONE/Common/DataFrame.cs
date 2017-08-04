using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace INCZONE.Common
{
    public class DataFrame
    {
        public string FrameType { get; set; }
        public string MsgID { get; set; }
        public int StartTime { get; set; }
        public int DurationTime { get; set; }
        public int SignPriority { get; set; } //0 to 7
        public MapNode Region { get; set; }
        public int Advisory { get; set; }
        public int SpeedLimit { get; set; }
        public CommonAnchor CommonAnchor { get; set; }
        public int CommonLaneWidth { get; set; } //in cm
        public int CommonDirectionality { get; set; } //0,1,2
        public Content Content { get; set; }
    }
}
