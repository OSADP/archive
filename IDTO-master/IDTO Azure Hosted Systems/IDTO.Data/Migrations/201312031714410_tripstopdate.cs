namespace IDTO.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class tripstopdate : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Trips", "TripStopDate", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Trips", "TripStopDate");
        }
    }
}
