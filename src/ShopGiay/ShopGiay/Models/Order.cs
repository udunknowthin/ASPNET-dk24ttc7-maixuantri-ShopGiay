using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShopGiay.Models
{
    public enum OrderStatus
    {
        Pending = 0,
        Processing = 1,
        Shipped = 2,
        Delivered = 3,
        Cancelled = 4
    }

    public class Order
    {
        public int Id { get; set; }
        public string UserId { get; set; }

        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public string Notes { get; set; }
        public string PaymentMethod { get; set; }

        public OrderStatus Status { get; set; }

        [Column(TypeName = "decimal")]
        public decimal TotalAmount { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public virtual ApplicationUser User { get; set; }
        public virtual ICollection<OrderItem> OrderItems { get; set; }
    }
}
