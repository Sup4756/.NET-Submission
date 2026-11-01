using System.Data;
using Microsoft.EntityFrameworkCore;
namespace ShoesShop.Core;

public class ShopService(ShopDbContext db)
{
    public IQueryable<Product> Catalog(string? q = null, string? category = null, string? brand = null, decimal? min = null, decimal? max = null)
    {
        var query = db.Products.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(x => x.Name.Contains(q) || x.Brand.Contains(q));
        if (!string.IsNullOrWhiteSpace(category))
            query = query.Where(x => x.Category == category);
        if (!string.IsNullOrWhiteSpace(brand))
            query = query.Where(x => x.Brand == brand);
        if (min.HasValue)
            query = query.Where(x => x.Price >= min);
        if (max.HasValue)
            query = query.Where(x => x.Price <= max);
        return query.OrderBy(x => x.Id);
    }
    public async Task AddToCart(string userId, int productId, int quantity = 1)
    {
        var p = await db.Products.FindAsync(productId) ?? throw new BusinessException("Sản phẩm không tồn tại.");
        var item = await db.CartItems.SingleOrDefaultAsync(x => x.UserId == userId && x.ProductId == productId);
        var total = (item?.Quantity ?? 0) + quantity;
        if (quantity < 1 || total > 99 || total > p.Stock)
            throw new BusinessException("Số lượng vượt quá tồn kho (tối đa 99 sản phẩm).");
        if (item is null)
            db.CartItems.Add(new()
            {
                UserId = userId,
                ProductId = productId,
                Quantity = quantity
            });
        else
            item.Quantity = total;
        await db.SaveChangesAsync();
    }
    public Task<List<CartItem>> Cart(string userId) => db.CartItems.Include(x => x.Product).Where(x => x.UserId == userId).ToListAsync();
    public async Task UpdateCart(string userId, int id, int quantity)
    {
        var item = await db.CartItems.Include(x => x.Product).SingleOrDefaultAsync(x => x.Id == id && x.UserId == userId) ?? throw new BusinessException("Không tìm thấy mục giỏ hàng.");
        if (quantity == 0)
            db.CartItems.Remove(item);
        else if (quantity < 1 || quantity > 99 || quantity > item.Product.Stock)
            throw new BusinessException("Số lượng không hợp lệ hoặc vượt tồn kho.");
        else
            item.Quantity = quantity;
        await db.SaveChangesAsync();
    }
    public async Task<int> Checkout(string userId, CheckoutInput input)
    {
        db.ChangeTracker.Clear();
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var items = await Cart(userId);
        if (items.Count == 0)
            throw new BusinessException("Giỏ hàng đang trống.");
        foreach (var item in items)
        {
            if (item.Quantity > item.Product.Stock)
                throw new BusinessException($"{item.Product.Name} không đủ tồn kho.");
            item.Product.Stock -= item.Quantity;
        }
        var order = new Order
        {
            UserId = userId,
            Recipient = input.Recipient,
            Phone = input.Phone,
            ShippingAddress = input.ShippingAddress,
            Total = items.Sum(x => x.Product.Price * x.Quantity),
            Lines = items.Select(x => new OrderLine { ProductId = x.ProductId, ProductName = x.Product.Name, UnitPrice = x.Product.Price, Quantity = x.Quantity }).ToList()
        };
        db.Orders.Add(order);
        db.CartItems.RemoveRange(items);
        await db.SaveChangesAsync();
        await tx.CommitAsync();
        return order.Id;
    }
    public static readonly string[] Statuses = ["Mới đặt", "Đang xử lý", "Đang giao", "Hoàn thành", "Đã hủy"];
    public async Task UpdateOrder(int id, string status)
    {
        if (!Statuses.Contains(status))
            throw new BusinessException("Trạng thái không hợp lệ.");
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var order = await db.Orders.Include(x => x.Lines).SingleOrDefaultAsync(x => x.Id == id) ?? throw new BusinessException("Không tìm thấy đơn hàng.");
        if (order.Status == status)
            return;
        var valid = order.Status switch
        {
            "Mới đặt" => status is "Đang xử lý" or "Đã hủy",
            "Đang xử lý" => status is "Đang giao" or "Đã hủy",
            "Đang giao" => status == "Hoàn thành",
            _ => false
        };
        if (!valid)
            throw new BusinessException("Không thể chuyển sang trạng thái đã chọn.");
        if (status == "Đã hủy")
        foreach (var line in order.Lines)
        {
            var p = await db.Products.IgnoreQueryFilters().SingleAsync(x => x.Id == line.ProductId);
            p.Stock += line.Quantity;
        }
        order.Status = status;
        await db.SaveChangesAsync();
        await tx.CommitAsync();
    }
}
