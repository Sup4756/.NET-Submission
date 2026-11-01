using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RazorShoesShop.Models;
namespace RazorShoesShop.Pages;

public class DetailsModel(ShopDbContext db, ShopService shop, Queries queries, Accounts accounts, UserManager<AppUser> users, SignInManager<AppUser> signIn) : ShopPage(db, shop, queries, accounts, users, signIn)
{

    public async Task<IActionResult> OnGetAsync(int id)
    {
        State.Product = await db.Products.SingleOrDefaultAsync(x => x.Id == id);
        return State.Product is null ? NotFound() : Page();
    }
    public async Task<IActionResult> OnPostAddAsync(int id, int quantity = 1)
    {
        if (User.Identity?.IsAuthenticated != true)
            return Challenge();
        try
        {
            await shop.AddToCart(UserId, id, quantity);
            TempData["Message"] = "Đã thêm vào giỏ hàng.";
        }
        catch (BusinessException e) { TempData["Message"] = e.Message; }
        return Redirect("/Products/" + id);
    }

}
