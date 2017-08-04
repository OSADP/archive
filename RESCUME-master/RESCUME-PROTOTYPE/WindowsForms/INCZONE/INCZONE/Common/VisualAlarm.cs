using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INCZONE.Common
{
    public class VisualAlarm
    {
        public VisualAlarm() { }

        public VisualAlarm(int tlevel0count, int tlevel1count, int tlevel2count, int tlevel3count)
        {
            // TODO: Complete member initialization
            this.tlevel0count = tlevel0count;
            this.tlevel1count = tlevel1count;
            this.tlevel2count = tlevel2count;
            this.tlevel3count = tlevel3count;
        }

        public int tlevel0count { get; set; }
        public int tlevel1count { get; set; }
        public int tlevel2count { get; set; }
        public int tlevel3count { get; set; }
    }
}
