using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
namespace ShoesShop.Core;

public static class Hosting
{
    public static void AddShop(this WebApplicationBuilder builder, string database)
    {
        builder.Services.AddAuthorization();
        var sqlite = builder.Configuration["DatabaseProvider"] == "Sqlite";
        builder.Services.AddDbContext<ShopDbContext>(o =>
        {
            var connection = builder.Configuration.GetConnectionString("DefaultConnection");
            if (sqlite)
                o.UseSqlite(connection ?? $"Data Source={database}.db");
            else
                o.UseSqlServer(connection ?? $"Server=(localdb)\\MSSQLLocalDB;Database={database};Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True");
        });
        builder.Services.AddIdentity<AppUser, IdentityRole>(o => { o.User.RequireUniqueEmail = true; o.Password.RequiredLength = 8; o.Lockout.MaxFailedAccessAttempts = 5; })
         .AddEntityFrameworkStores<ShopDbContext>().AddDefaultTokenProviders();
        builder.Services.AddScoped<IUserValidator<AppUser>, UniqueAccountValidator>();
        builder.Services.ConfigureApplicationCookie(o => { o.LoginPath = "/Account/Login"; o.AccessDeniedPath = "/Account/Denied"; o.Cookie.Name = database + ".Identity"; });
        builder.Services.Configure<SecurityStampValidatorOptions>(o => o.ValidationInterval = TimeSpan.Zero);
        builder.Services.AddScoped<Queries>();
        builder.Services.AddScoped<ShopService>();
        builder.Services.AddScoped<Accounts>();
    }
    public static async Task InitializeShop(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var sp = scope.ServiceProvider;
        var db = sp.GetRequiredService<ShopDbContext>();
        if (db.Database.IsSqlite())
            await db.Database.EnsureCreatedAsync();
        else
            await db.Database.MigrateAsync();
        var roles = sp.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var role in new[] { "Admin", "User" })
        if (!await roles.RoleExistsAsync(role))
            await roles.CreateAsync(new(role));
        if (!await db.Products.IgnoreQueryFilters().AnyAsync())
        {
            string[] names = ["Air Motion", "Court Classic", "Street Runner", "Trail Explorer", "Daily Canvas", "Urban Leather", "Cloud Runner", "Retro Court", "Sprint Pro", "Weekend Slip", "Hiking Peak", "City Flex"];
            for (int i = 0; i < names.Length; i++)
                db.Products.Add(new()
                {
                    Name = names[i],
                    Brand = new[] { "Nike", "Adidas", "Converse" }[i % 3],
                    Category = new[] { "Chạy bộ", "Thể thao", "Thời trang" }[i % 3],
                    Price = 690000 + i * 150000,
                    Stock = 20 + i,
                    Description = "Thiết kế nhẹ, êm chân và linh hoạt cho từng bước đi. Chất liệu thoáng khí, đế cao su bền bỉ. Sản phẩm chính hãng tại STEP Shoes."
                });
            await db.SaveChangesAsync();
        }
        if (app.Environment.EnvironmentName == "Development")
        {
            var users = sp.GetRequiredService<UserManager<AppUser>>();
            foreach (var role in new[] { "Admin", "User" })
            {
                var email = role.ToLowerInvariant() + "@stepshoes.local";
                if (!await db.Users.IgnoreQueryFilters().AnyAsync(x => x.NormalizedEmail == users.NormalizeEmail(email)))
                {
                    var user = new AppUser { UserName = email, Email = email, FullName = role == "Admin" ? "Quản trị viên" : "Khách hàng mẫu", EmailConfirmed = true };
                    var result = await users.CreateAsync(user, "Demo@12345");
                    if (!result.Succeeded)
                        throw new InvalidOperationException(string.Join(";", result.Errors.Select(x => x.Description)));
                    await users.AddToRoleAsync(user, role);
                }
            }
        }
    }
}
