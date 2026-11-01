namespace ShoesShop.Core;

public class ShopState
{
    public List<Product> Products { get; set; } = [];
    public Product? Product
    {
        get; set;
    }
    public List<CartItem> Cart { get; set; } = [];
    public List<Order> Orders { get; set; } = [];
    public Dictionary<string,string> Roles {get;set;}=new();
    public List<AppUser> Users { get; set; } = [];
    public List<string> Categories { get; set; } = [];
    public List<string> Brands { get; set; } = [];
    public string? Q
    {
        get; set;
    }
    public string? Category
    {
        get; set;
    }
    public string? Brand
    {
        get; set;
    }
    public decimal? Min
    {
        get; set;
    }
    public decimal? Max
    {
        get; set;
    }
    public int Page { get; set; } = 1;
    public int Pages { get; set; } = 1;
    public int Count
    {
        get; set;
    }
    public decimal Revenue
    {
        get; set;
    }
    public int OrderCount
    {
        get; set;
    }
    public int CustomerCount
    {
        get; set;
    }
    public string Title { get; set; } = "STEP Shoes";
    public string? Message
    {
        get; set;
    }
    public string? UserId
    {
        get; set;
    }
    public bool IsAdmin
    {
        get; set;
    }
    public ProfileInput Profile { get; set; } = new();
    public CheckoutInput Checkout { get; set; } = new();
    public LoginInput Login { get; set; } = new();
    public RegisterInput Register { get; set; } = new();
}
