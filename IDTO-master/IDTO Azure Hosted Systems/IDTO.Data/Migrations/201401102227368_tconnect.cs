namespace IDTO.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class tconnect : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.TConnectedVehicles",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        OriginallyScheduledDeparture = c.DateTime(nullable: false),
                        CurrentAcceptedHoldMinutes = c.Int(nullable: false),
                        TConnectStopCode = c.String(nullable: false),
                        ModifiedDate = c.DateTime(nullable: false),
                        ModifiedBy = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.TConnectOpportunities",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        CheckpointProviderId = c.Int(nullable: false),
                        CheckpointStopCode = c.String(nullable: false, maxLength: 20),
                        CheckpointRoute = c.String(maxLength: 100),
                        TConnectProviderId = c.Int(nullable: false),
                        TConnectStopCode = c.String(nullable: false, maxLength: 20),
                        TConnectRoute = c.String(maxLength: 100),
                        ModifiedDate = c.DateTime(nullable: false),
                        ModifiedBy = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Providers", t => t.CheckpointProviderId, cascadeDelete: false)
                .ForeignKey("dbo.Providers", t => t.TConnectProviderId, cascadeDelete: false)
                .Index(t => t.CheckpointProviderId)
                .Index(t => t.TConnectProviderId);
            
            CreateTable(
                "dbo.TConnectRequests",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        TConnectStatusId = c.Int(nullable: false),
                        TConnectId = c.Int(nullable: false),
                        TConnectedVehicleId = c.Int(nullable: false),
                        EstimatedTimeArrival = c.DateTime(nullable: false),
                        RequestedHoldMinutes = c.Int(nullable: false),
                        ModifiedDate = c.DateTime(nullable: false),
                        ModifiedBy = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.TConnects", t => t.TConnectId, cascadeDelete: true)
                .ForeignKey("dbo.TConnectedVehicles", t => t.TConnectedVehicleId, cascadeDelete: true)
                .ForeignKey("dbo.TConnectStatus", t => t.TConnectStatusId, cascadeDelete: false)
                .Index(t => t.TConnectId)
                .Index(t => t.TConnectedVehicleId)
                .Index(t => t.TConnectStatusId);
            
            CreateTable(
                "dbo.TConnects",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        TConnectStatusId = c.Int(nullable: false),
                        InboundStepId = c.Int(nullable: false),
                        OutboundStepId = c.Int(nullable: false),
                        InboundVehicle = c.String(),
                        StartWindow = c.DateTime(),
                        EndWindow = c.DateTime(),
                        CreatedDate = c.DateTime(nullable: false),
                        ModifiedDate = c.DateTime(nullable: false),
                        ModifiedBy = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Steps", t => t.InboundStepId, cascadeDelete: false)
                .ForeignKey("dbo.Steps", t => t.OutboundStepId, cascadeDelete: false)
                .ForeignKey("dbo.TConnectStatus", t => t.TConnectStatusId, cascadeDelete: true)
                .Index(t => t.InboundStepId)
                .Index(t => t.OutboundStepId)
                .Index(t => t.TConnectStatusId);
            
            CreateTable(
                "dbo.TConnectStatus",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.Id);

            //manually added
            CreateIndex("TConnectOpportunities", new[] { "CheckpointStopCode", "CheckpointRoute" ,
            "TConnectStopCode", "TConnectRoute" }, true, "IX_UniqueConnection");
    
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.TConnectRequests", "TConnectStatusId", "dbo.TConnectStatus");
            DropForeignKey("dbo.TConnectRequests", "TConnectedVehicleId", "dbo.TConnectedVehicles");
            DropForeignKey("dbo.TConnectRequests", "TConnectId", "dbo.TConnects");
            DropForeignKey("dbo.TConnects", "TConnectStatusId", "dbo.TConnectStatus");
            DropForeignKey("dbo.TConnects", "OutboundStepId", "dbo.Steps");
            DropForeignKey("dbo.TConnects", "InboundStepId", "dbo.Steps");
            DropForeignKey("dbo.TConnectOpportunities", "TConnectProviderId", "dbo.Providers");
            DropForeignKey("dbo.TConnectOpportunities", "CheckpointProviderId", "dbo.Providers");
            DropIndex("dbo.TConnectRequests", new[] { "TConnectStatusId" });
            DropIndex("dbo.TConnectRequests", new[] { "TConnectedVehicleId" });
            DropIndex("dbo.TConnectRequests", new[] { "TConnectId" });
            DropIndex("dbo.TConnects", new[] { "TConnectStatusId" });
            DropIndex("dbo.TConnects", new[] { "OutboundStepId" });
            DropIndex("dbo.TConnects", new[] { "InboundStepId" });
            DropIndex("dbo.TConnectOpportunities", new[] { "TConnectProviderId" });
            DropIndex("dbo.TConnectOpportunities", new[] { "CheckpointProviderId" });
            DropTable("dbo.TConnectStatus");
            DropTable("dbo.TConnects");
            DropTable("dbo.TConnectRequests");
            DropTable("dbo.TConnectOpportunities");
            DropTable("dbo.TConnectedVehicles");


            DropIndex("TConnectOpportunities", new[] { "CheckpointStopCode", "CheckpointRoute" ,
            "TConnectStopCode", "TConnectRoute" });
    
        }
    }
}
