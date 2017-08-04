//    Copyright 2014 Productivity Apex Inc.
//        http://www.productivityapex.com/
//
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
//
//        http://www.apache.org/licenses/LICENSE-2.0
//
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.

namespace PAI.FRATIS.SFL.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Miami_Add_JobShipperConsignee_and_LocationOptions : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.LocationOptions",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        LocationId = c.Int(),
                        IsDriverSelectable = c.Boolean(nullable: false),
                        IsCustomHandling = c.Boolean(nullable: false),
                        SubscriberId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Location", t => t.LocationId)
                .ForeignKey("dbo.Subscriber", t => t.SubscriberId)
                .Index(t => t.LocationId)
                .Index(t => t.SubscriberId);
            
            AddColumn("dbo.Job", "ShipperName", c => c.String());
            AddColumn("dbo.Job", "ConsigneeName", c => c.String());
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.LocationOptions", "SubscriberId", "dbo.Subscriber");
            DropForeignKey("dbo.LocationOptions", "LocationId", "dbo.Location");
            DropIndex("dbo.LocationOptions", new[] { "SubscriberId" });
            DropIndex("dbo.LocationOptions", new[] { "LocationId" });
            DropColumn("dbo.Job", "ConsigneeName");
            DropColumn("dbo.Job", "ShipperName");
            DropTable("dbo.LocationOptions");
        }
    }
}
