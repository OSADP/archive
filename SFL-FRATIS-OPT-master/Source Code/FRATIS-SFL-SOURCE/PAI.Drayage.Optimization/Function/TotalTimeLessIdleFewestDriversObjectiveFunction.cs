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

using PAI.Drayage.Optimization.Model.Metrics;

namespace PAI.Drayage.Optimization.Function
{
    public class TotalTimeLessIdleFewestDriversObjectiveFunction : IObjectiveFunction
    {
        public double GetObjectiveMeasure(RouteStatistics statistics)
        {
            var result =
                (statistics.TotalTime.TotalSeconds - statistics.TotalIdleTime.TotalSeconds) 
                * ((double)1 / (statistics.DriversWithAssignments + 1));
            return result;
        }
    }
    public class TotalTimeWithMassivePenalityForUnassignedJobs : IObjectiveFunction
    {
        public double GetObjectiveMeasure(RouteStatistics statistics)
        {
            var result = statistics.TotalTime.TotalSeconds + (1000000*statistics.UnassignedJobs);
            return result;
        }
    }
}