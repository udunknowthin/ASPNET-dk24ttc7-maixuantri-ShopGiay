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

            // If price changed since added to cart, refresh cart snapshots and force user back to cart page
            var changedItems = new List<CartItem>();
            foreach (var ci in cartItems)
            {
                string message;
                if (HasPriceChanged(ci, out message))
                {
                    changedItems.Add(ci);
                }
            }
            if (changedItems.Any())
            {
                // update snapshots in DB so user can retry after seeing the notice
                RefreshCartSnapshots(changedItems);
                TempData["Error"] = "Giá hoặc khuyến mãi một số sản phẩm đã thay đổi. Giỏ hàng đã được cập nhật theo giá hiện tại. Vui lòng kiểm tra lại và đặt hàng.";
                return RedirectToAction("Detail", "Cart");
            }

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

            // Re-check current DB pricing before placing order; if changed update snapshots and return user to cart
            var changedBeforePlace = new List<CartItem>();
            foreach (var ci in cartItems)
            {
                string message;
                if (HasPriceChanged(ci, out message))
                {
                    changedBeforePlace.Add(ci);
                }
            }
            if (changedBeforePlace.Any())
            {
                RefreshCartSnapshots(changedBeforePlace);
                TempData["Error"] = "Giá hoặc khuyến mãi một số sản phẩm đã thay đổi. Giỏ hàng đã được cập nhật theo giá hiện tại. Vui lòng kiểm tra lại và đặt hàng.";
                return RedirectToAction("Detail", "Cart");
            }

            // Validate stock availability before placing order
            foreach (var ci in cartItems)
            {
                var size = ci.ProductSize != null ? db.ProductSizes.Find(ci.ProductSize.Id) : db.ProductSizes.Find(ci.ProductSizeId);
                if (size == null)
                {
                    TempData["Error"] = "Không tìm thấy size cho sản phẩm trong giỏ hàng.";
                    return RedirectToAction("Confirm");
                }
                if (size.StockQuantity < ci.Quantity)
                {
                    TempData["Error"] = string.Format("Sản phẩm '{0}' chỉ còn {1} trong kho.", ci.Product != null ? ci.Product.Name : "", size.StockQuantity);
                    return RedirectToAction("Confirm");
                }
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
                        ProductImageUrl = ci.Product != null && ci.Product.Images != null && ci.Product.Images.Any() ? ci.Product.Images.First().ImageUrl : null,
                        CreatedAt = DateTime.Now
                    };
                }).ToList()
            };

            db.Orders.Add(order);

            // Deduct stock for each cart item
            foreach (var ci in cartItems)
            {
                var ps = db.ProductSizes.Find(ci.ProductSizeId);
                if (ps != null)
                {
                    ps.StockQuantity = Math.Max(0, ps.StockQuantity - ci.Quantity);
                }
            }

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

        private bool HasPriceChanged(CartItem cartItem, out string message)
        {
            message = null;
            var product = db.Products.Find(cartItem.ProductId);
            if (product == null)
            {
                message = "Sản phẩm trong giỏ hàng không còn tồn tại.";
                return true;
            }

            var currentUnitPrice = product.IsDiscounted
                ? product.Price * (decimal)(1 - product.DiscountPercentage / 100.0)
                : product.Price;
            var currentDiscount = product.IsDiscounted ? (double?)product.DiscountPercentage : null;

            if (cartItem.UnitPriceAtAdd != currentUnitPrice || cartItem.DiscountPercentageAtAdd != currentDiscount)
            {
                message = string.Format("Đặt hàng thất bại: giá hoặc khuyến mãi của sản phẩm '{0}' đã thay đổi. Vui lòng kiểm tra lại giỏ hàng.", product.Name);
                return true;
            }

            return false;
        }

        // Update the cart items' snapshot prices to current DB values for the given items
        private void RefreshCartSnapshots(IEnumerable<CartItem> items)
        {
            foreach (var ci in items)
            {
                var product = db.Products.Find(ci.ProductId);
                if (product == null) continue;
                var currentUnitPrice = product.IsDiscounted
                    ? product.Price * (decimal)(1 - product.DiscountPercentage / 100.0)
                    : product.Price;
                var currentDiscount = product.IsDiscounted ? (double?)product.DiscountPercentage : null;

                // update DB cart item if exists
                var dbItem = db.CartItems.Find(ci.Id);
                if (dbItem != null)
                {
                    dbItem.UnitPriceAtAdd = currentUnitPrice;
                    dbItem.DiscountPercentageAtAdd = currentDiscount;
                }
            }
            db.SaveChanges();
        }
    }
}
