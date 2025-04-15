using System.ComponentModel.DataAnnotations;

namespace GeoStream.Dtos.Administration.Roles
{
    public class RoleDto : BaseDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Status { get; set; }
    }
}
