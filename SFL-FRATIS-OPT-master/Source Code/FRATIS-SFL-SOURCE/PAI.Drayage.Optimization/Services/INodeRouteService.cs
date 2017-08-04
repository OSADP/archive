using System;
using System.Collections.Generic;
using PAI.CTIP.Services.Optimization.Model;

namespace PAI.CTIP.Services.Optimization
{
    public interface INodeRouteService
    {
        /// <summary>
        /// Calculates the trip length between two nodes
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="destination"></param>
        /// <returns></returns>
        RouteStatistics CalculateRouteStatistics(INode origin, INode destination);

        /// <summary>
        /// Gets the <see cref="NodeConnection"/> between two nodes
        /// </summary>
        /// <param name="startNode"></param>
        /// <param name="endNode"></param>
        /// <returns></returns>
        NodeConnection GetNodeConnection(INode startNode, INode endNode);

        /// <summary>
        /// Returns the <see cref="NodeTiming"/> for the next node
        /// </summary>
        /// <returns></returns>
        NodeTiming GetNodeTiming(INode currentNode, INode nextNode, DateTime currentNodeEndTime, RouteStatistics currentRouteStatistics);

        ///// <summary>
        ///// Creates a route solution from a list of nodes
        ///// </summary>
        ///// <param name="nodes"></param>
        ///// <returns></returns>
        RouteSolution CreateRouteSolution(IEnumerable<INode> nodes, DriverNode driverNode);

        ///// <summary>
        ///// Gets the best solution between a new list of <see cref="INode"/> and a current best <see cref="RouteSolution"/>
        ///// </summary>
        ///// <param name="nodes"></param>
        ///// <param name="driverNode"> </param>
        ///// <param name="bestSolution"></param>
        ///// <returns>the best solution</returns>
        RouteSolution GetBestFeasableSolution(IList<INode> nodes, DriverNode driverNode, RouteSolution bestSolution);
        
        RouteSolution GetBestSolution(RouteSolution left, RouteSolution right);

        Solution GetBestSolution(Solution left, Solution right);
    }
}