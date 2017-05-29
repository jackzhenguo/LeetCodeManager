namespace LeetcodeManager.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddTagsCombine : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Problems", "TagsCombine", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Problems", "TagsCombine");
        }
    }
}
