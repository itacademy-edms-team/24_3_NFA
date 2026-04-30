namespace Svodka.Application.DTOs
{
    public class SourceSyncResultDto
    {
        public int SourceId { get; set; }
        public int ItemsAdded { get; set; }
        public DateTime? LastPolledAtUtc { get; set; }
        public string? LastError { get; set; }
        public DateTime? LastErrorAtUtc { get; set; }
    }
}
