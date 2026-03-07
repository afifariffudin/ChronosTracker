using ChronosTracker.Core.Entities;
using ChronosTracker.Infrastructure.Data;
using ChronosTracker.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
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


    public async Task<IActionResult> Browse(int page = 0, List<int> platformIds = null, string searchTerm = null, long? minDate = null)
    {
        try
        {
            // 1. Auto-apply Global Hardware Profile if no temporary filter is set
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

            // 2. Fetch Total Count for the CURRENT Era (starts from minDate)
            int totalCount = await _igdbService.GetGamesCountAsync(platformIds, searchTerm, minDate);
            int totalPages = (int)Math.Ceiling(totalCount / 50.0);

            // API Limit: Offset cannot exceed 5000 (100 pages).
            if (totalPages > 100) totalPages = 100;

            // 3. Fetch Data from IGDB starting from the minDate era
            var games = await _igdbService.GetBrowseGamesAsync(page * 50, platformIds, searchTerm, minDate);
            var platforms = await _igdbService.GetPlatformsAsync();

            // 4. Match IGDB results against local DB
            var igdbIdsInPage = games.Select(g => g.id).ToList();
            var localGames = await _context.Games
                .Where(g => g.IGDBId.HasValue && igdbIdsInPage.Contains(g.IGDBId.Value))
                .ToListAsync();
            var localStatuses = localGames.ToDictionary(g => g.IGDBId!.Value, g => g.Status);
            var localDatesStarted = localGames.ToDictionary(g => g.IGDBId!.Value, g => g.DateStarted);
            var localDatesFinished = localGames.ToDictionary(g => g.IGDBId!.Value, g => g.DateFinished);

            // 5. Prepare View Metadata
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.Platforms = platforms;
            ViewBag.SelectedPlatforms = platformIds ?? new List<int>();
            ViewBag.LocalStatuses = localStatuses;
            ViewBag.LocalDatesStarted = localDatesStarted;
            ViewBag.LocalDatesFinished = localDatesFinished;
            ViewBag.SearchTerm = searchTerm;
            ViewBag.MinDate = minDate;
            ViewBag.LastTimestamp = games.LastOrDefault()?.first_release_date;

            return View(games);
        }
        catch (Exception ex)
        {
            return Content($"The Game Archive is currently unavailable. Error: {ex.Message}");
        }
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
    public async Task<IActionResult> UpdateStatus(int igdbId, string title, string coverUrl, string releaseDate, int status, string? seriesname, string? franchisename)
    {
        var game = await _context.Games.FirstOrDefaultAsync(g => g.IGDBId == igdbId);

        if (game == null)
        {
            game = new Game
            {
                Id = Guid.NewGuid(),
                IGDBId = igdbId,
                Title = title,
                CoverUrl = coverUrl,
                Status = status,
                SeriesName = seriesname,
                FranchiseName = franchisename,
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
                game.DateStarted = null;
                game.DateFinished = null;
                game.SeriesName = null;
                game.FranchiseName = null;
            }
            else
            {
                game.Status = status;
                game.SeriesName = seriesname;
                game.FranchiseName = franchisename;
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
}