namespace CampusTrade.Backend.Models;

public class Favorite
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int ProductId { get; set; }

    public DateTime CreateTime { get; set; } = DateTime.Now;

    public User? User { get; set; }

    public Product? Product { get; set; }
}
