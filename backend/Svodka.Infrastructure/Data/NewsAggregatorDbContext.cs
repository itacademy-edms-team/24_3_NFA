
using Microsoft.EntityFrameworkCore;
using Svodka.Domain.Entities;

namespace Svodka.Infrastructure.Data
{
    /// <summary>
    /// Контекст базы данных для агрегации новостей
    /// </summary>
    public class NewsAggregatorDbContext : DbContext
    {
        /// <summary>
        /// Конструктор контекста базы данных
        /// </summary>
        /// <param name="options">Опции конфигурации контекста</param>
        public NewsAggregatorDbContext(DbContextOptions<NewsAggregatorDbContext> options) : base(options)
        {
        }

        /// <summary>
        /// Набор сущностей новостей
        /// </summary>
        public DbSet<NewsItem> NewsItems { get; set; }

        /// <summary>
        /// Набор сущностей источников новостей
        /// </summary>
        public DbSet<NewsSource> NewsSources { get; set; }

        /// <summary>
        /// Настройка модели базы данных при создании
        /// </summary>
        /// <param name="modelBuilder">Построитель моделей</param>
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
                      .OnDelete(DeleteBehavior.Cascade)
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