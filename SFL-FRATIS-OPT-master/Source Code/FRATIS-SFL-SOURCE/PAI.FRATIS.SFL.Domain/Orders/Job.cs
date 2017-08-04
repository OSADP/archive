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
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using PAI.FRATIS.SFL.Domain.Equipment;
using PAI.FRATIS.SFL.Domain.Geography;
using PAI.FRATIS.SFL.Domain.Planning;

namespace PAI.FRATIS.SFL.Domain.Orders
{
    /// <summary>
    /// Represents a drayage job
    /// </summary>
    public class Job : EntitySubscriberBase, IArchivedEntity, IDatedEntity
    {
        /// <summary>
        /// Gets or sets the order number
        /// </summary>
        public virtual string OrderNumber { get; set; }

        public string ContainerNumber { get; set; }

        public string TrailerId { get; set; }

        public string BillOfLading { get; set; }

        public string LegacyId { get; set; }

        /// <summary>Gets or sets the driver sort order.</summary>
        public int DriverSortOrder { get; set; }

        /// <summary>
        /// Gets or sets whether this job was transmitted
        /// </summary>
        public bool IsTransmitted { get; set; }

        //public DateTime? DistanceProcessedDate { get; set; }

        /// <summary>
        /// Gets or sets the job due date
        /// </summary>
        public virtual DateTime? DueDate { get; set; }

        /// <summary>
        /// Gets or sets the date in which this record was last synchronized with an external system
        /// </summary>
        public DateTime? SyncDate { get; set; }

        /// <summary>
        /// Gets or sets the Job Group
        /// </summary>
        public virtual JobGroup JobGroup { get; set; }
        public int? JobGroupId { get; set; }

        /// <summary>
        /// Gets or sets the job template type.
        /// </summary>
        public JobTemplateType JobTemplateType { get; set; }

        /// <summary>
        /// Gets or sets the chassis
        /// </summary>
        public virtual Chassis Chassis { get; set; }
        public int? ChassisId { get; set; }

        /// <summary>
        /// Gets or sets the chassis
        /// </summary>
        public virtual ChassisOwner ChassisOwner { get; set; }
        public int? ChassisOwnerId { get; set; }

        /// <summary>
        /// Gets or sets the container
        /// </summary>
        public virtual Container Container { get; set; }
        public int? ContainerId { get; set; }

        /// <summary>
        /// Gets or sets the container
        /// </summary>
        public virtual ContainerOwner ContainerOwner { get; set; }
        public int? ContainerOwnerId { get; set; }

        /// <summary>
        /// Gets or sets the route stops
        /// </summary>
        private IList<RouteStop> _routeStops = null;
        public virtual IList<RouteStop> RouteStops
        {
            get
            {
                return _routeStops ?? (_routeStops = new List<RouteStop>());
            }
            set
            {
                _routeStops = value;
            }
        }

        /// <summary>
        /// Gets or sets the Pickup Number
        /// </summary>
        public string PickupNumber { get; set; }

        /// <summary>
        /// Gets or sets the Pickup Number
        /// </summary>
        public string BookingNumber { get; set; }

        /// <summary>
        /// Gets or sets the Pickup Number
        /// </summary>
        public string Notes { get; set; }

        /// <summary>
        /// Gets or sets the JobStatus
        /// </summary>
        public virtual JobStatus JobStatus { get; set; }

        /// <summary>
        /// Gets the shipper location based upon RouteStops within this Job
        /// </summary>
        public Location ShipperLocation
        {
            get
            {
                Location result = null;
                if (RouteStops != null && RouteStops.Count > 0)
                {
                    var first = RouteStops.FirstOrDefault();
                    if (first != null && first.Location != null)
                    {
                        result = first.Location;
                    }
                    if (RouteStops.FirstOrDefault(p => p.StopAction != null && p.StopAction.ShortName == "LL") != null)
                    {
                        var rs = RouteStops.FirstOrDefault(p => p.StopAction.ShortName.StartsWith("LL"));
                        if (rs != null && rs.Location != null)
                        {
                            result = rs.Location;
                        }
                    }
                    else if (RouteStops.FirstOrDefault(p => p.StopAction != null && p.StopAction.ShortName == "LU") != null)
                    {
                        var rs = RouteStops.FirstOrDefault(p => p.StopAction.ShortName.StartsWith("PL"));
                        if (rs != null && rs.Location != null)
                        {
                            result = rs.Location;
                        }
                    }
                }
                return result;
            }
        }

        /// <summary>
        /// Gets the Consignee Location based upon RouteStops within this model
        /// </summary>
        public Location ConsigneeLocation
        {
            get
            {
                Location result = null;
                if (RouteStops != null && RouteStops.Count > 0)
                {
                    var end = RouteStops.LastOrDefault();
                    if (end != null && end.Location != null)
                    {
                        result = end.Location;
                    }

                    if (RouteStops.FirstOrDefault(p => p.StopAction != null && p.StopAction.ShortName == "LL") != null)
                    {
                        var rs = RouteStops.FirstOrDefault(p => p.StopAction.ShortName.StartsWith("DL"));
                        if (rs != null && rs.Location != null)
                        {
                            result = rs.Location;
                        }
                    }
                    else if (RouteStops.FirstOrDefault(p => p.StopAction != null && p.StopAction.ShortName == "LU") != null)
                    {
                        var rs = RouteStops.FirstOrDefault(p => p.StopAction != null && p.StopAction.ShortName.StartsWith("LU"));
                        if (rs != null && rs.Location != null)
                        {
                            result = rs.Location;
                        }
                    }
                }
                return result;
            }
        }

        /// <summary>Gets or sets a value indicating whether is deleted.</summary>
        public bool IsDeleted { get; set; }

        public bool IsFailedGeocode { get; set; }

        /// <summary>
        /// Indicates whether this job is awaiting a 
        /// response from the Marine Terminal
        /// before it can be processed
        /// </summary>
        public bool IsPreparatory { get; set; }

        public virtual ICollection<PlanConfig> PlanConfigs { get; set; }

        public int? AssignedDriverId { get; set; }
        public virtual Driver AssignedDriver { get; set; }

        public DateTime? AssignedDateTime { get; set; }

        public int? JobPairId { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating whether the Terminal Import job is ready for pickup or processing
        /// </summary>
        public bool? IsReady { get; set; }

        public DateTime? LocationDistanceProcessedDate { get; set; }

        public DateTime? CreatedDate { get; set; }

        public DateTime? ModifiedDate { get; set; }

        /// <summary>
        /// Gets or sets the marine terminal response regarding availabilty
        /// </summary>
        public bool? IsMarineTerminalAvailable { get; set; }

        /// <summary>
        /// Gets or sets the marine terminal LAST modified date.
        /// </summary>
        public DateTime? TerminalModifiedDate{ get; set; }

        /// <summary>
        /// Gets or sets the estimated time of arrival for the driver
        /// at the destination - currently only populated for marine terminal destined orders
        /// </summary>
        public DateTime? AlgorithmETA { get; set; }

        public DateTime? EnRouteETA { get; set; }

        public DateTime? ActualETA { get; set; }

        /// <summary>
        /// Gets or sets whether this Order/Job passes validation
        /// </summary>
        public virtual bool IsValid
        {
            get { return IsValidNullable.HasValue && IsValidNullable.Value; }
            set { IsValidNullable = value; }
        }

        public bool? IsValidNullable { get; set; }
        /// <summary>
        /// Gets or sets if the Job's route stops contains a terminal order
        /// </summary>
        public bool IsTerminalOrder { get; set; }

        /// <summary>
        /// Gets or sets if the Job is a terminal order and the order is ready for pickup
        /// </summary>
        public bool? IsTerminalReady { get; set; }

        /// <summary>
        /// Gets or sets the time in which the order was set as completed
        /// </summary>
        public DateTime? CompletedDate { get; set; }

        public JobDataCollection DataCollection { get; set; }

        public TerminalJobType GetTerminalJobType(IEnumerable<int?> terminalLocationIds)
        {
            var result = TerminalJobType.Unspecified;
            if (RouteStops != null && terminalLocationIds != null && terminalLocationIds.Any())
            {
                var matchingRouteStop = RouteStops.FirstOrDefault(p => p.LocationId != null && terminalLocationIds.Contains(p.LocationId));
                if (matchingRouteStop != null)
                {
                    switch (matchingRouteStop.StopAction.ShortName)
                    {
                        case "PEWC":
                        case "DLWC":
                            result = TerminalJobType.Export;
                            break;
                        case "PLWC":
                            result = TerminalJobType.Import;
                            break;
                    }
                }
            }
            return result;
        }

        public bool IsHazmat { get; set; }

        public bool IsFlatbed { get; set; }

        public string ShipperName { get; set; }

        public string ConsigneeName { get; set; }

        //TODO remove when DB is updated with a Priority field
        [NotMapped]
        public double Priority { get; set; }
    }
}
