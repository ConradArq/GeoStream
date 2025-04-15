using GeoStream.Dtos.Administration.Roles;
using System.ComponentModel.DataAnnotations;

namespace GeoStream.Dtos.Administration.Users
{
    public class NewUserDto : BaseDto
    {
        [Required(ErrorMessage = "Field is required")]
        [EmailAddress(ErrorMessage = "The email address is not valid")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Field is required")]
        [DataType(DataType.Password)]
        [RegularExpression(
        @"^(?=.*?[A-Z])(?=.*?[a-z])(?=.*?\d)(?=.*?[^\w\s]).{8,}$",
        ErrorMessage = "The password must be at least 8 characters long and contain at least one uppercase letter, one lowercase letter, one digit, and one special character"
        )]
        public string Password { get; set; } = string.Empty;

        [Compare("Password", ErrorMessage = "The password confirmation does not match")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Field is required")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Field is required")]
        public string LastName { get; set; } = string.Empty;

        public string? Cedula { get; set; }
        public string? PhoneNumber { get; set; }

        [Required(ErrorMessage = "Field is required")]
        public string Gender { get; set; } = string.Empty;

        [Required(ErrorMessage = "Field is required")]
        public string SelectedRole { get; set; } = string.Empty;

        public List<RoleDto> Roles { get; set; } = new List<RoleDto>();
    }
}
