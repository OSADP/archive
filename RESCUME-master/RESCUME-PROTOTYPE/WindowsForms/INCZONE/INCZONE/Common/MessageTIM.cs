using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace INCZONE.Common
{
    public class MessageTIM
    {
        public MessageTIM() 
        {
            this.DataFrameCount = 3;
            this.DataFrames = new List<DataFrame>(this.DataFrameCount);
        }

        public int MsgID { get; set; }
        public int DataFrameCount { get; set; }
        public List<DataFrame> DataFrames { get; set; }

        //Frame 1 describes the extent of the incident zone advisory region.
        //Frame 2 will describe the incident zone lane closures
        //Frame 3 will describe the incident zone speed restrictions
        public enum FrameSet
        {
            Frame_1 = 1,
            Frame_2,
            Frame_3
        }
    }
}
