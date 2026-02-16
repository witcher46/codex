namespace WinOptimizePro.Models;

public sealed class StartupItem
{
    public required string Name { get; set; }
    public required string Source { get; set; }
    public required string Impact { get; set; }
    public bool IsEnabled { get; set; }
    public string Recommendation => Impact == "High" && IsEnabled
        ? "Disable to improve boot speed"
        : "No action required";
}
