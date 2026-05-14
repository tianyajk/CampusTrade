using CampusTrade.Backend.Models;
using CampusTrade.Backend.Services;
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
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Login(LoginRequest request)
    {
        try
        {
            var result = await authService.LoginAsync(request);
            return result.Success ? Ok(result) : Unauthorized(result);
        }
        catch
        {
            return StatusCode(500, ApiResponse<AuthResponse>.Fail("登录失败，请稍后重试"));
        }
    }
}
