using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;
using RazorShoesShop.Models;
namespace RazorShoesShop.Pages;

public abstract class ShopPage(ShopDbContext db, ShopService shop, Queries queries, Accounts accounts, UserManager<AppUser> users, SignInManager<AppUser> signIn) : PageModel
{
    protected readonly ShopDbContext db = db; protected readonly ShopService shop = shop; protected readonly Queries queries = queries; protected readonly Accounts accounts = accounts; protected readonly UserManager<AppUser> users = users; protected readonly SignInManager<AppUser> signIn = signIn;
    public ShopState State { get; set; } = new();
    protected string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;
}
