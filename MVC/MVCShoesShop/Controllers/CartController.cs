using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShoesShop.Core;
namespace MVCShoesShop.Controllers;

[Authorize]
public class CartController(ShopService shop) : Controller
{
    string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;
    [HttpGet("/Cart")] public async Task<IActionResult> Index() => View(new ShopState { Cart = await shop.Cart(UserId) });
    [HttpPost("/Cart/Update")]
    public async Task<IActionResult> Update(int id, int quantity)
    {
        try
        {
            await shop.UpdateCart(UserId, id, quantity);
        }
        catch (BusinessException e) { TempData["Message"] = e.Message; }
        return Redirect("/Cart");
    }
    [HttpPost("/Cart/Checkout")]
    public async Task<IActionResult> Checkout(CheckoutInput input)
    {
        if (!ModelState.IsValid)
        {
            TempData["Message"] = "Vui lòng nhập đầy đủ thông tin giao hàng hợp lệ.";
            return View("Index", new ShopState { Cart = await shop.Cart(UserId), Checkout = input });
        }
        try
        {
            await shop.Checkout(UserId, input);
            return Redirect("/Orders");
        }
        catch (BusinessException e) { TempData["Message"] = e.Message; return Redirect("/Cart"); }
    }
}
