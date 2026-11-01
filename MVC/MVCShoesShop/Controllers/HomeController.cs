using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShoesShop.Core;
namespace MVCShoesShop.Controllers;

public class HomeController(ShopDbContext db) : Controller
{
    public async Task<IActionResult> Index() => View(new ShopState { Products = await db.Products.Take(4).ToListAsync() });
    public IActionResult Error() => Problem("Có lỗi xảy ra. Vui lòng thử lại.");
}
