using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;
using System.Web.Mvc;

namespace ShopGiay.Models
{
    public class AdminDashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public int PendingOrders { get; set; }
        public List<Order> RecentOrders { get; set; }

        // Biểu đồ: phân bố trạng thái đơn hàng
        public int StatusPending { get; set; }
        public int StatusProcessing { get; set; }
        public int StatusShipped { get; set; }
        public int StatusDelivered { get; set; }
        public int StatusCancelled { get; set; }

        // Biểu đồ: doanh thu & số đơn theo tháng (6 tháng gần nhất)
        public List<string> MonthLabels { get; set; }
        public List<decimal> MonthlyRevenue { get; set; }
        public List<int> MonthlyOrders { get; set; }
    }

    public class AdminUserListViewModel
    {
        public List<AdminUserItemViewModel> Users { get; set; }
        public string SearchKeyword { get; set; }
    }

    public class AdminUserItemViewModel
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public bool IsAdmin { get; set; }
    }

    public class AdminEditUserViewModel
    {
        public string Id { get; set; }

        [Display(Name = "Email")]
        public string Email { get; set; }

        [Display(Name = "Họ và tên")]
        public string FullName { get; set; }

        [Display(Name = "Số điện thoại")]
        public string PhoneNumber { get; set; }

        [Display(Name = "Địa chỉ")]
        public string Address { get; set; }

        [Display(Name = "Quyền Admin")]
        public bool IsAdmin { get; set; }

        public bool IsCurrentUser { get; set; }
    }

    public class AdminOrderListViewModel
    {
        public List<Order> Orders { get; set; }
        public string SearchKeyword { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public OrderStatus? StatusFilter { get; set; }

        // Phân trang
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get { return (int)Math.Ceiling((double)TotalCount / PageSize); } }
    }

    public class AdminProductListViewModel
    {
        public List<Product> Products { get; set; }
        public List<Category> Categories { get; set; }
        public string SearchKeyword { get; set; }
        public int? CategoryId { get; set; }

        // Phân trang
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get { return (int)Math.Ceiling((double)TotalCount / PageSize); } }
    }

    public class ProductSizeInputModel
    {
        public int Id { get; set; } // 0 = mới, > 0 = đã tồn tại
        public string SizeName { get; set; }
        public int StockQuantity { get; set; }
    }

    public class AdminEditProductViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập SKU")]
        [Display(Name = "SKU")]
        public string SKU { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên sản phẩm")]
        [Display(Name = "Tên sản phẩm")]
        public string Name { get; set; }

        [Display(Name = "Mô tả ngắn")]
        public string Description { get; set; }

        [Display(Name = "Mô tả chi tiết")]
        public string LongDescription { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập giá")]
        [Display(Name = "Giá (₫)")]
        public decimal Price { get; set; }

        [Display(Name = "Đang giảm giá")]
        public bool IsDiscounted { get; set; }

        [Display(Name = "% Giảm giá")]
        [Range(0, 100, ErrorMessage = "Phần trăm giảm giá phải từ 0 đến 100.")]
        public double DiscountPercentage { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn danh mục")]
        [Display(Name = "Danh mục")]
        public int CategoryId { get; set; }

        public List<SelectListItem> CategoryOptions { get; set; }
        public List<ProductImage> ExistingImages { get; set; }
        public List<ProductSizeInputModel> Sizes { get; set; } = new List<ProductSizeInputModel>();
    }

    public class AdminCategoryListViewModel
    {
        public List<Category> Categories { get; set; }
        public string SearchKeyword { get; set; }

        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get { return (int)Math.Ceiling((double)TotalCount / PageSize); } }
    }

    public class AdminEditCategoryViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên danh mục")]
        [Display(Name = "Tên danh mục")]
        public string Name { get; set; }

        [Display(Name = "Mô tả")]
        public string Description { get; set; }

        public string CurrentImageUrl { get; set; }
    }
}

