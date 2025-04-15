using MessageProcessorService.Domain.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace MessageProcessorService.Infrastructure.Persistence.MSSQL
{
    public class GeoStreamDbContext : DbContext
    {
        public GeoStreamDbContext(DbContextOptions<GeoStreamDbContext> option) : base(option)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }

        public DbSet<Scanner> Scanners { get; set; }
        public DbSet<Hub> Hubs { get; set; }
        public DbSet<Asset> Assets { get; set; }
        public DbSet<AssetEmitter> AssetEmitters { get; set; }
    }
}
