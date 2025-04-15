namespace GeoStream.Models
{
    public class Status
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public virtual ICollection<ApplicationUser> Users { get; set; }
        public virtual ICollection<ApplicationRole> Roles { get; set; }
        public virtual ICollection<ApplicationUserRole> UserRoles { get; set; }
    }
}
