namespace IDTO.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class tripnotification : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Trips", "TripStartNotificationSent", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Trips", "TripStartNotificationSent");
        }
    }
}
