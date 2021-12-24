using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ShopDomainModel.Entities;

namespace Persistence
{
    public class ShopDbContext : DbContext
    {
        public ShopDbContext(DbContextOptions<ShopDbContext> options) : base(options)
        {
            Database.EnsureCreated();
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<Order> Orders { get; set; }

        public DbSet<OrderProduct> OrdersProducts { get; set; }

        //Data Source=(localdb)\MSSQLLocalDb;Initial Catalog=ShopDb;Integrated Security=True

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<OrderProduct>(item => item.HasKey(op => new { op.OrderId, op.ProductId }));

            modelBuilder.Entity<OrderProduct>()
                .HasOne(p => p.Product)
                .WithMany(o => o.Orders)
                .HasForeignKey(op => op.ProductId);

            modelBuilder.Entity<OrderProduct>()
                .HasOne(o => o.Order)
                .WithMany(p => p.Products)
                .HasForeignKey(op => op.OrderId);
        }
    }
}