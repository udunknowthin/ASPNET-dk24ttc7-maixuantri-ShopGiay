using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using ShopGiay.Models;

namespace ShopGiay.Controllers
{
    [Authorize]
    public class ManageController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        public ManageController()
        {
        }

        public ManageController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set 
            { 
                _signInManager = value; 
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        //
        // GET: /Manage/Index
        public async Task<ActionResult> Index(ManageMessageId? message)
        {
            ViewBag.StatusClass = "alert-success";
            ViewBag.StatusMessage =
                message == ManageMessageId.ChangePasswordSuccess ? "Mật khẩu đã được thay đổi."
                : message == ManageMessageId.UpdateProfileSuccess ? "Thông tin tài khoản đã được cập nhật."
                : message == ManageMessageId.Error ? "Đã xảy ra lỗi."
                : "";

            if (message == ManageMessageId.Error)
            {
                ViewBag.StatusClass = "alert-danger";
            }

            var userId = User.Identity.GetUserId();
            var user = await UserManager.FindByIdAsync(userId);
            var db = new ApplicationDbContext();
            var model = new IndexViewModel
            {
                HasPassword = HasPassword(),
                PhoneNumber = user?.PhoneNumber,
                FullName = user?.FullName,
                Address = user?.Address,
                Email = user?.Email,
                Orders = db.Orders.Where(o => o.UserId == userId).OrderByDescending(o => o.CreatedAt).ToList()
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> UpdateProfile(UpdateProfileViewModel model)
        {
            if (!ModelState.IsValid)
                return RedirectToAction("Index", new { message = ManageMessageId.Error });

            var userId = User.Identity.GetUserId();
            var user = await UserManager.FindByIdAsync(userId);
            if (user == null) return RedirectToAction("Index");

            user.FullName = (model.FullName ?? string.Empty).Trim();
            user.Address = (model.Address ?? string.Empty).Trim();
            user.PhoneNumber = (model.PhoneNumber ?? string.Empty).Trim();

            var result = await UserManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                return RedirectToAction("Index", new { message = ManageMessageId.UpdateProfileSuccess });
            }

            AddErrors(result);
            return RedirectToAction("Index", new { message = ManageMessageId.Error });
        }

        // GET: /Manage/ChangePassword
        public ActionResult ChangePassword()
        {
            return View();
        }

        //
        // POST: /Manage/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var result = await UserManager.ChangePasswordAsync(User.Identity.GetUserId(), model.OldPassword, model.NewPassword);
            if (result.Succeeded)
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                if (user != null)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                }
                return RedirectToAction("Index", new { Message = ManageMessageId.ChangePasswordSuccess });
            }
            AddErrors(result);
            return View(model);
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing && _userManager != null)
            {
                _userManager.Dispose();
                _userManager = null;
            }

            base.Dispose(disposing);
        }

#region Helpers
        // External login helpers removed

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private bool HasPassword()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (user != null)
            {
                return user.PasswordHash != null;
            }
            return false;
        }

        public enum ManageMessageId
        {
            ChangePasswordSuccess,
            UpdateProfileSuccess,
            Error
        }

#endregion
    }
}