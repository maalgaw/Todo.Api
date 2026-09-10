using Microsoft.EntityFrameworkCore;
using Todo.Api.Models;

namespace Todo.Api.Data;

public class TodoDbContext : DbContext
{
    public TodoDbContext(DbContextOptions<TodoDbContext> options) : base(options)
    {
    }

    public DbSet<TodoItem> TodoItems => Set<TodoItem>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<User> Users => Set<User>(); // Khai báo bảng Users mới
    public DbSet<TodoStep> TodoSteps => Set<TodoStep>();
    public DbSet<Friendship> Friendships => Set<Friendship>();
    public DbSet<TodoShare> TodoShares => Set<TodoShare>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TodoItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        });

        // Cấu hình ràng buộc cho bảng User
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Username).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
            entity.Property(e => e.PasswordHash).IsRequired();
            entity.Property(e => e.Role).HasDefaultValue("User");
        });

        // Config Friendship relationships (multiple foreign keys to User)
        modelBuilder.Entity<Friendship>(entity =>
        {
            entity.HasOne(f => f.User)
                  .WithMany()
                  .HasForeignKey(f => f.UserId)
                  .OnDelete(DeleteBehavior.Restrict); // Prevent multiple cascade paths

            entity.HasOne(f => f.Friend)
                  .WithMany()
                  .HasForeignKey(f => f.FriendId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<TodoShare>(entity =>
        {
            entity.HasOne(s => s.TodoItem)
                  .WithMany(t => t.Shares)
                  .HasForeignKey(s => s.TodoItemId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(s => s.User)
                  .WithMany()
                  .HasForeignKey(s => s.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
