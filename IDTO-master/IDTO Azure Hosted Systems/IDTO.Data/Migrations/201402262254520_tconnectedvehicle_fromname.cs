namespace IDTO.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class tconnectedvehicle_fromname : DbMigration
    {
        public override void Up()
        {
             //added
            //You have a default constraint on your column. You need to first drop the constraint, then alter your column.
            //Sql("ALTER TABLE TableName DROP CONSTRAINT [DF__Steps__FromName__3F115E1A]");//auto generated number, not repeatable on diff. server
            Sql(@"DECLARE @con nvarchar(128)
 SELECT @con = name
 FROM sys.default_constraints
 WHERE parent_object_id = object_id('dbo.Steps')
 AND col_name(parent_object_id, parent_column_id) = 'FromName';
 IF @con IS NOT NULL
     EXECUTE('ALTER TABLE [dbo].[Steps] DROP CONSTRAINT ' + @con)
 SELECT @con = name
 FROM sys.default_constraints
 WHERE parent_object_id = object_id('dbo.Steps')
 AND col_name(parent_object_id, parent_column_id) = 'ToName';
 IF @con IS NOT NULL
     EXECUTE('ALTER TABLE [dbo].[Steps] DROP CONSTRAINT ' + @con)");          
            
            
            AddColumn("dbo.TConnectedVehicles", "TConnectFromName", c => c.String(nullable: false, maxLength: 50));
            AlterColumn("dbo.Steps", "FromName", c => c.String(nullable: false, maxLength: 50));
            AlterColumn("dbo.Steps", "ToName", c => c.String(nullable: false, maxLength: 50));
            
            
            //added
            Sql(@"ALTER TABLE [dbo].[Steps] ADD  DEFAULT ('') FOR [ToName]");
            Sql(@"ALTER TABLE [dbo].[Steps] ADD  DEFAULT ('') FOR [FromName]");
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Steps", "ToName", c => c.String(nullable: false));
            AlterColumn("dbo.Steps", "FromName", c => c.String(nullable: false));
            DropColumn("dbo.TConnectedVehicles", "TConnectFromName");

        
        }
    }
}
