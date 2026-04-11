namespace ShopGiay.Migrations
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Migrations;
    using System.Linq;
    using ShopGiay.Models;

    internal sealed class Configuration : DbMigrationsConfiguration<ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
            AutomaticMigrationDataLossAllowed = true;
        }

        protected override void Seed(ApplicationDbContext context)
        {
            // Seed Categories
            var categories = new List<Category>
            {
                new Category { Id = 1, Name = "Giày Nam", Description = "Khám phá bộ sưu tập giày nam đa dạng, từ cổ điển đến hiện đại.", ImageUrl = "men.avif", CreatedAt = DateTime.Now },
                new Category { Id = 2, Name = "Giày Nữ", Description = "Phong cách và thời thượng với các mẫu giày nữ mới nhất.", ImageUrl = "women.avif", CreatedAt = DateTime.Now },
                new Category { Id = 3, Name = "Giày Trẻ Em", Description = "Giày bền đẹp, thiết kế phù hợp cho đôi chân đang lớn của bé.", ImageUrl = "kids.avif", CreatedAt = DateTime.Now },
                new Category { Id = 4, Name = "Giày Thể Thao", Description = "Nâng cao hiệu suất với dòng giày thể thao cao cấp.", ImageUrl = "sports.avif", CreatedAt = DateTime.Now },
                new Category { Id = 5, Name = "Hàng Mới Về", Description = "Dẫn đầu xu hướng với những mẫu mới nhất vừa về hàng.", ImageUrl = "trending.avif", CreatedAt = DateTime.Now }
            };
            categories.ForEach(c => context.Categories.AddOrUpdate(cat => cat.Id, c));
            context.SaveChanges();

            // Seed Products
            var products = new List<Product>
            {
                new Product { Id = 1,  SKU = "MEN-001",      Name = "Adidas Samba OG Trắng",           Description = "Mẫu giày cổ điển bất hủ với phần upper da trắng và đế cao su gum mang tính biểu tượng.",             Price = 2800000m, IsDiscounted = false, DiscountPercentage = 0,  CategoryId = 1, CreatedAt = DateTime.Now },
                new Product { Id = 2,  SKU = "MEN-002",      Name = "Adidas Adizero SL Xám",           Description = "Đệm êm ái và phản hồi lực đỉnh cao, lý tưởng cho những buổi chạy bộ hàng ngày.",                   Price = 3200000m, IsDiscounted = false, DiscountPercentage = 0,  CategoryId = 1, CreatedAt = DateTime.Now },
                new Product { Id = 3,  SKU = "WOMEN-001",    Name = "Adidas Tokyo Da Nâu",             Description = "Giày da nâu thanh lịch, thiết kế tinh tế dành cho phái nữ năng động.",                              Price = 2950000m, IsDiscounted = true,  DiscountPercentage = 15, CategoryId = 2, CreatedAt = DateTime.Now },
                new Product { Id = 4,  SKU = "WOMEN-002",    Name = "Adidas Handball Xanh Biển",       Description = "Giày handball phong cách vintage với màu xanh biển cổ điển, thời thượng.",                          Price = 2550000m, IsDiscounted = false, DiscountPercentage = 0,  CategoryId = 2, CreatedAt = DateTime.Now },
                new Product { Id = 5,  SKU = "KIDS-001",     Name = "Adidas Altaswim Đen",             Description = "Dép đen bền chắc dành cho trẻ em với quai cài an toàn, chống trơn trượt.",                          Price = 1450000m, IsDiscounted = false, DiscountPercentage = 0,  CategoryId = 3, CreatedAt = DateTime.Now },
                new Product { Id = 6,  SKU = "KIDS-002",     Name = "Adidas Disney Đen Hồng",          Description = "Giày trẻ em kết hợp họa tiết Disney đáng yêu, màu đen hồng nổi bật.",                              Price = 1890000m, IsDiscounted = true,  DiscountPercentage = 20, CategoryId = 3, CreatedAt = DateTime.Now },
                new Product { Id = 7,  SKU = "SPORTS-001",   Name = "Adidas Climacool Laced Kem",      Description = "Giày thể thao hiệu năng cao với công nghệ Climacool thoáng khí tiên tiến.",                        Price = 3750000m, IsDiscounted = false, DiscountPercentage = 0,  CategoryId = 4, CreatedAt = DateTime.Now },
                new Product { Id = 8,  SKU = "SPORTS-002",   Name = "Adidas Barreda Mary Jane Đen",    Description = "Giày Mary Jane thể thao màu đen cổ điển với độ hỗ trợ cổ chân vượt trội.",                         Price = 3350000m, IsDiscounted = true,  DiscountPercentage = 10, CategoryId = 4, CreatedAt = DateTime.Now },
                new Product { Id = 9,  SKU = "TRENDING-001", Name = "Adidas Ultraboost Kem Hồng",      Description = "Mẫu Ultraboost xu hướng mới nhất phối màu kem và hồng pastel đẹp mắt.",                            Price = 4800000m, IsDiscounted = true,  DiscountPercentage = 25, CategoryId = 5, CreatedAt = DateTime.Now },
                new Product { Id = 10, SKU = "TRENDING-002", Name = "Adidas Ultraboost 5 Bạc Xám",    Description = "Dòng Ultraboost 5 cao cấp với tông màu bạc xám sang trọng, hiện đại.",                             Price = 5100000m, IsDiscounted = false, DiscountPercentage = 0,  CategoryId = 5, CreatedAt = DateTime.Now }
            };
            products.ForEach(p => context.Products.AddOrUpdate(prod => prod.Id, p));
            context.SaveChanges();

            // Seed ProductImages
            var images = new List<ProductImage>
            {
                new ProductImage { Id = 1, ProductId = 1, ImageUrl = "Giay_Samba_01.avif", CreatedAt = DateTime.Now },
                new ProductImage { Id = 2, ProductId = 1, ImageUrl = "Giay_Samba_02.avif", CreatedAt = DateTime.Now },
                new ProductImage { Id = 3, ProductId = 1, ImageUrl = "Giay_Samba_03.avif", CreatedAt = DateTime.Now },
                new ProductImage { Id = 4, ProductId = 1, ImageUrl = "Giay_Samba_04.avif", CreatedAt = DateTime.Now },
                new ProductImage { Id = 5, ProductId = 1, ImageUrl = "Giay_Samba_05.avif", CreatedAt = DateTime.Now },
                new ProductImage { Id = 6, ProductId = 2, ImageUrl = "Giay_Adizero_01.avif", CreatedAt = DateTime.Now },
                new ProductImage { Id = 7, ProductId = 2, ImageUrl = "Giay_Adizero_02.avif", CreatedAt = DateTime.Now },
                new ProductImage { Id = 8, ProductId = 2, ImageUrl = "Giay_Adizero_03.avif", CreatedAt = DateTime.Now },
                new ProductImage { Id = 9, ProductId = 2, ImageUrl = "Giay_Adizero_04.avif", CreatedAt = DateTime.Now },
                new ProductImage { Id = 10, ProductId = 2, ImageUrl = "Giay_Adizero_05.avif", CreatedAt = DateTime.Now },
                new ProductImage { Id = 11, ProductId = 3, ImageUrl = "GIAY_TOKYO_01.avif", CreatedAt = DateTime.Now },
                new ProductImage { Id = 12, ProductId = 3, ImageUrl = "GIAY_TOKYO_02.avif", CreatedAt = DateTime.Now },
                new ProductImage { Id = 13, ProductId = 3, ImageUrl = "GIAY_TOKYO_03.avif", CreatedAt = DateTime.Now },
                new ProductImage { Id = 14, ProductId = 3, ImageUrl = "GIAY_TOKYO_04.avif", CreatedAt = DateTime.Now },
                new ProductImage { Id = 15, ProductId = 3, ImageUrl = "GIAY_TOKYO_05.avif", CreatedAt = DateTime.Now },
                new ProductImage { Id = 16, ProductId = 4, ImageUrl = "GIAY_HANDBALL_01.avif", CreatedAt = DateTime.Now },
                new ProductImage { Id = 17, ProductId = 4, ImageUrl = "GIAY_HANDBALL_02.avif", CreatedAt = DateTime.Now },
                new ProductImage { Id = 18, ProductId = 4, ImageUrl = "GIAY_HANDBALL_03.avif", CreatedAt = DateTime.Now },
                new ProductImage { Id = 19, ProductId = 4, ImageUrl = "GIAY_HANDBALL_04.avif", CreatedAt = DateTime.Now },
                new ProductImage { Id = 20, ProductId = 4, ImageUrl = "GIAY_HANDBALL_05.avif", CreatedAt = DateTime.Now },
                new ProductImage { Id = 21, ProductId = 5, ImageUrl = "Sandal_Altaswim_01.avif", CreatedAt = DateTime.Now },
                new ProductImage { Id = 22, ProductId = 5, ImageUrl = "Sandal_Altaswim_02.avif", CreatedAt = DateTime.Now },
                new ProductImage { Id = 23, ProductId = 5, ImageUrl = "Sandal_Altaswim_03.avif", CreatedAt = DateTime.Now },
                new ProductImage { Id = 24, ProductId = 5, ImageUrl = "Sandal_Altaswim_04.avif", CreatedAt = DateTime.Now },
                new ProductImage { Id = 25, ProductId = 5, ImageUrl = "Sandal_Altaswim_05.avif", CreatedAt = DateTime.Now },
                new ProductImage { Id = 26, ProductId = 6, ImageUrl = "GIAY_ADIDAS_DISNEY_01.avif", CreatedAt = DateTime.Now },
                new ProductImage { Id = 27, ProductId = 6, ImageUrl = "GIAY_ADIDAS_DISNEY_02.avif", CreatedAt = DateTime.Now },
                new ProductImage { Id = 28, ProductId = 6, ImageUrl = "GIAY_ADIDAS_DISNEY_03.avif", CreatedAt = DateTime.Now },
                new ProductImage { Id = 29, ProductId = 6, ImageUrl = "GIAY_ADIDAS_DISNEY_04.avif", CreatedAt = DateTime.Now },
                new ProductImage { Id = 30, ProductId = 6, ImageUrl = "GIAY_ADIDAS_DISNEY_05.avif", CreatedAt = DateTime.Now },
                new ProductImage { Id = 31, ProductId = 7, ImageUrl = "GIAY_CLIMACOOL_LACED_01.avif", CreatedAt = DateTime.Now },
                new ProductImage { Id = 32, ProductId = 7, ImageUrl = "GIAY_CLIMACOOL_LACED_02.avif", CreatedAt = DateTime.Now },
                new ProductImage { Id = 33, ProductId = 7, ImageUrl = "GIAY_CLIMACOOL_LACED_03.avif", CreatedAt = DateTime.Now },
                new ProductImage { Id = 34, ProductId = 7, ImageUrl = "GIAY_CLIMACOOL_LACED_04.avif", CreatedAt = DateTime.Now },
                new ProductImage { Id = 35, ProductId = 7, ImageUrl = "GIAY_CLIMACOOL_LACED_05.avif", CreatedAt = DateTime.Now },
                new ProductImage { Id = 36, ProductId = 8, ImageUrl = "GIAY_BARREDA_MARY_JANE_01.avif", CreatedAt = DateTime.Now },
                new ProductImage { Id = 37, ProductId = 8, ImageUrl = "GIAY_BARREDA_MARY_JANE_02.avif", CreatedAt = DateTime.Now },
                new ProductImage { Id = 38, ProductId = 8, ImageUrl = "GIAY_BARREDA_MARY_JANE_03.avif", CreatedAt = DateTime.Now },
                new ProductImage { Id = 39, ProductId = 8, ImageUrl = "GIAY_BARREDA_MARY_JANE_04.avif", CreatedAt = DateTime.Now },
                new ProductImage { Id = 40, ProductId = 8, ImageUrl = "GIAY_BARREDA_MARY_JANE_05.avif", CreatedAt = DateTime.Now },
                new ProductImage { Id = 41, ProductId = 9, ImageUrl = "Giay_ULTRABOOST_01.avif", CreatedAt = DateTime.Now },
                new ProductImage { Id = 42, ProductId = 9, ImageUrl = "Giay_ULTRABOOST_02.avif", CreatedAt = DateTime.Now },
                new ProductImage { Id = 43, ProductId = 9, ImageUrl = "Giay_ULTRABOOST_03.avif", CreatedAt = DateTime.Now },
                new ProductImage { Id = 44, ProductId = 9, ImageUrl = "Giay_ULTRABOOST_04.avif", CreatedAt = DateTime.Now },
                new ProductImage { Id = 45, ProductId = 9, ImageUrl = "Giay_ULTRABOOST_05.avif", CreatedAt = DateTime.Now },
                new ProductImage { Id = 46, ProductId = 10, ImageUrl = "Giay_Ultraboost_5_Xam_01.avif", CreatedAt = DateTime.Now },
                new ProductImage { Id = 47, ProductId = 10, ImageUrl = "Giay_Ultraboost_5_Xam_02.avif", CreatedAt = DateTime.Now },
                new ProductImage { Id = 48, ProductId = 10, ImageUrl = "Giay_Ultraboost_5_Xam_03.avif", CreatedAt = DateTime.Now },
                new ProductImage { Id = 49, ProductId = 10, ImageUrl = "Giay_Ultraboost_5_Xam_04.avif", CreatedAt = DateTime.Now },
                new ProductImage { Id = 50, ProductId = 10, ImageUrl = "Giay_Ultraboost_5_Xam_05.avif", CreatedAt = DateTime.Now }
            };
            images.ForEach(i => context.ProductImages.AddOrUpdate(img => img.Id, i));
            context.SaveChanges();

            // Seed ProductSizes
            var sizes = new List<ProductSize>
            {
                new ProductSize { Id = 1, ProductId = 1, SizeName = "39", StockQuantity = 10, CreatedAt = DateTime.Now },
                new ProductSize { Id = 2, ProductId = 1, SizeName = "40", StockQuantity = 15, CreatedAt = DateTime.Now },
                new ProductSize { Id = 3, ProductId = 1, SizeName = "41", StockQuantity = 20, CreatedAt = DateTime.Now },
                new ProductSize { Id = 4, ProductId = 1, SizeName = "42", StockQuantity = 12, CreatedAt = DateTime.Now },
                new ProductSize { Id = 5, ProductId = 1, SizeName = "43", StockQuantity = 8, CreatedAt = DateTime.Now },
                new ProductSize { Id = 6, ProductId = 2, SizeName = "39", StockQuantity = 14, CreatedAt = DateTime.Now },
                new ProductSize { Id = 7, ProductId = 2, SizeName = "40", StockQuantity = 18, CreatedAt = DateTime.Now },
                new ProductSize { Id = 8, ProductId = 2, SizeName = "41", StockQuantity = 22, CreatedAt = DateTime.Now },
                new ProductSize { Id = 9, ProductId = 2, SizeName = "42", StockQuantity = 5, CreatedAt = DateTime.Now },
                new ProductSize { Id = 10, ProductId = 2, SizeName = "43", StockQuantity = 30, CreatedAt = DateTime.Now },
                new ProductSize { Id = 11, ProductId = 3, SizeName = "39", StockQuantity = 25, CreatedAt = DateTime.Now },
                new ProductSize { Id = 12, ProductId = 3, SizeName = "40", StockQuantity = 10, CreatedAt = DateTime.Now },
                new ProductSize { Id = 13, ProductId = 3, SizeName = "41", StockQuantity = 16, CreatedAt = DateTime.Now },
                new ProductSize { Id = 14, ProductId = 3, SizeName = "42", StockQuantity = 11, CreatedAt = DateTime.Now },
                new ProductSize { Id = 15, ProductId = 3, SizeName = "43", StockQuantity = 9, CreatedAt = DateTime.Now },
                new ProductSize { Id = 16, ProductId = 4, SizeName = "39", StockQuantity = 20, CreatedAt = DateTime.Now },
                new ProductSize { Id = 17, ProductId = 4, SizeName = "40", StockQuantity = 8, CreatedAt = DateTime.Now },
                new ProductSize { Id = 18, ProductId = 4, SizeName = "41", StockQuantity = 15, CreatedAt = DateTime.Now },
                new ProductSize { Id = 19, ProductId = 4, SizeName = "42", StockQuantity = 15, CreatedAt = DateTime.Now },
                new ProductSize { Id = 20, ProductId = 4, SizeName = "43", StockQuantity = 12, CreatedAt = DateTime.Now },
                new ProductSize { Id = 21, ProductId = 5, SizeName = "30", StockQuantity = 18, CreatedAt = DateTime.Now },
                new ProductSize { Id = 22, ProductId = 5, SizeName = "31", StockQuantity = 10, CreatedAt = DateTime.Now },
                new ProductSize { Id = 23, ProductId = 5, SizeName = "32", StockQuantity = 12, CreatedAt = DateTime.Now },
                new ProductSize { Id = 24, ProductId = 5, SizeName = "33", StockQuantity = 14, CreatedAt = DateTime.Now },
                new ProductSize { Id = 25, ProductId = 5, SizeName = "34", StockQuantity = 7, CreatedAt = DateTime.Now },
                new ProductSize { Id = 26, ProductId = 6, SizeName = "31", StockQuantity = 22, CreatedAt = DateTime.Now },
                new ProductSize { Id = 27, ProductId = 6, SizeName = "32", StockQuantity = 12, CreatedAt = DateTime.Now },
                new ProductSize { Id = 28, ProductId = 6, SizeName = "33", StockQuantity = 19, CreatedAt = DateTime.Now },
                new ProductSize { Id = 29, ProductId = 6, SizeName = "34", StockQuantity = 6, CreatedAt = DateTime.Now },
                new ProductSize { Id = 30, ProductId = 6, SizeName = "35", StockQuantity = 10, CreatedAt = DateTime.Now },
                new ProductSize { Id = 31, ProductId = 7, SizeName = "39", StockQuantity = 11, CreatedAt = DateTime.Now },
                new ProductSize { Id = 32, ProductId = 7, SizeName = "40", StockQuantity = 20, CreatedAt = DateTime.Now },
                new ProductSize { Id = 33, ProductId = 7, SizeName = "41", StockQuantity = 13, CreatedAt = DateTime.Now },
                new ProductSize { Id = 34, ProductId = 7, SizeName = "42", StockQuantity = 8, CreatedAt = DateTime.Now },
                new ProductSize { Id = 35, ProductId = 7, SizeName = "43", StockQuantity = 5, CreatedAt = DateTime.Now },
                new ProductSize { Id = 36, ProductId = 8, SizeName = "39", StockQuantity = 19, CreatedAt = DateTime.Now },
                new ProductSize { Id = 37, ProductId = 8, SizeName = "40", StockQuantity = 10, CreatedAt = DateTime.Now },
                new ProductSize { Id = 38, ProductId = 8, SizeName = "41", StockQuantity = 17, CreatedAt = DateTime.Now },
                new ProductSize { Id = 39, ProductId = 8, SizeName = "42", StockQuantity = 10, CreatedAt = DateTime.Now },
                new ProductSize { Id = 40, ProductId = 8, SizeName = "43", StockQuantity = 23, CreatedAt = DateTime.Now },
                new ProductSize { Id = 41, ProductId = 9, SizeName = "39", StockQuantity = 25, CreatedAt = DateTime.Now },
                new ProductSize { Id = 42, ProductId = 9, SizeName = "40", StockQuantity = 14, CreatedAt = DateTime.Now },
                new ProductSize { Id = 43, ProductId = 9, SizeName = "41", StockQuantity = 10, CreatedAt = DateTime.Now },
                new ProductSize { Id = 44, ProductId = 9, SizeName = "42", StockQuantity = 21, CreatedAt = DateTime.Now },
                new ProductSize { Id = 45, ProductId = 9, SizeName = "43", StockQuantity = 16, CreatedAt = DateTime.Now },
                new ProductSize { Id = 46, ProductId = 10, SizeName = "39", StockQuantity = 13, CreatedAt = DateTime.Now },
                new ProductSize { Id = 47, ProductId = 10, SizeName = "40", StockQuantity = 9, CreatedAt = DateTime.Now },
                new ProductSize { Id = 48, ProductId = 10, SizeName = "41", StockQuantity = 15, CreatedAt = DateTime.Now },
                new ProductSize { Id = 49, ProductId = 10, SizeName = "42", StockQuantity = 12, CreatedAt = DateTime.Now },
                new ProductSize { Id = 50, ProductId = 10, SizeName = "43", StockQuantity = 28, CreatedAt = DateTime.Now }
            };
            sizes.ForEach(s => context.ProductSizes.AddOrUpdate(sz => sz.Id, s));
            context.SaveChanges();
        }
    }
}
