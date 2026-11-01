using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RazorShoesShop.Models;
namespace RazorShoesShop.Pages;

[Authorize]
public class CartModel(ShopDbContext db, ShopService shop, Queries queries, Accounts accounts, UserManager<AppUser> users, SignInManager<AppUser> signIn) : ShopPage(db, shop, queries, accounts, users, signIn)
{
    [BindProperty] public CheckoutInput Input { get; set; } = new();

    public async Task OnGetAsync()
    {
        State.Cart = await db.CartItems.Include(x => x.Product).Where(x => x.UserId == UserId).ToListAsync();
    }
    public async Task<IActionResult> OnPostUpdateAsync(int id, int quantity)
    {
        try
        {
            await shop.UpdateCart(UserId, id, quantity);
        }
        catch (BusinessException e) { TempData["Message"] = e.Message; }
        return Redirect("/Cart");
    }
    public async Task<IActionResult> OnPostCheckoutAsync()
    {
        if (!ModelState.IsValid)
        {
            await OnGetAsync();
            State.Checkout = Input;
            return Page();
        }
        try
        {
            await shop.Checkout(UserId, Input);
            return Redirect("/Orders");
        }
        catch (BusinessException e) { ModelState.AddModelError("", e.Message); await OnGetAsync(); State.Checkout = Input; return Page(); }
    }

}
