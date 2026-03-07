namespace ChronosTracker.Core.Entities
{
    public class Game
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public int? IGDBId { get; set; }
        public string Title {get; set; } = string.Empty;
        public string? CoverUrl { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public string? SeriesName { get; set; }
        public string? FranchiseName { get; set; }
        public bool SupportsEnglish { get; set; } = false;
        public int Status { get; set; } = 0; // 0: Archive, 1: Library, 2: Hidden
        public DateTime? DateCreated { get; set; } = DateTime.UtcNow;
        public DateTime? DateStarted { get; set; }
        public DateTime? DateFinished { get; set; }
    }
}
