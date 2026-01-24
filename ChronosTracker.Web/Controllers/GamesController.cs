using Microsoft.AspNetCore.Mvc;
using ChronosTracker.Infrastructure.Services;

namespace ChronosTracker.Web.Controllers;

public class GamesController : Controller
{
    private readonly ExcelImportService _importService;
    public GamesController(ExcelImportService importService)
    {
        _importService = importService;
    }

    [HttpPost]
    public IActionResult Import()
    { 
    string path = @"C:\Path\To\Your\games.csv"; // Update with your actual file path
        _importService.ImportAndSave(path);
        return RedirectToAction("Index", "Home");
    }
}
