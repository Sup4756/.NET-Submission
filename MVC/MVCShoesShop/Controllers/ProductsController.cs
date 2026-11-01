using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShoesShop.Core;
namespace MVCShoesShop.Controllers;

public class ProductsController(ShopDbContext db, ShopService shop, Queries queries) : Controller
{
    [HttpGet("/Products")] public async Task<IActionResult> Index(string? q, string? category, string? brand, decimal? min, decimal? max, int page = 1) => View(await queries.Products(q, category, brand, min, max, page));
    [HttpGet("/Products/{id:int}")]
    public async Task<IActionResult> Details(int id)
    {
        var p = await db.Products.SingleOrDefaultAsync(x => x.Id == id);
        return p is null ? NotFound() : View(new ShopState { Product = p });
    }
    [Authorize, HttpPost("/Products/{id:int}/Add")]
    public async Task<IActionResult> Add(int id, int quantity = 1)
    {
        try
        {
            await shop.AddToCart(User.FindFirstValue(ClaimTypes.NameIdentifier)!, id, quantity);
            TempData["Message"] = "Đã thêm sản phẩm vào giỏ hàng.";
        }
        catch (BusinessException e) { TempData["Message"] = e.Message; }
        return Redirect("/Products/" + id);
    }
}
