using System.ComponentModel.DataAnnotations;

namespace CampusTrade.Backend.Models;

public class UserDto
{
    public int Id { get; set; }

    public string Username { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? Avatar { get; set; }

    public string? Phone { get; set; }

    public DateTime CreateTime { get; set; }
}

public class UpdateUserProfileRequest
{
    [Required]
    [StringLength(50)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(100)]
    public string Email { get; set; } = string.Empty;

    [Phone]
    [StringLength(30)]
    public string? Phone { get; set; }
}

public class AvatarResponse
{
    public string Avatar { get; set; } = string.Empty;
}

public class UserProductSummaryDto
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public string? ImageUrl { get; set; }

    public string Category { get; set; } = string.Empty;

    public ProductStatus Status { get; set; }

    public DateTime CreateTime { get; set; }
}

public class UserHomeResponse
{
    public UserDto User { get; set; } = new();

    public List<UserProductSummaryDto> Products { get; set; } = new();
}
