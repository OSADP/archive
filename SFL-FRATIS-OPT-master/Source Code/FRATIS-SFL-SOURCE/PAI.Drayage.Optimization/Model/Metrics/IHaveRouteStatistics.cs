using System;
using System.Linq;

namespace PAI.Drayage.Optimization.Model.Metrics
{
    public interface IHaveRouteStatistics
    {
        /// <summary>
        /// Gets or sets route statistics
        /// </summary>
        RouteStatistics RouteStatistics { get; }
    }
}