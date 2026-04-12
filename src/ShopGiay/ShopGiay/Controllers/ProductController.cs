using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using ShopGiay.Models;

namespace ShopGiay.Controllers
{
    public class ProductController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public ActionResult List(int? categoryId, decimal? minPrice, decimal? maxPrice, bool? onlyDiscounted)
        {
            var query = db.Products
                .Include(p => p.Images)
                .Include(p => p.Category)
                .AsQueryable();

            if (categoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }

            if (onlyDiscounted.HasValue && onlyDiscounted.Value)
            {
                query = query.Where(p => p.IsDiscounted);
            }

            var products = query
                .OrderByDescending(p => p.CreatedAt)
                .ToList();

            if (minPrice.HasValue)
            {
                products = products.Where(p => (p.IsDiscounted ? p.Price * (decimal)(1 - p.DiscountPercentage / 100.0) : p.Price) >= minPrice.Value).ToList();
            }

            if (maxPrice.HasValue)
            {
                products = products.Where(p => (p.IsDiscounted ? p.Price * (decimal)(1 - p.DiscountPercentage / 100.0) : p.Price) <= maxPrice.Value).ToList();
            }

            var vm = new ProductListViewModel
            {
                Products = products,
                Categories = db.Categories.OrderBy(c => c.Name).ToList(),
                CategoryId = categoryId,
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                OnlyDiscounted = onlyDiscounted.HasValue && onlyDiscounted.Value
            };

            return View(vm);
        }

        public ActionResult Details(string sku)
        {
            if (string.IsNullOrEmpty(sku))
                return HttpNotFound();

            var product = db.Products
                .Include(p => p.Images)
                .Include(p => p.Sizes)
                .Include(p => p.Category)
                .FirstOrDefault(p => p.SKU == sku);

            if (product == null)
                return HttpNotFound();

            var relatedProducts = db.Products
                .Where(p => p.CategoryId == product.CategoryId && p.Id != product.Id)
                .Include(p => p.Images)
                .OrderByDescending(p => p.CreatedAt)
                .Take(4)
                .ToList();

            ViewBag.RelatedProducts = relatedProducts;

            return View(product);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
