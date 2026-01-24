using ChronosTracker.Core.Entities;
using ChronosTracker.Infrastructure.Data;
using CsvHelper;
using System.Globalization;


namespace ChronosTracker.Infrastructure.Services;

public class ExcelImportService
{
    private readonly AppDbContext _context;
    
    public ExcelImportService(AppDbContext context)
    {
        _context = context;
    }

    public void ImportAndSave(string filePath)
    {
        var games = MapExcelToGames(filePath);
        _context.Games.AddRange(games);
        _context.SaveChanges();
    }

    public static List<Game> MapExcelToGames(string filePath)
    {
        var games = new List<Game>();
       
        using var reader = new StreamReader(filePath);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

        csv.Read();
        csv.ReadHeader();

        while (csv.Read())
        {
            var game = new Game
            {
                Id = Guid.NewGuid(),
                Name = csv.GetField<string>("Name") ?? "Unknown",
                SteamAppId = csv.GetField("AppID"),
                ReleaseDate = DateTime.TryParse(csv.GetField("Release Date"), out var rDate) ? rDate : null,
                SteamRating = double.TryParse(csv.GetField("Steam Rating"), out var rating) ? rating : null,
                Genre = csv.GetField("Genres"),
                Developer = csv.GetField("Developers"),
                Publisher = csv.GetField("Publishers"),
                PlayStatus = csv.GetField("Play") ?? "N",
                DateStarted = DateTime.TryParse(csv.GetField("Start"), out var sDate) ? sDate : null,
                DateFinished = DateTime.TryParse(csv.GetField("End"), out var eDate) ? eDate : null,
            };

            games.Add(game);

        }

        return games;
    }
}
