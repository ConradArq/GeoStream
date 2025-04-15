using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using GeoStream.Dtos.Administration.Roles;
using GeoStream.Models;

namespace GeoStream.Services
{
    public class RoleService : IRoleService
    {
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public RoleService(RoleManager<ApplicationRole> roleManager, UserManager<ApplicationUser> userManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
        }

        public async Task<IdentityResult> CreateRoleAsync(NewRoleDto viewEditRole)
        {
            bool roleExists = await _roleManager.RoleExistsAsync(viewEditRole.Name);

            if (!roleExists)
            {
                ApplicationRole role = new ApplicationRole()
                {
                    Name = viewEditRole.Name,
                    NormalizedName = viewEditRole.Name
                };

                var result = await _roleManager.CreateAsync(role);

                if (!result.Succeeded)
                    throw new Exception(result.Errors.Select(error => error.Description).Aggregate((current, next) => current + next));

                return result;
            }
            else
            {
                return IdentityResult.Failed(new IdentityError { Code = "RoleExist", Description = $"El rol '{viewEditRole.Name}' ya existe" });
            }
        }

        public async Task<IdentityResult> UpdateRoleAsync(ViewEditRoleDto viewEditRole, bool ignoreNullAndEmpty = false)
        {
            ApplicationRole? role = await _roleManager.FindByIdAsync(viewEditRole.Id);

            if (role != null)
            {
                bool roleExists = await _roleManager.RoleExistsAsync(viewEditRole.Name) && viewEditRole.Name != role.Name;
                if (!roleExists)
                {
                    role.Name = ignoreNullAndEmpty && string.IsNullOrEmpty(viewEditRole.Name) ? role.Name : viewEditRole.Name;
                    role.StatusId = viewEditRole.StatusId == 0 ? role.StatusId : viewEditRole.StatusId;

                    var result = await _roleManager.UpdateAsync(role);

                    if (!result.Succeeded)
                        throw new Exception(result.Errors.Select(error => error.Description).Aggregate((current, next) => current + next));

                    return result;
                }
                else
                {
                    return IdentityResult.Failed(new IdentityError { Code = "RoleExist", Description = $"El rol '{viewEditRole.Name}' ya existe" });
                }
            }
            else
            {
                return IdentityResult.Failed(new IdentityError { Code = "RoleNotExist", Description = $"El rol '{viewEditRole.Name}' no existe" });
            }
        }

        public async Task<IdentityResult> DeleteRoleAsync(string roleId)
        {
            ApplicationRole? role = await _roleManager.FindByIdAsync(roleId);

            if (role != null)
            {
                if (await HasUsersAsync(role.Name!))
                {
                    return IdentityResult.Failed(new IdentityError { Code = "UserRolesExist", Description = $"El rol '{role.Name}' no se puede eliminar ya que existen users asociados a este rol. Elimine antes los users." });
                }

                var result = await _roleManager.DeleteAsync(role);

                if (!result.Succeeded)
                    throw new Exception(result.Errors.Select(error => error.Description).Aggregate((current, next) => current + next));

                return result;

            }
            else
            {
                return IdentityResult.Failed(new IdentityError { Code = "RoleNotExist", Description = $"El rol '{roleId}' no existe" });
            }
        }

        public async Task<List<RoleDto>> GetAllRolesAsync(bool onlyActive = false)
        {
            var roles = await _roleManager.Roles.Where(x=> !onlyActive || x.StatusId == (int)Models.Enums.Status.Active).Select(role => new RoleDto()
            {
                Id = role.Id,
                Name = role.Name!,
                StatusId = role.StatusId,
                Status = role.Status!.Name
            }).ToListAsync();

            return roles;
        }

        public async Task<ViewEditRoleDto> GetRoleByAsync(string roleId)
        {
            var role = await _roleManager.FindByIdAsync(roleId);

            if (role == null)
            {
                throw new InvalidOperationException($"Role '{roleId}' no encontrado");
            }

            var roleDto = new ViewEditRoleDto()
            {
                Id = role.Id,
                Name = role.Name!,
                StatusId = role.StatusId,
                Status = (await _roleManager.GetRoleNameAsync(role))!
            };

            return roleDto;
        }

        public async Task<bool> HasUsersAsync(string roleName)
        {
            var usersInRole = await _userManager.GetUsersInRoleAsync(roleName);
            return usersInRole.Any();
        }

    }
}
