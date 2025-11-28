
using Microsoft.EntityFrameworkCore;
using Svodka.Domain.Entities;

namespace Svodka.Infrastructure.Data
{
    public class NewsAggregatorDbContext : DbContext
    {
        public NewsAggregatorDbContext(DbContextOptions<NewsAggregatorDbContext> options) : base(options)
        {
        }

        public DbSet<NewsItem> NewsItems { get; set; }
        public DbSet<NewsSource> NewsSources { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<NewsItem>(entity =>
            {
                entity.HasIndex(e => new { e.SourceId, e.SourceItemId })
                      .IsUnique().HasDatabaseName("IX_NewsItem_SourceId_SourceItemId");

                entity.HasIndex(e => e.PublishedAtUtc)
                      .HasDatabaseName("IX_NewsItem_PublishedAtUtc");

                entity.HasOne(d => d.NewsSource)
                      .WithMany(p => p.NewsItems) 
                      .HasForeignKey(d => d.SourceId)
                      .OnDelete(DeleteBehavior.ClientSetNull) 
                      .HasConstraintName("FK_NewsItem_NewsSource_SourceId");
            });


            modelBuilder.Entity<NewsSource>(entity =>
            {

                entity.HasIndex(e => e.IsActive)
                      .HasDatabaseName("IX_NewsSource_IsActive");
            });
        }
    }
}