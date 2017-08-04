namespace IDTO.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class LastVehiclePosition : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.LastVehiclePositions",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        VehicleName = c.String(nullable: false),
                        PositionTimestamp = c.DateTime(nullable: false),
                        Latitude = c.Double(nullable: false),
                        Longitude = c.Double(nullable: false),
                        Speed = c.Double(nullable: false),
                        Heading = c.Short(nullable: false),
                        Accuracy = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.LastVehiclePositions");
        }
    }
}
