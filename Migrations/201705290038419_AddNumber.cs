namespace LeetcodeManager.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddNumber : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Problems", "Number", c => c.String(nullable: false, maxLength: 50));
            AlterColumn("dbo.Problems", "Title", c => c.String());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Problems", "Title", c => c.String(nullable: false, maxLength: 50));
            DropColumn("dbo.Problems", "Number");
        }
    }
}
