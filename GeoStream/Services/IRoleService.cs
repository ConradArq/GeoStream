using Microsoft.AspNetCore.Identity;
using GeoStream.Dtos.Administration.Roles;

namespace GeoStream.Services
{
    public interface IRoleService
    {
        Task<IdentityResult> CreateRoleAsync(NewRoleDto newRoleDto);
        Task<IdentityResult> UpdateRoleAsync(ViewEditRoleDto viewEditRole, bool ignoreNullAndEmpty = false);
        Task<IdentityResult> DeleteRoleAsync(string roleId);
        Task<List<RoleDto>> GetAllRolesAsync(bool onlyActive = false);
        Task<ViewEditRoleDto> GetRoleByAsync(string roleId);
        Task<bool> HasUsersAsync(string roleName);
    }
}
