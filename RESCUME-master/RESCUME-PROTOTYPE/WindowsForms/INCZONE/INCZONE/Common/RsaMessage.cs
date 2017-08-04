using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace INCZONE.Common
{
    public class RsaMessage
    {
        public RsaMessage() { }
        
        public RsaMessage(int MsgID, int MsgCnt, double Latitude, double Longitude, double Elevation) 
        {
            this.MsgCnt = MsgCnt;
            this.MsgCoordinate = new Coordinate(Longitude, Latitude, Elevation);
            this.MsgID = MsgID;
        }
        
        public int MsgID { get; set; }
        public int MsgCnt { get; set; }
        public Coordinate MsgCoordinate { get; set; }
    }
}
