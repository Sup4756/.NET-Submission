using Microsoft.EntityFrameworkCore;
namespace MVCShoesShop.Services;

public class Queries(ShopDbContext db, ShopService shop)
{
    public async Task<ShopState> Products(string? q, string? category, string? brand, decimal? min, decimal? max, int page)
    {
        var query = shop.Catalog(q, category, brand, min, max);
        var count = await query.CountAsync();
        var pages = Math.Max(1, (int)Math.Ceiling(count / 6d));
        page = Math.Clamp(page, 1, pages);
        return new()
        {
            Products = await query.Skip((page - 1) * 6).Take(6).ToListAsync(),
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
    public async Task<ShopState> Dashboard() => new() { Count = await db.Products.CountAsync(), OrderCount = await db.Orders.CountAsync(), CustomerCount = await db.Users.CountAsync(), Revenue = (await db.Orders.Where(x => x.Status != "Đã hủy").Select(x => x.Total).ToListAsync()).Sum() };
    public async Task<ShopState> Users(string? q, string? role, string? city, string? gender, string? locked, DateTime? birthDate = null)
    {
        var query = db.Users.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(x => x.FullName.Contains(q) || x.Email!.Contains(q) || (x.PhoneNumber != null && x.PhoneNumber.Contains(q)) || x.Address.Contains(q));
        if (!string.IsNullOrWhiteSpace(city))
            query = query.Where(x => x.City.Contains(city));
        if (birthDate.HasValue)
            query = query.Where(x => x.DateOfBirth == birthDate.Value.Date);
        if (!string.IsNullOrWhiteSpace(gender))
            query = query.Where(x => x.Gender == gender);
        if (role is "User" or "Admin")
            query = query.Where(x => db.UserRoles.Any(ur => ur.UserId == x.Id && db.Roles.Any(r => r.Id == ur.RoleId && r.Name == role)));
        var list = await query.OrderBy(x => x.FullName).ToListAsync();
        if (locked == "true")
            list = list.Where(x => x.LockoutEnd > DateTimeOffset.UtcNow).ToList();
        if (locked == "false")
            list = list.Where(x => x.LockoutEnd == null || x.LockoutEnd <= DateTimeOffset.UtcNow).ToList();
        var rolePairs = await (from ur in db.UserRoles
                               join r in db.Roles on ur.RoleId equals r.Id
                               select new { ur.UserId, Name = r.Name! }).ToListAsync();
        return new()
        {
            Users = list,
            Q = q,
            Roles = rolePairs.GroupBy(x => x.UserId).ToDictionary(g => g.Key, g => string.Join(", ", g.Select(x => x.Name)))
        };
    }
}
