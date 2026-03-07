using ChronosTracker.Infrastructure.Models;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Text;

namespace ChronosTracker.Infrastructure.Services;

public class IGDBService
{
    private readonly IConfiguration _config;
    private readonly HttpClient _httpClient;
    private string _accessToken;

    public IGDBService(IConfiguration config, HttpClient httpClient)
    {
        _config = config;
        _httpClient = httpClient;
    }

    public async Task<string> GetAccessTokenAsync()
    {
        if (!string.IsNullOrEmpty(_accessToken)) return _accessToken;

        var clientId = _config["IGDB:ClientId"];
        var clientSecret = _config["IGDB:ClientSecret"];
        var url = $"https://id.twitch.tv/oauth2/token?client_id={clientId}&client_secret={clientSecret}&grant_type=client_credentials";

        // Must be POST for Twitch Auth
        var response = await _httpClient.PostAsync(url, null);
        var content = await response.Content.ReadAsStringAsync();

        var tokenData = JsonConvert.DeserializeObject<TwitchToken>(content);
        _accessToken = tokenData?.access_token;

        return _accessToken;
    }

    public async Task<List<IGDBGame>> GetBrowseGamesAsync(int limit = 250, List<int> platformIds = null, string searchTerm = null, long? lastTimestamp = null, bool onlyEnglish = true)
    {
        var token = await GetAccessTokenAsync();
        var clientId = _config["IGDB:ClientId"];

        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("Client-ID", clientId);
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

        string fieldList = "fields name, url, summary, first_release_date, cover.url, " +
                           "platforms.name, genres.name, " +
                           "involved_companies.developer, involved_companies.company.name, " +
                           "collection.name, franchises.name, parent_game.name, " +
                           "total_rating, total_rating_count, hypes, " +
                           "external_games.external_game_source, external_games.uid, external_games.url, " +
                           "language_supports.language, language_supports.language_support_type;";

        string whereClause = BuildWhereClause(platformIds, searchTerm, lastTimestamp, onlyEnglish);

        string body = $"{fieldList} {whereClause}; sort first_release_date asc; limit {limit};";

        var content = new StringContent(body, Encoding.UTF8, "text/plain");
        var response = await _httpClient.PostAsync("https://api.igdb.com/v4/games", content);
        var jsonResponse = await response.Content.ReadAsStringAsync();

        System.Diagnostics.Debug.WriteLine($"IGDB Response: {jsonResponse}");

        return JsonConvert.DeserializeObject<List<IGDBGame>>(jsonResponse) ?? new List<IGDBGame>();
    }

    public async Task<int> GetGamesCountAsync(List<int> platformIds = null, string searchTerm = null, long? minDate = null, bool onlyEnglish = true)
    {
        var token = await GetAccessTokenAsync();
        var clientId = _config["IGDB:ClientId"];

        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("Client-ID", clientId);
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

        string whereClause = BuildWhereClause(platformIds, searchTerm, minDate, onlyEnglish);
        string body = $"{whereClause};";

        var content = new StringContent(body, Encoding.UTF8, "text/plain");
        var response = await _httpClient.PostAsync("https://api.igdb.com/v4/games/count", content);
        var jsonResponse = await response.Content.ReadAsStringAsync();

        var result = JsonConvert.DeserializeObject<IGDBCountResponse>(jsonResponse);
        return result?.count ?? 0;
    }

    private string BuildWhereClause(List<int> platformIds, string searchTerm, long? lastTimestamp = null, bool onlyEnglish = true)
    {
        long startTimestamp = lastTimestamp ?? 0;
        string where = $"where first_release_date > {startTimestamp} & first_release_date != null";

        if (onlyEnglish)
        {
            where += " & (" +
                             "(language_supports.language = 12 & language_supports.language_support_type = 3) | " +
                             "(language_supports.language = 12 & language_supports.language_support_type = 2) | " +
                             "language_supports = null" +
                             ")";
        }

        if (!string.IsNullOrEmpty(searchTerm))
        {
            where += $" & name ~ *\"{searchTerm}\"*";
        }

        if (platformIds != null && platformIds.Any())
        {
            where += $" & platforms = ({string.Join(",", platformIds)})";
        }

        return where;
    }

    public async Task<List<IGDBNestedItem>> GetPlatformsAsync()
    {
        var token = await GetAccessTokenAsync();
        var clientId = _config["IGDB:ClientId"];

        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("Client-ID", clientId);
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

        var body = "fields name; sort name asc; limit 500;";

        var content = new StringContent(body, Encoding.UTF8, "text/plain");
        var response = await _httpClient.PostAsync("https://api.igdb.com/v4/platforms", content);
        var jsonResponse = await response.Content.ReadAsStringAsync();

        return JsonConvert.DeserializeObject<List<IGDBNestedItem>>(jsonResponse) ?? new List<IGDBNestedItem>();
    }
}