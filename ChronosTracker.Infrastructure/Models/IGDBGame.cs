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

    public DateTime? ReleaseDate => first_release_date.HasValue
        ? DateTimeOffset.FromUnixTimeSeconds(first_release_date.Value).DateTime
        : null;
}

public class IGDBNestedItem
{
    public int id { get; set; }
    public string name { get; set; }
}

public class  IGDBCover
{
    public int id { get; set; }
    public string url { get; set; }
}
