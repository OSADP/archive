namespace MapEdit.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.mapLinks",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        startId = c.Int(nullable: false),
                        endId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.mapNodes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        latitude = c.Decimal(nullable: false, precision: 18, scale: 2),
                        longitude = c.Decimal(nullable: false, precision: 18, scale: 2),
                        elevation = c.Decimal(nullable: false, precision: 18, scale: 2),
                        laneWidth = c.Int(nullable: false),
                        directionality = c.Int(nullable: false),
                        xOffset = c.Int(nullable: false),
                        yOffset = c.Int(nullable: false),
                        zOffset = c.Int(nullable: false),
                        positionalAccuracyP1 = c.Int(nullable: false),
                        positionalAccuracyP2 = c.Int(nullable: false),
                        positionalAccuracyP3 = c.Int(nullable: false),
                        laneOrder = c.Int(nullable: false),
                        postedSpeed = c.Int(nullable: false),
                        mapSetId = c.Int(nullable: false),
                        distance = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.mapSets",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        name = c.String(),
                        description = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.mapSets");
            DropTable("dbo.mapNodes");
            DropTable("dbo.mapLinks");
        }
    }
}
