using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RazorShoesShop.Models;
namespace RazorShoesShop.Pages;

[Authorize(Roles = "Admin")]
public class AdminUsersModel(ShopDbContext db, ShopService shop, Queries queries, Accounts accounts, UserManager<AppUser> users, SignInManager<AppUser> signIn) : ShopPage(db, shop, queries, accounts, users, signIn)
{

    public async Task OnGetAsync(string? q, string? role, string? city, string? gender, string? locked, DateTime? birthDate)
    {
        State = await queries.Users(q, role, city, gender, locked, birthDate);
    }
    public async Task<IActionResult> OnPostDeleteAsync(string id)
    {
        try
        {
            await accounts.Delete(id, UserId);
        }
        catch (BusinessException e) { TempData["Message"] = e.Message; }
        return Redirect("/Admin/Accounts");
    }

}
