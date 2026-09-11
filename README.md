# TodoApp - Backend (RESTful Web API)

Hệ thống máy chủ dịch vụ (Backend API) cho ứng dụng **Quản lý công việc cá nhân & Cộng tác nhóm (TodoApp)**. Dự án được xây dựng bằng **C#** trên nền tảng **ASP.NET Core 9.0 Web API (.NET 9)**, sử dụng **Entity Framework Core** với **Microsoft SQL Server**, giao tiếp thời gian thực **SignalR**, gửi email khôi phục mật khẩu qua **Gmail SMTP (MailKit)**, mã hóa mật khẩu **BCrypt** và xác thực bảo mật **JWT Bearer Token**.

---

## 🚀 Các tính năng nghiệp vụ Backend

1. **`AuthController` (`/api/auth`):**
   - Đăng ký tài khoản (`POST /register`), Đăng nhập (`POST /login`).
   - Quên mật khẩu (`POST /forgot-password`): Tự động tạo mã OTP khôi phục 6 số và gửi về Gmail của người dùng qua dịch vụ SMTP.
   - Đặt lại mật khẩu (`POST /reset-password`): Kiểm tra mã OTP hợp lệ và băm mật khẩu mới qua BCrypt.
2. **`TodosController` (`/api/todos`):**
   - Thao tác CRUD công việc cá nhân, ghim công việc lên đầu (`/toggle-pinned`), đổi độ ưu tiên.
   - Đánh dấu hoàn thành (`/toggle`): Tự động sinh công việc tiếp theo nếu công việc có thiết lập lặp lại (Recurrence: Daily, Weekly, Monthly, Yearly).
   - Thùng rác: Xóa tạm vào thùng rác (`/trash`), khôi phục (`/restore`), xóa vĩnh viễn (`/permanent-delete`).
   - Chia sẻ công việc (`/share`) & phát tín hiệu thông báo thời gian thực qua SignalR Hub.
3. **`TodoStepsController` (`/api/todosteps`):**
   - Quản lý các bước con (Sub-tasks / Steps) trong từng công việc.
4. **`CategoriesController` (`/api/categories`):**
   - Quản lý nhóm danh mục / nhãn dán riêng của từng người dùng.
5. **`FriendsController` (`/api/friends`):**
   - Tìm kiếm người dùng, gửi lời mời kết bạn, chấp nhận/từ chối lời mời.
6. **`UsersController` (`/api/users`):**
   - Xem và cập nhật hồ sơ cá nhân (`/profile`), đổi mật khẩu.
   - API quản trị người dùng dành riêng cho quyền Admin (`/admin/users`).
7. **`TodoHub` (`/hubs/todo`):**
   - SignalR Hub truyền nhận thông điệp cập nhật công việc thời gian thực giữa các người dùng cùng nhóm chia sẻ.

---

## 🛠️ Công nghệ & Thư viện sử dụng

| Thư viện / Package | Phiên bản | Vai trò / Mục đích |
| :--- | :--- | :--- |
| **.NET SDK** | `net9.0` | Nền tảng phát triển ứng dụng Web API hiệu năng cao |
| **Microsoft.EntityFrameworkCore.SqlServer** | `9.0.19` | ORM kết nối và thao tác CSDL Microsoft SQL Server |
| **Microsoft.EntityFrameworkCore.Tools** | `9.0.19` | Công cụ Migration CSDL (`dotnet ef`) |
| **Microsoft.AspNetCore.Authentication.JwtBearer** | `9.0.19` | Middleware xác thực chuỗi JWT Token |
| **BCrypt.Net-Next** | `4.2.0` | Thuật toán băm mật khẩu bảo mật một chiều |
| **MailKit** | `4.17.0` | Thư viện gửi email thông báo mã OTP qua máy chủ SMTP Gmail |
| **Microsoft.AspNetCore.SignalR** | Built-in | Công nghệ WebSocket giao tiếp thời gian thực |
| **Microsoft.AspNetCore.OpenApi** | `9.0.19` | Hỗ trợ tài liệu hóa API chuẩn OpenAPI / Scalar |

---

## 📋 Yêu cầu hệ thống

- **.NET SDK**: Phiên bản `.NET 9.0 SDK` trở lên (`dotnet --version`).
- **Hệ quản trị CSDL**: Microsoft SQL Server (SQL Server Express, Developer Edition hoặc LocalDB).

---

## ⚙️ Hướng dẫn Cài đặt & Cấu hình

### Bước 1: Mở Terminal tại thư mục Backend

```bash
cd Todo.Api/Todo.Api
```

### Bước 2: Cấu hình `appsettings.json`

Mở file `appsettings.json` và cấu hình các thông số kết nối Database, Khóa bảo mật JWT và Dịch vụ gửi Email:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "JwtSettings": {
    "SecretKey": "DayLaChuoiBaoMatSecretKeyRatDaiToiThieu64KyTuDeMaHoaJWT1234567890",
    "Issuer": "TodoAppBackend",
    "Audience": "TodoAppFrontend"
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=TodoDb;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "EmailSettings": {
    "SmtpServer": "smtp.gmail.com",
    "Port": 587,
    "SenderName": "TodoApp Support",
    "SenderEmail": "email_cua_ban@gmail.com",
    "Password": "mat_khau_ung_dung_gmail_16_ky_tu"
  }
}
```

> **📌 Lưu ý quan trọng khi cấu hình:**
> 1. **Database:** Thay `localhost\\SQLEXPRESS` bằng tên instance SQL Server trên máy của bạn (ví dụ: `.\\SQLEXPRESS` hoặc `(localdb)\\mssqllocaldb`).
> 2. **Tự động tạo CSDL:** Trong file `Program.cs`, dự án đã cấu hình `db.Database.Migrate();` nên khi khởi động Server, hệ thống sẽ **tự động khởi tạo database `TodoDb` và tạo toàn bộ bảng** mà không cần chạy lệnh thủ công!
> 3. **Cấu hình gửi Email:** `Password` trong `EmailSettings` là **Mật khẩu ứng dụng (App Password)** gồm 16 ký tự được tạo từ Google Account (Security > 2-Step Verification > App Passwords), không phải mật khẩu đăng nhập Gmail thông thường.

---

## ▶️ Khởi chạy Backend Server

Chạy lệnh sau tại thư mục chứa file `.csproj`:

```bash
dotnet run
```

Sau khi khởi chạy thành công:
- **Server URL:** `http://localhost:5001`
- **SignalR Hub Endpoint:** `http://localhost:5001/hubs/todo`
- **OpenAPI Endpoint:** `http://localhost:5001/openapi/v1.json`

---

## 📁 Cấu trúc thư mục Backend

```
Todo.Api/
└── Todo.Api/
    ├── Controllers/          # 6 RESTful API Controllers
    │   ├── AuthController.cs
    │   ├── TodosController.cs
    │   ├── TodoStepsController.cs
    │   ├── CategoriesController.cs
    │   ├── FriendsController.cs
    │   └── UsersController.cs
    ├── Data/
    │   └── TodoDbContext.cs  # DbContext cấu hình các mối quan hệ bảng
    ├── Hubs/
    │   └── TodoHub.cs        # SignalR Hub truyền nhận tin nhắn Real-time
    ├── Middlewares/
    │   └── GlobalExceptionHandler.cs # Bắt lỗi ngoại lệ toàn cục
    ├── Models/               # Các lớp thực thể CSDL
    │   ├── User.cs
    │   ├── TodoItem.cs
    │   ├── TodoStep.cs
    │   ├── Category.cs
    │   ├── Friendship.cs
    │   ├── TodoShare.cs
    │   └── DTOs/             # Data Transfer Objects truyền nhận dữ liệu
    ├── Services/             # Xử lý Token Service và Email Service (MailKit)
    ├── appsettings.json      # File cấu hình SQL Server, JWT và Gmail SMTP
    ├── Todo.Api.csproj       # File định nghĩa gói thư viện .NET 9
    └── Program.cs            # Cấu hình DI, CORS, SignalR, Auth, Database Migration
```
