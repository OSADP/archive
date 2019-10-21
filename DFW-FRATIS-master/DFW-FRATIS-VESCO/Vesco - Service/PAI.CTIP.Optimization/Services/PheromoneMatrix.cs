//    Copyright 2013 Productivity Apex Inc.
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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PAI.CTIP.Optimization.Function;
using PAI.CTIP.Optimization.Model.Node;


namespace PAI.CTIP.Optimization.Services
{


    /// <summary>
    /// Represents a pheromone matrix
    /// </summary>
    public class PheromoneMatrix : IPheromoneMatrix
    {
        private readonly IDictionary<Tuple<INode, INode>, double> _pheromoneMatrix;
        private readonly IObjectiveFunction _objectiveFunction;
        private Dictionary<INode, double> _initialValues;

        public PheromoneMatrix(double initialPheromoneValue, double rho, double q, IObjectiveFunction objectiveFunction)
        {
            InitialPheromoneValue = initialPheromoneValue;
            Rho = rho;
            Q = q;

            _objectiveFunction = objectiveFunction;
            _pheromoneMatrix = new Dictionary<Tuple<INode, INode>, double>();
        }


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

        /// <summary>
        /// Clears the pheromone matrix
        /// </summary>
        public virtual void Clear()
        {
            _pheromoneMatrix.Clear();    
        }

        
        /// <summary>
        /// Gets the value
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="destination"></param>
        /// <returns></returns>
        public virtual double GetValue(INode origin, INode destination)
        {
            double value;
            if (!_pheromoneMatrix.TryGetValue(new Tuple<INode, INode>(origin, destination), out value))
            {
                value = GetInitialValue(new[] { origin, destination });
            }
            
            return value;
        }

        /// <summary>
        /// Gets the initial pheromone value for the provided nodes
        /// If multiple nodes have an initial value override set, then
        /// the highest value is returned.
        /// </summary>
        /// <param name="nodes"></param>
        /// <returns></returns>
        public virtual double GetInitialValue(IEnumerable<INode> nodes)
        {
            var initialValue = InitialPheromoneValue;
            if (_initialValues != null)
            {
                foreach (var node in nodes)
                {
                    double i;
                    if (_initialValues.TryGetValue(node, out i))
                    {
                        if (i > initialValue)
                        {
                            initialValue = i;
                        }
                    }
                }                
            }
            return initialValue;
        
        }
        
        public void SetInitialValues(Dictionary<INode, double> values)
        {
            _initialValues = new Dictionary<INode, double>();
            foreach (var kvp in values)
            {
                _initialValues[kvp.Key] = kvp.Value;
            }
        }

        /// <summary>
        /// Sets the value
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="destination"></param>
        /// <param name="value"></param>
        public virtual void SetValue(INode origin, INode destination, double value)
        {
            _pheromoneMatrix[new Tuple<INode, INode>(origin, destination)] = value;
        }

        /// <summary>
        /// Updates the pheromone matrix 
        /// </summary>
        /// <param name="nodes"></param>
        /// <param name="performanceMeasure"></param>
        public virtual void UpdatePheromoneMatrix(IList<INode> nodes, double performanceMeasure)
        {
            for (int i = 0; i < (nodes.Count - 1); i++)
            {
                var key = new Tuple<INode, INode>(nodes[i], nodes[i + 1]);

                double pheromone = 0;
                if (!_pheromoneMatrix.TryGetValue(key, out pheromone))
                {
                    pheromone = GetInitialValue(new[] { nodes[i], nodes[i + 1] });
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
                _pheromoneMatrix[key] = pheromone;
            }
        }
        
        /// <summary>
        /// Update pheromone
        /// </summary>
        /// <param name="solution"></param>
        public virtual void UpdatePheromoneMatrix(Solution solution)
        {
            double performanceMeasure = _objectiveFunction.GetObjectiveMeasure(solution.RouteStatistics);

            foreach (var routeSolution in solution.RouteSolutions)
            {
                UpdatePheromoneMatrix(routeSolution.AllNodes, performanceMeasure);
            }
        }
    }
}