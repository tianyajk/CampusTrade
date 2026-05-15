using System.ComponentModel.DataAnnotations;

namespace CampusTrade.Backend.Models;

public class CreateProductRequest
{
    [Required(ErrorMessage = "商品标题不能为空")]
    [StringLength(100, ErrorMessage = "商品标题不能超过 100 个字符")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "商品描述不能为空")]
    [StringLength(1000, ErrorMessage = "商品描述不能超过 1000 个字符")]
    public string Description { get; set; } = string.Empty;

    [Range(0.01, 999999.99, ErrorMessage = "商品价格必须大于 0")]
    public decimal Price { get; set; }

    [StringLength(255, ErrorMessage = "图片地址不能超过 255 个字符")]
    public string? ImageUrl { get; set; }

    [Required(ErrorMessage = "商品分类不能为空")]
    [StringLength(50, ErrorMessage = "商品分类不能超过 50 个字符")]
    public string Category { get; set; } = string.Empty;
}

public class UpdateProductRequest
{
    [Required(ErrorMessage = "商品标题不能为空")]
    [StringLength(100, ErrorMessage = "商品标题不能超过 100 个字符")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "商品描述不能为空")]
    [StringLength(1000, ErrorMessage = "商品描述不能超过 1000 个字符")]
    public string Description { get; set; } = string.Empty;

    [Range(0.01, 999999.99, ErrorMessage = "商品价格必须大于 0")]
    public decimal Price { get; set; }

    [StringLength(255, ErrorMessage = "图片地址不能超过 255 个字符")]
    public string? ImageUrl { get; set; }

    [Required(ErrorMessage = "商品分类不能为空")]
    [StringLength(50, ErrorMessage = "商品分类不能超过 50 个字符")]
    public string Category { get; set; } = string.Empty;
}

public class ProductDto
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public string? ImageUrl { get; set; }

    public string Category { get; set; } = string.Empty;

    public ProductStatus Status { get; set; }

    public int SellerId { get; set; }

    public string SellerName { get; set; } = string.Empty;

    public DateTime CreateTime { get; set; }
}

public class ProductListResponse
{
    public PagedResult<ProductDto> Products { get; set; } = new();
}

public class ProductDetailResponse
{
    public ProductDto Product { get; set; } = new();

    public UserDto Seller { get; set; } = new();
}

public class ImageUploadResponse
{
    public string ImageUrl { get; set; } = string.Empty;
}
