using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ShopGiay.Models
{
    public class CheckoutViewModel
    {
        [Required(ErrorMessage = "Full name is required.")]
        [Display(Name = "Full Name")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress]
        public string Email { get; set; }

        [Required(ErrorMessage = "Phone number is required.")]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Address is required.")]
        public string Address { get; set; }

        public string Notes { get; set; }

        [Display(Name = "Payment Method")]
        public string PaymentMethod { get; set; }

        public List<CartItem> CartItems { get; set; }
        public decimal Subtotal { get; set; }
    }
}
