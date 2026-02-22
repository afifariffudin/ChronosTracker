using ChronosTracker.Core.Entities;
using ChronosTracker.Infrastructure.Data;
using ChronosTracker.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChronosTracker.Web.Controllers;

public class GamesController : Controller
{
    private readonly ExcelImportService _importService;
    private readonly AppDbContext _context;
    private readonly IGDBService _igdbService;
    public GamesController(ExcelImportService importService, AppDbContext context, IGDBService igdbService)
    {
        _importService = importService;
        _context = context;
        _igdbService = igdbService;
    }

    public IActionResult Index()
    {
        var gamesList = _context.Games.ToList();
        return View(gamesList);
    }

    [HttpPost]
    public IActionResult Import()
    {
        string path = @"C:\Users\Afif\OneDrive\Afif\Desktop\Steam Games.csv"; // Update with your actual file path
        _importService.ImportAndSave(path);
        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> TestIGDB([FromServices] IGDBService igdbService)
    {
        try
        {
            var token = await igdbService.GetAccessTokenAsync();
            if (!string.IsNullOrEmpty(token))
            {
                TempData["IGDBStatus"] = "Success! Token received: " + token.Substring(0, 5) + "...";
            }
            else
            {
                TempData["IGDBStatus"] = "Failed: Received an empty token.";
            }
        }

        catch (Exception ex)
        {
            TempData["IGDBStatus"] = "Error: " + ex.Message;
        }

        return RedirectToAction("Index", "Home");
    }

    public async Task<IActionResult> Browse(int page = 0, List<int> platformIds = null)
    {
        if (platformIds == null || !platformIds.Any())
        {
            // Load user's hardware profile if no platform filter is provided
            var settings = await _context.UserSettings.FirstOrDefaultAsync();
            if (settings != null && !string.IsNullOrEmpty(settings.GlobalPlatformIds))
            {
                platformIds = settings.GlobalPlatformIds
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(int.Parse)
                    .ToList();
            }
        }

        int pageSize = 50;
        int offset = page * pageSize;

        var games = await _igdbService.GetBrowseGamesAsync(page * 50, platformIds);
        var platforms = await _igdbService.GetPlatformsAsync();

        ViewBag.CurrentPage = page;
        ViewBag.Platforms = platforms;
        ViewBag.SelectedPlatformIds = platformIds ?? new List<int>();

        return View(games);
    }

    public async Task<IActionResult> HardwareProfile()
    {
        var platforms = await _igdbService.GetPlatformsAsync();
        var settings = await _context.UserSettings.FirstOrDefaultAsync() ?? new UserSettings();

        //Convert the comma-separated string of platform IDs into a list of integers
        ViewBag.SelectedPlatforms = settings.GlobalPlatformIds
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(int.Parse)
            .ToList();

        return View(platforms);
    }

    // POST: Saves the selection to the database
    [HttpPost]
    public async Task<IActionResult> SaveHardwareProfile(List<int> platformIds)
    {
        var settings = await _context.UserSettings.FirstOrDefaultAsync();

        if (settings == null)
        {
            settings = new UserSettings();
            _context.UserSettings.Add(settings);
        }

        settings.GlobalPlatformIds = string.Join(",", platformIds);
        await _context.SaveChangesAsync();

        return RedirectToAction("Browse");
    }
}
