using CampusTrade.Backend.Data;
using CampusTrade.Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace CampusTrade.Backend.Services;

public class ProductService : IProductService
{
    private const string DefaultProductImage = "/assets/default-product.svg";

    private static readonly HashSet<string> AllowedImageExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg",
        ".jpeg",
        ".png",
        ".gif",
        ".webp"
    };

    private const long MaxImageSize = 5 * 1024 * 1024;
    private readonly CampusTradeDbContext dbContext;
    private readonly IWebHostEnvironment environment;

    public ProductService(CampusTradeDbContext dbContext, IWebHostEnvironment environment)
    {
        this.dbContext = dbContext;
        this.environment = environment;
    }

    public async Task<ApiResponse<PagedResult<ProductDto>>> GetListAsync(string? keyword, string? category, int page, int pageSize)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 50);

        var query = dbContext.Products
            .AsNoTracking()
            .Include(product => product.Seller)
            .Where(product => product.Status == ProductStatus.OnSale);

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var trimmedKeyword = keyword.Trim();
            query = query.Where(product => EF.Functions.Like(product.Title, $"%{trimmedKeyword}%"));
        }

        if (!string.IsNullOrWhiteSpace(category))
        {
            var trimmedCategory = category.Trim();
            query = query.Where(product => product.Category == trimmedCategory);
        }

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(product => product.CreateTime)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(product => ToProductDto(product))
            .ToListAsync();

        return ApiResponse<PagedResult<ProductDto>>.Ok(new PagedResult<ProductDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            Total = total
        }, "获取商品列表成功");
    }

    public async Task<ApiResponse<ProductDetailResponse>> GetDetailAsync(int productId)
    {
        var product = await dbContext.Products
            .AsNoTracking()
            .Include(item => item.Seller)
            .FirstOrDefaultAsync(item => item.Id == productId);

        if (product is null || product.Seller is null)
        {
            return ApiResponse<ProductDetailResponse>.Fail("商品不存在");
        }

        return ApiResponse<ProductDetailResponse>.Ok(new ProductDetailResponse
        {
            Product = ToProductDto(product),
            Seller = ToUserDto(product.Seller)
        }, "获取商品详情成功");
    }

    public async Task<ApiResponse<ProductDto>> CreateAsync(int sellerId, CreateProductRequest request)
    {
        var seller = await dbContext.Users.FindAsync(sellerId);
        if (seller is null)
        {
            return ApiResponse<ProductDto>.Fail("用户不存在");
        }

        var product = new Product
        {
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            Price = request.Price,
            Category = request.Category.Trim(),
            ImageUrl = GetImageUrl(request.ImageUrl),
            Status = ProductStatus.OnSale,
            SellerId = sellerId,
            CreateTime = DateTime.Now,
            Seller = seller
        };

        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync();

        return ApiResponse<ProductDto>.Ok(ToProductDto(product), "商品发布成功");
    }

    public async Task<ApiResponse<ProductDto>> UpdateAsync(int userId, int productId, UpdateProductRequest request)
    {
        var product = await dbContext.Products
            .Include(item => item.Seller)
            .FirstOrDefaultAsync(item => item.Id == productId);

        if (product is null)
        {
            return ApiResponse<ProductDto>.Fail("商品不存在");
        }

        if (product.SellerId != userId)
        {
            return ApiResponse<ProductDto>.Fail("无权操作该商品");
        }

        product.Title = request.Title.Trim();
        product.Description = request.Description.Trim();
        product.Price = request.Price;
        product.Category = request.Category.Trim();
        product.ImageUrl = GetImageUrl(request.ImageUrl);

        await dbContext.SaveChangesAsync();

        return ApiResponse<ProductDto>.Ok(ToProductDto(product), "商品修改成功");
    }

    public async Task<ApiResponse<object>> DeleteAsync(int userId, int productId)
    {
        var product = await dbContext.Products.FindAsync(productId);
        if (product is null)
        {
            return ApiResponse<object>.Fail("商品不存在");
        }

        if (product.SellerId != userId)
        {
            return ApiResponse<object>.Fail("无权操作该商品");
        }

        if (product.Status == ProductStatus.Sold)
        {
            return ApiResponse<object>.Fail("已售出商品不可删除");
        }

        dbContext.Products.Remove(product);
        await dbContext.SaveChangesAsync();

        return ApiResponse<object>.Ok(null, "商品删除成功");
    }

    public async Task<ApiResponse<ImageUploadResponse>> UploadImageAsync(int userId, IFormFile file)
    {
        if (file.Length == 0)
        {
            return ApiResponse<ImageUploadResponse>.Fail("请选择图片文件");
        }

        if (file.Length > MaxImageSize)
        {
            return ApiResponse<ImageUploadResponse>.Fail("图片文件不能超过 5MB");
        }

        var extension = Path.GetExtension(file.FileName);
        if (!AllowedImageExtensions.Contains(extension))
        {
            return ApiResponse<ImageUploadResponse>.Fail("图片仅支持 jpg、jpeg、png、gif、webp 格式");
        }

        var uploadDirectory = Path.Combine(environment.ContentRootPath, "frontend", "assets", "uploads", "products");
        Directory.CreateDirectory(uploadDirectory);

        var fileName = $"{userId}_{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
        var filePath = Path.Combine(uploadDirectory, fileName);

        await using (var stream = File.Create(filePath))
        {
            await file.CopyToAsync(stream);
        }

        var imageUrl = $"/assets/uploads/products/{fileName}";
        return ApiResponse<ImageUploadResponse>.Ok(new ImageUploadResponse { ImageUrl = imageUrl }, "图片上传成功");
    }

    private static string GetImageUrl(string? imageUrl)
    {
        return string.IsNullOrWhiteSpace(imageUrl) ? DefaultProductImage : imageUrl.Trim();
    }

    private static ProductDto ToProductDto(Product product)
    {
        return new ProductDto
        {
            Id = product.Id,
            Title = product.Title,
            Description = product.Description,
            Price = product.Price,
            ImageUrl = product.ImageUrl,
            Category = product.Category,
            Status = product.Status,
            SellerId = product.SellerId,
            SellerName = product.Seller?.Username ?? string.Empty,
            CreateTime = product.CreateTime
        };
    }

    private static UserDto ToUserDto(User user)
    {
        return new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            Avatar = user.Avatar,
            Phone = user.Phone,
            CreateTime = user.CreateTime
        };
    }
}
