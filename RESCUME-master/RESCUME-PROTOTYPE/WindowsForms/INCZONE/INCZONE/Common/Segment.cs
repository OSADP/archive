using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace INCZONE.Common
{
    public class Segment
    {
        public string IncidentName { get; set; }
        public LinkSegmentType SegmentType { get; set; }
        public DateType TimeofIncident { get; set; }
    }
}
