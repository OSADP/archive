namespace IDTO.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class TravelerTripFull : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Steps",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        TripId = c.Int(nullable: false),
                        StepNumber = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Trips", t => t.TripId, cascadeDelete: true)
                .Index(t => t.TripId);
            
            AddColumn("dbo.Travelers", "Email", c => c.String(nullable: false, maxLength: 255));
            AddColumn("dbo.Travelers", "PhoneNumber", c => c.String(nullable: false));
            AddColumn("dbo.Travelers", "CreatedDate", c => c.DateTime(nullable: false));
            AddColumn("dbo.Travelers", "InformedConsent", c => c.Boolean(nullable: false));
            AddColumn("dbo.Travelers", "InformedConsentDate", c => c.DateTime());
            AddColumn("dbo.Travelers", "DeactivatedDate", c => c.DateTime());
            AddColumn("dbo.Travelers", "DefaultMobilityFlag", c => c.Boolean(nullable: false));
            AddColumn("dbo.Travelers", "DefaultBicycleFlag", c => c.Boolean(nullable: false));
            AddColumn("dbo.Travelers", "DefaultPriority", c => c.String());
            AddColumn("dbo.Travelers", "DefaultTimezone", c => c.String());
            AddColumn("dbo.Travelers", "ModifiedDate", c => c.DateTime(nullable: false));
            AddColumn("dbo.Travelers", "ModifiedBy", c => c.String());
            AddColumn("dbo.Trips", "TravelerId", c => c.Int(nullable: false));
            AddColumn("dbo.Trips", "Origination", c => c.String(nullable: false));
            AddColumn("dbo.Trips", "Destination", c => c.String(nullable: false));
            AddColumn("dbo.Trips", "TripStartDate", c => c.DateTime(nullable: false));
           // AddColumn("dbo.Trips", "TripStopDate", c => c.DateTime(nullable: false));
            AddColumn("dbo.Trips", "MobilityFlag", c => c.Boolean(nullable: false));
            AddColumn("dbo.Trips", "BicycleFlag", c => c.Boolean(nullable: false));
            AddColumn("dbo.Trips", "PriorityCode", c => c.String(nullable: false));
            AddColumn("dbo.Trips", "CreatedDate", c => c.DateTime(nullable: false));
            AddColumn("dbo.Trips", "ModifiedDate", c => c.DateTime(nullable: false));
            AddColumn("dbo.Trips", "ModifiedBy", c => c.String());
            AlterColumn("dbo.Travelers", "FirstName", c => c.String(nullable: false, maxLength: 50));
            AlterColumn("dbo.Travelers", "LastName", c => c.String(nullable: false, maxLength: 50));
            CreateIndex("dbo.Trips", "TravelerId");
            AddForeignKey("dbo.Trips", "TravelerId", "dbo.Travelers", "Id", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Steps", "TripId", "dbo.Trips");
            DropForeignKey("dbo.Trips", "TravelerId", "dbo.Travelers");
            DropIndex("dbo.Steps", new[] { "TripId" });
            DropIndex("dbo.Trips", new[] { "TravelerId" });
            AlterColumn("dbo.Travelers", "LastName", c => c.String());
            AlterColumn("dbo.Travelers", "FirstName", c => c.String());
            DropColumn("dbo.Trips", "ModifiedBy");
            DropColumn("dbo.Trips", "ModifiedDate");
            DropColumn("dbo.Trips", "CreatedDate");
            DropColumn("dbo.Trips", "PriorityCode");
            DropColumn("dbo.Trips", "BicycleFlag");
            DropColumn("dbo.Trips", "MobilityFlag");
            DropColumn("dbo.Trips", "TripStartDate");
           // DropColumn("dbo.Trips", "TripStopDate");
            DropColumn("dbo.Trips", "Destination");
            DropColumn("dbo.Trips", "Origination");
            DropColumn("dbo.Trips", "TravelerId");
            DropColumn("dbo.Travelers", "ModifiedBy");
            DropColumn("dbo.Travelers", "ModifiedDate");
            DropColumn("dbo.Travelers", "DefaultTimezone");
            DropColumn("dbo.Travelers", "DefaultPriority");
            DropColumn("dbo.Travelers", "DefaultBicycleFlag");
            DropColumn("dbo.Travelers", "DefaultMobilityFlag");
            DropColumn("dbo.Travelers", "DeactivatedDate");
            DropColumn("dbo.Travelers", "InformedConsentDate");
            DropColumn("dbo.Travelers", "InformedConsent");
            DropColumn("dbo.Travelers", "CreatedDate");
            DropColumn("dbo.Travelers", "PhoneNumber");
            DropColumn("dbo.Travelers", "Email");
            DropTable("dbo.Steps");
        }
    }
}
