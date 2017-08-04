namespace IDTO.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class blockId : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Steps", "BlockIdentifier", c => c.String(maxLength: 50));
            AddColumn("dbo.TConnectedVehicles", "TConnectBlockIdentifier", c => c.String(maxLength: 50));
        }
        
        public override void Down()
        {
            DropColumn("dbo.TConnectedVehicles", "TConnectBlockIdentifier");
            DropColumn("dbo.Steps", "BlockIdentifier");
        }
    }
}
