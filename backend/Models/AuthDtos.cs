using System.ComponentModel.DataAnnotations;

namespace CampusTrade.Backend.Models;

public class RegisterRequest
{
    [Required]
    [StringLength(50)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [MinLength(6)]
    [StringLength(100)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(100)]
    public string Email { get; set; } = string.Empty;

    [Phone]
    [StringLength(30)]
    public string? Phone { get; set; }
}

public class LoginRequest
{
    [Required]
    [StringLength(100)]
    public string Account { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Password { get; set; } = string.Empty;
}
