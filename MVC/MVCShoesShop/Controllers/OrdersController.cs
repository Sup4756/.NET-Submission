using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MVCShoesShop.Models;
namespace MVCShoesShop.Controllers;

[Authorize]
public class OrdersController(ShopDbContext db) : Controller
{
    [HttpGet("/Orders")]
    public async Task<IActionResult> Index()
    {
        var id = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        return View(new ShopState { Orders = await db.Orders.Include(x => x.Lines).Where(x => x.UserId == id).OrderByDescending(x => x.Id).ToListAsync() });
    }
}
