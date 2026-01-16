Dưới đây là file README.md đã được điều chỉnh toàn bộ nội dung, cấu trúc và tính năng để khớp với dự án GreenSpace (dựa trên tài liệu SRS và DB Schema mình đã phân tích trước đó).

GreenSpace - E-commerce Platform for Plants & Decor
Hệ thống Thương mại Điện tử Cây cảnh và Giải pháp Không gian Xanh

GreenSpace là nền tảng E-commerce chuyên cung cấp cây cảnh, chậu cây và các giải pháp trang trí nội thất xanh. Hệ thống không chỉ bán hàng mà còn cung cấp kiến thức chăm sóc cây (Blog/Guides) và xây dựng cộng đồng yêu cây xanh.

📋 Mục lục
Tổng quan

Kiến trúc hệ thống

Công nghệ sử dụng

Tính năng chính

Cài đặt và triển khai

Cấu trúc dự án

Database Schema

API Documentation

🎯 Tổng quan
GreenSpace Backend được xây dựng theo kiến trúc Clean Architecture, tập trung vào hiệu năng, khả năng mở rộng và dễ dàng bảo trì. Hệ thống phục vụ các đối tượng:

Customer (Khách hàng): Tìm kiếm cây, mua hàng, theo dõi đơn, xem hướng dẫn chăm sóc.

Admin (Quản trị viên): Quản lý sản phẩm, kho vận, đơn hàng, khuyến mãi và báo cáo doanh thu.

🏗️ Kiến trúc hệ thống
Dự án tuân thủ mô hình Clean Architecture kết hợp với Repository Pattern:

┌─────────────────────────────────────────┐
│          Presentation Layer             │
│        (GreenSpace.WebAPI)              │
│   • Controllers & Middleware            │
│   • Authentication (JWT)                │
└──────────────┬──────────────────────────┘
               │
┌──────────────▼──────────────────────────┐
│          Application Layer              │
│      (GreenSpace.Infrastructure)        │
│   • Services (Business Logic)           │
│   • Repositories Implementation         │
│   • EF Core (DB Context)                │
│   • External Services (VNPAY, Email)    │
└──────────────┬──────────────────────────┘
               │
┌──────────────▼──────────────────────────┐
│             Domain Layer                │
│          (GreenSpace.Core)              │
│   • Entities (Product, Order, User...)  │
│   • Interfaces (IRepository, IService)  │
│   • DTOs & Enums                        │
└─────────────────────────────────────────┘
🛠️ Công nghệ sử dụng
Backend Framework
ASP.NET Core 8.0 - Web API

Entity Framework Core 8.0 - ORM

PostgreSQL 15+ - Database chính

AutoMapper - Mapping Object-to-Object

Authentication & Security
ASP.NET Core Identity - Quản lý User/Role

JWT Bearer - Xác thực Token

BCrypt - Mã hóa mật khẩu

Integrations (Tích hợp)
VNPAY / MoMo - Cổng thanh toán (Planned)

Cloudinary / Firebase Storage - Lưu trữ ảnh sản phẩm

Docker - Containerization

✨ Tính năng chính
1. Phân hệ Khách hàng (Storefront)
a. Sản phẩm & Danh mục
✅ Tìm kiếm & Lọc sản phẩm (Theo giá, loại cây, kích thước)

✅ Xem chi tiết: Hình ảnh, Mô tả, Hướng dẫn chăm sóc

✅ Lựa chọn biến thể: Size (S, M, L), Màu chậu, Mix cây

✅ Xem sản phẩm liên quan/Gợi ý

b. Đặt hàng & Thanh toán
✅ Quản lý Giỏ hàng (Thêm/Sửa/Xóa)

✅ Checkout: Nhập địa chỉ, Áp dụng mã giảm giá (Promotions)

✅ Thanh toán: COD, Chuyển khoản (VNPAY/QR)

✅ Theo dõi trạng thái đơn hàng (Pending -> Shipping -> Completed)

c. Nội dung & Tương tác
✅ Đọc Blog/Tin tức về cây cảnh

✅ Đánh giá & Bình luận sản phẩm (Reviews)

✅ Quản lý sổ địa chỉ giao hàng

2. Phân hệ Quản trị (Admin Panel)
a. Quản lý Sản phẩm (Catalog)
✅ CRUD Danh mục (Categories) đa cấp

✅ CRUD Sản phẩm & Biến thể (Variants)

✅ Quản lý kho hàng (Stock management)

✅ Upload thư viện ảnh sản phẩm

b. Quản lý Đơn hàng (Sales)
✅ Xem danh sách đơn hàng

✅ Cập nhật trạng thái đơn (Xác nhận, Giao hàng, Hủy)

✅ Xử lý hoàn tiền/khiếu nại

c. Marketing & Báo cáo
✅ Tạo mã giảm giá (Voucher/Coupon)

✅ Dashboard thống kê: Doanh thu, Sản phẩm bán chạy

✅ Quản lý bài viết Blog

🚀 Cài đặt và triển khai
Yêu cầu
.NET SDK 8.0+

PostgreSQL (Local hoặc Docker)

1. Clone & Config
Bash

git clone https://github.com/GreenSpace-Team/GreenSpace_BE.git
cd GreenSpace_BE
Tạo file src/GreenSpace.WebAPI/appsettings.Development.json:

JSON

{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=greenspace_db;Username=postgres;Password=your_password"
  },
  "JwtSettings": {
    "SecretKey": "SuperSecretKey@123456789_MustBeLongEnough",
    "Issuer": "GreenSpace_API",
    "Audience": "GreenSpace_Client"
  }
}
2. Database Migration
Chạy lệnh sau tại thư mục root của solution để khởi tạo Database:

Bash

cd src
# Restore packages
dotnet restore

# Chạy Migration
cd GreenSpace.Infrastructure
dotnet ef migrations add InitialCreate --startup-project ../GreenSpace.WebAPI
dotnet ef database update --startup-project ../GreenSpace.WebAPI
3. Run
Bash

cd ../GreenSpace.WebAPI
dotnet run
# API sẽ chạy tại: http://localhost:5000 hoặc https://localhost:5001
📁 Cấu trúc dự án
GreenSpace_BE/
├── src/
│   ├── GreenSpace.Core/            # Lõi hệ thống (Không phụ thuộc bên ngoài)
│   │   ├── Domain/
│   │   │   ├── Entities/           # Product, Variant, Order, Blog...
│   │   │   ├── Enums/              # OrderStatus, PaymentMethod...
│   │   ├── Interfaces/             # IProductRepository, IOrderService...
│   │   ├── DTOs/                   # ProductDto, CreateOrderDto...
│   │
│   ├── GreenSpace.Infrastructure/  # Triển khai logic & DB
│   │   ├── Persistence/            # AppDbContext, Repositories
│   │   ├── Services/               # ProductService, OrderService, AuthService
│   │   ├── External/               # VnpayService, CloudinaryService
│   │
│   └── GreenSpace.WebAPI/          # API Endpoint
│       ├── Controllers/            # AuthController, ProductController...
│       ├── Program.cs              # DI Config & Middleware
│
└── docker-compose.yml              # Deployment config
📚 API Documentation
Sau khi chạy dự án, truy cập Swagger UI để xem tài liệu API:

Local: https://localhost:7000/swagger

Production: https://your-domain.onrender.com/swagger

Các Module chính:

Auth: Đăng ký, Đăng nhập, Refresh Token.

Products: CRUD sản phẩm, biến thể.

Orders: Tạo đơn, xem lịch sử, cập nhật trạng thái.

Promotions: Kiểm tra và áp dụng mã giảm giá.

Blogs: Quản lý bài viết.

📞 Liên hệ
GreenSpace Team - EXE201 Project

GitHub: 

Made with 🌿 for a greener life.
