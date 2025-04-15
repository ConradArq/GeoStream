using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using GeoStream.Models;
using System.Reflection;

namespace GeoStream.Data
{
    public class GeoStreamDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string,
    IdentityUserClaim<string>, ApplicationUserRole, IdentityUserLogin<string>,
    IdentityRoleClaim<string>, IdentityUserToken<string>>
    {
        public GeoStreamDbContext(DbContextOptions<GeoStreamDbContext> option) : base(option) 
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ApplicationUser>()
            .HasMany(x => x.UserRoles)
            .WithOne(x => x.User)
            .HasForeignKey(x => x.UserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ApplicationRole>()
            .HasMany(x => x.UserRoles)
            .WithOne(x => x!.Role)
            .HasForeignKey(x => x.RoleId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ApplicationRole>()
            .HasMany(x => x.UserRoles)
            .WithOne(x => x!.Role)
            .HasForeignKey(x => x.RoleId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Status>()
            .HasMany(x => x.Users)
            .WithOne(x => x.Status)
            .HasForeignKey(x => x.StatusId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Status>()
            .HasMany(x => x.Roles)
            .WithOne(x => x.Status)
            .HasForeignKey(x => x.StatusId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Status>()
            .HasMany(x => x.UserRoles)
            .WithOne(x => x.Status)
            .HasForeignKey(x => x.StatusId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            foreach (var entry in ChangeTracker.Entries())
            {
                var entity = entry.Entity;
                var type = entity.GetType();

                switch (entry.State)
                {
                    case EntityState.Added:
                        PropertyInfo? createdDateProperty = type.GetProperty("CreatedDate");
                        if (createdDateProperty != null && createdDateProperty.CanWrite)
                        {
                            createdDateProperty.SetValue(entity, DateTime.Now);
                        }

                        PropertyInfo? createdByProperty = type.GetProperty("CreatedBy");
                        if (createdByProperty != null && createdByProperty.CanWrite)
                        {
                            createdByProperty.SetValue(entity, "test");
                        }
                        break;

                    case EntityState.Modified:
                        PropertyInfo? lastModifiedDateProperty = type.GetProperty("LastModifiedDate");
                        if (lastModifiedDateProperty != null && lastModifiedDateProperty.CanWrite)
                        {
                            lastModifiedDateProperty.SetValue(entity, DateTime.Now);
                        }

                        PropertyInfo? lastModifiedByProperty = type.GetProperty("LastModifiedBy");
                        if (lastModifiedByProperty != null && lastModifiedByProperty.CanWrite)
                        {
                            lastModifiedByProperty.SetValue(entity, "test");
                        }
                        break;
                }
            }

            return base.SaveChangesAsync(cancellationToken);
        }

        public void SeedData()
        {
            if (!Status.Any())
            {
                Status.AddRange(
                    new Status
                    {
                        Name = "Active"
                    },
                    new Status
                    {
                        Name = "Inactive"
                    });

                SaveChanges();
            }

            if (!ApplicationUsers.Any())
            {
                var admin = new ApplicationUser
                {
                    Id = "a18be9c0-aa65-4af8-bd17-00bd9344e575",
                    UserName = "admin",
                    NormalizedUserName = "ADMIN",
                    FirstName = "Admin",
                    LastName = "Admin",
                    Email = "admin@admin.com",
                    NormalizedEmail = "ADMIN@ADMIN.COM",
                    PhoneNumber = "555-555",
                    EmailConfirmed = true,
                    PhoneNumberConfirmed = true,
                    Gender = "F",
                    StatusId = (int)Models.Enums.Status.Active,
                    CreatedDate = DateTime.Now,
                    CreatedBy = "a18be9c0-aa65-4af8-bd17-00bd9344e575",
                    LastModifiedDate = DateTime.Now,
                    LastModifiedBy = "a18be9c0-aa65-4af8-bd17-00bd9344e575"
                };
                admin.PasswordHash = GeneratePassword(admin);

                ApplicationUsers.AddRange(admin);

                SaveChanges();
            }

            if (!ApplicationRoles.Any())
            {
                ApplicationRoles.AddRange(
                    new ApplicationRole
                    {
                        Id = "d2674562-193a-41e6-9a92-7f7cb04caf90",
                        Name = "Administrator",
                        NormalizedName = "ADMINISTRATOR",
                        StatusId = (int)Models.Enums.Status.Active
                    });

                SaveChanges();
            }

            if (!ApplicationUserRoles.Any())
            {
                ApplicationUserRoles.AddRange(
                    new ApplicationUserRole
                    {
                        UserId = "a18be9c0-aa65-4af8-bd17-00bd9344e575",
                        RoleId = "d2674562-193a-41e6-9a92-7f7cb04caf90",
                        StatusId = (int)Models.Enums.Status.Active
                    });

                SaveChanges();
            }
        }

        public DbSet<Status> Status { get; set; }
        public DbSet<ApplicationRole> ApplicationRoles { get; set; }
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<ApplicationUserRole> ApplicationUserRoles { get; set; }

        public string GeneratePassword(ApplicationUser user)
        {
            var passHash = new PasswordHasher<ApplicationUser>();
            return passHash.HashPassword(user, "Password2025$");
        }
    }
}
