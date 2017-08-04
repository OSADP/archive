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
using PAI.Drayage.Optimization.Model.Metrics;

namespace PAI.Drayage.Optimization.Model.Node
{
    /// <summary>
    /// Represents a Route Solution
    /// </summary>
    public class NodeRouteSolution : IHaveRouteStatistics, IEqualityComparer<NodeRouteSolution>
    {
        /// <summary>
        /// Gets or sets the driver node 
        /// </summary>
        public DriverNode DriverNode { get; set; }

        /// <summary>
        /// Gets or sets the start time for the series of routes
        /// </summary>
        public TimeSpan StartTime { get; set; }
        
        /// <summary>
        /// Gets or sets the internal nodes
        /// </summary>
        public IList<INode> Nodes { get; set; }

        /// <summary>
        /// Gets or sets the route statistics
        /// </summary>
        public RouteStatistics RouteStatistics { get; set; }
        
        /// <summary>
        /// Gets all the nodes
        /// </summary>
        public IList<INode> AllNodes
        {
            get
            {
                var allNodes = new List<INode>();

                if (DriverNode != null)
                    allNodes.Add(DriverNode);

                allNodes.AddRange(Nodes);

                if (DriverNode != null)
                    allNodes.Add(DriverNode);

                return allNodes;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NodeRouteSolution"/> class.
        /// </summary>
        public NodeRouteSolution()
        {
            Nodes = new List<INode>();
        }

        /// <summary>
        /// Gets the job count.
        /// </summary>
        /// <value>
        /// The job count.
        /// </value>
        public int JobCount
        {
            get
            {
                return Nodes.Count(node => node.GetType() == typeof (JobNode));
            }
        }
        
        /// <summary>
        /// Determines whether the specified objects are equal.
        /// </summary>
        public bool Equals(NodeRouteSolution x, NodeRouteSolution y)
        {
            return x.Nodes.SequenceEqual(y.Nodes);
        }

        /// <summary>
        /// Returns a hash code for the specified object.
        /// </summary>
        public int GetHashCode(NodeRouteSolution obj)
        {
            return Nodes.GetHashCode();
        }

        /// <summary>
        /// Returns a shallow copy 
        /// </summary>
        /// <returns></returns>
        public NodeRouteSolution Clone()
        {
            return this.MemberwiseClone() as NodeRouteSolution;
        }
    }
}