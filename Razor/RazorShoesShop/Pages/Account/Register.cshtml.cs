using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RazorShoesShop.Models;
namespace RazorShoesShop.Pages;

public class AccountRegisterModel(ShopDbContext db, ShopService shop, Queries queries, Accounts accounts, UserManager<AppUser> users, SignInManager<AppUser> signIn) : ShopPage(db, shop, queries, accounts, users, signIn)
{
    [BindProperty] public RegisterInput Input { get; set; } = new();

    public void OnGet()
    {
    }
    public async Task<IActionResult> OnPostAsync()
    {
        if (ModelState.IsValid)
        {
            var u = new AppUser { UserName = Input.Email, Email = Input.Email, FullName = Input.FullName };
            var result = await users.CreateAsync(u, Input.Password);
            if (result.Succeeded)
            {
                await users.AddToRoleAsync(u, "User");
                await signIn.SignInAsync(u, false);
                return Redirect("/Products");
            }
            foreach (var e in result.Errors)
                ModelState.AddModelError("", e.Description);
        }
        State.Register = Input;
        return Page();
    }

}
