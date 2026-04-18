using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ShopGiay.Models
{
    public class AdminDashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public int PendingOrders { get; set; }
        public List<Order> RecentOrders { get; set; }
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
    }

    public class AdminOrderListViewModel
    {
        public List<Order> Orders { get; set; }
        public string SearchKeyword { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public OrderStatus? StatusFilter { get; set; }
    }
}
