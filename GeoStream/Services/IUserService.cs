using Microsoft.AspNetCore.Identity;
using GeoStream.Dtos.Administration.Users;

namespace GeoStream.Services
{
    public interface IUserService
    {
        Task<SignInResult> LoginAsync(string email, string password, bool isPersistent);
        Task Logout();
        Task<IdentityResult> RegisterUserAsync(NewUserDto newUserDto);
        Task<IdentityResult> UpdateUserAsync(ViewEditUserDto viewEditUser, bool ignoreNullAndEmpty = false);
        Task<IdentityResult> ChangePasswordAsync(string userId, string currentPassword, string newPassword);
        Task<bool> SendPasswordResetLinkAsync(string email);
        Task<IdentityResult> ResetPasswordAsync(string email, string token, string newPassword);

        Task<IdentityResult> DeleteUserAsync(string userId);
        Task<List<UserDto>> GetAllUsersAsync();
        Task<(IEnumerable<UserDto>, int)> GetUsersAsync(int pageNumber, int pageSize, string searchString = "");
        Task<ViewEditUserDto> GetUserByAsync(string userId);
        Task<UserDto> GetUserByEmailAsync(string userEmail);
    }
}
