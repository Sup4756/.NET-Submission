using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ShoesShop.Core;
namespace RazorShoesShop.Pages;

[Authorize(Roles = "Admin")]
public class AdminProductsModel(ShopDbContext db, ShopService shop, Queries queries, Accounts accounts, UserManager<AppUser> users, SignInManager<AppUser> signIn) : ShopPage(db, shop, queries, accounts, users, signIn)
{

    public async Task OnGetAsync(string? q)
    {
        var query = db.Products.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(x => x.Name.Contains(q) || x.Brand.Contains(q));
        State.Products = await query.OrderBy(x => x.Id).ToListAsync();
        State.Q = q;
    }
    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var p = await db.Products.FindAsync(id);
        if (p is null)
            return NotFound();
        p.IsDeleted = true;
        db.CartItems.RemoveRange(db.CartItems.Where(x => x.ProductId == id));
        await db.SaveChangesAsync();
        return Redirect("/Admin/Products");
    }

}
