using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
namespace BlazorShoesShop.Services;

// Soft-deleted accounts retain their Identity keys and history. Do not reuse their email.
public class UniqueAccountValidator(ShopDbContext db) : IUserValidator<AppUser>
{
    public async Task<IdentityResult> ValidateAsync(UserManager<AppUser> manager, AppUser user)
    {
        var email = manager.NormalizeEmail(await manager.GetEmailAsync(user));
        var exists = await db.Users.IgnoreQueryFilters().AsNoTracking()
            .AnyAsync(x => x.Id != user.Id && x.NormalizedEmail == email);
        return exists
            ? IdentityResult.Failed(new IdentityError { Code = "DuplicateEmail", Description = "Email này đã được sử dụng." })
            : IdentityResult.Success;
    }
}
