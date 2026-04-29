
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
        /// Набор сущностей пользователей
        /// </summary>
        public DbSet<User> Users { get; set; }

        /// <summary>
        /// Набор сущностей тегов источников
        /// </summary>
        public DbSet<Tag> Tags { get; set; }

        /// <summary>
        /// Набор связей источников и тегов
        /// </summary>
        public DbSet<NewsSourceTag> NewsSourceTags { get; set; }

        /// <summary>
        /// Настройка модели базы данных при создании
        /// </summary>
        /// <param name="modelBuilder">Построитель моделей</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.Email).IsRequired().HasMaxLength(256);
                entity.Property(e => e.PasswordHash).IsRequired();
            });

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
                entity.Property(e => e.Type)
                      .HasConversion<string>();

                entity.HasIndex(e => e.IsActive)
                      .HasDatabaseName("IX_NewsSource_IsActive");

                entity.HasOne(d => d.User)
                      .WithMany(p => p.NewsSources)
                      .HasForeignKey(d => d.UserId)
                      .OnDelete(DeleteBehavior.Cascade)
                      .HasConstraintName("FK_NewsSource_User_UserId");
            });

            modelBuilder.Entity<Tag>(entity =>
            {
                entity.Property(e => e.Name)
                      .IsRequired()
                      .HasMaxLength(100);

                entity.Property(e => e.NormalizedName)
                      .IsRequired()
                      .HasMaxLength(100);

                entity.HasIndex(e => e.NormalizedName)
                      .IsUnique()
                      .HasDatabaseName("IX_Tag_NormalizedName");
            });

            modelBuilder.Entity<NewsSourceTag>(entity =>
            {
                entity.HasKey(e => new { e.NewsSourceId, e.TagId });

                entity.HasOne(e => e.NewsSource)
                      .WithMany(s => s.NewsSourceTags)
                      .HasForeignKey(e => e.NewsSourceId)
                      .OnDelete(DeleteBehavior.Cascade)
                      .HasConstraintName("FK_NewsSourceTag_NewsSource_NewsSourceId");

                entity.HasOne(e => e.Tag)
                      .WithMany(t => t.NewsSourceTags)
                      .HasForeignKey(e => e.TagId)
                      .OnDelete(DeleteBehavior.Cascade)
                      .HasConstraintName("FK_NewsSourceTag_Tag_TagId");
            });
        }
    }
}