using WinOptimizePro.Models;

namespace WinOptimizePro.Services;

public interface ISystemAnalysisService
{
    Task<SystemMetrics> AnalyzeAsync(CancellationToken ct = default);
}

public interface ICleanerService
{
    Task<IReadOnlyList<CleaningTarget>> GetCleaningTargetsAsync(CancellationToken ct = default);
    Task<long> CleanAsync(IEnumerable<CleaningTarget> targets, CancellationToken ct = default);
}

public interface IBrowserCleanerService
{
    Task<long> CleanBrowsersAsync(bool includeCookies, bool includeHistory, bool includeAutofill, CancellationToken ct = default);
}

public interface IStartupOptimizerService
{
    Task<IReadOnlyList<StartupItem>> GetStartupItemsAsync(CancellationToken ct = default);
    Task SetEnabledAsync(StartupItem item, bool enabled, CancellationToken ct = default);
}

public interface IRegistryAnalyzerService
{
    Task<IReadOnlyList<string>> FindBrokenEntriesAsync(CancellationToken ct = default);
    Task FixEntriesAsync(IEnumerable<string> entries, CancellationToken ct = default);
}

public interface IMalwareScannerService
{
    Task<IReadOnlyList<ScanFinding>> QuickScanAsync(CancellationToken ct = default);
}

public interface IPerformanceBoosterService
{
    Task<string> ApplyBoostAsync(bool gamingMode, CancellationToken ct = default);
}

public interface IPrivacySecurityService
{
    Task<IReadOnlyList<string>> AnalyzePrivacyRisksAsync(CancellationToken ct = default);
    Task ApplyPrivacyFixesAsync(CancellationToken ct = default);
}

public interface ISchedulerService
{
    Task ConfigureWeeklyCleaningAsync(CancellationToken ct = default);
}

public interface IBackupSafetyService
{
    Task CreateRestorePointAsync(string reason, CancellationToken ct = default);
    Task BackupConfigurationAsync(CancellationToken ct = default);
}

public interface ILoggerService
{
    Task LogAsync(string message, CancellationToken ct = default);
}
