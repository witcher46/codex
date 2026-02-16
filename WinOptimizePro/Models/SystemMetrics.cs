namespace WinOptimizePro.Models;

public sealed class SystemMetrics
{
    public double CpuUsagePercent { get; set; }
    public double RamUsagePercent { get; set; }
    public double DiskUsagePercent { get; set; }
    public TimeSpan StartupTime { get; set; }
    public int HealthScore { get; set; }
    public IReadOnlyList<string> RunningProcesses { get; set; } = Array.Empty<string>();
    public IReadOnlyList<string> BackgroundServices { get; set; } = Array.Empty<string>();
}
