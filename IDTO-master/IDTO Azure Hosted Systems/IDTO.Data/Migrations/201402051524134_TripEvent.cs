namespace IDTO.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class TripEvent : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.TripEvents",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        TripId = c.Int(nullable: false),
                        EventDate = c.DateTime(nullable: false),
                        Message = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Trips", t => t.TripId, cascadeDelete: true)
                .Index(t => t.TripId);
            
            AlterColumn("dbo.Steps", "RouteNumber", c => c.String(maxLength: 100));
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.TripEvents", "TripId", "dbo.Trips");
            DropIndex("dbo.TripEvents", new[] { "TripId" });
            AlterColumn("dbo.Steps", "RouteNumber", c => c.String());
            DropTable("dbo.TripEvents");
        }
    }
}
