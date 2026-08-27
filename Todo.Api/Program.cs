using Microsoft.EntityFrameworkCore;
using Todo.Api.Data;

using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Đăng ký Global Exception Handler
builder.Services.AddExceptionHandler<Todo.Api.Middlewares.GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

//Đăng ký sử dụng DbContext với SQL Server
builder.Services.AddDbContext<TodoDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

//Tạo giấy phép CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

//Tự động migrate
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<TodoDbContext>();
    db.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

//Cho phép CORS
app.UseCors("AllowFrontend");

// Kích hoạt Middleware bắt lỗi toàn cục
app.UseExceptionHandler();

app.UseAuthorization();

app.MapControllers();

app.Run();
