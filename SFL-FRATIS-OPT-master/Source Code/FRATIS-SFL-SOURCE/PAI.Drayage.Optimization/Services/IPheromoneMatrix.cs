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
using System.Linq;
using PAI.Drayage.Optimization.Model.Node;

namespace PAI.Drayage.Optimization.Services
{
    /// <summary>
    /// Represents a pheromone matrix, modifying probability values for subsequent solution generation runs, so that successful sub routes are more likely to be selected
    /// </summary>
    public interface IPheromoneMatrix
    {
        /// <summary>
        /// Gets or sets the initial pheromone value
        /// </summary>
        double InitialPheromoneValue { get; set; }

        /// <summary>
        /// Gets or sets the pheromone evaporation rate
        /// Valid ranges between 0.0 and 1.0
        /// Default value should be set as: 0.5
        /// </summary>
        double Rho { get; set; }

        /// <summary>
        /// Gets or sets the performance measure coeficient
        /// Default value should be set to: 1000
        /// </summary>
        double Q { get; set; }

        /// <summary>
        /// Clears the pheromone matrix
        /// </summary>
        void Clear();

        /// <summary>
        /// Gets the value
        /// </summary>
        /// <param name="origin">the start node of pheromone trail</param>
        /// <param name="destination">the end node of pheromone trail</param>
        /// <returns>the pheromone value</returns>
        double GetValue(INode origin, INode destination);

        /// <summary>
        /// Sets the value
        /// </summary>
        /// <param name="origin">the beginning location of the ant trail</param>
        /// <param name="destination">the ending location of the ant trail</param>
        /// <param name="value">the probability value to set on the route from origin to destination</param>
        void SetValue(INode origin, INode destination, double value);
        
        /// <summary>
        /// Update pheromone
        /// </summary>
        /// <param name="solution">the solution from which the matrix will be updated</param>
        void UpdatePheromoneMatrix(Solution solution);

    }
}