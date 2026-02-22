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

    [JsonProperty("external_games")]
    public List<IGDBExternalGame> external_games { get; set; } = new();

    [JsonIgnore]
    public string? SteamAppId => external_games?
        .FirstOrDefault(x => x.external_game_source == 1)?
        .uid;

    public DateTime? ReleaseDate => first_release_date.HasValue
        ? DateTimeOffset.FromUnixTimeSeconds(first_release_date.Value).DateTime
        : null;
}

public class IGDBExternalGame
{

    [JsonProperty("external_game_source")]
    public int external_game_source { get; set; }

    [JsonProperty("uid")]
    public string? uid { get; set; }
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