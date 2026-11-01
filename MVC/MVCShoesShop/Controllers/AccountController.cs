using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MVCShoesShop.Models;
namespace MVCShoesShop.Controllers;

public class AccountController(UserManager<AppUser> users, SignInManager<AppUser> signIn, Accounts accounts) : Controller
{
    [HttpGet("/Account/Login")] public IActionResult Login() => View(new ShopState());
    [HttpPost("/Account/Login")]
    public async Task<IActionResult> Login(LoginInput input)
    {
        if (ModelState.IsValid)
        {
            var result = await signIn.PasswordSignInAsync(input.Email, input.Password, false, true);
            if (result.Succeeded)
            {
                var u = await users.FindByEmailAsync(input.Email);
                return Redirect(await users.IsInRoleAsync(u!, "User") ? "/Products" : "/Admin");
            }
        }
        ModelState.AddModelError("", "Đăng nhập không thành công. Kiểm tra thông tin hoặc trạng thái tài khoản.");
        return View(new ShopState { Login = input });
    }
    [HttpGet("/Account/Register")] public IActionResult Register() => View(new ShopState());
    [HttpPost("/Account/Register")]
    public async Task<IActionResult> Register(RegisterInput input)
    {
        if (ModelState.IsValid)
        {
            var u = new AppUser { UserName = input.Email, Email = input.Email, FullName = input.FullName };
            var result = await users.CreateAsync(u, input.Password);
            if (result.Succeeded)
            {
                await users.AddToRoleAsync(u, "User");
                await signIn.SignInAsync(u, false);
                return Redirect("/Products");
            }
            foreach (var e in result.Errors)
                ModelState.AddModelError("", e.Description);
        }
        return View(new ShopState { Register = input });
    }
    [Authorize, HttpPost("/Account/Logout")]
    public async Task<IActionResult> Logout()
    {
        await signIn.SignOutAsync();
        return Redirect("/");
    }
    [Authorize, HttpGet("/Profile")] public async Task<IActionResult> Profile() => View(new ShopState { Profile = await accounts.Get(User.FindFirstValue(ClaimTypes.NameIdentifier)!) });
    [Authorize, HttpPost("/Profile")]
    public async Task<IActionResult> Profile(ProfileInput input)
    {
        if (ModelState.IsValid)
            try
            {
                var id = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
                await accounts.Save(id, input, false, id);
                await signIn.RefreshSignInAsync((await users.FindByIdAsync(id))!);
                TempData["Message"] = "Đã lưu thông tin.";
                return Redirect("/Profile");
            }
            catch (BusinessException e) { ModelState.AddModelError("", e.Message); }
        return View(new ShopState { Profile = input });
    }
    [HttpGet("/Account/Denied")]
    public IActionResult Denied()
    {
        Response.StatusCode = 403;
        return View(new ShopState());
    }
}
