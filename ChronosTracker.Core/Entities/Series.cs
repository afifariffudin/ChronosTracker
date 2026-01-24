namespace ChronosTracker.Core.Entities
{
    public class Series
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public List<Game> Games { get; set; } = new();

    }
}
