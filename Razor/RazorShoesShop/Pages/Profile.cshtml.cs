using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RazorShoesShop.Models;
namespace RazorShoesShop.Pages;

[Authorize]
public class ProfileModel(ShopDbContext db, ShopService shop, Queries queries, Accounts accounts, UserManager<AppUser> users, SignInManager<AppUser> signIn) : ShopPage(db, shop, queries, accounts, users, signIn)
{
    [BindProperty] public ProfileInput Input { get; set; } = new();

    public async Task OnGetAsync()
    {
        State.Profile = await accounts.Get(UserId);
    }
    public async Task<IActionResult> OnPostAsync()
    {
        if (ModelState.IsValid)
            try
            {
                await accounts.Save(UserId, Input, false, UserId);
                await signIn.RefreshSignInAsync((await users.FindByIdAsync(UserId))!);
                TempData["Message"] = "Đã lưu thông tin.";
                return Redirect("/Profile");
            }
            catch (BusinessException e) { ModelState.AddModelError("", e.Message); }
        State.Profile = Input;
        return Page();
    }

}
