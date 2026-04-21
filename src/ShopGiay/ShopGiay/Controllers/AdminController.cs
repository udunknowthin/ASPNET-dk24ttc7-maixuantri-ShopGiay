using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
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
        private const int OrderPageSize = 15;
        private const int ProductPageSize = 12;
        private const int CategoryPageSize = 12;
        private const long MaxImageBytes = 10L * 1024 * 1024; // 10 MB

        private static readonly Regex InvalidFolderCharsRegex = new Regex(@"[\\/:*?""<>|\x00]");

        private ApplicationDbContext db = new ApplicationDbContext();

        private ApplicationUserManager UserManager
        {
            get { return HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>(); }
        }

        // ─── DASHBOARD ────────────────────────────────────────────────────────────
        public ActionResult Index()
        {
            var today = DateTime.Today;

            // Biểu đồ: 6 tháng gần nhất
            var months = new List<DateTime>();
            for (int i = 5; i >= 0; i--) months.Add(today.AddMonths(-i));

            var monthLabels = months.Select(m => m.ToString("MM/yyyy")).ToList();

            var monthlyRevenue = months.Select(m =>
                db.Orders
                  .Where(o => o.Status == OrderStatus.Delivered
                           && o.CreatedAt.Year == m.Year
                           && o.CreatedAt.Month == m.Month)
                  .Select(o => o.TotalAmount)
                  .DefaultIfEmpty(0)
                  .Sum()
            ).ToList();

            var monthlyOrders = months.Select(m =>
                db.Orders.Count(o => o.CreatedAt.Year == m.Year && o.CreatedAt.Month == m.Month)
            ).ToList();

            var model = new AdminDashboardViewModel
            {
                TotalUsers = db.Users.Count(),
                TotalOrders = db.Orders.Count(),
                TotalRevenue = db.Orders
                    .Where(o => o.Status == OrderStatus.Delivered)
                    .Select(o => o.TotalAmount)
                    .DefaultIfEmpty(0)
                    .Sum(),
                PendingOrders = db.Orders.Count(o => o.Status == OrderStatus.Pending),
                RecentOrders = db.Orders.OrderByDescending(o => o.CreatedAt).Take(5).ToList(),

                StatusPending = db.Orders.Count(o => o.Status == OrderStatus.Pending),
                StatusProcessing = db.Orders.Count(o => o.Status == OrderStatus.Processing),
                StatusShipped = db.Orders.Count(o => o.Status == OrderStatus.Shipped),
                StatusDelivered = db.Orders.Count(o => o.Status == OrderStatus.Delivered),
                StatusCancelled = db.Orders.Count(o => o.Status == OrderStatus.Cancelled),

                MonthLabels = monthLabels,
                MonthlyRevenue = monthlyRevenue,
                MonthlyOrders = monthlyOrders
            };
            return View("~/Views/Admin/Dashboard/Index.cshtml", model);
        }

        // ─── USERS ────────────────────────────────────────────────────────────────
        public ActionResult Users(string keyword)
        {
            var query = db.Users.AsQueryable();
            if (!string.IsNullOrWhiteSpace(keyword))
                query = query.Where(u => u.Email.Contains(keyword) || u.FullName.Contains(keyword));

            var roleStore = new RoleStore<IdentityRole>(db);
            var roleMgr = new RoleManager<IdentityRole>(roleStore);
            var adminRole = roleMgr.FindByName("Admin");
            var adminIds = adminRole != null
                ? adminRole.Users.Select(ur => ur.UserId).ToList()
                : new List<string>();

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
                Address = user.Address,
                IsAdmin = UserManager.IsInRole(user.Id, "Admin"),
                IsCurrentUser = User.Identity.GetUserId() == user.Id
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
            if (!result.Succeeded)
            {
                foreach (var e in result.Errors) ModelState.AddModelError("", e);
                return View("~/Views/Admin/Dashboard/EditUser.cshtml", model);
            }

            // Handle role change — prevent self-demotion
            var currentUserId = User.Identity.GetUserId();
            if (model.Id != currentUserId)
            {
                var currentIsAdmin = await UserManager.IsInRoleAsync(model.Id, "Admin");
                if (model.IsAdmin && !currentIsAdmin)
                    await UserManager.AddToRoleAsync(model.Id, "Admin");
                else if (!model.IsAdmin && currentIsAdmin)
                    await UserManager.RemoveFromRoleAsync(model.Id, "Admin");
            }

            TempData["Success"] = "Cập nhật thông tin người dùng thành công.";
            return RedirectToAction("Users");
        }

        // ─── ORDERS ───────────────────────────────────────────────────────────────
        public ActionResult Orders(string keyword, DateTime? fromDate, DateTime? toDate, OrderStatus? status, int page = 1)
        {
            var query = db.Orders.Include(o => o.User).Include(o => o.OrderItems).AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
                query = query.Where(o => o.FullName.Contains(keyword)
                                      || o.Email.Contains(keyword)
                                      || o.PhoneNumber.Contains(keyword));
            if (fromDate.HasValue)
                query = query.Where(o => o.CreatedAt >= fromDate.Value);
            if (toDate.HasValue)
            {
                var endDate = toDate.Value.AddDays(1);
                query = query.Where(o => o.CreatedAt < endDate);
            }
            if (status.HasValue)
                query = query.Where(o => o.Status == status.Value);

            var totalCount = query.Count();
            var orders = query.OrderByDescending(o => o.CreatedAt)
                              .Skip((page - 1) * OrderPageSize)
                              .Take(OrderPageSize)
                              .ToList();

            var model = new AdminOrderListViewModel
            {
                Orders = orders,
                SearchKeyword = keyword,
                FromDate = fromDate,
                ToDate = toDate,
                StatusFilter = status,
                Page = page,
                PageSize = OrderPageSize,
                TotalCount = totalCount
            };
            return View("~/Views/Admin/Dashboard/Orders.cshtml", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateOrderStatus(int orderId, OrderStatus status, int page = 1,
            string keyword = null, DateTime? fromDate = null, DateTime? toDate = null, OrderStatus? statusFilter = null)
        {
            var order = db.Orders.Find(orderId);
            if (order == null) return HttpNotFound();

            if (order.Status == OrderStatus.Delivered || order.Status == OrderStatus.Cancelled)
            {
                TempData["Error"] = "Đơn hàng #" + orderId + " đã "
                    + (order.Status == OrderStatus.Delivered ? "giao thành công" : "bị hủy")
                    + ", không thể thay đổi trạng thái.";
                return RedirectToAction("Orders", new { keyword, fromDate, toDate, status = statusFilter, page });
            }

            order.Status = status;
            order.UpdatedAt = DateTime.Now;
            db.SaveChanges();
            TempData["Success"] = "Cập nhật trạng thái đơn hàng #" + orderId + " thành công.";
            return RedirectToAction("Orders", new { keyword, fromDate, toDate, status = statusFilter, page });
        }

        // ─── PRODUCTS ─────────────────────────────────────────────────────────────
        public ActionResult Products(string keyword, int? categoryId, int page = 1)
        {
            var query = db.Products.Include(p => p.Category).Include(p => p.Images).AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
                query = query.Where(p => p.Name.Contains(keyword) || p.SKU.Contains(keyword));
            if (categoryId.HasValue)
                query = query.Where(p => p.CategoryId == categoryId.Value);

            var totalCount = query.Count();
            var products = query.OrderByDescending(p => p.CreatedAt)
                                .Skip((page - 1) * ProductPageSize)
                                .Take(ProductPageSize)
                                .ToList();

            var model = new AdminProductListViewModel
            {
                Products = products,
                Categories = db.Categories.OrderBy(c => c.Name).ToList(),
                SearchKeyword = keyword,
                CategoryId = categoryId,
                Page = page,
                PageSize = ProductPageSize,
                TotalCount = totalCount
            };
            return View("~/Views/Admin/Dashboard/Products.cshtml", model);
        }

        public ActionResult EditProduct(int id)
        {
            var product = db.Products.Include(p => p.Images).FirstOrDefault(p => p.Id == id);
            if (product == null) return HttpNotFound();

            var model = new AdminEditProductViewModel
            {
                Id = product.Id,
                SKU = product.SKU,
                Name = product.Name,
                Description = product.Description,
                LongDescription = product.LongDescription,
                Price = product.Price,
                IsDiscounted = product.IsDiscounted,
                DiscountPercentage = product.DiscountPercentage,
                CategoryId = product.CategoryId,
                ExistingImages = product.Images.ToList(),
                CategoryOptions = db.Categories.OrderBy(c => c.Name)
                    .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
                    .ToList()
            };
            return View("~/Views/Admin/Dashboard/EditProduct.cshtml", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditProduct(AdminEditProductViewModel model)
        {
            if (!IsValidWindowsFolderName(model.SKU))
                ModelState.AddModelError("SKU", "SKU không được chứa các ký tự: \\ / : * ? \" < > |");

            if (!ModelState.IsValid)
            {
                model.ExistingImages = db.ProductImages.Where(i => i.ProductId == model.Id).ToList();
                model.CategoryOptions = db.Categories.OrderBy(c => c.Name)
                    .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
                    .ToList();
                return View("~/Views/Admin/Dashboard/EditProduct.cshtml", model);
            }

            var product = db.Products.Find(model.Id);
            if (product == null) return HttpNotFound();

            // Rename image folder if SKU changed
            if (!string.Equals(product.SKU, model.SKU, StringComparison.OrdinalIgnoreCase))
            {
                string oldFolder = Server.MapPath("~/Content/images/products/" + product.SKU + "/");
                string newFolder = Server.MapPath("~/Content/images/products/" + model.SKU + "/");
                if (Directory.Exists(oldFolder) && !Directory.Exists(newFolder))
                    Directory.Move(oldFolder, newFolder);
            }

            product.SKU = model.SKU;
            product.Name = model.Name;
            product.Description = model.Description;
            product.LongDescription = model.LongDescription;
            product.Price = model.Price;
            product.IsDiscounted = model.IsDiscounted;
            product.DiscountPercentage = model.IsDiscounted ? model.DiscountPercentage : 0;
            product.CategoryId = model.CategoryId;
            product.UpdatedAt = DateTime.Now;
            db.SaveChanges();

            var newFiles = GetValidUploadedFiles("uploadedImages");
            if (newFiles.Count > 0)
                SaveProductImages(product.Id, product.SKU, newFiles);

            TempData["Success"] = "Cập nhật sản phẩm \"" + product.Name + "\" thành công.";
            return RedirectToAction("Products");
        }

        public ActionResult CreateProduct()
        {
            var model = new AdminEditProductViewModel
            {
                CategoryOptions = db.Categories.OrderBy(c => c.Name)
                    .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
                    .ToList()
            };
            return View("~/Views/Admin/Dashboard/EditProduct.cshtml", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateProduct(AdminEditProductViewModel model)
        {
            if (!IsValidWindowsFolderName(model.SKU))
                ModelState.AddModelError("SKU", "SKU không được chứa các ký tự: \\ / : * ? \" < > |");

            var newFiles = GetValidUploadedFiles("uploadedImages");
            if (newFiles.Count < 3)
                ModelState.AddModelError("", "Vui lòng tải lên ít nhất 3 hình ảnh cho sản phẩm mới.");

            if (!ModelState.IsValid)
            {
                model.CategoryOptions = db.Categories.OrderBy(c => c.Name)
                    .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
                    .ToList();
                return View("~/Views/Admin/Dashboard/EditProduct.cshtml", model);
            }

            var product = new Product
            {
                SKU = model.SKU,
                Name = model.Name,
                Description = model.Description,
                LongDescription = model.LongDescription,
                Price = model.Price,
                IsDiscounted = model.IsDiscounted,
                DiscountPercentage = model.IsDiscounted ? model.DiscountPercentage : 0,
                CategoryId = model.CategoryId,
                CreatedAt = DateTime.Now
            };
            db.Products.Add(product);
            db.SaveChanges();

            SaveProductImages(product.Id, product.SKU, newFiles);

            TempData["Success"] = "Thêm sản phẩm \"" + product.Name + "\" thành công.";
            return RedirectToAction("Products");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteProductImage(int imageId, int productId)
        {
            var image = db.ProductImages.Find(imageId);
            if (image != null && image.ProductId == productId)
            {
                var product = db.Products.Find(productId);
                if (product != null)
                {
                    string filePath = Server.MapPath("~/Content/images/products/" + product.SKU + "/" + image.ImageUrl);
                    if (System.IO.File.Exists(filePath))
                        System.IO.File.Delete(filePath);
                }
                db.ProductImages.Remove(image);
                db.SaveChanges();
            }
            return RedirectToAction("EditProduct", new { id = productId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteProduct(int id)
        {
            var product = db.Products.Include(p => p.Images).FirstOrDefault(p => p.Id == id);
            if (product == null) return HttpNotFound();

            string folderPath = Server.MapPath("~/Content/images/products/" + product.SKU + "/");
            if (Directory.Exists(folderPath))
                Directory.Delete(folderPath, true);

            var name = product.Name;
            db.Products.Remove(product);
            db.SaveChanges();
            TempData["Success"] = "Đã xóa sản phẩm \"" + name + "\".";
            return RedirectToAction("Products");
        }

        // ─── CATEGORIES ───────────────────────────────────────────────────────────
        public ActionResult Categories(string keyword, int page = 1)
        {
            var query = db.Categories.AsQueryable();
            if (!string.IsNullOrWhiteSpace(keyword))
                query = query.Where(c => c.Name.Contains(keyword));

            var totalCount = query.Count();
            var categories = query.OrderBy(c => c.Name)
                                  .Skip((page - 1) * CategoryPageSize)
                                  .Take(CategoryPageSize)
                                  .ToList();

            var model = new AdminCategoryListViewModel
            {
                Categories = categories,
                SearchKeyword = keyword,
                Page = page,
                PageSize = CategoryPageSize,
                TotalCount = totalCount
            };

            ViewBag.PaginationPage = page;
            ViewBag.PaginationTotalPages = model.TotalPages;
            ViewBag.PaginationAction = "Categories";
            ViewBag.PaginationController = "Admin";
            ViewBag.PaginationRouteValues = new Dictionary<string, object> { { "keyword", keyword } };

            return View("~/Views/Admin/Dashboard/Categories.cshtml", model);
        }

        public ActionResult EditCategory(int id)
        {
            var category = db.Categories.Find(id);
            if (category == null) return HttpNotFound();

            var model = new AdminEditCategoryViewModel
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                CurrentImageUrl = category.ImageUrl
            };
            return View("~/Views/Admin/Dashboard/EditCategory.cshtml", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditCategory(AdminEditCategoryViewModel model, HttpPostedFileBase categoryImage)
        {
            if (categoryImage != null && categoryImage.ContentLength > MaxImageBytes)
                ModelState.AddModelError("", "Ảnh vượt quá 10 MB, vui lòng chọn ảnh nhỏ hơn.");

            if (!ModelState.IsValid)
            {
                model.CurrentImageUrl = db.Categories.Find(model.Id) != null ? db.Categories.Find(model.Id).ImageUrl : null;
                return View("~/Views/Admin/Dashboard/EditCategory.cshtml", model);
            }

            var category = db.Categories.Find(model.Id);
            if (category == null) return HttpNotFound();

            category.Name = model.Name;
            category.Description = model.Description;
            category.UpdatedAt = DateTime.Now;

            if (categoryImage != null && categoryImage.ContentLength > 0)
                category.ImageUrl = SaveCategoryImage(categoryImage);

            db.SaveChanges();
            TempData["Success"] = "Cập nhật danh mục \"" + category.Name + "\" thành công.";
            return RedirectToAction("Categories");
        }

        public ActionResult CreateCategory()
        {
            return View("~/Views/Admin/Dashboard/EditCategory.cshtml", new AdminEditCategoryViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateCategory(AdminEditCategoryViewModel model, HttpPostedFileBase categoryImage)
        {
            if (categoryImage != null && categoryImage.ContentLength > MaxImageBytes)
                ModelState.AddModelError("", "Ảnh vượt quá 10 MB, vui lòng chọn ảnh nhỏ hơn.");

            if (!ModelState.IsValid)
                return View("~/Views/Admin/Dashboard/EditCategory.cshtml", model);

            var category = new Category
            {
                Name = model.Name,
                Description = model.Description,
                CreatedAt = DateTime.Now
            };

            if (categoryImage != null && categoryImage.ContentLength > 0)
                category.ImageUrl = SaveCategoryImage(categoryImage);

            db.Categories.Add(category);
            db.SaveChanges();
            TempData["Success"] = "Thêm danh mục \"" + category.Name + "\" thành công.";
            return RedirectToAction("Categories");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteCategory(int id)
        {
            var category = db.Categories.Find(id);
            if (category == null) return HttpNotFound();

            if (db.Products.Any(p => p.CategoryId == id))
            {
                TempData["Error"] = "Không thể xóa danh mục đang có sản phẩm.";
                return RedirectToAction("Categories");
            }

            if (!string.IsNullOrWhiteSpace(category.ImageUrl))
            {
                string filePath = Server.MapPath("~/Content/images/categories/" + category.ImageUrl);
                if (System.IO.File.Exists(filePath))
                    System.IO.File.Delete(filePath);
            }

            var name = category.Name;
            db.Categories.Remove(category);
            db.SaveChanges();
            TempData["Success"] = "Đã xóa danh mục \"" + name + "\".";
            return RedirectToAction("Categories");
        }

        // ─── HELPERS ──────────────────────────────────────────────────────────────
        private static bool IsValidWindowsFolderName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return false;
            if (InvalidFolderCharsRegex.IsMatch(name)) return false;
            var trimmed = name.TrimEnd('.', ' ');
            if (trimmed.Length == 0 || name == "." || name == "..") return false;
            return name.Length <= 255;
        }

        private static string SanitizeFileName(string fileName)
        {
            var invalid = Path.GetInvalidFileNameChars();
            return string.Join("_", fileName.Split(invalid));
        }

        private List<HttpPostedFileBase> GetValidUploadedFiles(string inputName)
        {
            var result = new List<HttpPostedFileBase>();
            for (int i = 0; i < Request.Files.Count; i++)
            {
                if (Request.Files.GetKey(i) == inputName)
                {
                    var f = Request.Files[i];
                    if (f != null && f.ContentLength > 0)
                    {
                        if (f.ContentLength > MaxImageBytes)
                            ModelState.AddModelError("", $"Ảnh \"{Path.GetFileName(f.FileName)}\" vượt quá 10 MB, vui lòng chọn ảnh nhỏ hơn.");
                        else
                            result.Add(f);
                    }
                }
            }
            return result;
        }

        private void SaveProductImages(int productId, string sku, List<HttpPostedFileBase> files)
        {
            string folder = Server.MapPath("~/Content/images/products/" + sku + "/");
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            foreach (var file in files)
            {
                string fileName = SanitizeFileName(Path.GetFileName(file.FileName));
                // Avoid collisions
                string dest = Path.Combine(folder, fileName);
                if (System.IO.File.Exists(dest))
                {
                    string ext = Path.GetExtension(fileName);
                    string name = Path.GetFileNameWithoutExtension(fileName);
                    fileName = name + "_" + DateTime.Now.Ticks + ext;
                    dest = Path.Combine(folder, fileName);
                }
                file.SaveAs(dest);
                db.ProductImages.Add(new ProductImage
                {
                    ProductId = productId,
                    ImageUrl = fileName,
                    CreatedAt = DateTime.Now
                });
            }
            db.SaveChanges();
        }

        private string SaveCategoryImage(HttpPostedFileBase file)
        {
            string folder = Server.MapPath("~/Content/images/categories/");
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            string fileName = SanitizeFileName(Path.GetFileName(file.FileName));
            string dest = Path.Combine(folder, fileName);
            if (System.IO.File.Exists(dest))
            {
                string ext = Path.GetExtension(fileName);
                string name = Path.GetFileNameWithoutExtension(fileName);
                fileName = name + "_" + DateTime.Now.Ticks + ext;
                dest = Path.Combine(folder, fileName);
            }
            file.SaveAs(dest);
            return fileName;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}