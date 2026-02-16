namespace WinOptimizePro.Models;

public sealed class CleaningTarget
{
    public required string Name { get; set; }
    public required string Path { get; set; }
    public bool IsSelected { get; set; } = true;
    public long EstimatedBytes { get; set; }
}
