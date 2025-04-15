using System.ComponentModel.DataAnnotations;

namespace GeoStream.Dtos.Administration.Roles
{
    public class NewRoleDto : BaseDto
    {
        [Required(ErrorMessage = "Field is required")]
        public string Name { get; set; }
    }
}
