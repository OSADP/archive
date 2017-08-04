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
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

using PAI.Drayage.Optimization.Model.Metrics;

namespace PAI.Drayage.Optimization.Model.Node
{
    /// <summary>
    /// A container for the optimized routes and any jobs that were unable to be assigned
    /// </summary>
    public class Solution : IHaveRouteStatistics
    {
        /// <summary>
        /// Gets or sets the route solutions.
        /// </summary>
        /// <value>
        /// The route solutions.
        /// </value>
        private RouteStatistics _routeStatistics;
        private bool _isDirty = true;


        private ObservableCollection<NodeRouteSolution> _routeSolutions;
        public ObservableCollection<NodeRouteSolution> RouteSolutions
        {
            get
            {
                return _routeSolutions;
            }
            set
            {
                _routeSolutions = value;
                _isDirty = true;
                _routeSolutions.CollectionChanged += RouteSolutionsChanged;
            }
        }

        private void RouteSolutionsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            _isDirty = true;
        }
        
        public List<INode> UnassignedJobNodes { get; set; }
        
        public Solution()
        {
            RouteSolutions = new ObservableCollection<NodeRouteSolution>();
            UnassignedJobNodes = new List<INode>();
        }

        /// <summary>
        /// Gets the route statistics.
        /// </summary>
        /// <value>
        /// The route statistics.
        /// </value>
        public RouteStatistics RouteStatistics
        {
            get
            {
                if (_isDirty)
                {
                    _routeStatistics = new RouteStatistics();

                    foreach (var solution in RouteSolutions)
                    {
                        // add penalty for each driver
                        _routeStatistics.TotalExecutionTime += TimeSpan.FromMinutes(15);
                        _routeStatistics += solution.RouteStatistics;
                    }

                    _isDirty = false;
                }

                return _routeStatistics;
            }
            set
            {
                _routeStatistics = value;
            }
        }

        /// <summary>
        /// Clones this instance.
        /// </summary>
        /// <returns>A copy of the solution</returns>
        public Solution Clone()
        {
            var clone = new Solution();
            foreach (var routeSolution in RouteSolutions)
            {
                clone.RouteSolutions.Add(routeSolution.Clone());
            }
            return clone;
        }

        public void PruneEmptySolutions()
        {
            // Prune empty solutions
            RouteSolutions = new ObservableCollection<NodeRouteSolution>(RouteSolutions.Where(f => f.Nodes.Count > 0));
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            foreach (var nodeRouteSolution in RouteSolutions)
            {
                var s = string.Join("\t", nodeRouteSolution.Nodes.Select(f => "[" + f.Id + "]").ToArray());
                sb.AppendLine(s);
            }

            sb.AppendLine(this.RouteStatistics.ToString());

            return sb.ToString();
        }
    }
}
