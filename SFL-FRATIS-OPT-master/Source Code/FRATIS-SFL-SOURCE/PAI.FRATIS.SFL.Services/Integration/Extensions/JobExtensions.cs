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
using PAI.FRATIS.SFL.Domain.Orders;

namespace PAI.FRATIS.SFL.Services.Integration.Extensions
{
    public static class JobExtensions
    {
        public static bool IsChangedFrom(this Job job, Job targetJob)
        {
            try
            {
                if (job.ConsigneeName != targetJob.ConsigneeName || job.ShipperName != targetJob.ShipperName)
                {
                    return true;
                }

                if (job.IsHazmat != targetJob.IsHazmat || job.IsFlatbed != targetJob.IsFlatbed)
                {
                    return true;
                }

                if (job.ContainerNumber != targetJob.ContainerNumber)
                {
                    return true;
                }

                if (job.BillOfLading != targetJob.BillOfLading)
                {
                    return true;
                }


                if (job.TrailerId != targetJob.TrailerId)
                {
                    return true;
                }

                if (job.RouteStops.Count != targetJob.RouteStops.Count)
                {
                    return true;
                }

                if (job.RouteStops.Count != targetJob.RouteStops.Count)
                {
                    return true;
                }

                if (job.JobGroupId != targetJob.JobGroupId)
                {
                    return true;
                }

                if (job.IsValid != targetJob.IsValid)
                {
                    return true;
                }

                for (int i = 0; i < job.RouteStops.Count; i++)
                {
                    var rs = job.RouteStops[i];
                    var targetRs = targetJob.RouteStops[i];

                    if (rs.LocationId != targetRs.LocationId)
                    {
                        return true;
                    }

                    if (rs.StopAction != null && targetRs.StopAction == null ||  rs.StopAction.ShortName != targetRs.StopAction.ShortName)
                    {
                        return true;
                    }

                    if (rs.SortOrder != targetRs.SortOrder)
                    {
                        return true;
                    }

                    if (rs.StopDelay != targetRs.StopDelay)
                    {
                        return true;
                    }

                    if (rs.WindowStart != targetRs.WindowStart || rs.WindowEnd != targetRs.WindowEnd)
                    {
                        return true;
                    }

                    if (rs.WindowStart != targetRs.WindowStart || rs.WindowEnd != targetRs.WindowEnd)
                    {
                        return true;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Problem comparing job for changes: " + e.Message + e.StackTrace);
                return true;
            }

            return false;
        }

        public struct JobMapResult
        {
            public bool IsRouteStopsRecreated { get; set; }
            public bool IsJobError { get; set; }
        }

        /// <summary>
        /// Updates the job from the provided job values
        /// </summary>
        /// <param name="job"></param>
        /// <param name="sourceJob"></param>
        /// <returns>Whether route stops have been recreated</returns>
        public static JobMapResult UpdateFrom(this Job job, Job sourceJob)
        {
            var result = new JobMapResult();

            try
            {
                job.ConsigneeName = sourceJob.ConsigneeName;
                job.ShipperName = sourceJob.ShipperName;
                job.ContainerNumber = sourceJob.ContainerNumber;
                job.BillOfLading = sourceJob.BillOfLading;
                job.TrailerId = sourceJob.TrailerId;
                job.IsHazmat = sourceJob.IsHazmat;
                job.IsFlatbed = sourceJob.IsFlatbed;
                job.JobGroupId = sourceJob.JobGroupId;
                job.IsValid = sourceJob.IsValid;
                job.DueDate = sourceJob.DueDate;

                if (job.RouteStops.Count != sourceJob.RouteStops.Count)
                {
                    result.IsRouteStopsRecreated = true;
                    job.RouteStops = new List<RouteStop>();
                    foreach (var rsToAdd in sourceJob.RouteStops)
                    {
                        job.RouteStops.Add(new RouteStop());
                    }
                }

                for (int i = 0; i < job.RouteStops.Count; i++)
                {
                    try
                    {
                        var rs = job.RouteStops[i];

                        var sourceRs = sourceJob.RouteStops[i];

                        rs.LocationId = sourceRs.LocationId;
                        rs.StopAction = sourceRs.StopAction;
                        rs.StopDelay = sourceRs.StopDelay;
                        rs.WindowStart = sourceRs.WindowStart;
                        rs.WindowEnd = sourceRs.WindowEnd;
                        rs.SortOrder = sourceRs.SortOrder;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Error updating routestops for job {0}", job.OrderNumber);
                    }
                }
            }
            catch (Exception e)
            {
                result.IsJobError = true;
                Console.WriteLine("Problem setting job from changes: " + e.Message + e.StackTrace.ToString());
            }

            return result;
        }
    }
}