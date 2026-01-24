namespace ChronosTracker.Core.Entities
{
    public class Game
    {
        // Identifier for the game
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? SteamAppId { get; set; }

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
        public DateTime? DateStarted { get; set; }
        public DateTime? DateFinished { get; set; }

        // Series Association
        public Guid? SeriesID { get; set; }
        public Series? Series { get; set; }
    }
}
