using System.ComponentModel.DataAnnotations;

namespace CampusTrade.Backend.Models;

public enum ProductStatus
{
    OnSale = 0,
    Sold = 1
}

public class Product
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;

    [Range(0.01, 999999.99)]
    public decimal Price { get; set; }

    [StringLength(255)]
    public string? ImageUrl { get; set; }

    [Required]
    [StringLength(50)]
    public string Category { get; set; } = string.Empty;

    public ProductStatus Status { get; set; } = ProductStatus.OnSale;

    public int SellerId { get; set; }

    public DateTime CreateTime { get; set; } = DateTime.Now;

    public User? Seller { get; set; }

    public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();

    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
