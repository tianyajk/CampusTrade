using System.ComponentModel.DataAnnotations;

namespace CampusTrade.Backend.Models;

public class Message
{
    public int Id { get; set; }

    public int SenderId { get; set; }

    public int ReceiverId { get; set; }

    [Required]
    [StringLength(1000)]
    public string Content { get; set; } = string.Empty;

    public DateTime SendTime { get; set; } = DateTime.Now;

    public User? Sender { get; set; }

    public User? Receiver { get; set; }
}
