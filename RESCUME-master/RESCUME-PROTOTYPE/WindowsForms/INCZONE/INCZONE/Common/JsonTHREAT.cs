using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace INCZONE.Common
{
    public class JsonTHREAT
    {
        public int tlevel0count {get; set;}
        public int tlevel1count {get; set;}
        public int tlevel2count { get; set; }
        public int tlevel3count { get; set; }

        public override string ToString()
        {
            return string.Format("tlevel0count : {0}, tlevel1count : {1}, tlevel2count : {2}, tlevel2count : {3}", tlevel0count, tlevel1count, tlevel2count, tlevel3count);

        }
    }
}
