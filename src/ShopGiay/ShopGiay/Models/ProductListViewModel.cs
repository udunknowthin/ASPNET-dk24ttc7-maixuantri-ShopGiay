using System;
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

        // Phân trang
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get { return (int)Math.Ceiling((double)TotalCount / PageSize); } }
    }
}

