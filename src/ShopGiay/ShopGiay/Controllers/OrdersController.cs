using System.Linq;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using ShopGiay.Models;

namespace ShopGiay.Controllers
{
    [Authorize]
    public class OrdersController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public ActionResult Index()
        {
            var userId = User.Identity.GetUserId();
            var orders = db.Orders.Where(o => o.UserId == userId).OrderByDescending(o => o.CreatedAt).ToList();
            return View(orders);
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
