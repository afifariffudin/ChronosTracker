using ChronosTracker.Infrastructure.Models;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

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

        var response = await _httpClient.PostAsync(url, null);
        var content = await response.Content.ReadAsStringAsync();

        var tokenData = JsonConvert.DeserializeObject<TwitchToken>(content);
        _accessToken = tokenData.access_token;

        return _accessToken;
    }

    public async Task<List<IGDBGame>> GetBrowseGamesAsync(int offset = 0, List<int> platformIds = null)
    {
        var token = await GetAccessTokenAsync();
        var clientId = _config["IGDB:ClientId"];

        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("Client-ID", clientId);
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

        string platformFilter = "";
        if (platformIds != null && platformIds.Any())
        {
            var ids = string.Join(",", platformIds);
            platformFilter = $"& platforms = ({ids})";
        }

        var body = $"fields name, url, first_release_date, summary, platforms.name, genres.name, cover.url; " +
            $"where first_release_date >0 {platformFilter}; " +
            $"sort first_release_date asc; " +
            $"limit 50; " +
            $"offset {offset};";

        var content = new StringContent(body, System.Text.Encoding.UTF8, "text/plain");
        var response = await _httpClient.PostAsync("https://api.igdb.com/v4/games", content);
        var jsonResponse = await response.Content.ReadAsStringAsync();

        return JsonConvert.DeserializeObject<List<IGDBGame>>(jsonResponse) ?? new List<IGDBGame>();

    }

    public async Task<List<IGDBNestedItem>> GetPlatformsAsync()
    {
        var token = await GetAccessTokenAsync();
        var clientId = _config["IGDB:ClientId"];

        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("Client-ID", clientId);
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

        var body = $"fields name; sort name asc; limit 500;";
        var content = new StringContent(body, System.Text.Encoding.UTF8, "text/plain");

        var response = await _httpClient.PostAsync("https://api.igdb.com/v4/platforms", content);
        var jsonResponse = await response.Content.ReadAsStringAsync();

        return JsonConvert.DeserializeObject<List<IGDBNestedItem>>(jsonResponse) ?? new List<IGDBNestedItem>();
    }
}
