using CampusTrade.Backend.Data;
using CampusTrade.Backend.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CampusTrade.Backend.Services;

public class AuthService : IAuthService
{
    private readonly CampusTradeDbContext dbContext;
    private readonly IPasswordHasher<User> passwordHasher;

    public AuthService(CampusTradeDbContext dbContext, IPasswordHasher<User> passwordHasher)
    {
        this.dbContext = dbContext;
        this.passwordHasher = passwordHasher;
    }

    public async Task<ApiResponse<UserDto>> RegisterAsync(RegisterRequest request)
    {
        var username = request.Username.Trim();
        var email = request.Email.Trim();

        if (await dbContext.Users.AnyAsync(user => user.Username == username))
        {
            return ApiResponse<UserDto>.Fail("用户名已存在");
        }

        if (await dbContext.Users.AnyAsync(user => user.Email == email))
        {
            return ApiResponse<UserDto>.Fail("邮箱已存在");
        }

        var user = new User
        {
            Username = username,
            Email = email,
            Phone = string.IsNullOrWhiteSpace(request.Phone) ? null : request.Phone.Trim(),
            CreateTime = DateTime.Now
        };

        user.PasswordHash = passwordHasher.HashPassword(user, request.Password);

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        return ApiResponse<UserDto>.Ok(ToUserDto(user), "注册成功");
    }

    public async Task<ApiResponse<UserDto>> LoginAsync(LoginRequest request)
    {
        var account = request.Account.Trim();
        var user = await dbContext.Users.FirstOrDefaultAsync(item => item.Username == account || item.Email == account);

        if (user is null)
        {
            return ApiResponse<UserDto>.Fail("账号或密码错误");
        }

        var result = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (result == PasswordVerificationResult.Failed)
        {
            return ApiResponse<UserDto>.Fail("账号或密码错误");
        }

        return ApiResponse<UserDto>.Ok(ToUserDto(user), "登录成功");
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
