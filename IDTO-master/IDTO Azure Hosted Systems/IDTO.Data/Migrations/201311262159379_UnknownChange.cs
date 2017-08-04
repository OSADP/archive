namespace IDTO.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UnknownChange : DbMigration
    {
        public override void Up()
        {
            RenameTable(name: "dbo.Traveler", newName: "Travelers");
            RenameTable(name: "dbo.Trip", newName: "Trips");
        }
        
        public override void Down()
        {
            RenameTable(name: "dbo.Trips", newName: "Trip");
            RenameTable(name: "dbo.Travelers", newName: "Traveler");
        }
    }
}
