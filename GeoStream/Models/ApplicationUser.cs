using Microsoft.AspNetCore.Identity;

namespace GeoStream.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? Cedula { get; set; } = null;
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string? PhoneNumber { get; set; }
        public string Gender { get; set; }
        public string? PlaceOfBirth { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? PlaceOfResidence { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? LastModifiedDate { get; set; }
        public string? LastModifiedBy { get; set; }
        public int StatusId { get; set; } = (int)Enums.Status.Active;
        public virtual Status Status { get; set; }
        public virtual ICollection<ApplicationUserRole> UserRoles { get; set; }

    }
}
