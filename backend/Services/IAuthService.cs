using CampusTrade.Backend.Models;

namespace CampusTrade.Backend.Services;

public interface IAuthService
{
    Task<ApiResponse<UserDto>> RegisterAsync(RegisterRequest request);

    Task<ApiResponse<AuthResponse>> LoginAsync(LoginRequest request);
}
