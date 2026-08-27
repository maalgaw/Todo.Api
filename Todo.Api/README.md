# TodoApp - Backend (API)

Đây là mã nguồn Backend cho ứng dụng TodoApp, được xây dựng bằng **ASP.NET Core Web API**.

## Công nghệ sử dụng

- **Framework:** .NET (C#)
- **Database:** SQLite
- **ORM:** Entity Framework Core
- **Xác thực:** JWT (JSON Web Tokens)
- **Bảo mật mật khẩu:** BCrypt

## Yêu cầu hệ thống

- Đã cài đặt [.NET SDK](https://dotnet.microsoft.com/download) (Khuyên dùng .NET 9).

## Hướng dẫn Cài đặt & Chạy ứng dụng

### Bước 1: Cấu hình Khóa bảo mật (JWT Secret)

Mở file `appsettings.json` và đảm bảo bạn có chuỗi bảo mật JWT (`SecretKey`) đủ độ dài (ít nhất 64 ký tự). Ví dụ:

```json
"JwtSettings": {
  "SecretKey": "DayLaMotChiecChiaKhoaBiMatCuaTodoAppCanPhaiRatDaiVaKhoDoan12345678",
  "Issuer": "TodoApp",
  "Audience": "TodoAppUser",
  "ExpiryMinutes": 60
}
```

### Bước 2: Khởi tạo Database (SQLite)

Mở Terminal tại thư mục chứa file `.csproj` (`Todo.Api`) và chạy lệnh sau để áp dụng các bảng dữ liệu:

```bash
dotnet ef database update
```

_(Nếu bạn chưa có công cụ EF Core, hãy cài đặt bằng lệnh: `dotnet tool install --global dotnet-ef`)_

### Bước 3: Chạy Server

Dùng lệnh sau để khởi động Backend:

```bash
dotnet run
```

Backend sẽ mặc định chạy trên cổng **`http://localhost:5001`**. Nó đã được cấu hình sẵn CORS để cho phép Frontend gọi API tới.
