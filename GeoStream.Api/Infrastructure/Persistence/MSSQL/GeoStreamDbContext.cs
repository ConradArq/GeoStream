using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using GeoStream.Api.Domain.Models;
using GeoStream.Api.Domain.Models.Entities;
using GeoStream.Api.Infrastructure.Interfaces.Logging;
using GeoStream.Api.Infrastructure.Logging.Models.Enums;
using GeoStream.Api.Infrastructure.Logging.Models;
using Route = GeoStream.Api.Domain.Models.Entities.Route;
using GeoStream.Api.Extensions;

namespace GeoStream.Api.Infrastructure.Persistence.MSSQL
{
    public class GeoStreamDbContext : DbContext
    {
        public string? CurrentUserId { get; set; }
        private readonly IApiLogger _apiLogger;

        public GeoStreamDbContext(DbContextOptions<GeoStreamDbContext> options, IApiLogger apiLogger) : base(options)
        {
            _apiLogger = apiLogger;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Asset>()
                .HasOne(v => v.Route)
                .WithMany(r => r.Assets)
                .HasForeignKey(v => v.RouteId)
                .OnDelete(DeleteBehavior.Restrict);
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            foreach (var entry in ChangeTracker.Entries<BaseDomainModel>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedDate = DateTime.Now.InTimeZone();
                        entry.Entity.CreatedBy = CurrentUserId;
                        break;

                    case EntityState.Modified:
                        entry.Entity.LastModifiedDate = DateTime.Now.InTimeZone();
                        entry.Entity.LastModifiedBy = CurrentUserId;
                        break;
                }
            }

            var changedEntities = ChangeTracker.Entries<BaseDomainModel>()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Deleted)
                .Select(entry => new
                {
                    Entry = entry,
                    OriginalState = entry.State,
                    EntityName = Model.FindEntityType(entry.Entity.GetType())?.GetTableName(),
                    Schema = Model.FindEntityType(entry.Entity.GetType())?.GetSchema(),
                    OldValues = JsonSerializer.Serialize(entry.Properties.Select(property => new Dictionary<string, object?>()
                    {
                    { property.Metadata.Name, property.OriginalValue }
                    }))
                })
                .ToList();

            int result = await base.SaveChangesAsync(cancellationToken);

            foreach (var changedEntity in changedEntities)
            {
                var auditLog = new AuditLog
                {
                    EntityName = $"{(changedEntity.Schema != null ? string.Concat(changedEntity.Schema, ".") : string.Empty)}{changedEntity.EntityName}"
                };

                switch (changedEntity.OriginalState)
                {
                    case EntityState.Added:
                        auditLog.EventType = EventType.Create;
                        auditLog.NewData = JsonSerializer.Serialize(changedEntity.Entry.Properties.Select(property => new Dictionary<string, object?>()
                    {
                        { property.Metadata.Name, property.CurrentValue }
                    }));
                        break;

                    case EntityState.Modified:
                        auditLog.EventType = EventType.Update;
                        auditLog.OldData = changedEntity.OldValues;
                        auditLog.NewData = JsonSerializer.Serialize(changedEntity.Entry.Properties.Select(property => new Dictionary<string, object?>()
                    {
                        { property.Metadata.Name, property.CurrentValue }
                    }));
                        break;

                    case EntityState.Deleted:
                        auditLog.EventType = EventType.Delete;
                        auditLog.OldData = changedEntity.OldValues;
                        break;
                }

                _apiLogger.LogInfo(auditLog);
            }

            return result;
        }

        public DbSet<Scanner> Scanners { get; set; }
        public DbSet<Hub> Hubs { get; set; }
        public DbSet<Asset> Assets { get; set; }
        public DbSet<AssetEmitter> AssetEmitters { get; set; }
        public DbSet<Route> Routes { get; set; }
        public DbSet<RouteHub> RouteHubs { get; set; }
        public DbSet<SpecialAccess> SpecialAccesss { get; set; }
    }
}
