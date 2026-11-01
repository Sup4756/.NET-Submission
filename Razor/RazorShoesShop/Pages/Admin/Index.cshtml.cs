using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ShoesShop.Core;
namespace RazorShoesShop.Pages;

[Authorize(Roles = "Admin")]
public class AdminIndexModel(ShopDbContext db, ShopService shop, Queries queries, Accounts accounts, UserManager<AppUser> users, SignInManager<AppUser> signIn) : ShopPage(db, shop, queries, accounts, users, signIn)
{
    public async Task OnGetAsync()
    {
        State = new()
        {
            Count = await db.Products.CountAsync(),
            OrderCount = await db.Orders.CountAsync(),
            CustomerCount = await db.Users.CountAsync(),
            Revenue = (await db.Orders.Where(x => x.Status != "Đã hủy").Select(x => x.Total).ToListAsync()).Sum()
        };
    }
}
