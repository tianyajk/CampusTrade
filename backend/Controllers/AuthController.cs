using System.Security.Claims;
using CampusTrade.Backend.Models;
using CampusTrade.Backend.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace CampusTrade.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService authService;

    public AuthController(IAuthService authService)
    {
        this.authService = authService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<ApiResponse<UserDto>>> Register(RegisterRequest request)
    {
        try
        {
            var result = await authService.RegisterAsync(request);
            return result.Success ? Ok(result) : BadRequest(result);
        }
        catch
        {
            return StatusCode(500, ApiResponse<UserDto>.Fail("注册失败，请稍后重试"));
        }
    }

    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<UserDto>>> Login(LoginRequest request)
    {
        try
        {
            var result = await authService.LoginAsync(request);
            if (!result.Success)
            {
                return Unauthorized(result);
            }

            var user = result.Data!;

            // 创建 Claims 身份信息
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Name, user.Username),
                new(ClaimTypes.Email, user.Email)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            // 写入 Cookie
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal);

            return Ok(result);
        }
        catch
        {
            return StatusCode(500, ApiResponse<UserDto>.Fail("登录失败，请稍后重试"));
        }
    }

    [HttpPost("logout")]
    public async Task<ActionResult<ApiResponse<object>>> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Ok(ApiResponse<object>.Ok(null, "已退出登录"));
    }
}
