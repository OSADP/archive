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

namespace PAI.Drayage.Optimization.Model
{
    /// <summary>
    /// Represents a probablity item
    /// </summary>
    public class ProbabilityItem
    {
        /// <summary>
        /// Gets or sets the reference element
        /// </summary>
        public object Element { get; set; }

        /// <summary>
        /// Gets or sets the top probability
        /// </summary>
        public double TopProbability { get; set; }

        /// <summary>
        /// Gets or sets the probability
        /// </summary>
        public double Probability { get; set; }

        /// <summary>
        /// Gets or sets the cumulative probability
        /// </summary>
        public double CumulativeProbability { get; set; }
    }
}