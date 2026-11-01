using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RazorShoesShop.Models;
namespace RazorShoesShop.Pages;

[Authorize(Roles = "Admin")]
public class AdminUserEditModel(ShopDbContext db, ShopService shop, Queries queries, Accounts accounts, UserManager<AppUser> users, SignInManager<AppUser> signIn) : ShopPage(db, shop, queries, accounts, users, signIn)
{
    [BindProperty] public ProfileInput Input { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(string id)
    {
        if (!await db.Users.AnyAsync(x => x.Id == id)) return NotFound();
        State.Profile = await accounts.Get(id);
        State.UserId = id;
        State.IsAdmin = true;
        return Page();
    }
    public async Task<IActionResult> OnPostAsync(string id)
    {
        if (ModelState.IsValid)
            try
            {
                await accounts.Save(id, Input, true, UserId);
                return Redirect("/Admin/Accounts");
            }
            catch (BusinessException e) { ModelState.AddModelError("", e.Message); }
        State.Profile = Input;
        State.UserId = id;
        State.IsAdmin = true;
        return Page();
    }

}
