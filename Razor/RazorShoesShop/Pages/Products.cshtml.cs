using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ShoesShop.Core;
namespace RazorShoesShop.Pages;

public class ProductsModel(ShopDbContext db, ShopService shop, Queries queries, Accounts accounts, UserManager<AppUser> users, SignInManager<AppUser> signIn) : ShopPage(db, shop, queries, accounts, users, signIn)
{

    public async Task OnGetAsync(string? q, string? category, string? brand, decimal? min, decimal? max, [FromQuery(Name = "page")] int page = 1)
    {
        var query = db.Products.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(x => x.Name.Contains(q) || x.Brand.Contains(q));
        if (!string.IsNullOrWhiteSpace(category))
            query = query.Where(x => x.Category == category);
        if (!string.IsNullOrWhiteSpace(brand))
            query = query.Where(x => x.Brand == brand);
        if (min.HasValue)
            query = query.Where(x => x.Price >= min);
        if (max.HasValue)
            query = query.Where(x => x.Price <= max);
        var count = await query.CountAsync();
        var pages = Math.Max(1, (int)Math.Ceiling(count / 6d));
        page = Math.Clamp(page, 1, pages);
        State = new()
        {
            Products = await query.OrderBy(x => x.Id).Skip((page - 1) * 6).Take(6).ToListAsync(),
            Count = count,
            Pages = pages,
            Page = page,
            Q = q,
            Category = category,
            Brand = brand,
            Min = min,
            Max = max,
            Categories = await db.Products.Select(x => x.Category).Distinct().ToListAsync(),
            Brands = await db.Products.Select(x => x.Brand).Distinct().ToListAsync()
        };
    }
}
