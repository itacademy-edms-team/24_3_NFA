namespace Svodka.Domain.Entities
{
    /// <summary>
    /// Представляет собой единую новость/публикацию, независимо от источника.
    /// </summary>
    public class NewsItem
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string Link { get; set; } = string.Empty;

        public DateTime PublishedAtUtc { get; set; }

        public int SourceId { get; set; }

        public string SourceItemId { get; set; } = string.Empty;

        public string? Author { get; set; }

        public string? ImageUrl { get; set; }

        public string? Category { get; set; }

        public DateTime IndexedAtUtc { get; set; }

        public virtual NewsSource? NewsSource { get; set; }
    }
}