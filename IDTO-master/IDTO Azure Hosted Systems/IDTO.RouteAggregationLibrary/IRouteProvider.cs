using System;
using System.Collections.Generic;
using IDTO.RouteAggregationLibrary.OpenTripPlanner;
using IDTO.RouteAggregationLibrary.OpenTripPlanner.Model;

namespace IDTO.RouteAggregationLibrary
{
    interface IRouteProvider
    {
        void FindAvailableRoutes(decimal fromPlace, decimal toPlace);

        StopTimesList FindStopTimes(string agency, string stopId, long startTime, long endTime);

        StopList FindStopsNearPoint(float latitude, float longitude, int radius);
    }
}
