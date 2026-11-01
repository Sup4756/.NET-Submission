using System.Security.Claims;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using BlazorShoesShop.Models;
namespace BlazorShoesShop.Components;

public abstract class ShopComponent : ComponentBase
{
    [Inject] protected ShopDbContext db { get; set; } = null!;
    [Inject] protected ShopService shop { get; set; } = null!;
    [Inject] protected Queries queries { get; set; } = null!;
    [Inject] protected Accounts accounts { get; set; } = null!;
    [Inject] protected UserManager<AppUser> users { get; set; } = null!;
    [Inject] protected SignInManager<AppUser> signIn { get; set; } = null!;
    [Inject] protected NavigationManager nav { get; set; } = null!;
    [CascadingParameter] protected HttpContext Http { get; set; } = null!;
    protected string UserId => Http.User.FindFirstValue(ClaimTypes.NameIdentifier)!;
    protected ShopState s = new();
    protected string? message;
    protected bool Validate(object input)
    {
        var results = new List<ValidationResult>();
        var valid = Validator.TryValidateObject(input, new(input), results, true);
        if (!valid)
            message = string.Join(" ", results.Select(x => x.ErrorMessage));
        return valid;
    }
    protected async Task Run(Func<Task> action)
    {
        try
        {
            await action();
        }
        catch (BusinessException e) { message = e.Message; }
    }
}
