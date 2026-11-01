using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RazorShoesShop.Models;
namespace RazorShoesShop.Pages;

public class AccountLoginModel(ShopDbContext db, ShopService shop, Queries queries, Accounts accounts, UserManager<AppUser> users, SignInManager<AppUser> signIn) : ShopPage(db, shop, queries, accounts, users, signIn)
{
    [BindProperty] public LoginInput Input { get; set; } = new();

    public void OnGet()
    {
    }
    public async Task<IActionResult> OnPostAsync()
    {
        if (ModelState.IsValid)
        {
            var result = await signIn.PasswordSignInAsync(Input.Email, Input.Password, false, true);
            if (result.Succeeded)
            {
                var u = await users.FindByEmailAsync(Input.Email);
                return Redirect(await users.IsInRoleAsync(u!, "User") ? "/Products" : "/Admin");
            }
        }
        ModelState.AddModelError("", "Đăng nhập không thành công. Kiểm tra thông tin hoặc trạng thái tài khoản.");
        State.Login = Input;
        return Page();
    }

}
