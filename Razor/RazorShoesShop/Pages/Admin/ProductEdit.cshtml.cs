using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ShoesShop.Core;
namespace RazorShoesShop.Pages;

[Authorize(Roles = "Admin")]
public class AdminProductEditModel(ShopDbContext db, ShopService shop, Queries queries, Accounts accounts, UserManager<AppUser> users, SignInManager<AppUser> signIn) : ShopPage(db, shop, queries, accounts, users, signIn)
{
    [BindProperty] public Product Input { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id = 0)
    {
        State.Product = id == 0 ? new Product() : await db.Products.FindAsync(id);
        return State.Product is null ? NotFound() : Page();
    }
    public async Task<IActionResult> OnPostAsync(int id = 0)
    {
        if (!ModelState.IsValid)
        {
            State.Product = Input;
            return Page();
        }
        if (id == 0)
        {
            Input.Id = 0;
            db.Products.Add(Input);
        }
        else
        {
            var p = await db.Products.FindAsync(id);
            if (p is null)
                return NotFound();
            p.Name = Input.Name;
            p.Brand = Input.Brand;
            p.Category = Input.Category;
            p.Price = Input.Price;
            p.Stock = Input.Stock;
            p.Description = Input.Description;
        }
        await db.SaveChangesAsync();
        return Redirect("/Admin/Products");
    }

}
