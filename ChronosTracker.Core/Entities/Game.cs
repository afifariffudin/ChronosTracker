namespace ChronosTracker.Core.Entities
{
    public class Game
    {
        // Identity
        public Guid Id { get; set; }
        public int? IGDBId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Slug { get; set; }

        // Links & Media
        public string? CoverUrl { get; set; }
        public string? IGDBUrl { get; set; }
        public string? SteamUrl { get; set; }
        public string? GogUrl { get; set; }
        public string? EpicUrl { get; set; }

        // Game Details
        public string? Summary { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public string? Developer { get; set; }
        public string? Genres { get; set; }
        public string? Platforms { get; set; }
        public string? SeriesName { get; set; }
        public string? FranchiseName { get; set; }
        public string? ParentGameTitle { get; set; }
        public bool SupportsEnglish { get; set; } = false;

        // Ratings
        public double? WorthinessScore { get; set; }

        // User Tracking
        public int Status { get; set; } = 0; // 0: Archive, 1: Library, 2: Hidden
        public DateTime? DateCreated { get; set; } = DateTime.UtcNow;
        public DateTime? DateStarted { get; set; }
        public DateTime? DateFinished { get; set; }
    }
}