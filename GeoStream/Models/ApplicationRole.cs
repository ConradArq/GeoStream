using Microsoft.AspNetCore.Identity;

namespace GeoStream.Models
{
    public class ApplicationRole : IdentityRole
    {
        public DateTime CreatedDate { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? LastModifiedDate { get; set; }
        public string? LastModifiedBy { get; set; }
        public int StatusId { get; set; } = (int)Enums.Status.Active;
        public virtual Status? Status { get; set; }
        public virtual ICollection<ApplicationUserRole> UserRoles { get; set; }
    }
}
