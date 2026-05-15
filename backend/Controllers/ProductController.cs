using System.Security.Claims;
using CampusTrade.Backend.Models;
using CampusTrade.Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CampusTrade.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductController : ControllerBase
{
    private readonly IProductService productService;

    public ProductController(IProductService productService)
    {
        this.productService = productService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<ProductDto>>>> GetList(
        [FromQuery] string? keyword,
        [FromQuery] string? category,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 12)
    {
        try
        {
            var result = await productService.GetListAsync(keyword, category, page, pageSize);
            return Ok(result);
        }
        catch
        {
            return StatusCode(500, ApiResponse<PagedResult<ProductDto>>.Fail("获取商品列表失败，请稍后重试"));
        }
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<ProductDetailResponse>>> GetDetail(int id)
    {
        try
        {
            var result = await productService.GetDetailAsync(id);
            return result.Success ? Ok(result) : NotFound(result);
        }
        catch
        {
            return StatusCode(500, ApiResponse<ProductDetailResponse>.Fail("获取商品详情失败，请稍后重试"));
        }
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<ApiResponse<ProductDto>>> Create(CreateProductRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId is null)
            {
                return Unauthorized(ApiResponse<ProductDto>.Fail("请先登录"));
            }

            var result = await productService.CreateAsync(userId.Value, request);
            return result.Success ? Ok(result) : BadRequest(result);
        }
        catch
        {
            return StatusCode(500, ApiResponse<ProductDto>.Fail("发布商品失败，请稍后重试"));
        }
    }

    [Authorize]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse<ProductDto>>> Update(int id, UpdateProductRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId is null)
            {
                return Unauthorized(ApiResponse<ProductDto>.Fail("请先登录"));
            }

            var result = await productService.UpdateAsync(userId.Value, id, request);
            if (result.Success)
            {
                return Ok(result);
            }

            return ToErrorResult(result);
        }
        catch
        {
            return StatusCode(500, ApiResponse<ProductDto>.Fail("修改商品失败，请稍后重试"));
        }
    }

    [Authorize]
    [HttpPost("upload-image")]
    public async Task<ActionResult<ApiResponse<ImageUploadResponse>>> UploadImage([FromForm] IFormFile file)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId is null)
            {
                return Unauthorized(ApiResponse<ImageUploadResponse>.Fail("请先登录"));
            }

            var result = await productService.UploadImageAsync(userId.Value, file);
            return result.Success ? Ok(result) : BadRequest(result);
        }
        catch
        {
            return StatusCode(500, ApiResponse<ImageUploadResponse>.Fail("图片上传失败，请稍后重试"));
        }
    }

    [Authorize]
    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(int id)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId is null)
            {
                return Unauthorized(ApiResponse<object>.Fail("请先登录"));
            }

            var result = await productService.DeleteAsync(userId.Value, id);
            if (result.Success)
            {
                return Ok(result);
            }

            return ToErrorResult(result);
        }
        catch
        {
            return StatusCode(500, ApiResponse<object>.Fail("删除商品失败，请稍后重试"));
        }
    }

    private ActionResult<ApiResponse<T>> ToErrorResult<T>(ApiResponse<T> result)
    {
        return result.Message switch
        {
            "商品不存在" => NotFound(result),
            "无权操作该商品" => StatusCode(403, result),
            _ => BadRequest(result)
        };
    }

    private int? GetCurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(value, out var userId) ? userId : null;
    }
}
