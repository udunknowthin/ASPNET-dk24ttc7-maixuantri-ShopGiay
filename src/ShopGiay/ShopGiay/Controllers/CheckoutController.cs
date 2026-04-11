using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using ShopGiay.Models;

namespace ShopGiay.Controllers
{
    [Authorize]
    public class CheckoutController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public ActionResult Confirm()
        {
            var userId = User.Identity.GetUserId();
            var user = db.Users.Find(userId);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var cart = db.Carts
                .Include(c => c.CartItems.Select(ci => ci.Product.Images))
                .Include(c => c.CartItems.Select(ci => ci.ProductSize))
                .FirstOrDefault(c => c.UserId == userId);

            var cartItems = cart != null && cart.CartItems != null
                ? cart.CartItems.ToList()
                : new List<CartItem>();

            var subtotal = cartItems.Sum(ci =>
            {
                var price = ci.Product != null && ci.Product.IsDiscounted
                    ? ci.Product.Price * (decimal)(1 - ci.Product.DiscountPercentage / 100.0)
                    : (ci.Product != null ? ci.Product.Price : 0);
                return price * ci.Quantity;
            });

            var vm = new CheckoutViewModel
            {
                FullName = user.FullName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                PhoneNumber = user.PhoneNumber ?? string.Empty,
                Address = user.Address ?? string.Empty,
                CartItems = cartItems,
                Subtotal = subtotal
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PlaceOrder(CheckoutViewModel model)
        {
            var userId = User.Identity.GetUserId();
            var user = db.Users.Find(userId);
            if (user == null) return new HttpUnauthorizedResult();

            var cart = db.Carts
                .Include(c => c.CartItems.Select(ci => ci.Product))
                .Include(c => c.CartItems.Select(ci => ci.ProductSize))
                .FirstOrDefault(c => c.UserId == userId);

            var cartItems = cart != null && cart.CartItems != null
                ? cart.CartItems.ToList()
                : new List<CartItem>();

            if (!cartItems.Any())
            {
                TempData["Error"] = "Giỏ hàng của bạn đang trống.";
                return RedirectToAction("Confirm");
            }

            // Update user info
            user.FullName = model.FullName;
            user.PhoneNumber = model.PhoneNumber;
            user.Address = model.Address;
            db.SaveChanges();

            // Build order
            var total = cartItems.Sum(ci =>
            {
                var price = ci.Product != null && ci.Product.IsDiscounted
                    ? ci.Product.Price * (decimal)(1 - ci.Product.DiscountPercentage / 100.0)
                    : (ci.Product != null ? ci.Product.Price : 0);
                return price * ci.Quantity;
            });

            var order = new Order
            {
                UserId = userId,
                FullName = model.FullName,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                Address = model.Address,
                Notes = model.Notes,
                PaymentMethod = model.PaymentMethod ?? "COD",
                Status = OrderStatus.Pending,
                TotalAmount = total,
                CreatedAt = DateTime.Now,
                OrderItems = cartItems.Select(ci =>
                {
                    var price = ci.Product != null && ci.Product.IsDiscounted
                        ? ci.Product.Price * (decimal)(1 - ci.Product.DiscountPercentage / 100.0)
                        : (ci.Product != null ? ci.Product.Price : 0);
                    return new OrderItem
                    {
                        ProductId = ci.ProductId,
                        ProductName = ci.Product != null ? ci.Product.Name : string.Empty,
                        SizeName = ci.ProductSize != null ? ci.ProductSize.SizeName : string.Empty,
                        UnitPrice = price,
                        Quantity = ci.Quantity,
                        CreatedAt = DateTime.Now
                    };
                }).ToList()
            };

            db.Orders.Add(order);

            // Clear cart
            if (cart != null && cart.CartItems != null)
            {
                db.CartItems.RemoveRange(cart.CartItems);
            }

            db.SaveChanges();

            TempData["Success"] = string.Format("Đặt hàng thành công! Mã đơn hàng của bạn là #{0}.", order.Id);
            return RedirectToAction("Success", new { id = order.Id });
        }

        public ActionResult Success(int id)
        {
            var userId = User.Identity.GetUserId();
            var order = db.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefault(o => o.Id == id && o.UserId == userId);

            if (order == null)
                return HttpNotFound();

            return View(order);
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
