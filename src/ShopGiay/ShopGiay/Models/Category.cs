using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ShopGiay.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required, MaxLength(255)]
        public string Name { get; set; }

        public string ImageUrl { get; set; }

        public string Description { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public virtual ICollection<Product> Products { get; set; }
    }
}
