namespace MapEdit.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addForeignKeys : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.mapLinks", "endMapNode_Id", c => c.Int());
            AddColumn("dbo.mapLinks", "startMapNode_Id", c => c.Int());
            CreateIndex("dbo.mapLinks", "endMapNode_Id");
            CreateIndex("dbo.mapLinks", "startMapNode_Id");
            CreateIndex("dbo.mapNodes", "mapSetId");
            AddForeignKey("dbo.mapNodes", "mapSetId", "dbo.mapSets", "Id", cascadeDelete: true);
            AddForeignKey("dbo.mapLinks", "endMapNode_Id", "dbo.mapNodes", "Id");
            AddForeignKey("dbo.mapLinks", "startMapNode_Id", "dbo.mapNodes", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.mapLinks", "startMapNode_Id", "dbo.mapNodes");
            DropForeignKey("dbo.mapLinks", "endMapNode_Id", "dbo.mapNodes");
            DropForeignKey("dbo.mapNodes", "mapSetId", "dbo.mapSets");
            DropIndex("dbo.mapNodes", new[] { "mapSetId" });
            DropIndex("dbo.mapLinks", new[] { "startMapNode_Id" });
            DropIndex("dbo.mapLinks", new[] { "endMapNode_Id" });
            DropColumn("dbo.mapLinks", "startMapNode_Id");
            DropColumn("dbo.mapLinks", "endMapNode_Id");
        }
    }
}
