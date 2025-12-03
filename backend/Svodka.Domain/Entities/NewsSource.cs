using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace Svodka.Domain.Entities
{
    /// <summary>
    /// Представляет собой источник новостей
    /// </summary>
    public class NewsSource
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Type {  get; set; } = string.Empty;
        public string Configuration { get; set; } = string.Empty;
        public bool IsActive {  get; set; }
        public DateTime? LastPolledAtUtc { get; set; }
        public DateTime? LastErrorAtUtc { get; set; }
        public string? LastError { get; set; }

        [JsonIgnore]
        public virtual ICollection<NewsItem> NewsItems { get; set; } = new List<NewsItem>();

    }
}
