namespace WinOptimizePro.Models;

public sealed class ScanFinding
{
    public required string FilePath { get; set; }
    public required string SignatureName { get; set; }
    public required string RiskLevel { get; set; }
}
