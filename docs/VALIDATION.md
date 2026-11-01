# Kiểm tra sau khi đơn giản hóa dự án

Ngày kiểm tra: 01/11/2026.

- Solution chỉ còn ba web project Razor Pages, Blazor và MVC; mỗi bài tự chứa Models, Data, Services và migrations, không tham chiếu project khác.
- Build solution thành công: 0 lỗi, 0 cảnh báo.
- Đã chạy riêng từng ứng dụng với database SQLite riêng: danh mục/lọc/phân trang, đăng ký/đăng nhập, thêm giỏ hàng, đặt hàng, xem đơn hàng, sửa hồ sơ và đăng nhập/truy cập các trang quản trị đều hoạt động.
- Đã kiểm tra migrations của từng project: model khớp migration; SQL tạo ra giống cấu trúc trước khi tách. Giữ nguyên ID migration ban đầu để dùng tiếp các database đã tạo.

Project kiểm thử, các script kiểm thử và phụ thuộc test runner đã được gỡ theo scope. Các bước chạy thử cơ bản nằm trong README. Script `scripts/run-dev.sh` chỉ dùng khởi động ứng dụng tùy chọn trên Linux.

Môi trường hiện tại là Linux nên chưa trực tiếp chạy Visual Studio/MSSQLLocalDB trên Windows. LocalDB vẫn là cấu hình mặc định khi mở từng bài trong Visual Studio; không thay đổi tên database hay dữ liệu cũ.
