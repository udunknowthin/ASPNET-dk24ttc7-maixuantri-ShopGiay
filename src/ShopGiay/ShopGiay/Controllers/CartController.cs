using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Newtonsoft.Json;
using ShopGiay.Models;

namespace ShopGiay.Controllers
{
    public class CartController : Controller
    {
        private const string SessionCartKey = "SessionCartItems";
        private ApplicationDbContext db = new ApplicationDbContext();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddToCart(int productId, int productSizeId, int quantity)
        {
            if (quantity <= 0)
            {
                TempData["Error"] = "Số lượng không hợp lệ.";
                return RedirectToAction("Index", "Home");
            }

            var product = db.Products.Find(productId);
            if (product == null)
            {
                TempData["Error"] = "Sản phẩm không tồn tại.";
                return RedirectToAction("Index", "Home");
            }

            var productSize = db.ProductSizes
                .FirstOrDefault(ps => ps.Id == productSizeId && ps.ProductId == productId);
            if (productSize == null)
            {
                TempData["Error"] = "Không tìm thấy size đã chọn.";
                return RedirectToAction("Details", "Product", new { sku = product.SKU });
            }

            var userId = User.Identity.GetUserId();

            if (string.IsNullOrEmpty(userId))
            {
                // Guest cart - use session
                var sessionItems = GetSessionCartItems();
                var existingItem = sessionItems.FirstOrDefault(i => i.ProductId == productId && i.ProductSizeId == productSizeId);

                var updatedQuantity = existingItem != null ? existingItem.Quantity + quantity : quantity;

                if (productSize.StockQuantity < updatedQuantity)
                {
                    TempData["Error"] = string.Format("Không đủ hàng trong kho. Chỉ còn {0} sản phẩm.", productSize.StockQuantity);
                    return RedirectToAction("Details", "Product", new { sku = product.SKU });
                }

                if (existingItem != null)
                {
                    existingItem.Quantity = updatedQuantity;
                }
                else
                {
                    // capture price snapshot
                    var unitPrice = product.IsDiscounted
                        ? product.Price * (decimal)(1 - product.DiscountPercentage / 100.0)
                        : product.Price;
                    sessionItems.Add(new SessionCartItem
                    {
                        ProductId = productId,
                        ProductSizeId = productSizeId,
                        Quantity = quantity,
                        UnitPriceAtAdd = unitPrice,
                        DiscountPercentageAtAdd = product.IsDiscounted ? (double?)product.DiscountPercentage : null
                    });
                }

                SaveSessionCartItems(sessionItems);
                TempData["Success"] = "Đã thêm sản phẩm vào giỏ hàng!";
                return RedirectToAction("Details", "Product", new { sku = product.SKU });
            }

            // Logged-in user cart - use DB
            if (productSize.StockQuantity < quantity)
            {
                TempData["Error"] = string.Format("Không đủ hàng trong kho. Chỉ còn {0} sản phẩm.", productSize.StockQuantity);
                return RedirectToAction("Details", "Product", new { sku = product.SKU });
            }

            var cart = db.Carts
                .Include(c => c.CartItems)
                .FirstOrDefault(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new Cart
                {
                    UserId = userId,
                    CreatedAt = DateTime.Now
                };
                db.Carts.Add(cart);
                db.SaveChanges();
            }

            var existingCartItem = db.CartItems
                .FirstOrDefault(ci => ci.CartId == cart.Id
                    && ci.ProductId == productId
                    && ci.ProductSizeId == productSizeId);

            if (existingCartItem != null)
            {
                var newQuantity = existingCartItem.Quantity + quantity;
                if (productSize.StockQuantity < newQuantity)
                {
                    TempData["Error"] = string.Format("Không đủ hàng trong kho. Chỉ còn {0} sản phẩm.", productSize.StockQuantity);
                    return RedirectToAction("Details", "Product", new { sku = product.SKU });
                }
                existingCartItem.Quantity = newQuantity;
                existingCartItem.UpdatedAt = DateTime.Now;
            }
            else
            {
                var cartItem = new CartItem
                {
                    CartId = cart.Id,
                    ProductId = productId,
                    ProductSizeId = productSizeId,
                    Quantity = quantity,
                    CreatedAt = DateTime.Now
                };
                // capture price snapshot
                var unitPrice = product.IsDiscounted
                    ? product.Price * (decimal)(1 - product.DiscountPercentage / 100.0)
                    : product.Price;
                cartItem.UnitPriceAtAdd = unitPrice;
                cartItem.DiscountPercentageAtAdd = product.IsDiscounted ? (double?)product.DiscountPercentage : null;
                db.CartItems.Add(cartItem);
            }

            db.SaveChanges();
            TempData["Success"] = "Đã thêm sản phẩm vào giỏ hàng!";
            return RedirectToAction("Details", "Product", new { sku = product.SKU });
        }

        public ActionResult Detail()
        {
            var userId = User.Identity.GetUserId();

            if (string.IsNullOrEmpty(userId))
            {
                var sessionItems = GetSessionCartItems();
                var mappedItems = BuildSessionCartItems(sessionItems);
                var guestCart = new Cart
                {
                    CartItems = mappedItems
                };
                return View(guestCart);
            }

            var cart = db.Carts
                .Include(c => c.CartItems.Select(ci => ci.Product.Images))
                .Include(c => c.CartItems.Select(ci => ci.ProductSize))
                .FirstOrDefault(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new Cart { UserId = userId, CreatedAt = DateTime.Now };
            }

            return View(cart);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RemoveFromCart(int productId, int productSizeId)
        {
            var userId = User.Identity.GetUserId();

            if (string.IsNullOrEmpty(userId))
            {
                var sessionItems = GetSessionCartItems();
                sessionItems.RemoveAll(i => i.ProductId == productId && i.ProductSizeId == productSizeId);
                SaveSessionCartItems(sessionItems);
                return RedirectToAction("Detail");
            }

            var cartItem = db.CartItems
                .Include(ci => ci.Cart)
                .FirstOrDefault(ci => ci.Cart.UserId == userId
                    && ci.ProductId == productId
                    && ci.ProductSizeId == productSizeId);

            if (cartItem != null)
            {
                db.CartItems.Remove(cartItem);
                db.SaveChanges();
            }

            return RedirectToAction("Detail");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateQuantity(int productId, int productSizeId, int quantity)
        {
            if (quantity <= 0)
            {
                return Json(new { success = false, message = "Số lượng không hợp lệ." });
            }

            var productSize = db.ProductSizes
                .FirstOrDefault(ps => ps.Id == productSizeId && ps.ProductId == productId);

            if (productSize == null)
            {
                return Json(new { success = false, message = "Không tìm thấy size." });
            }

            if (productSize.StockQuantity < quantity)
            {
                return Json(new { success = false, message = string.Format("Không đủ hàng trong kho. Chỉ còn {0} sản phẩm.", productSize.StockQuantity) });
            }

            var userId = User.Identity.GetUserId();

            if (string.IsNullOrEmpty(userId))
            {
                var sessionItems = GetSessionCartItems();
                var sessionItem = sessionItems.FirstOrDefault(i => i.ProductId == productId && i.ProductSizeId == productSizeId);
                if (sessionItem == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy sản phẩm trong giỏ hàng." });
                }
                sessionItem.Quantity = quantity;
                SaveSessionCartItems(sessionItems);
                return Json(new { success = true, message = "Đã cập nhật giỏ hàng!" });
            }

            var cartItem = db.CartItems
                .Include(ci => ci.Cart)
                .FirstOrDefault(ci => ci.Cart.UserId == userId
                    && ci.ProductId == productId
                    && ci.ProductSizeId == productSizeId);

            if (cartItem == null)
            {
                return Json(new { success = false, message = "Không tìm thấy sản phẩm trong giỏ hàng." });
            }

            cartItem.Quantity = quantity;
            cartItem.UpdatedAt = DateTime.Now;
            db.SaveChanges();
            return Json(new { success = true, message = "Đã cập nhật giỏ hàng!" });
        }

        #region Session Cart Helpers

        [ChildActionOnly]
        public ActionResult CartBadge()
        {
            int count = 0;
            var userId = User.Identity.GetUserId();

            if (string.IsNullOrEmpty(userId))
            {
                var sessionItems = GetSessionCartItems();
                count = sessionItems.Sum(i => i.Quantity);
            }
            else
            {
                var cart = db.Carts
                    .Include(c => c.CartItems)
                    .FirstOrDefault(c => c.UserId == userId);
                if (cart != null)
                {
                    count = cart.CartItems.Sum(ci => ci.Quantity);
                }
            }

            return PartialView("_CartBadge", count);
        }

        private List<SessionCartItem> GetSessionCartItems()
        {
            var session = System.Web.HttpContext.Current?.Session;
            var json = session != null ? session[SessionCartKey] as string : null;
            if (string.IsNullOrEmpty(json))
                return new List<SessionCartItem>();
            return JsonConvert.DeserializeObject<List<SessionCartItem>>(json) ?? new List<SessionCartItem>();
        }

        private void SaveSessionCartItems(List<SessionCartItem> items)
        {
            var session = System.Web.HttpContext.Current?.Session;
            if (session != null)
            {
                session[SessionCartKey] = JsonConvert.SerializeObject(items);
            }
        }

        private List<CartItem> BuildSessionCartItems(List<SessionCartItem> sessionItems)
        {
            var result = new List<CartItem>();
            foreach (var si in sessionItems)
            {
                var product = db.Products.Include(p => p.Images).FirstOrDefault(p => p.Id == si.ProductId);
                var size = db.ProductSizes.Find(si.ProductSizeId);
                if (product != null && size != null)
                {
                    result.Add(new CartItem
                    {
                        ProductId = si.ProductId,
                        ProductSizeId = si.ProductSizeId,
                        Quantity = si.Quantity,
                        Product = product,
                        ProductSize = size,
                        UnitPriceAtAdd = si.UnitPriceAtAdd,
                        DiscountPercentageAtAdd = si.DiscountPercentageAtAdd
                    });
                }
            }
            return result;
        }

        #endregion

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        // Merge session cart items into the logged-in user's cart (overwrite existing DB cart)
        public void MergeSessionCartToUserCart(string userId)
        {
            if (string.IsNullOrEmpty(userId)) return;

            var sessionItems = GetSessionCartItems();
            if (sessionItems == null || !sessionItems.Any()) return;

            var cart = db.Carts.Include(c => c.CartItems).FirstOrDefault(c => c.UserId == userId);
            if (cart == null)
            {
                cart = new Cart { UserId = userId, CreatedAt = DateTime.Now };
                db.Carts.Add(cart);
                db.SaveChanges();
            }

            // Remove existing cart items and replace with session items
            var existingItems = db.CartItems.Where(ci => ci.CartId == cart.Id).ToList();
            foreach (var ei in existingItems)
            {
                db.CartItems.Remove(ei);
            }
            db.SaveChanges();

            foreach (var si in sessionItems)
            {
                // Validate product/size exist
                var product = db.Products.Find(si.ProductId);
                var size = db.ProductSizes.Find(si.ProductSizeId);
                if (product == null || size == null) continue;

                db.CartItems.Add(new CartItem
                {
                    CartId = cart.Id,
                    ProductId = si.ProductId,
                    ProductSizeId = si.ProductSizeId,
                    Quantity = si.Quantity,
                    CreatedAt = DateTime.Now
                });
            }

            db.SaveChanges();

            // Clear session cart
            SaveSessionCartItems(new List<SessionCartItem>());
        }
    }

    public class SessionCartItem
    {
        public int ProductId { get; set; }
        public int ProductSizeId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPriceAtAdd { get; set; }
        public double? DiscountPercentageAtAdd { get; set; }
    }
}
