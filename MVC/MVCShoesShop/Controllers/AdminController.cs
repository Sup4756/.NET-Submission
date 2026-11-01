using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShoesShop.Core;
namespace MVCShoesShop.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController(ShopDbContext db, ShopService shop, Queries queries, Accounts accounts) : Controller
{
    [HttpGet("/Admin")] public async Task<IActionResult> Index() => View(await queries.Dashboard());
    [HttpGet("/Admin/Products")] public async Task<IActionResult> Products(string? q) => View(new ShopState { Products = await shop.Catalog(q).ToListAsync(), Q = q });
    [HttpGet("/Admin/Products/Edit/{id:int?}")]
    public async Task<IActionResult> ProductEdit(int id = 0)
    {
        var p = id == 0 ? new Product() : await db.Products.FindAsync(id);
        return p is null ? NotFound() : View(new ShopState { Product = p });
    }
    [HttpPost("/Admin/Products/Edit/{id:int?}")]
    public async Task<IActionResult> ProductEdit(Product input, int id = 0)
    {
        ModelState.Remove("input.Id");
        if (!ModelState.IsValid)
            return View(new ShopState { Product = input });
        if (id == 0)
        {
            input.Id = 0;
            db.Products.Add(input);
        }
        else
        {
            var p = await db.Products.FindAsync(id);
            if (p is null)
                return NotFound();
            p.Name = input.Name;
            p.Brand = input.Brand;
            p.Category = input.Category;
            p.Price = input.Price;
            p.Stock = input.Stock;
            p.Description = input.Description;
        }
        await db.SaveChangesAsync();
        return Redirect("/Admin/Products");
    }
    [HttpPost("/Admin/Products/Delete/{id:int}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var p = await db.Products.FindAsync(id);
        if (p is null)
            return NotFound();
        p.IsDeleted = true;
        db.CartItems.RemoveRange(db.CartItems.Where(x => x.ProductId == id));
        await db.SaveChangesAsync();
        return Redirect("/Admin/Products");
    }
    [HttpGet("/Admin/Orders")] public async Task<IActionResult> Orders() => View(new ShopState { Orders = await db.Orders.Include(x => x.Lines).OrderByDescending(x => x.Id).ToListAsync() });
    [HttpPost("/Admin/Orders/Update")]
    public async Task<IActionResult> UpdateOrder(int id, string status)
    {
        try
        {
            await shop.UpdateOrder(id, status);
        }
        catch (BusinessException e) { TempData["Message"] = e.Message; }
        return Redirect("/Admin/Orders");
    }
    [HttpGet("/Admin/Accounts"), HttpGet("/Admin/Customers")]
    public async Task<IActionResult> Users(string? q, string? role, string? city, string? gender, string? locked, DateTime? birthDate)
    {
        if (Request.Path == "/Admin/Customers")
            role = "User";
        return View(await queries.Users(q, role, city, gender, locked, birthDate));
    }
    [HttpGet("/Admin/Accounts/Edit/{id}")] public async Task<IActionResult> UserEdit(string id)
    {
        if (!await db.Users.AnyAsync(x => x.Id == id)) return NotFound();
        return View(new ShopState { Profile = await accounts.Get(id), UserId = id, IsAdmin = true });
    }
    [HttpPost("/Admin/Accounts/Edit/{id}")]
    public async Task<IActionResult> UserEdit(string id, ProfileInput input)
    {
        if (ModelState.IsValid)
            try
            {
                await accounts.Save(id, input, true, User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                return Redirect("/Admin/Accounts");
            }
            catch (BusinessException e) { ModelState.AddModelError("", e.Message); }
        return View(new ShopState { Profile = input, UserId = id, IsAdmin = true });
    }
    [HttpPost("/Admin/Accounts/Delete/{id}")]
    public async Task<IActionResult> DeleteUser(string id)
    {
        try
        {
            await accounts.Delete(id, User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        }
        catch (BusinessException e) { TempData["Message"] = e.Message; }
        return Redirect("/Admin/Accounts");
    }
}
