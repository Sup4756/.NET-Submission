using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
namespace MVCShoesShop.Data;

public class ShopDbContextFactory : IDesignTimeDbContextFactory<ShopDbContext>
{
    public ShopDbContext CreateDbContext(string[] args) => new(new DbContextOptionsBuilder<ShopDbContext>().UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=StepShoesMvc;Trusted_Connection=True;TrustServerCertificate=True").Options);
}
