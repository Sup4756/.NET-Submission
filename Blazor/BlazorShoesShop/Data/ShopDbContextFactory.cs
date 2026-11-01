using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
namespace BlazorShoesShop.Data;

public class ShopDbContextFactory : IDesignTimeDbContextFactory<ShopDbContext>
{
    public ShopDbContext CreateDbContext(string[] args) => new(new DbContextOptionsBuilder<ShopDbContext>().UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=StepShoesBlazor;Trusted_Connection=True;TrustServerCertificate=True").Options);
}
