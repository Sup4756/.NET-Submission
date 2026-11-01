using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
namespace ShoesShop.Core;

public class ShopDbContextFactory : IDesignTimeDbContextFactory<ShopDbContext>
{
    public ShopDbContext CreateDbContext(string[] args) => new(new DbContextOptionsBuilder<ShopDbContext>().UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=StepShoesDesign;Trusted_Connection=True;TrustServerCertificate=True").Options);
}
