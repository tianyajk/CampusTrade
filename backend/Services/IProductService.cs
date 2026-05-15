using CampusTrade.Backend.Models;

namespace CampusTrade.Backend.Services;

public interface IProductService
{
    Task<ApiResponse<PagedResult<ProductDto>>> GetListAsync(string? keyword, string? category, int page, int pageSize);

    Task<ApiResponse<ProductDetailResponse>> GetDetailAsync(int productId);

    Task<ApiResponse<ProductDto>> CreateAsync(int sellerId, CreateProductRequest request);

    Task<ApiResponse<ProductDto>> UpdateAsync(int userId, int productId, UpdateProductRequest request);

    Task<ApiResponse<object>> DeleteAsync(int userId, int productId);

    Task<ApiResponse<ImageUploadResponse>> UploadImageAsync(int userId, IFormFile file);
}
