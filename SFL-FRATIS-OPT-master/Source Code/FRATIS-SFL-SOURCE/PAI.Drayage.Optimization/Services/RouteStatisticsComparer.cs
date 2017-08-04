using System;
using System.Collections.Generic;
using System.Linq;
using PAI.Drayage.Optimization.Function;
using PAI.Drayage.Optimization.Model.Metrics;

namespace PAI.Drayage.Optimization.Services
{
    public interface IRouteStatisticsComparer : IComparer<RouteStatistics>, IComparer<IHaveRouteStatistics>
    {
    }

    public class RouteStatisticsComparer : IRouteStatisticsComparer
    {
        private readonly IObjectiveFunction _objectiveFunction;

        public RouteStatisticsComparer(IObjectiveFunction objectiveFunction)
        {
            _objectiveFunction = objectiveFunction;
        }

        /// <summary>
        /// Compares route solutions
        /// </summary>
        public int Compare(RouteStatistics left, RouteStatistics right)
        {
            if (left.DriversWithAssignments == 1 || right.DriversWithAssignments == 1)
            {
                ;
            }
            double leftMeasure = _objectiveFunction.GetObjectiveMeasure(left);
            double rightMeasure = _objectiveFunction.GetObjectiveMeasure(right);
            return leftMeasure.CompareTo(rightMeasure);
        }

        /// <summary>
        /// Compares two IHaveRouteStatistics.
        /// </summary>
        public int Compare(IHaveRouteStatistics x, IHaveRouteStatistics y)
        {
            return Compare(x.RouteStatistics, y.RouteStatistics);
        }
    }


    
}