//    Copyright 2014 Productivity Apex Inc.
//        http://www.productivityapex.com/
// 
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
// 
//        http://www.apache.org/licenses/LICENSE-2.0
// 
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using PAI.Drayage.Optimization.Function;
using PAI.Drayage.Optimization.Model;
using PAI.Drayage.Optimization.Model.Node;

namespace PAI.Drayage.Optimization.Services
{
    /// <summary>
    /// Represents a pheromone matrix
    /// </summary>
    public class PheromoneMatrix : IPheromoneMatrix
    {
        
        private readonly IObjectiveFunction _objectiveFunction;

        /// <summary>
        /// Gets or sets the initial pheromone value
        /// </summary>
        public double InitialPheromoneValue { get; set; }

        /// <summary>
        /// Gets or sets the pheromone evaporation rate
        /// </summary>
        public double Rho { get; set; }

        /// <summary>
        /// Gets or sets the performance measure coeficient
        /// </summary>
        public double Q { get; set; }

        public ConcurrentDictionary<Tuple<Location, Location>, double> PheromoneMatrixMap { get; private set; }
        
        public PheromoneMatrix(IObjectiveFunction objectiveFunction)
        {
            _objectiveFunction = objectiveFunction;
            
            PheromoneMatrixMap = new ConcurrentDictionary<Tuple<Location, Location>, double>();
        }

        /// <summary>
        /// Clears the pheromone matrix
        /// </summary>
        public virtual void Clear()
        {
            PheromoneMatrixMap.Clear();
        }

        private static Tuple<Location, Location> GetKey(INode node1, INode node2)
        {
            var originLocation = node1.RouteStops.First().Location;
            var destinationLocation = node2.RouteStops.First().Location;
            var key = new Tuple<Location, Location>(originLocation, destinationLocation);
            return key;
        }

        /// <summary>
        /// Gets the value
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="destination"></param>
        /// <returns></returns>
        public virtual double GetValue(INode origin, INode destination)
        {
            if (origin == null) throw new ArgumentNullException("origin");
            if (destination == null) throw new ArgumentNullException("destination");

            double value = PheromoneMatrixMap.GetOrAdd(GetKey(origin, destination), InitialPheromoneValue);
            
            return value;
        }

        /// <summary>
        /// Sets the value
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="destination"></param>
        /// <param name="value"></param>
        public virtual void SetValue(INode origin, INode destination, double value)
        {
            if (origin == null) throw new ArgumentNullException("origin");
            if (destination == null) throw new ArgumentNullException("destination");
            
            PheromoneMatrixMap[GetKey(origin, destination)] = value;
        }

        /// <summary>
        /// Updates the pheromone matrix 
        /// </summary>
        /// <param name="nodes"></param>
        /// <param name="performanceMeasure"></param>
        public virtual void UpdatePheromoneMatrix(IList<INode> nodes, double performanceMeasure)
        {
            if (nodes == null) throw new ArgumentNullException("nodes");

            // lock should already have been made for this recursive call
            for (int i = 0; i < (nodes.Count - 1); i++)
            {
                var key = GetKey(nodes[i], nodes[i + 1]);

                double pheromone = 0;
                
                if (!PheromoneMatrixMap.TryGetValue(key, out pheromone))
                {
                    pheromone = InitialPheromoneValue;
                }

                if (performanceMeasure == 0)
                {
                    pheromone = 0;
                }
                else
                {
                    pheromone = (Rho * pheromone) + (Q / performanceMeasure);
                }

                //update matrix
                PheromoneMatrixMap[key] = pheromone;

            }
        }
        
        /// <summary>
        /// Update pheromone
        /// </summary>
        /// <param name="solution"></param>
        public virtual void UpdatePheromoneMatrix(Solution solution)
        {
            if (solution == null) throw new ArgumentNullException("solution");
            if (solution.RouteSolutions == null) throw new ArgumentNullException("solution.RouteSolutions");

            var statistics = solution.RouteStatistics;
            statistics.DriversWithAssignments = solution.RouteSolutions.Count(p => p.JobCount > 0);
            var unassignedJobCount = solution.UnassignedJobNodes.Count();
            statistics.UnassignedJobs = unassignedJobCount;
            solution.RouteStatistics = statistics;

            double performanceMeasure = _objectiveFunction.GetObjectiveMeasure(solution.RouteStatistics);

            foreach (var routeSolution in solution.RouteSolutions)
            {
                UpdatePheromoneMatrix(routeSolution.AllNodes, performanceMeasure);
            }
            
        }

    }
}