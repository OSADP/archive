namespace IDTO.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Indexes : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.TConnectedVehicles", "TConnectRoute", c => c.String(maxLength: 100));
            AlterColumn("dbo.Travelers", "ModifiedBy", c => c.String(maxLength: 20));
            AlterColumn("dbo.TConnectedVehicles", "TConnectStopCode", c => c.String(nullable: false, maxLength: 20));
            AlterColumn("dbo.TConnectedVehicles", "ModifiedBy", c => c.String(nullable: false, maxLength: 20));


            //manually added
            CreateIndex("Travelers", new[] { "Email" }, true, "IX_UniqueEmail");
            //manually added
            CreateIndex("Steps", new[] { "TripId", "StartDate" }, true, "IX_UniqueStep");
            //manually added
            CreateIndex("TConnectedVehicles", new[] { "OriginallyScheduledDeparture", "TConnectStopCode", "TConnectRoute" }, true, "IX_UniqueVehicle");
            //manually added
            CreateIndex("TConnectRequests", new[] { "TConnectId" }, true, "IX_UniqueRequest");
        }
        
        public override void Down()
        {
            AlterColumn("dbo.TConnectedVehicles", "ModifiedBy", c => c.String(nullable: false));
            AlterColumn("dbo.TConnectedVehicles", "TConnectStopCode", c => c.String(nullable: false));
            AlterColumn("dbo.Travelers", "ModifiedBy", c => c.String());
            DropColumn("dbo.TConnectedVehicles", "TConnectRoute");


            //manually added
            DropIndex("Travelers", new[] { "Email" });
            //manually added
            DropIndex("Steps", new[] { "TripId", "StartDate" });
            //manually added
            DropIndex("TConnectedVehicles", new[] { "OriginallyScheduledDeparture", "TConnectStopCode", "TConnectRoute" });
            //manually added
            DropIndex("TConnectRequests", new[] { "TConnectId" });
        }
    }
}
