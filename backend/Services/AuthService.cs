using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CampusTrade.Backend.Data;
using CampusTrade.Backend.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace CampusTrade.Backend.Services;

public class AuthService : IAuthService
{
    private readonly CampusTradeDbContext dbContext;
    private readonly IPasswordHasher<User> passwordHasher;
    private readonly IConfiguration configuration;

    public AuthService(CampusTradeDbContext dbContext, IPasswordHasher<User> passwordHasher, IConfiguration configuration)
    {
        this.dbContext = dbContext;
        this.passwordHasher = passwordHasher;
        this.configuration = configuration;
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

    public async Task<ApiResponse<AuthResponse>> LoginAsync(LoginRequest request)
    {
        var account = request.Account.Trim();
        var user = await dbContext.Users.FirstOrDefaultAsync(item => item.Username == account || item.Email == account);

        if (user is null)
        {
            return ApiResponse<AuthResponse>.Fail("账号或密码错误");
        }

        var result = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (result == PasswordVerificationResult.Failed)
        {
            return ApiResponse<AuthResponse>.Fail("账号或密码错误");
        }

        var expiresAt = DateTime.Now.AddMinutes(configuration.GetValue("Jwt:ExpireMinutes", 10080));
        var token = GenerateToken(user, expiresAt);

        return ApiResponse<AuthResponse>.Ok(new AuthResponse
        {
            Token = token,
            ExpiresAt = expiresAt,
            User = ToUserDto(user)
        }, "登录成功");
    }

    private string GenerateToken(User user, DateTime expiresAt)
    {
        var jwtKey = configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key is not configured.");
        var jwtIssuer = configuration["Jwt:Issuer"];
        var jwtAudience = configuration["Jwt:Audience"];
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.Email, user.Email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N"))
        };

        var token = new JwtSecurityToken(
            issuer: jwtIssuer,
            audience: jwtAudience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
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
