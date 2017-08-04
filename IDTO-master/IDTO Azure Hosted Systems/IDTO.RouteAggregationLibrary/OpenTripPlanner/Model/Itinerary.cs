﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDTO.RouteAggregationLibrary.OpenTripPlanner.Model
{
    public class Itinerary
    {
        public int duration { get; set; }
        public long startTime { get; set; }
        public long endTime { get; set; }
        public int walkTime { get; set; }
        public int transitTime { get; set; }
        public int waitingTime { get; set; }
        public int walkDistance { get; set; }    
        public List<Leg> legs { get; set; }
    }
}
