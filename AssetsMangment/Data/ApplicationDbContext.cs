using Microsoft.EntityFrameworkCore;
using AssetsMangment.Models;
using System.Text.Json;

namespace AssetsMangment.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
        {
        }
        public DbSet<Asset> Assets { get; set; }
        public DbSet<AssetRelationship> AssetRelationships { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<AssetRelationship>()
                .HasOne(r => r.SourceAsset)
                .WithMany(a => a.OutgoingRelationships)
                .HasForeignKey(r => r.SourceAssetId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<AssetRelationship>()
                .HasOne(r => r.TargetAsset)
                .WithMany(a => a.IncomingRelationships)
                .HasForeignKey(r => r.TargetAssetId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Asset>()
                .HasIndex(a => new { a.Type, a.Value })
                .IsUnique();
            modelBuilder.Entity<Asset>(entity =>
            {
                entity.Property(e => e.Tags)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null)
                         ?? new List<string>()
                )
                .HasColumnType("jsonb");

                entity.Property(e => e.Metadata)
                    .HasConversion(
                        v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                        v => JsonSerializer.Deserialize<Dictionary<string, object>>(v, (JsonSerializerOptions?)null)
                             ?? new Dictionary<string, object>()
                    )
                    .HasColumnType("jsonb");
            });
        }

    }
}
