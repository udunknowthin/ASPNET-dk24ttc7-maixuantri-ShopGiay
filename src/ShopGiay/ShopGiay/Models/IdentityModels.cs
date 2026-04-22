using System;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace ShopGiay.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; }
        public string Address { get; set; }

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            return userIdentity;
        }
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<ProductSize> ProductSizes { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Xóa các cột Identity không sử dụng
            modelBuilder.Entity<ApplicationUser>()
                .Ignore(u => u.PhoneNumberConfirmed)
                .Ignore(u => u.TwoFactorEnabled)
                .Ignore(u => u.LockoutEndDateUtc)
                .Ignore(u => u.LockoutEnabled)
                .Ignore(u => u.AccessFailedCount)
                .Ignore(u => u.EmailConfirmed);

            modelBuilder.Entity<Cart>()
                .HasRequired(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Order>()
                .HasRequired(o => o.User)
                .WithMany()
                .HasForeignKey(o => o.UserId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<OrderItem>()
                .HasRequired(oi => oi.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(oi => oi.OrderId)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<OrderItem>()
                .HasRequired(oi => oi.Product)
                .WithMany()
                .HasForeignKey(oi => oi.ProductId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<CartItem>()
                .HasRequired(ci => ci.Product)
                .WithMany()
                .HasForeignKey(ci => ci.ProductId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<CartItem>()
                .HasRequired(ci => ci.ProductSize)
                .WithMany()
                .HasForeignKey(ci => ci.ProductSizeId)
                .WillCascadeOnDelete(false);
        }

        public override int SaveChanges()
        {
            HandleTimestamps();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync()
        {
            HandleTimestamps();
            return base.SaveChangesAsync();
        }

        private void HandleTimestamps()
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

            foreach (var entry in entries)
            {
                var createdAtProp = entry.Entity.GetType().GetProperty("CreatedAt");
                var updatedAtProp = entry.Entity.GetType().GetProperty("UpdatedAt");

                if (entry.State == EntityState.Added && createdAtProp != null)
                {
                    createdAtProp.SetValue(entry.Entity, DateTime.Now);
                }
                else if (entry.State == EntityState.Modified && updatedAtProp != null)
                {
                    updatedAtProp.SetValue(entry.Entity, DateTime.Now);
                    if (createdAtProp != null)
                    {
                        entry.Property("CreatedAt").IsModified = false;
                    }
                }
            }
        }
    }
}