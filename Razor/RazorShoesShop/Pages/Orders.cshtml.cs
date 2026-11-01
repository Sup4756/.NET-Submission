using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ShoesShop.Core;
namespace RazorShoesShop.Pages;

[Authorize]
public class OrdersModel(ShopDbContext db, ShopService shop, Queries queries, Accounts accounts, UserManager<AppUser> users, SignInManager<AppUser> signIn) : ShopPage(db, shop, queries, accounts, users, signIn)
{
    public async Task OnGetAsync()
    {
        State.Orders = await db.Orders.Include(x => x.Lines).Where(x => x.UserId == UserId).OrderByDescending(x => x.Id).ToListAsync();
    }
}
