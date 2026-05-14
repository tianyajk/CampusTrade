using CampusTrade.Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace CampusTrade.Backend.Data;

public class CampusTradeDbContext : DbContext
{
    public CampusTradeDbContext(DbContextOptions<CampusTradeDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();

    public DbSet<Product> Products => Set<Product>();

    public DbSet<Favorite> Favorites => Set<Favorite>();

    public DbSet<Message> Messages => Set<Message>();

    public DbSet<Order> Orders => Set<Order>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(user => user.Username).IsUnique();
            entity.HasIndex(user => user.Email).IsUnique();
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.Property(product => product.Price).HasPrecision(10, 2);
            entity.HasIndex(product => product.Category);
            entity.HasIndex(product => product.Status);

            entity.HasOne(product => product.Seller)
                .WithMany(user => user.Products)
                .HasForeignKey(product => product.SellerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Favorite>(entity =>
        {
            entity.HasIndex(favorite => new { favorite.UserId, favorite.ProductId }).IsUnique();

            entity.HasOne(favorite => favorite.User)
                .WithMany(user => user.Favorites)
                .HasForeignKey(favorite => favorite.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(favorite => favorite.Product)
                .WithMany(product => product.Favorites)
                .HasForeignKey(favorite => favorite.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Message>(entity =>
        {
            entity.HasIndex(message => new { message.SenderId, message.ReceiverId });
            entity.HasIndex(message => message.SendTime);

            entity.HasOne(message => message.Sender)
                .WithMany(user => user.SentMessages)
                .HasForeignKey(message => message.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(message => message.Receiver)
                .WithMany(user => user.ReceivedMessages)
                .HasForeignKey(message => message.ReceiverId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.Property(order => order.Price).HasPrecision(10, 2);
            entity.HasIndex(order => order.Status);

            entity.HasOne(order => order.Buyer)
                .WithMany(user => user.Orders)
                .HasForeignKey(order => order.BuyerId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(order => order.Product)
                .WithMany(product => product.Orders)
                .HasForeignKey(order => order.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
