namespace ChronosTracker.Core.Entities
{
    public class Game
    {
        // Identifier for the game
        public Guid Id { get; set; }
        public int? IGDBId { get; set; }
        public string Title {get; set; } = string.Empty;
        public string? SteamAppId { get; set; }
        public string? CoverUrl { get; set; }
        public string? SteamStoreUrl => !string.IsNullOrEmpty(SteamAppId)
            ? $"https://store.steampowered.com/app/{SteamAppId}"
            : null;

        // Chronology
        public DateTime? ReleaseDate { get; set; }

        // Metadata
        public double? SteamRating { get; set; }
        public string? Genre { get; set; }
        public string? Developer { get; set; }
        public string? Publisher { get; set; }
        public int PopularityScore { get; set; }
        public bool IsSinglePlayer { get; set; }

        // Progress Tracking
        public string PlayStatus { get; set; } = "N";
        public int Status { get; set; } = 0;
        public DateTime? DateStarted { get; set; }
        public DateTime? DateFinished { get; set; }
        public DateTime? DateCreated { get; set; } = DateTime.Now;

        // Series Association
        public Guid? SeriesID { get; set; }
        public Series? Series { get; set; }
    }
}
