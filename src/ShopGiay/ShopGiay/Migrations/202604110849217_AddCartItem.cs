namespace ShopGiay.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddCartItem : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.CartItems", "UnitPriceAtAdd", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.CartItems", "DiscountPercentageAtAdd", c => c.Double());
            AddColumn("dbo.OrderItems", "ProductImageUrl", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.OrderItems", "ProductImageUrl");
            DropColumn("dbo.CartItems", "DiscountPercentageAtAdd");
            DropColumn("dbo.CartItems", "UnitPriceAtAdd");
        }
    }
}
