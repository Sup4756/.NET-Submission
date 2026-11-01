using System.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
namespace ShoesShop.Core;

public class Accounts(UserManager<AppUser> users, ShopDbContext db)
{
    public async Task<ProfileInput> Get(string id)
    {
        var u = await users.FindByIdAsync(id) ?? throw new BusinessException("Tài khoản không tồn tại.");
        return new()
        {
            FullName = u.FullName,
            Email = u.Email!,
            PhoneNumber = u.PhoneNumber,
            Gender = u.Gender,
            DateOfBirth = u.DateOfBirth,
            Address = u.Address,
            City = u.City,
            Role = await users.IsInRoleAsync(u, "Admin") ? "Admin" : "User",
            Locked = await users.IsLockedOutAsync(u)
        };
    }
    static void Check(IdentityResult result)
    {
        if (!result.Succeeded)
            throw new BusinessException(string.Join(" ", result.Errors.Select(x => x.Description)));
    }
    public async Task Save(string id, ProfileInput input, bool admin, string actorId)
    {
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var u = await users.FindByIdAsync(id) ?? throw new BusinessException("Tài khoản không tồn tại.");
        if (input.DateOfBirth > DateTime.Today)
            throw new BusinessException("Ngày sinh không hợp lệ.");
        if (admin && input.Role is not ("User" or "Admin"))
            throw new BusinessException("Vai trò không hợp lệ.");
        if (admin && id == actorId && (input.Locked || input.Role != "Admin"))
            throw new BusinessException("Không thể tự khóa hoặc hạ quyền tài khoản quản trị đang đăng nhập.");
        u.FullName = input.FullName;
        u.PhoneNumber = input.PhoneNumber;
        u.Gender = input.Gender;
        u.DateOfBirth = input.DateOfBirth;
        u.Address = input.Address;
        u.City = input.City;
        u.Email = input.Email;
        u.UserName = input.Email;
        Check(await users.UpdateAsync(u));
        if (admin)
        {
            var roles = await users.GetRolesAsync(u);
            if (!roles.Contains(input.Role))
            {
                Check(await users.RemoveFromRolesAsync(u, roles));
                Check(await users.AddToRoleAsync(u, input.Role));
            }
            Check(await users.SetLockoutEnabledAsync(u, true));
            Check(await users.SetLockoutEndDateAsync(u, input.Locked ? DateTimeOffset.MaxValue : null));
        }
        Check(await users.UpdateSecurityStampAsync(u));
        await tx.CommitAsync();
    }
    public async Task Delete(string id, string actorId)
    {
        if (id == actorId)
            throw new BusinessException("Không thể xóa tài khoản đang đăng nhập.");
        var u = await users.FindByIdAsync(id) ?? throw new BusinessException("Không tìm thấy tài khoản.");
        u.IsDeleted = true;
        Check(await users.UpdateAsync(u));
        Check(await users.UpdateSecurityStampAsync(u));
    }
}
