using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ShopGiay.Models;

namespace ShopGiay.Controllers
{
    public class HomeController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public ActionResult Index()
        {
            var categories = db.Categories
                .Include(c => c.Products)
                .Where(c => c.Products.Any())
                .Take(5)
                .ToList();

            var newProducts = db.Products
                .Include(p => p.Sizes)
                .Include(p => p.Images)
                .Include(p => p.Category)
                .OrderByDescending(p => p.CreatedAt)
                .Take(8)
                .ToList();

            var viewModel = new HomePageViewModel
            {
                Categories = categories,
                NewProducts = newProducts
            };

            return View(viewModel);
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