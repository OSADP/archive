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
using PAI.FRATIS.SFL.Domain.Orders;

namespace PAI.FRATIS.SFL.Domain.Planning
{
    public enum TruckState
    {
        Bobtail = 0x2,
        Chassis = 0x4,
        Empty = 0x8,
        Loaded = 0x20,
    }

    public partial class RouteSegmentMetric : EntitySubscriberBase, ISortableEntity
    {
        public int? JobId { get; set; }
        //public virtual Job Job { get; set; }

        //public int? RouteStopId { get; set; }

        /// <summary>
        /// Gets or sets the PlanDriver
        /// </summary>
        public virtual PlanDriver PlanDriver { get; set; }
        public virtual int? PlanDriverId { get; set; }

        /// <summary>
        /// Gets or sets the start stop
        /// </summary>
        public virtual RouteStop StartStop { get; set; }
        public virtual int? StartStopId { get; set; }

        /// <summary>
        /// Gets or sets the end stop
        /// </summary>
        public virtual RouteStop EndStop { get; set; }
        public virtual int? EndStopId { get; set; }
        
        /// <summary>
        /// Gets or sets the truck state
        /// </summary>
        public virtual TruckState TruckState { get; set; }

        /// <summary>
        /// Gets or sets the start time of this segment
        /// </summary>
        public virtual long? StartTime { get; set; }

        public TimeSpan StartTimeSpan
        {
            get
            {
                return StartTime.HasValue ? new TimeSpan(StartTime.Value) : TimeSpan.Zero;
            }
        }

        /// <summary>
        /// Gets or sets the execution time
        /// </summary>
        public virtual long TotalExecutionTime { get; set; }

        /// <summary>
        /// Gets or sets the total time
        /// </summary>
        public virtual long TotalTravelTime { get; set; }

        public TimeSpan TotalTravelTimeSpan
        {
            get
            {
                return new TimeSpan(TotalTravelTime);
            }
        }

        //public string ArrivalTimeString
        //{
        //    get
        //    {
        //        // TODO urgent timezone
        //        return DepartureTimeSpan.Value.Add(TotalTravelTimeSpan).Add(new TimeSpan(10, 0, 0)).ToString();
        //    }
        //}

        /// <summary>
        /// Gets or sets the distance (mi)
        /// </summary>
        public virtual decimal TotalTravelDistance { get; set; }

        /// <summary>
        /// Gets or sets the SortOrder
        /// </summary>
        public virtual int SortOrder { get; set; }

        public long DepartureTime
        {
            get
            {
                return StartTimeSpan.Ticks + TotalIdleTime;
            }
        }

        public TimeSpan DepartureTimeSpan
        {
            get
            {
                return new TimeSpan(DepartureTime);
            }
        }

        public long TotalIdleTime { get; set; }

        public TimeSpan TotalIdleTimeSpan
        {
            get
            {
                return new TimeSpan(TotalIdleTime);
            }
        }

        public long TotalQueueTime { get; set; }

        public TimeSpan TotalQueueTimeSpan
        {
            get
            {
                return new TimeSpan(TotalQueueTime);
            }
        }

        public static RouteSegmentMetric operator +(RouteSegmentMetric c1, RouteSegmentMetric c2)
        {
            var result = new RouteSegmentMetric()
            {
                TotalIdleTime = c1.TotalIdleTime + c2.TotalIdleTime,
                TotalExecutionTime = c1.TotalExecutionTime + c2.TotalExecutionTime,
                TotalTravelTime = c1.TotalTravelTime + c2.TotalTravelTime,
                TotalTravelDistance = c1.TotalTravelDistance + c2.TotalTravelDistance,
                TotalQueueTime = c1.TotalQueueTime + c2.TotalQueueTime
            };
            return result;
        }

    }

}
