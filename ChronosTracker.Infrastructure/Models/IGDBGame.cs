using Newtonsoft.Json;

namespace ChronosTracker.Infrastructure.Models;

public class IGDBGame
{
    public int id { get; set; }
    public string name { get; set; }
    public string url { get; set; }
    public long? first_release_date { get; set; }
    public string summary { get; set; }
    public List<IGDBNestedItem> platforms { get; set; }
    public List<IGDBNestedItem> genres { get; set; }
    public IGDBCover cover { get; set; }
    public List<InvolvedCompany> involved_companies { get; set; }
    public IGDBNestedItem collection { get; set; }
    public List<IGDBNestedItem> franchises { get; set; }
    public IGDBNestedItem parent_game { get; set; }
    public double? total_rating { get; set; }
    public int? total_rating_count { get; set; }
    public int? hypes { get; set; }

    [JsonProperty("language_supports")]
    public List<IGDBLanguageSupport> language_supports { get; set; } = new();

    [JsonIgnore]
    public bool SupportsEnglish => language_supports?.Any(ls => ls.language == 12) ?? false;

    [JsonProperty("external_games")]
    public List<IGDBExternalGame> external_games { get; set; } = new();

    [JsonIgnore]
    public string? SteamAppId => external_games?
        .FirstOrDefault(x => x.external_game_source == 1)?
        .url;

    [JsonIgnore]
    public string? GOGId => external_games?
        .FirstOrDefault(x => x.external_game_source == 5)?
        .url;

    [JsonIgnore]
    public string? EpicGamesId => external_games?
        .FirstOrDefault(x => x.external_game_source == 26)?
        .url;

    [JsonIgnore]
    public double WorthinessScore
    {
        get
        {
            if (!total_rating.HasValue || (total_rating_count ?? 0) == 0)
                return 50.0;

            double rawScore = total_rating.Value / 100.0;
            int count = total_rating_count.Value;

            double score = rawScore - (rawScore - 0.5) * Math.Pow(2, -Math.Log10(count + 1));

            return Math.Round(score * 100, 1);
        }
    }

    public DateTime? ReleaseDate => first_release_date.HasValue
        ? DateTimeOffset.FromUnixTimeSeconds(first_release_date.Value).DateTime
        : null;
}

public class IGDBLanguageSupport
{
    public int id { get; set; }
    public int language { get; set; } // This will be '12' for English
    public int language_support_type { get; set; }
}

public class IGDBExternalGame
{

    [JsonProperty("external_game_source")]
    public int external_game_source { get; set; }

    [JsonProperty("url")]
    public string? url { get; set; }
}

public class IGDBNestedItem
{
    public int id { get; set; }
    public string name { get; set; }
}

public class IGDBCover
{
    public int id { get; set; }
    public string url { get; set; }
}

public class InvolvedCompany
{
    public int id { get; set; }
    public bool developer { get; set; }
    public IGDBNestedItem company { get; set; }
}

public class IGDBCountResponse
{
    public int count { get; set; }
}