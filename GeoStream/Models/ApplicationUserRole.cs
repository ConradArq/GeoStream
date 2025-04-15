using Microsoft.AspNetCore.Identity;

namespace GeoStream.Models
{
    public class ApplicationUserRole : IdentityUserRole<string>
    {
        public int StatusId { get; set; } = (int)Enums.Status.Active;
        public virtual Status Status { get; set; }
        public virtual ApplicationUser User { get; set; }
        public virtual ApplicationRole Role { get; set; }
    }
}
