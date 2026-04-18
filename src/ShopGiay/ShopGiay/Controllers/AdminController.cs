using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using ShopGiay.Models;

namespace ShopGiay.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        private ApplicationUserManager UserManager
        {
            get { return HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>(); }
        }

        public ActionResult Index()
        {
            var model = new AdminDashboardViewModel
            {
                TotalUsers = db.Users.Count(),
                TotalOrders = db.Orders.Count(),
                TotalRevenue = db.Orders.Where(o => o.Status != OrderStatus.Cancelled).Select(o => o.TotalAmount).DefaultIfEmpty(0).Sum(),
                PendingOrders = db.Orders.Count(o => o.Status == OrderStatus.Pending),
                RecentOrders = db.Orders.OrderByDescending(o => o.CreatedAt).Take(5).ToList()
            };
            return View("~/Views/Admin/Dashboard/Index.cshtml", model);
        }

        public ActionResult Users(string keyword)
        {
            var query = db.Users.AsQueryable();
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(u => u.Email.Contains(keyword) || u.FullName.Contains(keyword));
            }

            var roleStore = new RoleStore<IdentityRole>(db);
            var roleMgr = new RoleManager<IdentityRole>(roleStore);
            var adminRole = roleMgr.FindByName("Admin");
            var adminIds = adminRole != null ? adminRole.Users.Select(ur => ur.UserId).ToList() : new System.Collections.Generic.List<string>();

            var model = new AdminUserListViewModel
            {
                SearchKeyword = keyword,
                Users = query.ToList().Select(u => new AdminUserItemViewModel
                {
                    Id = u.Id,
                    Email = u.Email,
                    FullName = u.FullName,
                    PhoneNumber = u.PhoneNumber,
                    Address = u.Address,
                    IsAdmin = adminIds.Contains(u.Id)
                }).ToList()
            };
            return View("~/Views/Admin/Dashboard/Users.cshtml", model);
        }

        public ActionResult EditUser(string id)
        {
            var user = UserManager.FindById(id);
            if (user == null) return HttpNotFound();
            var model = new AdminEditUserViewModel
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber,
                Address = user.Address
            };
            return View("~/Views/Admin/Dashboard/EditUser.cshtml", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditUser(AdminEditUserViewModel model)
        {
            if (!ModelState.IsValid)
                return View("~/Views/Admin/Dashboard/EditUser.cshtml", model);

            var user = await UserManager.FindByIdAsync(model.Id);
            if (user == null) return HttpNotFound();

            user.FullName = model.FullName;
            user.PhoneNumber = model.PhoneNumber;
            user.Address = model.Address;
            var result = await UserManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                TempData["Success"] = "Cập nhật thông tin người dùng thành công.";
                return RedirectToAction("Users");
            }
            foreach (var e in result.Errors) ModelState.AddModelError("", e);
            return View("~/Views/Admin/Dashboard/EditUser.cshtml", model);
        }

        public ActionResult Orders(string keyword, DateTime? fromDate, DateTime? toDate, OrderStatus? status)
        {
            var query = db.Orders.Include(o => o.User).Include(o => o.OrderItems).AsQueryable();
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(o => o.FullName.Contains(keyword) || o.Email.Contains(keyword) || o.PhoneNumber.Contains(keyword));
            }
            if (fromDate.HasValue)
                query = query.Where(o => o.CreatedAt >= fromDate.Value);
            if (toDate.HasValue)
            {
                var endDate = toDate.Value.AddDays(1);
                query = query.Where(o => o.CreatedAt < endDate);
            }
            if (status.HasValue)
                query = query.Where(o => o.Status == status.Value);

            var model = new AdminOrderListViewModel
            {
                Orders = query.OrderByDescending(o => o.CreatedAt).ToList(),
                SearchKeyword = keyword,
                FromDate = fromDate,
                ToDate = toDate,
                StatusFilter = status
            };
            return View("~/Views/Admin/Dashboard/Orders.cshtml", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateOrderStatus(int orderId, OrderStatus status)
        {
            var order = db.Orders.Find(orderId);
            if (order == null) return HttpNotFound();
            order.Status = status;
            order.UpdatedAt = DateTime.Now;
            db.SaveChanges();
            TempData["Success"] = "Cập nhật trạng thái đơn hàng #" + orderId + " thành công.";
            return RedirectToAction("Orders");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
