using ChronosTracker.Core.Entities;
using ChronosTracker.Infrastructure.Data;
using ChronosTracker.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;

namespace ChronosTracker.Web.Controllers;

public class GamesController : Controller
{
    private readonly AppDbContext _context;
    private readonly IGDBService _igdbService;

    public GamesController(AppDbContext context, IGDBService igdbService)
    {
        _context = context;
        _igdbService = igdbService;
    }

    public async Task<IActionResult> Browse(long? lastTimestamp = null, List<int> platformIds = null, string searchTerm = null)
    {
        try
        {
            // 1. Auto-apply Global Hardware Profile
            if (platformIds == null || !platformIds.Any())
            {
                var settings = await _context.UserSettings.FirstOrDefaultAsync();
                if (settings != null && !string.IsNullOrEmpty(settings.GlobalPlatformIds))
                {
                    platformIds = settings.GlobalPlatformIds
                        .Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(int.Parse)
                        .ToList();
                }
            }

            // 2. Get ALL IDs from your local DB to exclude (Hidden, Interested, etc.)
            // This is key to ensuring the API doesn't return things you've already processed
            var interactedIds = await _context.Games
                .Where(g => g.Status != 0)
                .Select(g => g.IGDBId)
                .Where(id => id.HasValue)
                .Cast<int>()
                .ToListAsync();

            // 3. Fetch a "Big Bucket" (250) from IGDB starting from lastTimestamp
            // We fetch more than we need so we have enough leftovers after filtering
            var bigBatch = await _igdbService.GetBrowseGamesAsync(250, platformIds, searchTerm, lastTimestamp);

            // 4. Filter the Big Batch in memory against your local database
            var filteredGames = bigBatch
                .Where(g => !interactedIds.Contains(g.id))
                .Take(50) // Now we "fill" the page to exactly 50
                .ToList();

            var platforms = await _igdbService.GetPlatformsAsync();

            // 5. Match IGDB results against local DB (for status badges/dates)
            var igdbIdsInPage = filteredGames.Select(g => g.id).ToList();
            var localGames = await _context.Games
                .Where(g => g.IGDBId.HasValue && igdbIdsInPage.Contains(g.IGDBId.Value))
                .ToListAsync();

            var localStatuses = localGames.ToDictionary(g => g.IGDBId!.Value, g => g.Status);
            var localDatesStarted = localGames.ToDictionary(g => g.IGDBId!.Value, g => g.DateStarted);
            var localDatesFinished = localGames.ToDictionary(g => g.IGDBId!.Value, g => g.DateFinished);

            // 6. Prepare View Metadata
            // Note: We are passing 'LastTimestamp' instead of 'Page'
            ViewBag.Platforms = platforms;
            ViewBag.SelectedPlatforms = platformIds ?? new List<int>();
            ViewBag.LocalStatuses = localStatuses;
            ViewBag.LocalDatesStarted = localDatesStarted;
            ViewBag.LocalDatesFinished = localDatesFinished;
            ViewBag.SearchTerm = searchTerm;
            ViewBag.MinDate = lastTimestamp; // Tracks where we started

            // This is the most important part for your "Next" button:
            ViewBag.NextTimestamp = filteredGames.LastOrDefault()?.first_release_date;

            return View(filteredGames);
        }
        catch (Exception ex)
        {
            return Content($"The Game Archive is currently unavailable. Error: {ex.Message}");
        }
    }

    public async Task<IActionResult> Library()
    {
        // This is like a 'Filtered View' of your local tracker sheet
        var myLibrary = await _context.Games
            .Where(g => g.Status == 1)
            .OrderBy(g => g.ReleaseDate)
            .ThenBy(g => g.Title)
            .ToListAsync();

        return View(myLibrary);
    }

    public async Task<IActionResult> HardwareProfile()
    {
        var platforms = await _igdbService.GetPlatformsAsync();
        var settings = await _context.UserSettings.FirstOrDefaultAsync() ?? new UserSettings();

        var selectedIds = new List<int>();
        if (!string.IsNullOrEmpty(settings.GlobalPlatformIds))
        {
            selectedIds = settings.GlobalPlatformIds
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(int.Parse)
                .ToList();
        }

        ViewBag.SelectedPlatforms = selectedIds;
        return View(platforms);
    }

    [HttpPost]
    public async Task<IActionResult> SaveHardwareProfile(List<int> platformIds)
    {
        var settings = await _context.UserSettings.FirstOrDefaultAsync();
        if (settings == null)
        {
            settings = new UserSettings();
            _context.UserSettings.Add(settings);
        }

        settings.GlobalPlatformIds = string.Join(",", platformIds ?? new List<int>());
        await _context.SaveChangesAsync();
        return RedirectToAction("Browse");
    }

    [HttpPost]
    public async Task<IActionResult> UpdateStatus(
        // Identity
        int igdbId,
        string title,
        string? slug,
        // Links & Media
        string? coverUrl,
        string? igdbUrl,
        string? steamUrl,
        string? gogUrl,
        string? epicUrl,
        // Game Details
        string? summary,
        string? releaseDate,
        string? developer,
        string? genres,
        string? platforms,
        string? seriesName,
        string? franchiseName,
        string? parentGameTitle,
        bool supportsEnglish,
        // Ratings
        double? worthinessScore,
        // User Tracking
        int status)

    {
        var game = await _context.Games.FirstOrDefaultAsync(g => g.IGDBId == igdbId);

        if (game == null)
        {
            game = new Game
            {
                Id = Guid.NewGuid(),
                IGDBId = igdbId,
                Title = title,
                Slug = slug,
                CoverUrl = coverUrl,
                IGDBUrl = igdbUrl,
                SteamUrl = steamUrl,
                GogUrl = gogUrl,
                EpicUrl = epicUrl,
                Summary = summary,
                Developer = developer,
                Genres = genres,
                Platforms = platforms,
                SeriesName = seriesName,
                FranchiseName = franchiseName,
                ParentGameTitle = parentGameTitle,
                WorthinessScore = worthinessScore,
                SupportsEnglish = supportsEnglish,
                Status = status,
                DateCreated = DateTime.UtcNow
            };

            if (DateTime.TryParse(releaseDate, out DateTime rDate))
            {
                game.ReleaseDate = rDate;
            }
            _context.Games.Add(game);
        }
        else
        {
            if (game.Status == status)
            {
                game.Status = 0;
            }
            else
            {
                if (DateTime.TryParse(releaseDate, out DateTime rDate))
                {
                    game.ReleaseDate = rDate;
                }
                game.CoverUrl = coverUrl;
                game.SteamUrl = steamUrl;
                game.GogUrl = gogUrl;
                game.EpicUrl = epicUrl;
                game.Platforms = platforms;
                game.SeriesName = seriesName;
                game.FranchiseName = franchiseName;
                game.ParentGameTitle = parentGameTitle;
                game.WorthinessScore = worthinessScore;
                game.SupportsEnglish = supportsEnglish;
                game.Status = status;
            }
        }

        await _context.SaveChangesAsync();
        return Json(new { success = true, newStatus = status });
    }

    [HttpPost]
    public async Task<IActionResult> UpdateDates(int igdbId, string type, DateTime? date)
    {
        var game = await _context.Games.FirstOrDefaultAsync(g => g.IGDBId == igdbId);
        if (game != null)
        {
            if (type == "start") game.DateStarted = date;
            if (type == "finish") game.DateFinished = date;

            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }
        return Json(new { success = false, message = "Save the game as 'Interested' first!" });
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View();
    }
}