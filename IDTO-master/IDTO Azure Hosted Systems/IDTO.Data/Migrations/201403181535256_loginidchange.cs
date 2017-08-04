namespace IDTO.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class loginidchange : DbMigration
    {
        public override void Up()
        {
            //added
            //You have a default constraint on your column. You need to first drop the constraint, then alter your column.
            Sql(@"DECLARE @con nvarchar(128)
 SELECT @con = name
 FROM sys.default_constraints
 WHERE parent_object_id = object_id('dbo.Travelers')
 AND col_name(parent_object_id, parent_column_id) = 'LoginId';
 IF @con IS NOT NULL
     EXECUTE('ALTER TABLE [dbo].[Travelers] DROP CONSTRAINT ' + @con)");          


            AlterColumn("dbo.Travelers", "LoginId", c => c.String(nullable: false, maxLength: 80));

            //manually added
            CreateIndex("Travelers", new[] { "LoginId" }, true, "IX_UniqueLoginId");
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Travelers", "LoginId", c => c.Int(nullable: false));

            //manually added
            DropIndex("Travelers", new[] { "LoginId" });
        }
    }
}
