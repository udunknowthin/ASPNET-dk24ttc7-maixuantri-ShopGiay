using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShopGiay.Models
{
    public class OrderItem
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int ProductId { get; set; }

        public string ProductName { get; set; }
        public string SizeName { get; set; }

        [Column(TypeName = "decimal")]
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // store snapshot of image url to display later
        public string ProductImageUrl { get; set; }

        public virtual Order Order { get; set; }
        public virtual Product Product { get; set; }
    }
}
