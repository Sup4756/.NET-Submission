using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
namespace RazorShoesShop.Data;

public class ShopDbContext(DbContextOptions<ShopDbContext> options) : IdentityDbContext<AppUser>(options)
{
    public DbSet<Product> Products => Set<Product>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderLine> OrderLines => Set<OrderLine>();
    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);
        b.Entity<Product>().HasQueryFilter(x => !x.IsDeleted);
        b.Entity<AppUser>().HasQueryFilter(x => !x.IsDeleted);
        b.Entity<Product>().Property(x => x.Price).HasPrecision(18, 2);
        b.Entity<Order>().Property(x => x.Total).HasPrecision(18, 2);
        b.Entity<OrderLine>().Property(x => x.UnitPrice).HasPrecision(18, 2);
        b.Entity<CartItem>().HasIndex(x => new { x.UserId, x.ProductId }).IsUnique();
        b.Entity<CartItem>().HasOne(x => x.Product).WithMany().OnDelete(DeleteBehavior.Restrict);
        b.Entity<Order>().HasOne(x => x.User).WithMany().OnDelete(DeleteBehavior.Restrict);
        b.Entity<OrderLine>().HasOne<Product>().WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);
    }
}
