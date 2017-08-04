namespace MapEdit.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class start : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.mapLinks",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        mapSetId = c.Guid(),
                        startMapNodeId = c.Guid(),
                        endMapNodeId = c.Guid(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.mapNodes", t => t.endMapNodeId)
                .ForeignKey("dbo.mapSets", t => t.mapSetId)
                .ForeignKey("dbo.mapNodes", t => t.startMapNodeId)
                .Index(t => t.mapSetId)
                .Index(t => t.startMapNodeId)
                .Index(t => t.endMapNodeId);
            
            CreateTable(
                "dbo.mapNodes",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        latitude = c.Double(nullable: false),
                        longitude = c.Double(nullable: false),
                        elevation = c.Double(nullable: false),
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
                        mapSetId = c.Guid(nullable: false),
                        distance = c.Int(nullable: false),
                        LaneDirection = c.String(),
                        LaneType = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.mapSets", t => t.mapSetId, cascadeDelete: true)
                .Index(t => t.mapSetId);
            
            CreateTable(
                "dbo.mapSets",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        name = c.String(),
                        description = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.mapLinks", "startMapNodeId", "dbo.mapNodes");
            DropForeignKey("dbo.mapLinks", "mapSetId", "dbo.mapSets");
            DropForeignKey("dbo.mapLinks", "endMapNodeId", "dbo.mapNodes");
            DropForeignKey("dbo.mapNodes", "mapSetId", "dbo.mapSets");
            DropIndex("dbo.mapNodes", new[] { "mapSetId" });
            DropIndex("dbo.mapLinks", new[] { "endMapNodeId" });
            DropIndex("dbo.mapLinks", new[] { "startMapNodeId" });
            DropIndex("dbo.mapLinks", new[] { "mapSetId" });
            DropTable("dbo.mapSets");
            DropTable("dbo.mapNodes");
            DropTable("dbo.mapLinks");
        }
    }
}
