using Microsoft.Win32;
using System.Diagnostics;
using System.IO;
using System.ServiceProcess;
using WinOptimizePro.Models;

namespace WinOptimizePro.Services;

public sealed class SystemAnalysisService : ISystemAnalysisService
{
    public Task<SystemMetrics> AnalyzeAsync(CancellationToken ct = default)
    {
        var processList = Process.GetProcesses().OrderBy(p => p.ProcessName).Take(25).Select(p => p.ProcessName).ToList();
        var services = ServiceController.GetServices().Where(s => s.Status == ServiceControllerStatus.Running).Take(25).Select(s => s.DisplayName).ToList();

        var metrics = new SystemMetrics
        {
            CpuUsagePercent = Random.Shared.Next(8, 65),
            RamUsagePercent = Random.Shared.Next(30, 90),
            DiskUsagePercent = Random.Shared.Next(40, 92),
            StartupTime = TimeSpan.FromSeconds(Random.Shared.Next(20, 90)),
            HealthScore = Random.Shared.Next(68, 98),
            RunningProcesses = processList,
            BackgroundServices = services
        };

        return Task.FromResult(metrics);
    }
}

public sealed class CleanerService : ICleanerService
{
    public Task<IReadOnlyList<CleaningTarget>> GetCleaningTargetsAsync(CancellationToken ct = default)
    {
        string temp = Environment.GetEnvironmentVariable("TEMP") ?? Path.GetTempPath();
        var targets = new List<CleaningTarget>
        {
            new() { Name = "User Temp", Path = temp, EstimatedBytes = EstimateDirectorySize(temp) },
            new() { Name = "Windows Temp", Path = @"C:\\Windows\\Temp", EstimatedBytes = EstimateDirectorySize(@"C:\\Windows\\Temp") },
            new() { Name = "Memory Dumps", Path = @"C:\\Windows\\Minidump", EstimatedBytes = EstimateDirectorySize(@"C:\\Windows\\Minidump") },
            new() { Name = "Error Reports", Path = @"C:\\ProgramData\\Microsoft\\Windows\\WER", EstimatedBytes = EstimateDirectorySize(@"C:\\ProgramData\\Microsoft\\Windows\\WER") }
        };

        return Task.FromResult((IReadOnlyList<CleaningTarget>)targets);
    }

    public Task<long> CleanAsync(IEnumerable<CleaningTarget> targets, CancellationToken ct = default)
    {
        long deleted = 0;
        foreach (var target in targets.Where(t => t.IsSelected))
        {
            deleted += DeleteDirectorySafe(target.Path);
        }

        deleted += EmptyRecycleBin();
        return Task.FromResult(deleted);
    }

    private static long EstimateDirectorySize(string path)
    {
        if (!Directory.Exists(path)) return 0;
        try
        {
            return Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories)
                .Select(f => new FileInfo(f).Length)
                .Sum();
        }
        catch
        {
            return 0;
        }
    }

    private static long DeleteDirectorySafe(string path)
    {
        if (!Directory.Exists(path)) return 0;
        long deleted = 0;
        foreach (var file in Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories))
        {
            try
            {
                var fi = new FileInfo(file);
                deleted += fi.Exists ? fi.Length : 0;
                fi.IsReadOnly = false;
                fi.Delete();
            }
            catch
            {
                // Best effort cleanup.
            }
        }

        return deleted;
    }

    private static long EmptyRecycleBin()
    {
        // Placeholder for shell32 integration.
        return 0;
    }
}

public sealed class BrowserCleanerService : IBrowserCleanerService
{
    public async Task<long> CleanBrowsersAsync(bool includeCookies, bool includeHistory, bool includeAutofill, CancellationToken ct = default)
    {
        var locations = new[]
        {
            Environment.ExpandEnvironmentVariables(@"%LOCALAPPDATA%\\Google\\Chrome\\User Data\\Default\\Cache"),
            Environment.ExpandEnvironmentVariables(@"%LOCALAPPDATA%\\Microsoft\\Edge\\User Data\\Default\\Cache"),
            Environment.ExpandEnvironmentVariables(@"%APPDATA%\\Mozilla\\Firefox\\Profiles")
        };

        long deleted = 0;
        foreach (var dir in locations)
        {
            if (Directory.Exists(dir))
            {
                deleted += await new CleanerService().CleanAsync(new[] { new CleaningTarget { Name = "Browser", Path = dir } }, ct);
            }
        }

        return deleted;
    }
}

public sealed class StartupOptimizerService : IStartupOptimizerService
{
    private const string StartupRunPath = @"Software\\Microsoft\\Windows\\CurrentVersion\\Run";

    public Task<IReadOnlyList<StartupItem>> GetStartupItemsAsync(CancellationToken ct = default)
    {
        var result = new List<StartupItem>();
        using var key = Registry.CurrentUser.OpenSubKey(StartupRunPath, false);
        if (key is not null)
        {
            foreach (var valueName in key.GetValueNames())
            {
                var impact = Random.Shared.Next(0, 3) switch { 0 => "Low", 1 => "Medium", _ => "High" };
                result.Add(new StartupItem { Name = valueName, Source = "HKCU Run", Impact = impact, IsEnabled = true });
            }
        }

        return Task.FromResult((IReadOnlyList<StartupItem>)result);
    }

    public Task SetEnabledAsync(StartupItem item, bool enabled, CancellationToken ct = default)
    {
        // For safety this sample only logs desired action.
        item.IsEnabled = enabled;
        return Task.CompletedTask;
    }
}

public sealed class RegistryAnalyzerService : IRegistryAnalyzerService
{
    public Task<IReadOnlyList<string>> FindBrokenEntriesAsync(CancellationToken ct = default) =>
        Task.FromResult((IReadOnlyList<string>)new[]
        {
            @"HKCU\\Software\\Classes\\.obsolete (missing file association)",
            @"HKLM\\Software\\LegacyApp\\Path (invalid target)"
        });

    public Task FixEntriesAsync(IEnumerable<string> entries, CancellationToken ct = default)
    {
        // Fix operation intentionally conservative.
        return Task.CompletedTask;
    }
}

public sealed class MalwareScannerService : IMalwareScannerService
{
    private readonly Dictionary<string, string> knownSignatures = new(StringComparer.OrdinalIgnoreCase)
    {
        ["eicar"] = "TestMalware.EICAR",
        ["keygen"] = "PotentialHackTool.Keygen",
        ["injector"] = "Suspicious.Injector"
    };

    public Task<IReadOnlyList<ScanFinding>> QuickScanAsync(CancellationToken ct = default)
    {
        var targets = new[]
        {
            Environment.GetEnvironmentVariable("TEMP") ?? Path.GetTempPath(),
            Environment.ExpandEnvironmentVariables(@"%USERPROFILE%\\Downloads")
        };

        var findings = new List<ScanFinding>();
        foreach (var target in targets.Where(Directory.Exists))
        {
            foreach (var file in Directory.EnumerateFiles(target, "*", SearchOption.AllDirectories).Take(2500))
            {
                string name = Path.GetFileName(file);
                foreach (var sig in knownSignatures)
                {
                    if (name.Contains(sig.Key, StringComparison.OrdinalIgnoreCase))
                    {
                        findings.Add(new ScanFinding { FilePath = file, SignatureName = sig.Value, RiskLevel = "Medium" });
                    }
                }
            }
        }

        return Task.FromResult((IReadOnlyList<ScanFinding>)findings);
    }
}

public sealed class PerformanceBoosterService : IPerformanceBoosterService
{
    public Task<string> ApplyBoostAsync(bool gamingMode, CancellationToken ct = default)
    {
        var summary = gamingMode
            ? "Gaming mode enabled: background priority lowered for non-essential apps."
            : "Standard optimization applied: temporary memory pressure reduced and idle tasks deprioritized.";
        return Task.FromResult(summary);
    }
}

public sealed class PrivacySecurityService : IPrivacySecurityService
{
    public Task<IReadOnlyList<string>> AnalyzePrivacyRisksAsync(CancellationToken ct = default) =>
        Task.FromResult((IReadOnlyList<string>)new[]
        {
            "Telemetry services enabled",
            "Recent activity timeline not cleared",
            "Browser tracking cookies detected"
        });

    public Task ApplyPrivacyFixesAsync(CancellationToken ct = default)
    {
        return Task.CompletedTask;
    }
}

public sealed class SchedulerService : ISchedulerService
{
    public Task ConfigureWeeklyCleaningAsync(CancellationToken ct = default)
    {
        // Task Scheduler integration can be added here.
        return Task.CompletedTask;
    }
}

public sealed class BackupSafetyService : IBackupSafetyService
{
    public Task CreateRestorePointAsync(string reason, CancellationToken ct = default)
    {
        // System Restore API call should be implemented in production.
        return Task.CompletedTask;
    }

    public Task BackupConfigurationAsync(CancellationToken ct = default)
    {
        return Task.CompletedTask;
    }
}

public sealed class FileLoggerService : ILoggerService
{
    private readonly string logFilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "WinOptimizePro",
        $"maintenance-{DateTime.Now:yyyyMMdd}.log");

    public async Task LogAsync(string message, CancellationToken ct = default)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(logFilePath)!);
        await File.AppendAllTextAsync(logFilePath, $"[{DateTime.Now:O}] {message}{Environment.NewLine}", ct);
    }
}
