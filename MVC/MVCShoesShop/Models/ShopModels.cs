using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
namespace MVCShoesShop.Models;

public class AppUser : IdentityUser
{
    [Required, StringLength(100)] public string FullName { get; set; } = "";
    [StringLength(20)] public string Gender { get; set; } = "";
    public DateTime? DateOfBirth
    {
        get; set;
    }
    [StringLength(250)] public string Address { get; set; } = "";
    [StringLength(100)] public string City { get; set; } = "";
    public bool IsDeleted
    {
        get; set;
    }
}
public class Product
{
    public int Id
    {
        get; set;
    }
    [Required, StringLength(120)] public string Name { get; set; } = "";
    [Required, StringLength(60)] public string Brand { get; set; } = "";
    [Required, StringLength(60)] public string Category { get; set; } = "";
    [Range(1000, 100000000)]
    public decimal Price
    {
        get; set;
    }
    [Range(0, 100000)]
    public int Stock
    {
        get; set;
    }
    [Required, StringLength(3000)] public string Description { get; set; } = "";
    public bool IsDeleted
    {
        get; set;
    }
}
public class CartItem
{
    public int Id
    {
        get; set;
    }
    public string UserId { get; set; } = "";
    public AppUser User { get; set; } = null!;
    public int ProductId
    {
        get; set;
    }
    public Product Product { get; set; } = null!;
    [Range(1, 99)]
    public int Quantity
    {
        get; set;
    }
}
public class Order
{
    public int Id
    {
        get; set;
    }
    public string UserId { get; set; } = "";
    public AppUser User { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = "Mới đặt";
    public string Recipient { get; set; } = "";
    public string Phone { get; set; } = "";
    public string ShippingAddress { get; set; } = "";
    public decimal Total
    {
        get; set;
    }
    public List<OrderLine> Lines { get; set; } = [];
}
public class OrderLine
{
    public int Id
    {
        get; set;
    }
    public int OrderId
    {
        get; set;
    }
    public Order Order { get; set; } = null!;
    public int ProductId
    {
        get; set;
    }
    public string ProductName { get; set; } = "";
    public decimal UnitPrice
    {
        get; set;
    }
    public int Quantity
    {
        get; set;
    }
}
public class LoginInput
{
    [Required, EmailAddress] public string Email { get; set; } = "";
    [Required, DataType(DataType.Password)] public string Password { get; set; } = "";
}
public class RegisterInput : LoginInput
{
    [Required, StringLength(100)] public string FullName { get; set; } = "";
    [Compare(nameof(Password)), DataType(DataType.Password)] public string ConfirmPassword { get; set; } = "";
}
public class ProfileInput
{
    [Required, StringLength(100)] public string FullName { get; set; } = "";
    [Required, EmailAddress] public string Email { get; set; } = "";
    [Phone]
    public string? PhoneNumber
    {
        get; set;
    }
    [StringLength(20)] public string Gender { get; set; } = "";
    [DataType(DataType.Date)]
    public DateTime? DateOfBirth
    {
        get; set;
    }
    [StringLength(250)] public string Address { get; set; } = "";
    [StringLength(100)] public string City { get; set; } = "";
    public string Role { get; set; } = "User";
    public bool Locked
    {
        get; set;
    }
}
public class CheckoutInput
{
    [Required, StringLength(100)] public string Recipient { get; set; } = "";
    [Required, Phone] public string Phone { get; set; } = "";
    [Required, StringLength(350)] public string ShippingAddress { get; set; } = "";
}
public class BusinessException(string message) : Exception(message);
