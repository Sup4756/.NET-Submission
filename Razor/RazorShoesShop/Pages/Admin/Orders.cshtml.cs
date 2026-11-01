using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ShoesShop.Core;
namespace RazorShoesShop.Pages;

[Authorize(Roles = "Admin")]
public class AdminOrdersModel(ShopDbContext db, ShopService shop, Queries queries, Accounts accounts, UserManager<AppUser> users, SignInManager<AppUser> signIn) : ShopPage(db, shop, queries, accounts, users, signIn)
{

    public async Task OnGetAsync()
    {
        State.Orders = await db.Orders.Include(x => x.Lines).OrderByDescending(x => x.Id).ToListAsync();
    }
    public async Task<IActionResult> OnPostUpdateAsync(int id, string status)
    {
        try
        {
            await shop.UpdateOrder(id, status);
        }
        catch (BusinessException e) { TempData["Message"] = e.Message; }
        return Redirect("/Admin/Orders");
    }

}
