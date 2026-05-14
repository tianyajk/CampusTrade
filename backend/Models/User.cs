using System.ComponentModel.DataAnnotations;

namespace CampusTrade.Backend.Models;

public class User
{
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    public string PasswordHash { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(100)]
    public string Email { get; set; } = string.Empty;

    [StringLength(255)]
    public string? Avatar { get; set; }

    [Phone]
    [StringLength(30)]
    public string? Phone { get; set; }

    public DateTime CreateTime { get; set; } = DateTime.Now;

    public ICollection<Product> Products { get; set; } = new List<Product>();

    public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();

    public ICollection<Message> SentMessages { get; set; } = new List<Message>();

    public ICollection<Message> ReceivedMessages { get; set; } = new List<Message>();

    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
