using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShopGiay.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required, MaxLength(50)]
        [Index(IsUnique = true)]
        public string SKU { get; set; }

        [Required, MaxLength(255)]
        public string Name { get; set; }

        public string Description { get; set; }

        public string LongDescription { get; set; }

        [Column(TypeName = "decimal")]
        public decimal Price { get; set; }

        public bool IsDiscounted { get; set; }
        public double DiscountPercentage { get; set; }

        public int CategoryId { get; set; }
        public virtual Category Category { get; set; }

        public virtual ICollection<ProductSize> Sizes { get; set; }
        public virtual ICollection<ProductImage> Images { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
