using System.Collections.Generic;

namespace ShopGiay.Models
{
    public class ProductListViewModel
    {
        public List<Product> Products { get; set; }
        public List<Category> Categories { get; set; }

        public int? CategoryId { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public bool OnlyDiscounted { get; set; }
    }
}
