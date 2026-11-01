using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using ShoesShop.Core;
using Xunit;
namespace ShoesShop.Tests;

public class CommerceTests : IDisposable
{
    readonly SqliteConnection connection = new("Data Source=:memory:");
    readonly ShopDbContext db;
    readonly ShopService shop;
    public CommerceTests()
    {
        connection.Open();
        db = new(new DbContextOptionsBuilder<ShopDbContext>().UseSqlite(connection).Options);
        db.Database.EnsureCreated();
        db.Users.AddRange(new AppUser { Id = "one", FullName = "One", Email = "one@example.test" }, new AppUser { Id = "two", FullName = "Two", Email = "two@example.test" });
        db.Products.Add(new Product { Id = 1, Name = "Shoe", Brand = "STEP", Category = "Running", Description = "Test", Price = 100000, Stock = 5 });
        db.SaveChanges();
        shop = new(db);
    }
    [Fact]
    public async Task CartIsStoredAndCannotExceedStock()
    {
        await shop.AddToCart("one", 1, 3);
        await Assert.ThrowsAsync<BusinessException>(() => shop.AddToCart("one", 1, 3));
        Assert.Equal(3, (await shop.Cart("one")).Single().Quantity);
        Assert.Empty(await shop.Cart("two"));
    }
    [Fact]
    public async Task UserCannotUpdateAnotherUsersCart()
    {
        await shop.AddToCart("one", 1);
        var item = (await shop.Cart("one")).Single();
        await Assert.ThrowsAsync<BusinessException>(() => shop.UpdateCart("two", item.Id, 0));
        Assert.Single(await shop.Cart("one"));
    }
    [Fact]
    public async Task CheckoutCalculatesTotalAndDecrementsStock()
    {
        await shop.AddToCart("one", 1, 2);
        int id = await shop.Checkout("one", new()
        {
            Recipient = "One",
            Phone = "0901234567",
            ShippingAddress = "Home"
        });
        var order = await db.Orders.Include(x => x.Lines).SingleAsync(x => x.Id == id);
        Assert.Equal(200000, order.Total);
        Assert.Equal(2, order.Lines.Single().Quantity);
        Assert.Equal(3, (await db.Products.SingleAsync()).Stock);
        Assert.Empty(await shop.Cart("one"));
    }
    [Fact]
    public async Task InsufficientStockDoesNotCreateOrder()
    {
        await shop.AddToCart("one", 1, 3);
        var p = await db.Products.SingleAsync();
        p.Stock = 1;
        await db.SaveChangesAsync();
        await Assert.ThrowsAsync<BusinessException>(() => shop.Checkout("one", new()));
        Assert.Empty(await db.Orders.ToListAsync());
        Assert.Single(await shop.Cart("one"));
    }
    [Fact]
    public async Task CancelRestocksExactlyOnceAndCannotReopen()
    {
        await shop.AddToCart("one", 1, 2);
        var id = await shop.Checkout("one", new());
        await shop.UpdateOrder(id, "Đã hủy");
        await shop.UpdateOrder(id, "Đã hủy");
        Assert.Equal(5, (await db.Products.SingleAsync()).Stock);
        await Assert.ThrowsAsync<BusinessException>(() => shop.UpdateOrder(id, "Đang xử lý"));
    }
    [Fact]
    public async Task OrderMustFollowValidTransitions()
    {
        await shop.AddToCart("one", 1);
        var id = await shop.Checkout("one", new());
        await Assert.ThrowsAsync<BusinessException>(() => shop.UpdateOrder(id, "Hoàn thành"));
        await shop.UpdateOrder(id, "Đang xử lý");
        await shop.UpdateOrder(id, "Đang giao");
        await shop.UpdateOrder(id, "Hoàn thành");
        await Assert.ThrowsAsync<BusinessException>(() => shop.UpdateOrder(id, "Đã hủy"));
    }
    [Fact]
    public async Task ZeroQuantityRemovesCartItem()
    {
        await shop.AddToCart("one", 1);
        await shop.UpdateCart("one", (await shop.Cart("one")).Single().Id, 0);
        Assert.Empty(await shop.Cart("one"));
    }
    [Fact]
    public async Task SoftDeletedUserIsHiddenButDataIsRetained()
    {
        var u = await db.Users.SingleAsync(x => x.Id == "one");
        u.IsDeleted = true;
        await db.SaveChangesAsync();
        Assert.Single(await db.Users.ToListAsync());
        Assert.Equal(2, await db.Users.IgnoreQueryFilters().CountAsync());
    }
    public void Dispose()
    {
        db.Dispose();
        connection.Dispose();
    }
}
