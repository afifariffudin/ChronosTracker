namespace ChronosTracker.Core.Entities;

public class UserSettings
{
    public int Id { get; set; }

    // We store the platform IDs as a comma-separated string (e.g., "48,49,167")
    public string GlobalPlatformIds { get; set; } = string.Empty;
}