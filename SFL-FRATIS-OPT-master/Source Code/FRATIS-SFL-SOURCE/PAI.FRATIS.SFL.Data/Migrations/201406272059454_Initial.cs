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

namespace PAI.FRATIS.SFL.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Camera",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Description = c.String(),
                        Url = c.String(),
                        SubscriberId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Subscriber", t => t.SubscriberId)
                .Index(t => t.SubscriberId);
            
            CreateTable(
                "dbo.Subscriber",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(maxLength: 255),
                        IsActive = c.Boolean(nullable: false),
                        CreatedDate = c.DateTime(),
                        ModifiedDate = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.User",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Username = c.String(maxLength: 255),
                        Password = c.String(),
                        PasswordSalt = c.String(),
                        PasswordFormat = c.Int(nullable: false),
                        FailedLogins = c.Int(nullable: false),
                        LockedUntil = c.DateTime(),
                        Active = c.Boolean(nullable: false),
                        Deleted = c.Boolean(nullable: false),
                        LastIpAddress = c.String(),
                        LastLoginDate = c.DateTime(),
                        LastActivityDate = c.DateTime(nullable: false),
                        TimeZoneId = c.String(),
                        IsAdmin = c.Boolean(nullable: false),
                        CreatedDate = c.DateTime(),
                        ModifiedDate = c.DateTime(),
                        SubscriberId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Subscriber", t => t.SubscriberId, cascadeDelete: true)
                .Index(t => t.SubscriberId);
            
            CreateTable(
                "dbo.Chassis",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Enabled = c.Boolean(nullable: false),
                        DisplayName = c.String(),
                        IsDomestic = c.Boolean(),
                        SubscriberId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Subscriber", t => t.SubscriberId)
                .Index(t => t.SubscriberId);
            
            CreateTable(
                "dbo.Container",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Enabled = c.Boolean(nullable: false),
                        DisplayName = c.String(),
                        IsDomestic = c.Boolean(nullable: false),
                        SubscriberId = c.Int(),
                        ContainerOwner_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Subscriber", t => t.SubscriberId)
                .ForeignKey("dbo.ContainerOwner", t => t.ContainerOwner_Id)
                .Index(t => t.SubscriberId)
                .Index(t => t.ContainerOwner_Id);
            
            CreateTable(
                "dbo.ChassisOwner",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        DisplayName = c.String(),
                        SubscriberId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Subscriber", t => t.SubscriberId)
                .Index(t => t.SubscriberId);
            
            CreateTable(
                "dbo.ContainerOwner",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ShortName = c.String(),
                        DisplayName = c.String(),
                        IsDeleted = c.Boolean(nullable: false),
                        IsDomestic = c.Boolean(),
                        SubscriberId = c.Int(),
                        AllowedChassisOwner_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ChassisOwner", t => t.AllowedChassisOwner_Id)
                .ForeignKey("dbo.Subscriber", t => t.SubscriberId)
                .Index(t => t.AllowedChassisOwner_Id)
                .Index(t => t.SubscriberId);
            
            CreateTable(
                "dbo.Driver",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        FirstName = c.String(),
                        LastName = c.String(),
                        DriverType = c.String(),
                        Phone = c.String(),
                        Email = c.String(),
                        StartingLocationId = c.Int(),
                        EarliestStartTime = c.Long(nullable: false),
                        AvailableDutyHours = c.Double(nullable: false),
                        AvailableDrivingHours = c.Double(nullable: false),
                        IsPlaceholderDriver = c.Boolean(nullable: false),
                        Position_TimeStamp = c.DateTime(),
                        Position_Description = c.String(),
                        Position_PositionText = c.String(),
                        Position_LegacyId = c.Int(nullable: false),
                        Position_Latitude = c.Double(),
                        Position_Longitude = c.Double(),
                        SubscriberId = c.Int(),
                        PlanConfig_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Location", t => t.StartingLocationId)
                .ForeignKey("dbo.Subscriber", t => t.SubscriberId)
                .ForeignKey("dbo.PlanConfig", t => t.PlanConfig_Id)
                .Index(t => t.StartingLocationId)
                .Index(t => t.SubscriberId)
                .Index(t => t.PlanConfig_Id);
            
            CreateTable(
                "dbo.JobGroup",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Ordering = c.Int(nullable: false),
                        ShiftStartTime = c.Time(precision: 7),
                        SubscriberId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Subscriber", t => t.SubscriberId)
                .Index(t => t.SubscriberId);
            
            CreateTable(
                "dbo.Location",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        WebFleetId = c.String(),
                        Email = c.String(),
                        DisplayName = c.String(),
                        StreetNumber = c.String(),
                        Street = c.String(),
                        City = c.String(),
                        State = c.String(),
                        Zip = c.String(),
                        Longitude = c.Double(),
                        Latitude = c.Double(),
                        Note = c.String(),
                        LegacyId = c.String(),
                        Phone = c.String(),
                        WaitingTime = c.Int(),
                        IsDeleted = c.Boolean(nullable: false),
                        SubscriberId = c.Int(),
                        LocationGroup_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Subscriber", t => t.SubscriberId)
                .ForeignKey("dbo.LocationGroup", t => t.LocationGroup_Id)
                .Index(t => t.SubscriberId)
                .Index(t => t.LocationGroup_Id);
            
            CreateTable(
                "dbo.ExternalSync",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        OrganizationId = c.Int(),
                        TimeStamp = c.DateTime(),
                        CreatedDate = c.DateTime(),
                        ModifiedDate = c.DateTime(),
                        SubscriberId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Subscriber", t => t.SubscriberId)
                .Index(t => t.SubscriberId);
            
            CreateTable(
                "dbo.JobAcceptance",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        JobId = c.Int(),
                        AcceptedTime = c.DateTime(),
                        ETATime = c.DateTime(),
                        ModifiedTime = c.DateTime(),
                        DriverId = c.Int(),
                        SubscriberId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Job", t => t.JobId)
                .ForeignKey("dbo.Subscriber", t => t.SubscriberId)
                .Index(t => t.JobId)
                .Index(t => t.SubscriberId);
            
            CreateTable(
                "dbo.Job",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        OrderNumber = c.String(),
                        ContainerNumber = c.String(),
                        TrailerId = c.String(),
                        BillOfLading = c.String(),
                        DriverSortOrder = c.Int(nullable: false),
                        IsTransmitted = c.Boolean(nullable: false),
                        DueDate = c.DateTime(),
                        SyncDate = c.DateTime(),
                        JobGroupId = c.Int(),
                        JobTemplateType = c.Int(nullable: false),
                        ChassisId = c.Int(),
                        ChassisOwnerId = c.Int(),
                        ContainerId = c.Int(),
                        ContainerOwnerId = c.Int(),
                        PickupNumber = c.String(),
                        BookingNumber = c.String(),
                        Notes = c.String(),
                        JobStatus = c.Int(nullable: false),
                        IsDeleted = c.Boolean(nullable: false),
                        IsPreparatory = c.Boolean(nullable: false),
                        AssignedDriverId = c.Int(),
                        AssignedDateTime = c.DateTime(),
                        JobPairId = c.Int(),
                        IsReady = c.Boolean(),
                        LocationDistanceProcessedDate = c.DateTime(),
                        CreatedDate = c.DateTime(),
                        ModifiedDate = c.DateTime(),
                        IsMarineTerminalAvailable = c.Boolean(),
                        TerminalModifiedDate = c.DateTime(),
                        AlgorithmETA = c.DateTime(),
                        EnRouteETA = c.DateTime(),
                        ActualETA = c.DateTime(),
                        IsValid = c.Boolean(nullable: false),
                        IsTerminalOrder = c.Boolean(nullable: false),
                        IsTerminalReady = c.Boolean(),
                        CompletedDate = c.DateTime(),
                        DataCollection_DriverId = c.Int(),
                        DataCollection_CreatedDateTime = c.DateTime(),
                        DataCollection_CreatedUserId = c.Int(),
                        DataCollection_AssignedUserId = c.Int(),
                        DataCollection_AssignedDateTime = c.DateTime(),
                        DataCollection_CompletedDateTime = c.DateTime(),
                        DataCollection_CompletedUserId = c.Int(),
                        SubscriberId = c.Int(),
                        Plan_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Driver", t => t.AssignedDriverId)
                .ForeignKey("dbo.Chassis", t => t.ChassisId)
                .ForeignKey("dbo.ChassisOwner", t => t.ChassisOwnerId)
                .ForeignKey("dbo.Container", t => t.ContainerId)
                .ForeignKey("dbo.ContainerOwner", t => t.ContainerOwnerId)
                .ForeignKey("dbo.JobGroup", t => t.JobGroupId)
                .ForeignKey("dbo.Subscriber", t => t.SubscriberId)
                .ForeignKey("dbo.Plan", t => t.Plan_Id)
                .Index(t => t.AssignedDriverId)
                .Index(t => t.ChassisId)
                .Index(t => t.ChassisOwnerId)
                .Index(t => t.ContainerId)
                .Index(t => t.ContainerOwnerId)
                .Index(t => t.JobGroupId)
                .Index(t => t.SubscriberId)
                .Index(t => t.Plan_Id);
            
            CreateTable(
                "dbo.PlanConfig",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        DueDate = c.DateTime(nullable: false),
                        JobGroupId = c.Int(),
                        ShiftStartTime = c.Time(precision: 7),
                        CreatedDate = c.DateTime(),
                        ModifiedDate = c.DateTime(),
                        SubscriberId = c.Int(),
                        DefaultDriver_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Driver", t => t.DefaultDriver_Id)
                .ForeignKey("dbo.JobGroup", t => t.JobGroupId)
                .ForeignKey("dbo.Subscriber", t => t.SubscriberId)
                .Index(t => t.DefaultDriver_Id)
                .Index(t => t.JobGroupId)
                .Index(t => t.SubscriberId);
            
            CreateTable(
                "dbo.RouteStop",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        JobId = c.Int(),
                        SortOrder = c.Int(nullable: false),
                        IsDynamicStop = c.Boolean(nullable: false),
                        StopActionId = c.Int(),
                        LocationId = c.Int(),
                        WindowStart = c.Long(nullable: false),
                        WindowEnd = c.Long(nullable: false),
                        StopDelay = c.Long(),
                        CreatedDate = c.DateTime(),
                        ModifiedDate = c.DateTime(),
                        EstimatedETA = c.DateTime(),
                        ActualETA = c.DateTime(),
                        SubscriberId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Job", t => t.JobId)
                .ForeignKey("dbo.Location", t => t.LocationId)
                .ForeignKey("dbo.StopAction", t => t.StopActionId)
                .ForeignKey("dbo.Subscriber", t => t.SubscriberId)
                .Index(t => t.JobId)
                .Index(t => t.LocationId)
                .Index(t => t.StopActionId)
                .Index(t => t.SubscriberId);
            
            CreateTable(
                "dbo.StopAction",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        ShortName = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.LocationDistance",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        DayOfWeek = c.Int(),
                        DepartureDay = c.DateTime(),
                        EndLocationId = c.Int(),
                        StartLocationId = c.Int(),
                        CreatedDate = c.DateTime(),
                        ModifiedDate = c.DateTime(),
                        Distance = c.Decimal(precision: 18, scale: 2),
                        TravelTime = c.Long(),
                        Hour0_TravelTime = c.Long(),
                        Hour1_TravelTime = c.Long(),
                        Hour2_TravelTime = c.Long(),
                        Hour3_TravelTime = c.Long(),
                        Hour4_TravelTime = c.Long(),
                        Hour5_TravelTime = c.Long(),
                        Hour6_TravelTime = c.Long(),
                        Hour7_TravelTime = c.Long(),
                        Hour8_TravelTime = c.Long(),
                        Hour9_TravelTime = c.Long(),
                        Hour10_TravelTime = c.Long(),
                        Hour11_TravelTime = c.Long(),
                        Hour12_TravelTime = c.Long(),
                        Hour13_TravelTime = c.Long(),
                        Hour14_TravelTime = c.Long(),
                        Hour15_TravelTime = c.Long(),
                        Hour16_TravelTime = c.Long(),
                        Hour17_TravelTime = c.Long(),
                        Hour18_TravelTime = c.Long(),
                        Hour19_TravelTime = c.Long(),
                        Hour20_TravelTime = c.Long(),
                        Hour21_TravelTime = c.Long(),
                        Hour22_TravelTime = c.Long(),
                        Hour23_TravelTime = c.Long(),
                        SubscriberId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Location", t => t.EndLocationId)
                .ForeignKey("dbo.Location", t => t.StartLocationId)
                .ForeignKey("dbo.Subscriber", t => t.SubscriberId)
                .Index(t => t.EndLocationId)
                .Index(t => t.StartLocationId)
                .Index(t => t.SubscriberId);
            
            CreateTable(
                "dbo.LocationGroup",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        ParentId = c.Int(),
                        IsHomeLocation = c.Boolean(nullable: false),
                        SubscriberId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.LocationGroup", t => t.ParentId)
                .ForeignKey("dbo.Subscriber", t => t.SubscriberId)
                .Index(t => t.ParentId)
                .Index(t => t.SubscriberId);
            
            CreateTable(
                "dbo.LocationHours",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        StartTime = c.DateTime(),
                        EndTime = c.DateTime(),
                        IsPeak = c.Boolean(),
                        SubscriberId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Subscriber", t => t.SubscriberId)
                .Index(t => t.SubscriberId);
            
            CreateTable(
                "dbo.LocationHoursDayOfWeek",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        DayOfWeek = c.Int(nullable: false),
                        SubscriberId = c.Int(),
                        LocationHours_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Subscriber", t => t.SubscriberId)
                .ForeignKey("dbo.LocationHours", t => t.LocationHours_Id)
                .Index(t => t.SubscriberId)
                .Index(t => t.LocationHours_Id);
            
            CreateTable(
                "dbo.PlanDriverJob",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        JobId = c.Int(nullable: false),
                        SortOrder = c.Int(nullable: false),
                        DepartureTime = c.Long(nullable: false),
                        SubscriberId = c.Int(),
                        PlanDriver_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Job", t => t.JobId, cascadeDelete: true)
                .ForeignKey("dbo.PlanDriver", t => t.PlanDriver_Id)
                .ForeignKey("dbo.Subscriber", t => t.SubscriberId)
                .Index(t => t.JobId)
                .Index(t => t.PlanDriver_Id)
                .Index(t => t.SubscriberId);
            
            CreateTable(
                "dbo.PlanDriver",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        DriverId = c.Int(nullable: false),
                        DepartureTime = c.Long(nullable: false),
                        SubscriberId = c.Int(),
                        Plan_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Driver", t => t.DriverId, cascadeDelete: true)
                .ForeignKey("dbo.Subscriber", t => t.SubscriberId)
                .ForeignKey("dbo.Plan", t => t.Plan_Id)
                .Index(t => t.DriverId)
                .Index(t => t.SubscriberId)
                .Index(t => t.Plan_Id);
            
            CreateTable(
                "dbo.Plan",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Run = c.Int(nullable: false),
                        IsAccepted = c.Boolean(nullable: false),
                        UserCreated = c.Boolean(nullable: false),
                        UserModified = c.Boolean(nullable: false),
                        Transmitted = c.Boolean(nullable: false),
                        PlanConfigId = c.Int(),
                        CreatedDate = c.DateTime(),
                        ModifiedDate = c.DateTime(),
                        SubscriberId = c.Int(),
                        JobGroup_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.JobGroup", t => t.JobGroup_Id)
                .ForeignKey("dbo.PlanConfig", t => t.PlanConfigId)
                .ForeignKey("dbo.Subscriber", t => t.SubscriberId)
                .Index(t => t.JobGroup_Id)
                .Index(t => t.PlanConfigId)
                .Index(t => t.SubscriberId);
            
            CreateTable(
                "dbo.QueueDeviceSegment",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        DisplayName = c.String(),
                        Device1Identifier = c.String(maxLength: 25),
                        Device2Identifier = c.String(maxLength: 25),
                        VehicleCount = c.Int(nullable: false),
                        TotalDelay = c.Long(nullable: false),
                        CurrentAverage = c.Int(nullable: false),
                        CreatedDate = c.DateTime(),
                        ModifiedDate = c.DateTime(),
                        Segment = c.Int(nullable: false),
                        SubscriberId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Subscriber", t => t.SubscriberId)
                .Index(t => t.SubscriberId);
            
            CreateTable(
                "dbo.RouteSegmentMetric",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        JobId = c.Int(),
                        PlanDriverId = c.Int(),
                        StartStopId = c.Int(),
                        EndStopId = c.Int(),
                        TruckState = c.Int(nullable: false),
                        StartTime = c.Long(),
                        TotalExecutionTime = c.Long(nullable: false),
                        TotalTravelTime = c.Long(nullable: false),
                        TotalTravelDistance = c.Decimal(nullable: false, precision: 18, scale: 2),
                        SortOrder = c.Int(nullable: false),
                        TotalIdleTime = c.Long(nullable: false),
                        TotalQueueTime = c.Long(nullable: false),
                        SubscriberId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.RouteStop", t => t.EndStopId)
                .ForeignKey("dbo.PlanDriver", t => t.PlanDriverId)
                .ForeignKey("dbo.RouteStop", t => t.StartStopId)
                .ForeignKey("dbo.Subscriber", t => t.SubscriberId)
                .Index(t => t.EndStopId)
                .Index(t => t.PlanDriverId)
                .Index(t => t.StartStopId)
                .Index(t => t.SubscriberId);
            
            CreateTable(
                "dbo.Setting",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Value = c.String(),
                        SubscriberId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Subscriber", t => t.SubscriberId)
                .Index(t => t.SubscriberId);
            
            CreateTable(
                "dbo.State",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Abbreviation = c.String(),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.TrafficHotSpot",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Latitude = c.Double(),
                        Longitude = c.Double(),
                        SubscriberId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Subscriber", t => t.SubscriberId)
                .Index(t => t.SubscriberId);
            
            CreateTable(
                "dbo.TrafficRoute",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        StartLatitude = c.Double(),
                        StartLongitude = c.Double(),
                        EndLatitude = c.Double(),
                        EndLongitude = c.Double(),
                        CreatedDate = c.DateTime(),
                        ModifiedDate = c.DateTime(),
                        ResultUpdatedTime = c.DateTime(),
                        Result = c.String(),
                        SubscriberId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Subscriber", t => t.SubscriberId)
                .Index(t => t.SubscriberId);
            
            CreateTable(
                "dbo.Vehicle",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        LegacyId = c.String(),
                        DriverId = c.Int(),
                        VehicleType = c.String(),
                        PositionText = c.String(),
                        Position_TimeStamp = c.DateTime(),
                        Position_Description = c.String(),
                        Position_PositionText = c.String(),
                        Position_LegacyId = c.Int(nullable: false),
                        Position_Latitude = c.Double(),
                        Position_Longitude = c.Double(),
                        SubscriberId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Driver", t => t.DriverId)
                .ForeignKey("dbo.Subscriber", t => t.SubscriberId)
                .Index(t => t.DriverId)
                .Index(t => t.SubscriberId);
            
            CreateTable(
                "dbo.WeatherCity",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        DisplayName = c.String(),
                        CityCode = c.String(),
                        LastUpdated = c.DateTime(),
                        WeatherData = c.String(),
                        SubscriberId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Subscriber", t => t.SubscriberId)
                .Index(t => t.SubscriberId);
            
            CreateTable(
                "dbo.ContainerXChassis",
                c => new
                    {
                        Container_Id = c.Int(nullable: false),
                        Chassis_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.Container_Id, t.Chassis_Id })
                .ForeignKey("dbo.Container", t => t.Container_Id, cascadeDelete: true)
                .ForeignKey("dbo.Chassis", t => t.Chassis_Id, cascadeDelete: true)
                .Index(t => t.Container_Id)
                .Index(t => t.Chassis_Id);
            
            CreateTable(
                "dbo.JobGroupDriver",
                c => new
                    {
                        JobGroup_Id = c.Int(nullable: false),
                        Driver_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.JobGroup_Id, t.Driver_Id })
                .ForeignKey("dbo.JobGroup", t => t.JobGroup_Id, cascadeDelete: true)
                .ForeignKey("dbo.Driver", t => t.Driver_Id, cascadeDelete: true)
                .Index(t => t.JobGroup_Id)
                .Index(t => t.Driver_Id);
            
            CreateTable(
                "dbo.PlanConfigJob",
                c => new
                    {
                        PlanConfig_Id = c.Int(nullable: false),
                        Job_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.PlanConfig_Id, t.Job_Id })
                .ForeignKey("dbo.PlanConfig", t => t.PlanConfig_Id, cascadeDelete: true)
                .ForeignKey("dbo.Job", t => t.Job_Id, cascadeDelete: true)
                .Index(t => t.PlanConfig_Id)
                .Index(t => t.Job_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.WeatherCity", "SubscriberId", "dbo.Subscriber");
            DropForeignKey("dbo.Vehicle", "SubscriberId", "dbo.Subscriber");
            DropForeignKey("dbo.Vehicle", "DriverId", "dbo.Driver");
            DropForeignKey("dbo.TrafficRoute", "SubscriberId", "dbo.Subscriber");
            DropForeignKey("dbo.TrafficHotSpot", "SubscriberId", "dbo.Subscriber");
            DropForeignKey("dbo.Setting", "SubscriberId", "dbo.Subscriber");
            DropForeignKey("dbo.RouteSegmentMetric", "SubscriberId", "dbo.Subscriber");
            DropForeignKey("dbo.RouteSegmentMetric", "StartStopId", "dbo.RouteStop");
            DropForeignKey("dbo.RouteSegmentMetric", "PlanDriverId", "dbo.PlanDriver");
            DropForeignKey("dbo.RouteSegmentMetric", "EndStopId", "dbo.RouteStop");
            DropForeignKey("dbo.QueueDeviceSegment", "SubscriberId", "dbo.Subscriber");
            DropForeignKey("dbo.Job", "Plan_Id", "dbo.Plan");
            DropForeignKey("dbo.Plan", "SubscriberId", "dbo.Subscriber");
            DropForeignKey("dbo.Plan", "PlanConfigId", "dbo.PlanConfig");
            DropForeignKey("dbo.Plan", "JobGroup_Id", "dbo.JobGroup");
            DropForeignKey("dbo.PlanDriver", "Plan_Id", "dbo.Plan");
            DropForeignKey("dbo.PlanDriverJob", "SubscriberId", "dbo.Subscriber");
            DropForeignKey("dbo.PlanDriver", "SubscriberId", "dbo.Subscriber");
            DropForeignKey("dbo.PlanDriverJob", "PlanDriver_Id", "dbo.PlanDriver");
            DropForeignKey("dbo.PlanDriver", "DriverId", "dbo.Driver");
            DropForeignKey("dbo.PlanDriverJob", "JobId", "dbo.Job");
            DropForeignKey("dbo.LocationHours", "SubscriberId", "dbo.Subscriber");
            DropForeignKey("dbo.LocationHoursDayOfWeek", "LocationHours_Id", "dbo.LocationHours");
            DropForeignKey("dbo.LocationHoursDayOfWeek", "SubscriberId", "dbo.Subscriber");
            DropForeignKey("dbo.LocationGroup", "SubscriberId", "dbo.Subscriber");
            DropForeignKey("dbo.Location", "LocationGroup_Id", "dbo.LocationGroup");
            DropForeignKey("dbo.LocationGroup", "ParentId", "dbo.LocationGroup");
            DropForeignKey("dbo.LocationDistance", "SubscriberId", "dbo.Subscriber");
            DropForeignKey("dbo.LocationDistance", "StartLocationId", "dbo.Location");
            DropForeignKey("dbo.LocationDistance", "EndLocationId", "dbo.Location");
            DropForeignKey("dbo.JobAcceptance", "SubscriberId", "dbo.Subscriber");
            DropForeignKey("dbo.JobAcceptance", "JobId", "dbo.Job");
            DropForeignKey("dbo.Job", "SubscriberId", "dbo.Subscriber");
            DropForeignKey("dbo.RouteStop", "SubscriberId", "dbo.Subscriber");
            DropForeignKey("dbo.RouteStop", "StopActionId", "dbo.StopAction");
            DropForeignKey("dbo.RouteStop", "LocationId", "dbo.Location");
            DropForeignKey("dbo.RouteStop", "JobId", "dbo.Job");
            DropForeignKey("dbo.PlanConfig", "SubscriberId", "dbo.Subscriber");
            DropForeignKey("dbo.PlanConfigJob", "Job_Id", "dbo.Job");
            DropForeignKey("dbo.PlanConfigJob", "PlanConfig_Id", "dbo.PlanConfig");
            DropForeignKey("dbo.PlanConfig", "JobGroupId", "dbo.JobGroup");
            DropForeignKey("dbo.Driver", "PlanConfig_Id", "dbo.PlanConfig");
            DropForeignKey("dbo.PlanConfig", "DefaultDriver_Id", "dbo.Driver");
            DropForeignKey("dbo.Job", "JobGroupId", "dbo.JobGroup");
            DropForeignKey("dbo.Job", "ContainerOwnerId", "dbo.ContainerOwner");
            DropForeignKey("dbo.Job", "ContainerId", "dbo.Container");
            DropForeignKey("dbo.Job", "ChassisOwnerId", "dbo.ChassisOwner");
            DropForeignKey("dbo.Job", "ChassisId", "dbo.Chassis");
            DropForeignKey("dbo.Job", "AssignedDriverId", "dbo.Driver");
            DropForeignKey("dbo.ExternalSync", "SubscriberId", "dbo.Subscriber");
            DropForeignKey("dbo.Driver", "SubscriberId", "dbo.Subscriber");
            DropForeignKey("dbo.Driver", "StartingLocationId", "dbo.Location");
            DropForeignKey("dbo.Location", "SubscriberId", "dbo.Subscriber");
            DropForeignKey("dbo.JobGroup", "SubscriberId", "dbo.Subscriber");
            DropForeignKey("dbo.JobGroupDriver", "Driver_Id", "dbo.Driver");
            DropForeignKey("dbo.JobGroupDriver", "JobGroup_Id", "dbo.JobGroup");
            DropForeignKey("dbo.ContainerOwner", "SubscriberId", "dbo.Subscriber");
            DropForeignKey("dbo.Container", "ContainerOwner_Id", "dbo.ContainerOwner");
            DropForeignKey("dbo.ContainerOwner", "AllowedChassisOwner_Id", "dbo.ChassisOwner");
            DropForeignKey("dbo.ChassisOwner", "SubscriberId", "dbo.Subscriber");
            DropForeignKey("dbo.Chassis", "SubscriberId", "dbo.Subscriber");
            DropForeignKey("dbo.Container", "SubscriberId", "dbo.Subscriber");
            DropForeignKey("dbo.ContainerXChassis", "Chassis_Id", "dbo.Chassis");
            DropForeignKey("dbo.ContainerXChassis", "Container_Id", "dbo.Container");
            DropForeignKey("dbo.Camera", "SubscriberId", "dbo.Subscriber");
            DropForeignKey("dbo.User", "SubscriberId", "dbo.Subscriber");
            DropIndex("dbo.WeatherCity", new[] { "SubscriberId" });
            DropIndex("dbo.Vehicle", new[] { "SubscriberId" });
            DropIndex("dbo.Vehicle", new[] { "DriverId" });
            DropIndex("dbo.TrafficRoute", new[] { "SubscriberId" });
            DropIndex("dbo.TrafficHotSpot", new[] { "SubscriberId" });
            DropIndex("dbo.Setting", new[] { "SubscriberId" });
            DropIndex("dbo.RouteSegmentMetric", new[] { "SubscriberId" });
            DropIndex("dbo.RouteSegmentMetric", new[] { "StartStopId" });
            DropIndex("dbo.RouteSegmentMetric", new[] { "PlanDriverId" });
            DropIndex("dbo.RouteSegmentMetric", new[] { "EndStopId" });
            DropIndex("dbo.QueueDeviceSegment", new[] { "SubscriberId" });
            DropIndex("dbo.Job", new[] { "Plan_Id" });
            DropIndex("dbo.Plan", new[] { "SubscriberId" });
            DropIndex("dbo.Plan", new[] { "PlanConfigId" });
            DropIndex("dbo.Plan", new[] { "JobGroup_Id" });
            DropIndex("dbo.PlanDriver", new[] { "Plan_Id" });
            DropIndex("dbo.PlanDriverJob", new[] { "SubscriberId" });
            DropIndex("dbo.PlanDriver", new[] { "SubscriberId" });
            DropIndex("dbo.PlanDriverJob", new[] { "PlanDriver_Id" });
            DropIndex("dbo.PlanDriver", new[] { "DriverId" });
            DropIndex("dbo.PlanDriverJob", new[] { "JobId" });
            DropIndex("dbo.LocationHours", new[] { "SubscriberId" });
            DropIndex("dbo.LocationHoursDayOfWeek", new[] { "LocationHours_Id" });
            DropIndex("dbo.LocationHoursDayOfWeek", new[] { "SubscriberId" });
            DropIndex("dbo.LocationGroup", new[] { "SubscriberId" });
            DropIndex("dbo.Location", new[] { "LocationGroup_Id" });
            DropIndex("dbo.LocationGroup", new[] { "ParentId" });
            DropIndex("dbo.LocationDistance", new[] { "SubscriberId" });
            DropIndex("dbo.LocationDistance", new[] { "StartLocationId" });
            DropIndex("dbo.LocationDistance", new[] { "EndLocationId" });
            DropIndex("dbo.JobAcceptance", new[] { "SubscriberId" });
            DropIndex("dbo.JobAcceptance", new[] { "JobId" });
            DropIndex("dbo.Job", new[] { "SubscriberId" });
            DropIndex("dbo.RouteStop", new[] { "SubscriberId" });
            DropIndex("dbo.RouteStop", new[] { "StopActionId" });
            DropIndex("dbo.RouteStop", new[] { "LocationId" });
            DropIndex("dbo.RouteStop", new[] { "JobId" });
            DropIndex("dbo.PlanConfig", new[] { "SubscriberId" });
            DropIndex("dbo.PlanConfigJob", new[] { "Job_Id" });
            DropIndex("dbo.PlanConfigJob", new[] { "PlanConfig_Id" });
            DropIndex("dbo.PlanConfig", new[] { "JobGroupId" });
            DropIndex("dbo.Driver", new[] { "PlanConfig_Id" });
            DropIndex("dbo.PlanConfig", new[] { "DefaultDriver_Id" });
            DropIndex("dbo.Job", new[] { "JobGroupId" });
            DropIndex("dbo.Job", new[] { "ContainerOwnerId" });
            DropIndex("dbo.Job", new[] { "ContainerId" });
            DropIndex("dbo.Job", new[] { "ChassisOwnerId" });
            DropIndex("dbo.Job", new[] { "ChassisId" });
            DropIndex("dbo.Job", new[] { "AssignedDriverId" });
            DropIndex("dbo.ExternalSync", new[] { "SubscriberId" });
            DropIndex("dbo.Driver", new[] { "SubscriberId" });
            DropIndex("dbo.Driver", new[] { "StartingLocationId" });
            DropIndex("dbo.Location", new[] { "SubscriberId" });
            DropIndex("dbo.JobGroup", new[] { "SubscriberId" });
            DropIndex("dbo.JobGroupDriver", new[] { "Driver_Id" });
            DropIndex("dbo.JobGroupDriver", new[] { "JobGroup_Id" });
            DropIndex("dbo.ContainerOwner", new[] { "SubscriberId" });
            DropIndex("dbo.Container", new[] { "ContainerOwner_Id" });
            DropIndex("dbo.ContainerOwner", new[] { "AllowedChassisOwner_Id" });
            DropIndex("dbo.ChassisOwner", new[] { "SubscriberId" });
            DropIndex("dbo.Chassis", new[] { "SubscriberId" });
            DropIndex("dbo.Container", new[] { "SubscriberId" });
            DropIndex("dbo.ContainerXChassis", new[] { "Chassis_Id" });
            DropIndex("dbo.ContainerXChassis", new[] { "Container_Id" });
            DropIndex("dbo.Camera", new[] { "SubscriberId" });
            DropIndex("dbo.User", new[] { "SubscriberId" });
            DropTable("dbo.PlanConfigJob");
            DropTable("dbo.JobGroupDriver");
            DropTable("dbo.ContainerXChassis");
            DropTable("dbo.WeatherCity");
            DropTable("dbo.Vehicle");
            DropTable("dbo.TrafficRoute");
            DropTable("dbo.TrafficHotSpot");
            DropTable("dbo.State");
            DropTable("dbo.Setting");
            DropTable("dbo.RouteSegmentMetric");
            DropTable("dbo.QueueDeviceSegment");
            DropTable("dbo.Plan");
            DropTable("dbo.PlanDriver");
            DropTable("dbo.PlanDriverJob");
            DropTable("dbo.LocationHoursDayOfWeek");
            DropTable("dbo.LocationHours");
            DropTable("dbo.LocationGroup");
            DropTable("dbo.LocationDistance");
            DropTable("dbo.StopAction");
            DropTable("dbo.RouteStop");
            DropTable("dbo.PlanConfig");
            DropTable("dbo.Job");
            DropTable("dbo.JobAcceptance");
            DropTable("dbo.ExternalSync");
            DropTable("dbo.Location");
            DropTable("dbo.JobGroup");
            DropTable("dbo.Driver");
            DropTable("dbo.ContainerOwner");
            DropTable("dbo.ChassisOwner");
            DropTable("dbo.Container");
            DropTable("dbo.Chassis");
            DropTable("dbo.User");
            DropTable("dbo.Subscriber");
            DropTable("dbo.Camera");
        }
    }
}
