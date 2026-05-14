using CampusTrade.Backend.Data;
using CampusTrade.Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace CampusTrade.Backend.Services;

public class UserService : IUserService
{
    private static readonly HashSet<string> AllowedAvatarExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg",
        ".jpeg",
        ".png",
        ".gif",
        ".webp"
    };

    private const long MaxAvatarSize = 5 * 1024 * 1024;
    private readonly CampusTradeDbContext dbContext;
    private readonly IWebHostEnvironment environment;

    public UserService(CampusTradeDbContext dbContext, IWebHostEnvironment environment)
    {
        this.dbContext = dbContext;
        this.environment = environment;
    }

    public async Task<ApiResponse<UserDto>> GetProfileAsync(int userId)
    {
        var user = await dbContext.Users.FindAsync(userId);
        if (user is null)
        {
            return ApiResponse<UserDto>.Fail("用户不存在");
        }

        return ApiResponse<UserDto>.Ok(ToUserDto(user), "获取用户信息成功");
    }

    public async Task<ApiResponse<UserDto>> UpdateProfileAsync(int userId, UpdateUserProfileRequest request)
    {
        var user = await dbContext.Users.FindAsync(userId);
        if (user is null)
        {
            return ApiResponse<UserDto>.Fail("用户不存在");
        }

        var username = request.Username.Trim();
        var email = request.Email.Trim();

        if (await dbContext.Users.AnyAsync(item => item.Id != userId && item.Username == username))
        {
            return ApiResponse<UserDto>.Fail("用户名已存在");
        }

        if (await dbContext.Users.AnyAsync(item => item.Id != userId && item.Email == email))
        {
            return ApiResponse<UserDto>.Fail("邮箱已存在");
        }

        user.Username = username;
        user.Email = email;
        user.Phone = string.IsNullOrWhiteSpace(request.Phone) ? null : request.Phone.Trim();

        await dbContext.SaveChangesAsync();

        return ApiResponse<UserDto>.Ok(ToUserDto(user), "个人信息修改成功");
    }

    public async Task<ApiResponse<AvatarResponse>> UploadAvatarAsync(int userId, IFormFile file)
    {
        var user = await dbContext.Users.FindAsync(userId);
        if (user is null)
        {
            return ApiResponse<AvatarResponse>.Fail("用户不存在");
        }

        if (file.Length == 0)
        {
            return ApiResponse<AvatarResponse>.Fail("请选择头像文件");
        }

        if (file.Length > MaxAvatarSize)
        {
            return ApiResponse<AvatarResponse>.Fail("头像文件不能超过 5MB");
        }

        var extension = Path.GetExtension(file.FileName);
        if (!AllowedAvatarExtensions.Contains(extension))
        {
            return ApiResponse<AvatarResponse>.Fail("头像仅支持 jpg、jpeg、png、gif、webp 格式");
        }

        var uploadDirectory = Path.Combine(environment.ContentRootPath, "frontend", "assets", "uploads", "avatars");
        Directory.CreateDirectory(uploadDirectory);

        var fileName = $"{userId}_{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
        var filePath = Path.Combine(uploadDirectory, fileName);

        await using (var stream = File.Create(filePath))
        {
            await file.CopyToAsync(stream);
        }

        user.Avatar = $"/assets/uploads/avatars/{fileName}";
        await dbContext.SaveChangesAsync();

        return ApiResponse<AvatarResponse>.Ok(new AvatarResponse
        {
            Avatar = user.Avatar
        }, "头像上传成功");
    }

    public async Task<ApiResponse<UserHomeResponse>> GetUserHomeAsync(int id)
    {
        var user = await dbContext.Users
            .Include(item => item.Products.OrderByDescending(product => product.CreateTime).Take(12))
            .FirstOrDefaultAsync(item => item.Id == id);

        if (user is null)
        {
            return ApiResponse<UserHomeResponse>.Fail("用户不存在");
        }

        var response = new UserHomeResponse
        {
            User = ToUserDto(user),
            Products = user.Products
                .OrderByDescending(product => product.CreateTime)
                .Select(product => new UserProductSummaryDto
                {
                    Id = product.Id,
                    Title = product.Title,
                    Price = product.Price,
                    ImageUrl = product.ImageUrl,
                    Category = product.Category,
                    Status = product.Status,
                    CreateTime = product.CreateTime
                })
                .ToList()
        };

        return ApiResponse<UserHomeResponse>.Ok(response, "获取用户主页成功");
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
