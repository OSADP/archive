namespace IDTO.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class travelerloginlocation : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.TravelerLocations",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        TravelerId = c.Int(nullable: false),
                        PositionTimestamp = c.DateTime(nullable: false),
                        Latitude = c.Double(nullable: false),
                        Longitude = c.Double(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Travelers", t => t.TravelerId, cascadeDelete: true)
                .Index(t => t.TravelerId);
            
            AddColumn("dbo.Travelers", "LoginId", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.TravelerLocations", "TravelerId", "dbo.Travelers");
            DropIndex("dbo.TravelerLocations", new[] { "TravelerId" });
            DropColumn("dbo.Travelers", "LoginId");
            DropTable("dbo.TravelerLocations");
        }
    }
}
