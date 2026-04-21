using System;
using System.Linq;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using ShopGiay.Models;

namespace ShopGiay.Controllers
{
    [Authorize]
    public class OrdersController : Controller
    {
        private const int PageSize = 10;
        private ApplicationDbContext db = new ApplicationDbContext();

        public ActionResult Index(int page = 1)
        {
            var userId = User.Identity.GetUserId();
            var query = db.Orders.Where(o => o.UserId == userId);
            var totalCount = query.Count();
            var orders = query.OrderByDescending(o => o.CreatedAt)
                              .Skip((page - 1) * PageSize)
                              .Take(PageSize)
                              .ToList();

            var model = new UserOrdersViewModel
            {
                Orders = orders,
                Page = page,
                PageSize = PageSize,
                TotalCount = totalCount
            };
            return View(model);
        }

        public ActionResult Details(int id)
        {
            var userId = User.Identity.GetUserId();
            var order = db.Orders.Include("OrderItems").FirstOrDefault(o => o.Id == id && o.UserId == userId);
            if (order == null) return HttpNotFound();
            return View(order);
        }
    }
}

