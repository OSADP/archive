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
    
    public partial class Add_Logging_and_JobOptions : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.LogEntry",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        LogLevel = c.Int(nullable: false),
                        Message = c.String(),
                        FullMessage = c.String(),
                        AuditDate = c.DateTime(nullable: false),
                        SubscriberId = c.Int(),
                        User_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Subscriber", t => t.SubscriberId)
                .ForeignKey("dbo.User", t => t.User_Id)
                .Index(t => t.SubscriberId)
                .Index(t => t.User_Id);
            
            CreateTable(
                "dbo.SyncLogEntry",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        TimeStamp = c.DateTime(),
                        Title = c.String(),
                        Message = c.String(),
                        UpdatedJobCount = c.Int(nullable: false),
                        CreatedJobCount = c.Int(nullable: false),
                        GeocodeErrorCount = c.Int(nullable: false),
                        JobErrorCount = c.Int(nullable: false),
                        SubscriberId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Subscriber", t => t.SubscriberId)
                .Index(t => t.SubscriberId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.SyncLogEntry", "SubscriberId", "dbo.Subscriber");
            DropForeignKey("dbo.LogEntry", "User_Id", "dbo.User");
            DropForeignKey("dbo.LogEntry", "SubscriberId", "dbo.Subscriber");
            DropIndex("dbo.SyncLogEntry", new[] { "SubscriberId" });
            DropIndex("dbo.LogEntry", new[] { "User_Id" });
            DropIndex("dbo.LogEntry", new[] { "SubscriberId" });
            DropTable("dbo.SyncLogEntry");
            DropTable("dbo.LogEntry");
        }
    }
}
