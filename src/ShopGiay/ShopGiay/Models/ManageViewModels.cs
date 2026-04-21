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
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Current password")]
        public string OldPassword { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
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

