using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ShopGiay.Models
{
    public class IndexViewModel
    {
        public bool HasPassword { get; set; }
        public string PhoneNumber { get; set; }
        // Profile
        public string FullName { get; set; }
        public string Address { get; set; }
        public string Email { get; set; }
        // Orders placed by the user
        public IList<ShopGiay.Models.Order> Orders { get; set; }
    }

    // Keep only necessary view models for Manage: change password and profile update
    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập mật khẩu hiện tại.")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu hiện tại")]
        public string OldPassword { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu mới.")]
        [StringLength(100, ErrorMessage = "{0} phải có ít nhất {2} ký tự.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu mới")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Xác nhận mật khẩu mới")]
        [Compare("NewPassword", ErrorMessage = "Mật khẩu mới và xác nhận mật khẩu không khớp.")]
        public string ConfirmPassword { get; set; }
    }

    public class UpdateProfileViewModel
    {
        public string FullName { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
    }

    public class UserOrdersViewModel
    {
        public List<Order> Orders { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get { return (int)Math.Ceiling((double)TotalCount / PageSize); } }
    }
}

