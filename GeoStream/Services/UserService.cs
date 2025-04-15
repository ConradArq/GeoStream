using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using GeoStream.Dtos.Administration.Users;
using GeoStream.Models;
using System.Transactions;

namespace GeoStream.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public UserService(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager, SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
        }
        public async Task<SignInResult> LoginAsync(string email, string password, bool isPersistent)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
                return SignInResult.Failed;

            var roleNames = await _userManager.GetRolesAsync(user);

            var result = await _signInManager.PasswordSignInAsync(user, password, isPersistent, lockoutOnFailure: false);

            return result;
        }
        public async Task Logout()
        {
            await _signInManager.SignOutAsync();
        }

        public async Task<IdentityResult> RegisterUserAsync(NewUserDto newUserDto)
        {
            var userExist = await _userManager.FindByEmailAsync(newUserDto.Email);
            if (userExist != null)
            {
                return IdentityResult.Failed(new IdentityError { Code = "UserExist", Description = $"El user '{newUserDto.Email}' ya existe" });
            }

            var roleExist = await _roleManager.RoleExistsAsync(newUserDto.SelectedRole);
            if (!roleExist)
            {
                return IdentityResult.Failed(new IdentityError { Code = "RoleNotExist", Description = $"El rol '{newUserDto.SelectedRole}' no existe" });
            }

            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {
                    var user = new ApplicationUser { Email = newUserDto.Email, UserName = newUserDto.Email, FirstName = newUserDto.FirstName, LastName = newUserDto.LastName, Cedula = newUserDto.Cedula, PhoneNumber = newUserDto.PhoneNumber, Gender = newUserDto.Gender };

                    var result = await _userManager.CreateAsync(user, newUserDto.Password);

                    if (!result.Succeeded)
                    {
                        return result;
                    }

                    result = await _userManager.AddToRoleAsync(user, newUserDto.SelectedRole);

                    scope.Complete();
                    return result;
                }
                catch (Exception)
                {
                    scope.Dispose();
                    throw;
                }
            }
        }

        public async Task<IdentityResult> UpdateUserAsync(ViewEditUserDto viewEditUser, bool ignoreNullAndEmpty = false)
        {
            var user = await _userManager.FindByIdAsync(viewEditUser.Id);
            if (user == null)
            {
                return IdentityResult.Failed(new IdentityError { Code = "UserNotFound", Description = $"El user {viewEditUser.Id} no existe" });
            }

            var userByEmail = await _userManager.FindByEmailAsync(viewEditUser.Email);
            if (userByEmail != null && userByEmail.Id != user.Id)
            {
                return IdentityResult.Failed(new IdentityError { Code = "UserExist", Description = $"Ya existe un user con email '{viewEditUser.Email}'" });
            }

            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {
                    if (!string.IsNullOrEmpty(viewEditUser.Password))
                    {
                        var passwordResult = await _userManager.RemovePasswordAsync(user);
                        if (!passwordResult.Succeeded)
                        {
                            return passwordResult;
                        }
                        var addPasswordResult = await _userManager.AddPasswordAsync(user, viewEditUser.Password);
                        if (!addPasswordResult.Succeeded)
                        {
                            return addPasswordResult;
                        }
                    }

                    if (!string.IsNullOrEmpty(viewEditUser.RoleId))
                    {
                        var roleExist = await _roleManager.RoleExistsAsync(viewEditUser.RoleId);
                        if (!roleExist)
                        {
                            return IdentityResult.Failed(new IdentityError { Code = "RoleNotExist", Description = $"El rol '{viewEditUser.RoleId}' no existe." });
                        }

                        var currentRoles = await _userManager.GetRolesAsync(user);

                        var removeRolesResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
                        if (!removeRolesResult.Succeeded)
                        {
                            return removeRolesResult;
                        }

                        var addToRoleResult = await _userManager.AddToRoleAsync(user, viewEditUser.RoleId);
                        if (!addToRoleResult.Succeeded)
                        {
                            return addToRoleResult;
                        }
                    }

                    user.Email = ignoreNullAndEmpty && string.IsNullOrEmpty(viewEditUser.Email) ? user.Email : viewEditUser.Email;
                    user.FirstName = ignoreNullAndEmpty && string.IsNullOrEmpty(viewEditUser.FirstName) ? user.FirstName : viewEditUser.FirstName;
                    user.LastName = ignoreNullAndEmpty && string.IsNullOrEmpty(viewEditUser.LastName) ? user.LastName : viewEditUser.LastName;
                    user.Cedula = ignoreNullAndEmpty && string.IsNullOrEmpty(viewEditUser.Cedula) ? user.Cedula : viewEditUser.Cedula;
                    user.PhoneNumber = ignoreNullAndEmpty && string.IsNullOrEmpty(viewEditUser.PhoneNumber) ? user.PhoneNumber : viewEditUser.PhoneNumber;
                    user.Gender = ignoreNullAndEmpty && string.IsNullOrEmpty(viewEditUser.Gender) ? user.Gender : viewEditUser.Gender;
                    user.StatusId = viewEditUser.StatusId == 0 ? user.StatusId : viewEditUser.StatusId;

                    var result = await _userManager.UpdateAsync(user);

                    scope.Complete();
                    return result;
                }
                catch (Exception)
                {
                    scope.Dispose();
                    throw;
                }
            }
            
        }
        public async Task<IdentityResult> ChangePasswordAsync(string userId, string currentPassword, string newPassword)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return IdentityResult.Failed(new IdentityError { Description = "User not found." });

            var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
            return result;
        }

        public async Task<bool> SendPasswordResetLinkAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return false;

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            // Code to send the token via email goes here
            // You would typically generate a link with the token as part of the query string
            return true;
        }

        public async Task<IdentityResult> ResetPasswordAsync(string email, string token, string newPassword)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                // To avoid account enumeration attacks, you might want to avoid signaling that the user doesn't exist.
                return IdentityResult.Success;
            }

            var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
            return result;
        }

        public async Task<IdentityResult> DeleteUserAsync(string userId)
        {
            ApplicationUser? user = await _userManager.FindByIdAsync(userId);

            if (user != null)
            {
                var result = await _userManager.DeleteAsync(user);

                if (!result.Succeeded)
                    throw new Exception(result.Errors.Select(error => error.Description).Aggregate((current, next) => current + next));

                return result;
            }
            else
            {
                return IdentityResult.Failed(new IdentityError { Code = "UserNotExist", Description = $"El user '{userId}' no existe" });
            }
        }

        public async Task<List<UserDto>> GetAllUsersAsync()
        {
            var users = _userManager.Users.ToList();
            var userListDtos = new List<UserDto>();

            foreach (var user in users)
            {
                var roleName = await GetRoleNameForUser(user);

                var userDto = new UserDto
                {
                    Id = user.Id,
                    FullName = string.Concat(user.FirstName, " ", user.LastName),
                    Email = user.Email!,
                    Role = roleName,
                    StatusId = user.StatusId,
                    Status = Enum.IsDefined(typeof(Models.Enums.Status), user.StatusId)
                        ? ((Models.Enums.Status)user.StatusId).ToString()
                        : string.Empty
                };

                userListDtos.Add(userDto);
            }

            return userListDtos;
        }

        public async Task<(IEnumerable<UserDto>, int)> GetUsersAsync(int pageNumber, int pageSize, string searchString = "")
        {
            var query = _userManager.Users;

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(user =>
                    user.FirstName.Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
                    user.LastName.Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
                    user.Email!.Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
                    user.UserRoles.Any(x => x.Role.Name!.Contains(searchString, StringComparison.OrdinalIgnoreCase)));
            }

            var totalItems = await query.CountAsync();

            Type stateType = typeof(Models.Enums.Status);
            var items = await query
                .OrderBy(user => user.FirstName)
                .Skip(pageNumber * pageSize)
                .Take(pageSize)
                .Select(x => new UserDto
                {
                    Id = x.Id,
                    FullName = string.Concat(x.FirstName, " ", x.LastName),
                    Email = x.Email!,
                    Role = x.UserRoles.FirstOrDefault()!.Role.Name!,
                    StatusId = x.StatusId,
                    Status = Enum.IsDefined(stateType, x.StatusId)
                            ? ((Models.Enums.Status)x.StatusId).ToString()
                            : string.Empty
                }).ToListAsync();

            return (items, totalItems);
        }


        public async Task<ViewEditUserDto> GetUserByAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                throw new InvalidOperationException($"User '{userId}' no encontrado");
            }

            var roleName = await GetRoleNameForUser(user);
            var userDto = new ViewEditUserDto
            {
                Id = user.Id,
                Email = user.Email!,
                FirstName = user.FirstName!,
                LastName = user.LastName,
                Cedula = user.Cedula,
                PhoneNumber = user.PhoneNumber,
                Gender = user.Gender,
                RoleId = roleName
            };
            return userDto;
        }

        public async Task<UserDto> GetUserByEmailAsync(string userEmail)
        {
            var user = await _userManager.FindByEmailAsync(userEmail);

            if (user == null)
            {
                throw new InvalidOperationException($"User con email '{userEmail}' no encontrado");
            }

            var roleName = await GetRoleNameForUser(user);

            var userDto = new UserDto
            {
                Id = user.Id,
                FullName = string.Concat(user.FirstName, " ", user.LastName),
                Email = user.Email!,
                Role = roleName,
                StatusId = user.StatusId,
                Status = Enum.IsDefined(typeof(Models.Enums.Status), user.StatusId)
                    ? ((Models.Enums.Status)user.StatusId).ToString()
                    : string.Empty
            };

            return userDto;
        }

        private async Task<string> GetRoleNameForUser(ApplicationUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);

            var roleName = roles.FirstOrDefault();
            return roleName ?? "No Role";
        }
    }
}
