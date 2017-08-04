namespace IDTO.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class StepsProviders : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Modes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Providers",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                        ProviderTypeId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ProviderTypes", t => t.ProviderTypeId, cascadeDelete: true)
                .Index(t => t.ProviderTypeId);
            
            CreateTable(
                "dbo.ProviderTypes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.Id);

            AddColumn("dbo.Steps", "ModeId", c => c.Int(nullable: false, defaultValue: 1));
            AddColumn("dbo.Steps", "StartDate", c => c.DateTime(nullable: false));
            AddColumn("dbo.Steps", "EndDate", c => c.DateTime(nullable: false));
            AddColumn("dbo.Steps", "FromName", c => c.String(nullable: false));
            AddColumn("dbo.Steps", "FromStopCode", c => c.String());
            AddColumn("dbo.Steps", "FromProviderId", c => c.Int());
            AddColumn("dbo.Steps", "ToName", c => c.String(nullable: false));
            AddColumn("dbo.Steps", "ToStopCode", c => c.String());
            AddColumn("dbo.Steps", "ToProviderId", c => c.Int());
            AddColumn("dbo.Steps", "Distance", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.Steps", "RouteNumber", c => c.String());
             RenameColumn("dbo.Trips", "TripStopDate","TripEndDate");
            CreateIndex("dbo.Steps", "FromProviderId");
            CreateIndex("dbo.Steps", "ModeId");
            CreateIndex("dbo.Steps", "ToProviderId");
            AddForeignKey("dbo.Steps", "FromProviderId", "dbo.Providers", "Id");
            AddForeignKey("dbo.Steps", "ModeId", "dbo.Modes", "Id", cascadeDelete: true);
            AddForeignKey("dbo.Steps", "ToProviderId", "dbo.Providers", "Id");
           
        }
        
        public override void Down()
        {
            AddColumn("dbo.Trips", "TripStopDate", c => c.DateTime(nullable: false));
            DropForeignKey("dbo.Steps", "ToProviderId", "dbo.Providers");
            DropForeignKey("dbo.Steps", "ModeId", "dbo.Modes");
            DropForeignKey("dbo.Steps", "FromProviderId", "dbo.Providers");
            DropForeignKey("dbo.Providers", "ProviderTypeId", "dbo.ProviderTypes");
            DropIndex("dbo.Steps", new[] { "ToProviderId" });
            DropIndex("dbo.Steps", new[] { "ModeId" });
            DropIndex("dbo.Steps", new[] { "FromProviderId" });
            DropIndex("dbo.Providers", new[] { "ProviderTypeId" });
            DropColumn("dbo.Trips", "TripEndDate");
            DropColumn("dbo.Steps", "RouteNumber");
            DropColumn("dbo.Steps", "Distance");
            DropColumn("dbo.Steps", "ToProviderId");
            DropColumn("dbo.Steps", "ToStopCode");
            DropColumn("dbo.Steps", "ToName");
            DropColumn("dbo.Steps", "FromProviderId");
            DropColumn("dbo.Steps", "FromStopCode");
            DropColumn("dbo.Steps", "FromName");
            DropColumn("dbo.Steps", "EndDate");
            DropColumn("dbo.Steps", "StartDate");
            DropColumn("dbo.Steps", "ModeId");
            DropTable("dbo.ProviderTypes");
            DropTable("dbo.Providers");
            DropTable("dbo.Modes");
        }
    }
}
