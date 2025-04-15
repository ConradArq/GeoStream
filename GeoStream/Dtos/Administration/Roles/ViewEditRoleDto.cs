using System.ComponentModel.DataAnnotations;

namespace GeoStream.Dtos.Administration.Roles
{
    public class ViewEditRoleDto : BaseDto
    {
        public string Id { get; set; } = string.Empty;

        [Required(ErrorMessage = "Field is required")]
        public string Name { get; set; } = string.Empty;

        public string? Status { get; set; }
    }
}
