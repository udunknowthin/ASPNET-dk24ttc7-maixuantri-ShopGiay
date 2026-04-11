using System;
using System.Collections.Generic;

namespace ShopGiay.Models
{
    public class Cart
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public virtual ApplicationUser User { get; set; }
        public virtual ICollection<CartItem> CartItems { get; set; }
    }
}
