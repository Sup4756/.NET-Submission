using ShoesShop.Core;
var builder = WebApplication.CreateBuilder(args);
builder.AddShop("StepShoesRazor");
builder.Services.AddRazorPages();
var app = builder.Build();
if (!app.Environment.IsDevelopment()) { app.UseExceptionHandler("/Error"); app.UseHsts(); app.UseHttpsRedirection(); }
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapRazorPages();
await app.InitializeShop();
await app.RunAsync();
public partial class Program;
