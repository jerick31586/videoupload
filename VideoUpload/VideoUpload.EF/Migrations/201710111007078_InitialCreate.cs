namespace VideoUpload.EF.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Activities",
                c => new
                    {
                        ActivityID = c.Int(nullable: false, identity: true),
                        Type = c.String(),
                        Value = c.String(),
                        Description = c.String(),
                    })
                .PrimaryKey(t => t.ActivityID);
            
            CreateTable(
                "dbo.AppLogs",
                c => new
                    {
                        AppLogID = c.Int(nullable: false, identity: true),
                        Message = c.String(),
                        Type = c.String(),
                        Source = c.String(),
                        Url = c.String(),
                        LogDate = c.DateTime(nullable: false),
                        UserName = c.String(),
                    })
                .PrimaryKey(t => t.AppLogID);
            
            CreateTable(
                "dbo.Attachments",
                c => new
                    {
                        PostAttachmentID = c.String(nullable: false, maxLength: 128),
                        FileName = c.String(),
                        MIMEType = c.String(),
                        FileSize = c.Int(nullable: false),
                        FileUrl = c.String(),
                        PostID = c.Int(nullable: false),
                        AttachmentNo = c.String(),
                        DateCreated = c.DateTime(),
                        ThumbnailFileName = c.String(),
                        ThumbnailUrl = c.String(),
                    })
                .PrimaryKey(t => t.PostAttachmentID)
                .ForeignKey("dbo.Posts", t => t.PostID, cascadeDelete: true)
                .Index(t => t.PostID);
            
            CreateTable(
                "dbo.Posts",
                c => new
                    {
                        PostID = c.Int(nullable: false, identity: true),
                        PlateNumber = c.String(nullable: false),
                        Description = c.String(nullable: false),
                        DateUploaded = c.DateTime(nullable: false),
                        EditedBy = c.String(),
                        DateEdited = c.DateTime(),
                        UserID = c.String(maxLength: 128),
                        HasApproval = c.Boolean(nullable: false),
                        IsApproved = c.Boolean(nullable: false),
                        DateApproved = c.DateTime(),
                        Approver = c.String(maxLength: 15),
                        HasPlayedVideo = c.Boolean(nullable: false),
                        DatePlayedVideo = c.DateTime(),
                        BranchID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.PostID)
                .ForeignKey("dbo.Branches", t => t.BranchID, cascadeDelete: true)
                .ForeignKey("dbo.Users", t => t.UserID)
                .Index(t => t.UserID)
                .Index(t => t.BranchID);
            
            CreateTable(
                "dbo.Branches",
                c => new
                    {
                        BranchID = c.Int(nullable: false, identity: true),
                        BranchName = c.String(),
                        Name = c.String(),
                        Address = c.String(),
                        PhoneNumber = c.String(),
                        FaxNumber = c.String(),
                        Email = c.String(),
                    })
                .PrimaryKey(t => t.BranchID);
            
            CreateTable(
                "dbo.Jobcards",
                c => new
                    {
                        JobcardNo = c.String(nullable: false, maxLength: 128),
                        CustomerName = c.String(),
                        ChassisNo = c.String(),
                        PlateNo = c.String(),
                        Mileage = c.String(),
                        BranchID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.JobcardNo)
                .ForeignKey("dbo.Branches", t => t.BranchID, cascadeDelete: true)
                .Index(t => t.BranchID);
            
            CreateTable(
                "dbo.HealthCheckDetails",
                c => new
                    {
                        HealCheckDetailsID = c.Int(nullable: false, identity: true),
                        HcCode = c.String(nullable: false, maxLength: 128),
                        JobcardNo = c.String(nullable: false, maxLength: 128),
                        Status = c.String(),
                        Comments = c.String(),
                    })
                .PrimaryKey(t => t.HealCheckDetailsID)
                .ForeignKey("dbo.HealthChecks", t => t.HcCode, cascadeDelete: true)
                .ForeignKey("dbo.Jobcards", t => t.JobcardNo, cascadeDelete: true)
                .Index(t => t.HcCode)
                .Index(t => t.JobcardNo);
            
            CreateTable(
                "dbo.HealthChecks",
                c => new
                    {
                        HcCode = c.String(nullable: false, maxLength: 128),
                        Description = c.String(),
                        HcGroup = c.String(),
                    })
                .PrimaryKey(t => t.HcCode);
            
            CreateTable(
                "dbo.Users",
                c => new
                    {
                        UserID = c.String(nullable: false, maxLength: 128),
                        UserName = c.String(nullable: false),
                        FirstName = c.String(nullable: false),
                        LastName = c.String(nullable: false),
                        JobTitle = c.String(),
                        EmployeeNo = c.String(),
                        Email = c.String(nullable: false),
                        WorkAddress = c.String(),
                        PhoneNumber = c.String(),
                        DirectLine = c.String(),
                        FaxNumber = c.String(),
                        EmailConfirmed = c.Boolean(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        PasswordHash = c.String(maxLength: 128),
                        SecurityStamp = c.String(maxLength: 128),
                        EmailPass = c.String(maxLength: 128),
                        MobileNumber = c.String(),
                        BranchID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.UserID)
                .ForeignKey("dbo.Branches", t => t.BranchID, cascadeDelete: true)
                .Index(t => t.BranchID);
            
            CreateTable(
                "dbo.Histories",
                c => new
                    {
                        HistoryID = c.Int(nullable: false, identity: true),
                        Recipient = c.String(nullable: false),
                        Name = c.String(),
                        DateSent = c.DateTime(nullable: false),
                        UserID = c.String(nullable: false, maxLength: 128),
                        Type = c.String(),
                        PostID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.HistoryID)
                .ForeignKey("dbo.Users", t => t.UserID, cascadeDelete: true)
                .ForeignKey("dbo.Posts", t => t.PostID, cascadeDelete: false)
                .Index(t => t.UserID)
                .Index(t => t.PostID);
            
            CreateTable(
                "dbo.UserClaims",
                c => new
                    {
                        ClaimID = c.Int(nullable: false, identity: true),
                        ClaimType = c.String(),
                        ClaimValue = c.String(),
                        UserID = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.ClaimID)
                .ForeignKey("dbo.Users", t => t.UserID)
                .Index(t => t.UserID);
            
            CreateTable(
                "dbo.Customers",
                c => new
                    {
                        CustomerCode = c.String(nullable: false, maxLength: 128),
                        Title = c.String(),
                        FirstName = c.String(),
                        SecondName = c.String(),
                        ThirdName = c.String(),
                        LastName = c.String(),
                        Gender = c.Int(nullable: false),
                        DateOfBirth = c.DateTime(),
                        CompanyName = c.String(),
                        Position = c.String(),
                        Department = c.String(),
                        Email = c.String(),
                        Address = c.String(),
                        City = c.String(),
                        Region = c.String(),
                        PostalCode = c.String(),
                        Country = c.String(),
                        SendEmail = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.CustomerCode);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Histories", "PostID", "dbo.Posts");
            DropForeignKey("dbo.Users", "BranchID", "dbo.Branches");
            DropForeignKey("dbo.UserClaims", "UserID", "dbo.Users");
            DropForeignKey("dbo.Posts", "UserID", "dbo.Users");
            DropForeignKey("dbo.Histories", "UserID", "dbo.Users");
            DropForeignKey("dbo.Posts", "BranchID", "dbo.Branches");
            DropForeignKey("dbo.Jobcards", "BranchID", "dbo.Branches");
            DropForeignKey("dbo.HealthCheckDetails", "JobcardNo", "dbo.Jobcards");
            DropForeignKey("dbo.HealthCheckDetails", "HcCode", "dbo.HealthChecks");
            DropForeignKey("dbo.Attachments", "PostID", "dbo.Posts");
            DropIndex("dbo.UserClaims", new[] { "UserID" });
            DropIndex("dbo.Histories", new[] { "PostID" });
            DropIndex("dbo.Histories", new[] { "UserID" });
            DropIndex("dbo.Users", new[] { "BranchID" });
            DropIndex("dbo.HealthCheckDetails", new[] { "JobcardNo" });
            DropIndex("dbo.HealthCheckDetails", new[] { "HcCode" });
            DropIndex("dbo.Jobcards", new[] { "BranchID" });
            DropIndex("dbo.Posts", new[] { "BranchID" });
            DropIndex("dbo.Posts", new[] { "UserID" });
            DropIndex("dbo.Attachments", new[] { "PostID" });
            DropTable("dbo.Customers");
            DropTable("dbo.UserClaims");
            DropTable("dbo.Histories");
            DropTable("dbo.Users");
            DropTable("dbo.HealthChecks");
            DropTable("dbo.HealthCheckDetails");
            DropTable("dbo.Jobcards");
            DropTable("dbo.Branches");
            DropTable("dbo.Posts");
            DropTable("dbo.Attachments");
            DropTable("dbo.AppLogs");
            DropTable("dbo.Activities");
        }
    }
}
