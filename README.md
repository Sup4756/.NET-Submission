# STEP Shoes — Razor Pages, Blazor và MVC

Ba bài tập bán giày dùng .NET 10, Entity Framework Core và ASP.NET Core Identity. Mở **`ShoesShop.slnx`** ở thư mục gốc bằng Visual Studio 2026 (18.x).

## Chạy bằng Visual Studio và MSSQLLocalDB

1. Cài workload **ASP.NET and web development**, **.NET 10 SDK** và SQL Server Express LocalDB qua Visual Studio Installer.
2. Mở `ShoesShop.slnx`, chờ NuGet restore. Chuột phải một trong ba web project → **Set as Startup Project**.
3. Chọn profile **http** rồi nhấn **Ctrl+F5**. Mỗi ứng dụng tự áp dụng migration, tạo database và nạp sản phẩm mẫu ở lần chạy đầu.
4. Đăng ký tài khoản mới hoặc dùng tài khoản demo bên dưới. Có thể cấu hình nhiều startup projects vì mỗi bài có cổng riêng.

| Bài | Project | HTTP | Database mặc định |
| --- | --- | --- | --- |
| Razor Pages | `Razor/RazorShoesShop` | 5101 | `StepShoesRazor` |
| Blazor | `Blazor/BlazorShoesShop` | 5102 | `StepShoesBlazor` |
| MVC | `MVC/MVCShoesShop` | 5103 | `StepShoesMvc` |

Cả ba kết nối `Server=(localdb)\MSSQLLocalDB` bằng Windows Authentication, dùng cùng một thiết kế database nhưng dữ liệu độc lập. Xem trong **View → SQL Server Object Explorer → SQL Server → (localdb)\MSSQLLocalDB → Databases**. Không cần tạo bảng thủ công.

Nếu chọn profile **https**, tin cậy chứng chỉ bằng `dotnet dev-certs https --trust`; cổng HTTPS tương ứng là 7101, 7102, 7103.

### Tài khoản demo — chỉ tự tạo ở Development

| Vai trò | Email | Mật khẩu demo |
| --- | --- | --- |
| Admin | `admin@stepshoes.local` | `Demo@12345` |
| User | `user@stepshoes.local` | `Demo@12345` |

Đây là dữ liệu mẫu cho bài tập, không dùng khi triển khai công khai. Môi trường Production không tự tạo các tài khoản này. Tài khoản đăng ký luôn nhận vai trò User. Đăng nhập User về `/Products`, Admin về `/Admin`. Trang admin và các thao tác quản trị được bảo vệ ở server.

## Chạy bằng dòng lệnh

Tại thư mục gốc:

```sh
dotnet restore ShoesShop.slnx
dotnet build ShoesShop.slnx
dotnet run --project Razor/RazorShoesShop --launch-profile http
# Hoặc:
dotnet run --project Blazor/BlazorShoesShop --launch-profile http
dotnet run --project MVC/MVCShoesShop --launch-profile http
```

### Linux / môi trường không có LocalDB

LocalDB chỉ hỗ trợ Windows. Trên Linux có hai lựa chọn:

- Kết nối SQL Server thật bằng biến `ConnectionStrings__DefaultConnection` (hoặc user-secrets của web project). Giữ `DatabaseProvider=SqlServer` hoặc không đặt biến này. Mỗi project nên dùng tên database riêng. Không commit mật khẩu vào appsettings.
- Chạy SQLite để phát triển và kiểm thử khi chưa có SQL Server. Đây là **database lưu trên đĩa**, không phải dữ liệu giả trong bộ nhớ. Schema được tạo bằng EF `EnsureCreated`; SQL Server dùng migration riêng. Không trộn file SQLite với migration SQL Server.

```sh
# Bash: chạy cả ba ứng dụng với ba file SQLite độc lập
./scripts/run-dev.sh --sqlite
# Ctrl+C dừng cả ba. Database: artifacts/data; log: artifacts/logs.
```

Hoặc chạy riêng (Bash):

```sh
ASPNETCORE_ENVIRONMENT=Development DatabaseProvider=Sqlite \
ConnectionStrings__DefaultConnection='Data Source=step-local.db' \
dotnet run --project Blazor/BlazorShoesShop --no-launch-profile --urls http://127.0.0.1:5102
```

## Chức năng

- Navbar khách hàng: logo, trang chủ, sản phẩm, tìm kiếm, giỏ hàng và menu tài khoản.
- Danh mục lọc theo tên/thương hiệu, loại, giá; phân trang 6 sản phẩm/trang; trang chi tiết và thêm giỏ hàng.
- Giỏ hàng lưu database theo tài khoản, cập nhật số lượng, xóa, đặt hàng COD, lịch sử đơn hàng và thông tin giao hàng.
- Hồ sơ: họ tên, giới tính, ngày sinh, số điện thoại, email, địa chỉ và thành phố.
- Sidebar admin: tổng quan, đơn hàng, sản phẩm, khách hàng và tài khoản.
- Sản phẩm: thêm, xem, sửa, xóa mềm và tìm kiếm. Xóa sản phẩm cũng loại sản phẩm đó khỏi giỏ hàng; dòng đơn hàng cũ vẫn giữ tên và giá lúc mua.
- Tài khoản: lọc theo tên/email/SĐT/địa chỉ, giới tính, ngày sinh, thành phố, vai trò, trạng thái; xem/sửa, đổi User/Admin, khóa/mở khóa và xóa mềm. Trang khách hàng chỉ liệt kê User.
- Đổi quyền, khóa và xóa mềm vô hiệu hóa phiên đăng nhập cũ. Admin không thể tự xóa, tự khóa hoặc hạ quyền tài khoản đang đăng nhập. User chỉ sửa hồ sơ chính mình; dữ liệu Role/Locked gửi lên từ trang hồ sơ không được áp dụng.
- Đặt hàng dùng transaction và kiểm tra tồn kho; giá/tổng tiền được tính lại ở server. Hủy đơn trước khi giao hoàn tồn kho đúng một lần.
- Đơn hàng: `Mới đặt → Đang xử lý → Đang giao → Hoàn thành`; có thể hủy từ `Mới đặt` hoặc `Đang xử lý`.

Thanh toán COD là luồng mua hàng của bài tập; chưa tích hợp cổng thanh toán trực tuyến. Hình giày SVG là hình minh họa dùng chung, không phải ảnh từng mẫu giày.

## Kiến trúc

- **Razor**: `.cshtml` + `PageModel`, trực tiếp truy vấn `ShopDbContext` cho danh mục, chi tiết, giỏ hàng, đơn hàng và quản trị. Dùng các dịch vụ chung cho thao tác cần transaction/Identity.
- **Blazor**: các Razor Components, routing/layout/components riêng; dùng **static server-side rendering (SSR)** và named form handlers. Các thao tác được xử lý trong component, không dùng MVC controller để dựng trang. SSR cho phép Identity ghi cookie ngay trong HTTP request. Không yêu cầu WebAssembly hoặc JavaScript cho nghiệp vụ.
- **MVC**: controller nhận request, model/viewmodel giữ dữ liệu, `.cshtml` view hiển thị; các hành động POST có kiểm tra antiforgery.
- **Shared/ShoesShop.Core**: entity, `ShopDbContext : IdentityDbContext<AppUser>`, migration SQL Server, nghiệp vụ giỏ hàng/đơn hàng và quản lý Identity. Ba web project dùng cùng component dữ liệu nhưng không chuyển hướng sang một web project khác.
- Ba giao diện dùng cùng CSS và hình minh họa, có bố cục responsive. Các file giao diện riêng ở từng project để thể hiện đúng kiến trúc bài học.

Xem [kết quả kiểm thử](docs/VALIDATION.md), [thiết kế database](docs/DATABASE.md) và [SQL migration](docs/database.sql).

## Migrations

Ứng dụng tự gọi `MigrateAsync` khi dùng SQL Server. Để tạo migration mới hoặc xuất SQL:

```sh
dotnet tool restore
dotnet ef migrations add TenThayDoi --project Shared/ShoesShop.Core --startup-project Razor/RazorShoesShop
dotnet ef migrations script --idempotent --project Shared/ShoesShop.Core --startup-project Razor/RazorShoesShop --output docs/database.sql
```

Factory design-time dùng SQL Server và chỉ phục vụ tạo migration; kết nối runtime được chọn riêng cho từng web project. Không cần cài SQL Server khi chỉ build hoặc tạo SQL script.

## Kiểm thử

```sh
dotnet test tests/ShoesShop.Tests
```

8 test database SQLite kiểm tra tồn kho, tổng tiền, transaction đặt hàng, trạng thái đơn hàng, hủy đơn, tách giỏ hàng theo user và xóa mềm. Chúng không thay thế kiểm thử SQL Server.

Sau khi chạy cả ba ứng dụng với database Development dùng để kiểm thử:

```sh
python -m pip install -r tests/requirements.txt
python tests/smoke.py
# Có thể truyền một hoặc nhiều URL của ứng dụng thay cho bộ cổng mặc định.
```

Kiểm tra trình duyệt (tùy chọn): cài `playwright`, chạy `playwright install chromium`, sau đó `python tests/browser_smoke.py`. Có thể đặt `CHROMIUM_PATH` nếu Chromium đã có sẵn. Kiểm tra desktop/mobile và chụp ảnh vào `artifacts/screenshots`.

Smoke test tạo tài khoản, sản phẩm và đơn hàng thực trong database được chọn. Nên dùng database phát triển riêng. Test kiểm tra HTTP, cookie Identity, quyền admin, lọc/phân trang, mua hàng, hồ sơ, CRUD sản phẩm, khóa/đổi quyền/xóa mềm và CSRF.

## Nếu chưa chạy được trên máy Windows

- `dotnet --list-sdks`: cần có SDK 10.0.x; nếu không có, bổ sung trong Visual Studio Installer.
- `sqllocaldb info MSSQLLocalDB`, rồi `sqllocaldb start MSSQLLocalDB` nếu instance chưa chạy.
- Nếu lỗi kết nối, kiểm tra LocalDB đã được cài và không có biến `ConnectionStrings__DefaultConnection` hoặc `DatabaseProvider` cũ đang ghi đè cấu hình.
- Nếu cổng bận, dừng phiên chạy trước hoặc đổi `Properties/launchSettings.json` của project đó.
