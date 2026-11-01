# Kết quả kiểm thử

Ngày kiểm tra: 01/11/2026. Môi trường Linux, .NET SDK 10.0.403; EF Core / Identity 10.0.14.

| Phạm vi | Kết quả |
| --- | --- |
| Build `ShoesShop.slnx` | Thành công, 0 lỗi, 0 cảnh báo |
| `dotnet test tests/ShoesShop.Tests` | 8 passed, 0 failed, 0 skipped |
| HTTP integration `tests/smoke.py` | 84 kiểm tra/bài, tổng 252 passed |
| Chromium `tests/browser_smoke.py` | 66 passed; desktop 1440px và mobile 390px |
| Khởi động lại sau khi xóa mềm tài khoản demo | Thành công, không tạo lại tài khoản đã xóa |
| SQL Server migration | Đã tạo InitialShop và SQL script idempotent; không có thay đổi model chưa được ghi migration |

Các kiểm thử runtime ở trên dùng **ba database SQLite thật, riêng biệt, lưu trên đĩa** qua tùy chọn `DatabaseProvider=Sqlite`. Không dùng EF InMemory hoặc mock database.

HTTP kiểm tra đăng ký/đăng nhập, chuyển hướng theo vai trò, trang admin bị chặn với User, lọc/phân trang, thêm giỏ hàng, đặt hàng, tách dữ liệu giữa tài khoản, sửa hồ sơ, CRUD sản phẩm, hủy đơn hàng, đổi role, khóa tài khoản, thu hồi phiên đăng nhập, xóa mềm, giữ email đã sử dụng và chống CSRF cả đăng xuất.

Browser kiểm tra các trang khách hàng, tìm kiếm thực qua form, đăng nhập admin, hiển thị vai trò và bố cục trên màn hình nhỏ. Ảnh chụp được lưu trong `artifacts/screenshots` (không commit).

## Giới hạn kiểm chứng

- Chưa chạy Visual Studio hoặc MSSQLLocalDB trên Windows trong môi trường Linux này.
- Đã thử tải SQL Server Linux qua container MCR và gói Microsoft; proxy từ chối các đích tải xuống được chuyển hướng. Vì vậy chưa xác nhận thực thi migration và transaction trên SQL Server tại đây.
- SQL Server LocalDB vẫn là **cấu hình mặc định** khi mở project bằng Visual Studio. Mỗi bài tự tạo database riêng và áp dụng migration. Cần chạy lần đầu trên máy Windows theo README để xác nhận môi trường LocalDB của máy đó.
- Các kết quả trên được ghi nhận trong lần kiểm thử trước khi đưa mã nguồn lên GitHub.
