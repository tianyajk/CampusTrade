namespace CampusTrade.Backend.Models;

public enum OrderStatus
{
    PendingPayment = 0,
    Trading = 1,
    Completed = 2
}

public class Order
{
    public int Id { get; set; }

    public int BuyerId { get; set; }

    public int ProductId { get; set; }

    public decimal Price { get; set; }

    public OrderStatus Status { get; set; } = OrderStatus.PendingPayment;

    public DateTime CreateTime { get; set; } = DateTime.Now;

    public User? Buyer { get; set; }

    public Product? Product { get; set; }
}
