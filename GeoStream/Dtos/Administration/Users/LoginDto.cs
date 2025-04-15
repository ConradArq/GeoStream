using GeoStream.Models;
using System.ComponentModel.DataAnnotations;

namespace GeoStream.Dtos.Administration.Users
{
    public class LoginDto : BaseDto
    {
        [Required(ErrorMessage = "Field is required")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Field is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Remember me")]
        public bool RememberMe { get; set; }
    }
}
