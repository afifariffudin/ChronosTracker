using Microsoft.AspNetCore.Mvc;
using ChronosTracker.Infrastructure.Services;
using ChronosTracker.Infrastructure.Data;
using ChronosTracker.Core.Entities;

namespace ChronosTracker.Web.Controllers;

public class GamesController : Controller
{
    private readonly ExcelImportService _importService;
    private readonly AppDbContext _context;
    public GamesController(ExcelImportService importService, AppDbContext context)
    {
        _importService = importService;
        _context = context;
    }

    public IActionResult Index()
    {
        var gamesList = _context.Games.ToList();
        return View(gamesList);
    }

    [HttpPost]
    public IActionResult Import()
    { 
    string path = @"C:\Path\To\Your\games.csv"; // Update with your actual file path
        _importService.ImportAndSave(path);
        return RedirectToAction("Index");
    }


}
