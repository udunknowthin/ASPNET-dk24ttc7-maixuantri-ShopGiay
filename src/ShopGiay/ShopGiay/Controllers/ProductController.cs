using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using ShopGiay.Models;

namespace ShopGiay.Controllers
{
    public class ProductController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

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
