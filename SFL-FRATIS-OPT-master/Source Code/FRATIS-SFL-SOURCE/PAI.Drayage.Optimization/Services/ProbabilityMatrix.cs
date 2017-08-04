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
using System.Collections.Generic;
using System.Linq;
using PAI.Drayage.Optimization.Common;
using PAI.Drayage.Optimization.Function;
using PAI.Drayage.Optimization.Model;
using PAI.Drayage.Optimization.Model.Node;

namespace PAI.Drayage.Optimization.Services
{
    /// <summary>The probability matrix.</summary>
    public class ProbabilityMatrix : IProbabilityMatrix
    {
        private readonly IRouteService _routeService;
        private readonly IRouteStatisticsService _routeStatisticsService;
        private readonly IObjectiveFunction _objectiveFunction;
        private readonly IRandomNumberGenerator _randomNumberGenerator;

        private IPheromoneMatrix PheromoneMatrix { get; set; }
        public float Alpha { get; set; }
        public float Beta { get; set; }
        public float Zeta { get; set; }

        public double? ForcedHomeProbaility { get; set; }

        public ProbabilityMatrix(IPheromoneMatrix pheromoneMatrix, IRouteService routeService, IObjectiveFunction objectiveFunction, IRandomNumberGenerator randomNumberGenerator, IRouteStatisticsService routeStatisticsService)
        {
            _routeService = routeService;
            _objectiveFunction = objectiveFunction;
            _randomNumberGenerator = randomNumberGenerator;
            _routeStatisticsService = routeStatisticsService;
            PheromoneMatrix = pheromoneMatrix;
        }

        /// <summary>
        /// Returns list of probability data
        /// </summary>
        /// <param name="currentNode"></param>
        /// <param name="availableNodeTimings"> </param>
        /// <returns></returns>
        public virtual IList<ProbabilityItem> BuildProbabilityDataMatrix(INode currentNode, IEnumerable<NodeTiming> availableNodeTimings)
        {
            var probabilityDataList = new List<ProbabilityItem>();

            // build probability data matrix for all unprocessed nodes
            foreach (var nodeTiming in availableNodeTimings)
            {
                var topProbability = CalculateProbability(currentNode, nodeTiming);    
                
                var probabilityItem = new ProbabilityItem()
                {
                    Element = nodeTiming.Node,
                    TopProbability = topProbability
                };

                // store the result
                probabilityDataList.Add(probabilityItem);
            }

            CalculateProbabilities(probabilityDataList);

            return probabilityDataList;
        }

        /// <summary>
        /// Calculates the probabilities for a list of <see cref="ProbabilityItem"/>
        /// </summary>
        /// <param name="probabilityDataList"></param>
        public virtual void CalculateProbabilities(IList<ProbabilityItem> probabilityDataList)
        {
            double topProbSummation = probabilityDataList.Sum(f => f.TopProbability);
            double cumulativeProbability = 0.0;

            foreach (var probabilityData in probabilityDataList)
            {
                probabilityData.Probability = probabilityData.TopProbability / topProbSummation;
                cumulativeProbability += probabilityData.Probability;
                probabilityData.CumulativeProbability = cumulativeProbability;
            }
        }
        
        /// <summary>
        /// Calculates the probability of selecting a given node
        /// </summary>
        public virtual double CalculateProbability(INode currentNode, NodeTiming nodeTiming)
        {
            if (currentNode == null) throw new ArgumentNullException("currentNode");
            if (nodeTiming == null) throw new ArgumentNullException("nodeTiming");

            var node = nodeTiming.Node;
            var pheromone = PheromoneMatrix.GetValue(currentNode, node);
            
            double topProbability = 1.0;
            
            if (pheromone > 0)
            {
                if (Alpha > 0)
                {
                    topProbability *= Math.Pow(pheromone, Alpha);
                }

                var routeStatistics = _routeStatisticsService.GetRouteStatistics(currentNode, node, nodeTiming.EndExecutionTime);
                var performanceMeasure = _objectiveFunction.GetObjectiveMeasure(routeStatistics);
                
                if (Beta > 0 && performanceMeasure > 0)
                {
                    topProbability *= Math.Pow(1 / performanceMeasure, Beta);
                }
            }

            var priority = Math.Max(node.Priority, 1);
            if (Zeta > 0)
            {
                topProbability *= 1 + Math.Pow(Zeta, priority);
            }

            return topProbability;
        }
        
        /// <summary>
        /// Nomiate node from probability data
        /// </summary>
        /// <param name="probabilityData"></param>
        /// <returns></returns>
        public virtual object GetNominatedElement(IList<ProbabilityItem> probabilityData)
        {
            if (probabilityData == null) throw new ArgumentNullException("probabilityData");

            var rand = _randomNumberGenerator.NextDouble();

            var data = probabilityData
                .OrderBy(f => f.CumulativeProbability)
                .FirstOrDefault(d => d.CumulativeProbability > rand);

            return data != null ? data.Element : (probabilityData.First().Element);
        }
    }
}