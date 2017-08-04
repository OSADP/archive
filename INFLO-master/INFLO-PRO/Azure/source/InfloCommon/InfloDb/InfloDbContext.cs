/*!
    @file         InfloDb/InfloDbContext.cs
    @author       Joshua Branch

    @copyright
    Copyright (c) 2013 Battelle Memorial Institute. All rights reserved.

    @par
    Unauthorized use or duplication may violate state, federal and/or
    international laws including the Copyright Laws of the United States
    and of other international jurisdictions.

    @par
    @verbatim
    Battelle Memorial Institute
    505 King Avenue
    Columbus, Ohio  43201
    @endverbatim

    @brief
    TBD

    @details
    TBD
*/

using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;

namespace InfloCommon.InfloDb
{
    
    public partial class InfloDbContext : DbContext
    {
        public InfloDbContext(string connectionString)
            : base(connectionString)
        {
            /*
             * This GREATLY reduces the time it takes to add an entity of a large dbSet.
             */
            base.Configuration.AutoDetectChangesEnabled = false;
        }


        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }

        public DbSet<Configuration_ESS> Configuration_ESS { get; set; }
        public DbSet<Configuration_INFLOThresholds> Configuration_INFLOThresholds { get; set; }
        public DbSet<Configuration_Roadway> Configuration_Roadway { get; set; }
        public DbSet<Configuration_RoadwayESS> Configuration_RoadwayESS { get; set; }
        public DbSet<Configuration_RoadwayLinks> Configuration_RoadwayLinks { get; set; }
        public DbSet<Configuration_RoadwayLinksDetectorStation> Configuration_RoadwayLinksDetectorStation { get; set; }
        public DbSet<Configuration_RoadwayLinksESS> Configuration_RoadwayLinksESS { get; set; }
        public DbSet<Configuration_RoadwayMileMarkers> Configuration_RoadwayMileMarkers { get; set; }
        public DbSet<Configuration_RoadwaySubLinks> Configuration_RoadwaySubLinks { get; set; }
        public DbSet<Configuration_TSSDetectorStation> Configuration_TSSDetectorStation { get; set; }
        public DbSet<TME_CVData_Input> TME_CVData_Input { get; set; }
        public DbSet<TME_CVData_Input_Processed> TME_CVData_Input_Processed { get; set; }
        public DbSet<TME_CVData_SubLink> TME_CVData_SubLink { get; set; }
        public DbSet<TME_ESSData_Input> TME_ESSData_Input { get; set; }
        public DbSet<TME_ESSMobileData_Input> TME_ESSMobileData_Input { get; set; }
        public DbSet<TME_TSSData_Input> TME_TSSData_Input { get; set; }
        public DbSet<TME_TSSESS_Link> TME_TSSESS_Link { get; set; }
        public DbSet<TMEOutput_QWARN_QueueInfo> TMEOutput_QWARN_QueueInfo { get; set; }
        public DbSet<TMEOutput_QWARNMessage_CV> TMEOutput_QWARNMessage_CV { get; set; }
        public DbSet<TMEOutput_ShockWaveInformation> TMEOutput_ShockWaveInformation { get; set; }
        public DbSet<TMEOutput_SPDHARMMessage_CV> TMEOutput_SPDHARMMessage_CV { get; set; }
        public DbSet<TMEOutput_SPDHARMMessage_Infrastructure> TMEOutput_SPDHARMMessage_Infrastructure { get; set; }
        public DbSet<TMEOutput_WRTM_Alerts> TMEOutput_WRTM_Alerts { get; set; }
    }
}
