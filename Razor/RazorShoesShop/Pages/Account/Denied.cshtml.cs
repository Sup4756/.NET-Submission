using Microsoft.AspNetCore.Mvc.RazorPages;
using RazorShoesShop.Models;
namespace RazorShoesShop.Pages; public class AccountDeniedModel : PageModel { public ShopState State { get; } = new(); public void OnGet() { Response.StatusCode = 403; } }
