using BlazorShoesShop.Components;
using ShoesShop.Core;
var builder = WebApplication.CreateBuilder(args);
builder.AddShop("StepShoesBlazor");
builder.Services.AddRazorComponents();
builder.Services.AddCascadingAuthenticationState();
var app = builder.Build();
if (!app.Environment.IsDevelopment()) { app.UseExceptionHandler("/Error"); app.UseHsts(); app.UseHttpsRedirection(); }
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();
// Identity cookies must be written in an HTTP request; logout remains a CSRF-protected POST.
app.MapPost("/Account/Logout", async (Microsoft.AspNetCore.Identity.SignInManager<AppUser> signIn) =>
{
    await signIn.SignOutAsync();
    return Results.LocalRedirect("/");
}).RequireAuthorization().AddEndpointFilter(async (context, next) =>
{
    try
    {
        await context.HttpContext.RequestServices
            .GetRequiredService<Microsoft.AspNetCore.Antiforgery.IAntiforgery>()
            .ValidateRequestAsync(context.HttpContext);
    }
    catch (Microsoft.AspNetCore.Antiforgery.AntiforgeryValidationException)
    {
        return Results.BadRequest();
    }
    return await next(context);
});
app.MapRazorComponents<App>();
await app.InitializeShop();
await app.RunAsync();
public partial class Program;
