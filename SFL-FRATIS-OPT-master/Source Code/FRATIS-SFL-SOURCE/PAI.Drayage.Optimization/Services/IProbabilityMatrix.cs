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
using PAI.Drayage.Optimization.Model;
using PAI.Drayage.Optimization.Model.Node;

namespace PAI.Drayage.Optimization.Services
{
    /// <summary>
    /// Provides an interface for calculating the probabilities of selecting a node, and selects the next node to try
    /// </summary>
    public interface IProbabilityMatrix
    {
        /// <summary>
        /// Returns list of probability data
        /// </summary>
        /// <param name="currentNode"></param>
        /// <param name="availableNodeTimings"> </param>
        /// <returns></returns>
        IList<ProbabilityItem> BuildProbabilityDataMatrix(INode currentNode, IEnumerable<NodeTiming> availableNodeTimings);

        /// <summary>
        /// Calculates the probabilities for a list of <see cref="ProbabilityItem"/>
        /// </summary>
        /// <param name="probabilityDataList"></param>
        void CalculateProbabilities(IList<ProbabilityItem> probabilityDataList);

        /// <summary>
        /// Nomiate node from probability data
        /// </summary>
        /// <param name="probabilityData"></param>
        /// <returns></returns>
        object GetNominatedElement(IList<ProbabilityItem> probabilityData);
    }
}