using CampusTrade.Backend.Models;

namespace CampusTrade.Backend.Services;

public interface IUserService
{
    Task<ApiResponse<UserDto>> GetProfileAsync(int userId);

    Task<ApiResponse<UserDto>> UpdateProfileAsync(int userId, UpdateUserProfileRequest request);

    Task<ApiResponse<AvatarResponse>> UploadAvatarAsync(int userId, IFormFile file);

    Task<ApiResponse<UserHomeResponse>> GetUserHomeAsync(int id);
}
