using System.Security.Claims;
using CampusTrade.Backend.Models;
using CampusTrade.Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CampusTrade.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserService userService;

    public UserController(IUserService userService)
    {
        this.userService = userService;
    }

    [Authorize]
    [HttpGet("profile")]
    public async Task<ActionResult<ApiResponse<UserDto>>> GetProfile()
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId is null)
            {
                return Unauthorized(ApiResponse<UserDto>.Fail("请先登录"));
            }

            var result = await userService.GetProfileAsync(userId.Value);
            return result.Success ? Ok(result) : NotFound(result);
        }
        catch
        {
            return StatusCode(500, ApiResponse<UserDto>.Fail("获取用户信息失败，请稍后重试"));
        }
    }

    [Authorize]
    [HttpPut("profile")]
    public async Task<ActionResult<ApiResponse<UserDto>>> UpdateProfile(UpdateUserProfileRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId is null)
            {
                return Unauthorized(ApiResponse<UserDto>.Fail("请先登录"));
            }

            var result = await userService.UpdateProfileAsync(userId.Value, request);
            if (result.Success)
            {
                return Ok(result);
            }

            return result.Message == "用户不存在" ? NotFound(result) : BadRequest(result);
        }
        catch
        {
            return StatusCode(500, ApiResponse<UserDto>.Fail("修改用户信息失败，请稍后重试"));
        }
    }

    [Authorize]
    [HttpPost("avatar")]
    public async Task<ActionResult<ApiResponse<AvatarResponse>>> UploadAvatar([FromForm] IFormFile file)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId is null)
            {
                return Unauthorized(ApiResponse<AvatarResponse>.Fail("请先登录"));
            }

            var result = await userService.UploadAvatarAsync(userId.Value, file);
            if (result.Success)
            {
                return Ok(result);
            }

            return result.Message == "用户不存在" ? NotFound(result) : BadRequest(result);
        }
        catch
        {
            return StatusCode(500, ApiResponse<AvatarResponse>.Fail("头像上传失败，请稍后重试"));
        }
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<UserHomeResponse>>> GetUserHome(int id)
    {
        try
        {
            var result = await userService.GetUserHomeAsync(id);
            return result.Success ? Ok(result) : NotFound(result);
        }
        catch
        {
            return StatusCode(500, ApiResponse<UserHomeResponse>.Fail("获取用户主页失败，请稍后重试"));
        }
    }

    private int? GetCurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(value, out var userId) ? userId : null;
    }
}
