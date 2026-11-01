using ShoesShop.Core;
var builder = WebApplication.CreateBuilder(args);
builder.AddShop("StepShoesMvc");
builder.Services.AddControllersWithViews(o => o.Filters.Add(new Microsoft.AspNetCore.Mvc.AutoValidateAntiforgeryTokenAttribute()));
var app = builder.Build();
if (!app.Environment.IsDevelopment()) { app.UseExceptionHandler("/Home/Error"); app.UseHsts(); app.UseHttpsRedirection(); }
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
await app.InitializeShop();
await app.RunAsync();
public partial class Program;
