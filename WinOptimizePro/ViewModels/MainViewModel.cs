using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows;
using WinOptimizePro.Core;
using WinOptimizePro.Models;
using WinOptimizePro.Services;

namespace WinOptimizePro.ViewModels;

public sealed class MainViewModel : ObservableObject
{
    private readonly ISystemAnalysisService analysisService;
    private readonly ICleanerService cleanerService;
    private readonly IBrowserCleanerService browserCleanerService;
    private readonly IStartupOptimizerService startupOptimizerService;
    private readonly IRegistryAnalyzerService registryAnalyzerService;
    private readonly IMalwareScannerService malwareScannerService;
    private readonly IPerformanceBoosterService performanceBoosterService;
    private readonly IPrivacySecurityService privacySecurityService;
    private readonly ISchedulerService schedulerService;
    private readonly IBackupSafetyService backupSafetyService;
    private readonly ILoggerService loggerService;

    private bool isDarkMode = true;
    private int healthScore;
    private double cpuUsage;
    private double ramUsage;
    private double diskUsage;
    private string startupTime = "--";
    private string statusMessage = "Ready.";

    public MainViewModel()
        : this(new SystemAnalysisService(), new CleanerService(), new BrowserCleanerService(),
            new StartupOptimizerService(), new RegistryAnalyzerService(), new MalwareScannerService(),
            new PerformanceBoosterService(), new PrivacySecurityService(), new SchedulerService(),
            new BackupSafetyService(), new FileLoggerService())
    {
    }

    public MainViewModel(
        ISystemAnalysisService analysisService,
        ICleanerService cleanerService,
        IBrowserCleanerService browserCleanerService,
        IStartupOptimizerService startupOptimizerService,
        IRegistryAnalyzerService registryAnalyzerService,
        IMalwareScannerService malwareScannerService,
        IPerformanceBoosterService performanceBoosterService,
        IPrivacySecurityService privacySecurityService,
        ISchedulerService schedulerService,
        IBackupSafetyService backupSafetyService,
        ILoggerService loggerService)
    {
        this.analysisService = analysisService;
        this.cleanerService = cleanerService;
        this.browserCleanerService = browserCleanerService;
        this.startupOptimizerService = startupOptimizerService;
        this.registryAnalyzerService = registryAnalyzerService;
        this.malwareScannerService = malwareScannerService;
        this.performanceBoosterService = performanceBoosterService;
        this.privacySecurityService = privacySecurityService;
        this.schedulerService = schedulerService;
        this.backupSafetyService = backupSafetyService;
        this.loggerService = loggerService;

        AnalyzeCommand = new AsyncRelayCommand(_ => RunCommandAsync(AnalyzeAsync));
        OneClickOptimizeCommand = new AsyncRelayCommand(_ => RunCommandAsync(OneClickOptimizeAsync));
        CleanCommand = new AsyncRelayCommand(_ => RunCommandAsync(CleanAsync));
        BrowserCleanCommand = new AsyncRelayCommand(_ => RunCommandAsync(BrowserCleanAsync));
        ScanMalwareCommand = new AsyncRelayCommand(_ => RunCommandAsync(ScanMalwareAsync));
        ToggleThemeCommand = new RelayCommand(_ => IsDarkMode = !IsDarkMode);
    }

    public ObservableCollection<string> Processes { get; } = new();
    public ObservableCollection<string> Services { get; } = new();
    public ObservableCollection<CleaningTarget> CleaningTargets { get; } = new();
    public ObservableCollection<StartupItem> StartupItems { get; } = new();
    public ObservableCollection<ScanFinding> Findings { get; } = new();
    public ObservableCollection<string> PrivacyRisks { get; } = new();

    public ICommand AnalyzeCommand { get; }
    public ICommand OneClickOptimizeCommand { get; }
    public ICommand CleanCommand { get; }
    public ICommand BrowserCleanCommand { get; }
    public ICommand ScanMalwareCommand { get; }
    public ICommand ToggleThemeCommand { get; }

    public bool IsDarkMode
    {
        get => isDarkMode;
        set => SetProperty(ref isDarkMode, value);
    }

    public int HealthScore
    {
        get => healthScore;
        set => SetProperty(ref healthScore, value);
    }

    public double CpuUsage
    {
        get => cpuUsage;
        set => SetProperty(ref cpuUsage, value);
    }

    public double RamUsage
    {
        get => ramUsage;
        set => SetProperty(ref ramUsage, value);
    }

    public double DiskUsage
    {
        get => diskUsage;
        set => SetProperty(ref diskUsage, value);
    }

    public string StartupTime
    {
        get => startupTime;
        set => SetProperty(ref startupTime, value);
    }

    public string StatusMessage
    {
        get => statusMessage;
        set => SetProperty(ref statusMessage, value);
    }

    private async Task AnalyzeAsync()
    {
        var metrics = await analysisService.AnalyzeAsync();
        CpuUsage = metrics.CpuUsagePercent;
        RamUsage = metrics.RamUsagePercent;
        DiskUsage = metrics.DiskUsagePercent;
        StartupTime = $"{metrics.StartupTime.TotalSeconds:N0}s";
        HealthScore = metrics.HealthScore;

        Processes.Clear();
        foreach (var p in metrics.RunningProcesses) Processes.Add(p);

        Services.Clear();
        foreach (var s in metrics.BackgroundServices) Services.Add(s);

        CleaningTargets.Clear();
        foreach (var target in await cleanerService.GetCleaningTargetsAsync()) CleaningTargets.Add(target);

        StartupItems.Clear();
        foreach (var item in await startupOptimizerService.GetStartupItemsAsync()) StartupItems.Add(item);

        PrivacyRisks.Clear();
        foreach (var risk in await privacySecurityService.AnalyzePrivacyRisksAsync()) PrivacyRisks.Add(risk);

        StatusMessage = "System analysis completed.";
        await loggerService.LogAsync("Analysis completed");
    }

    private async Task CleanAsync()
    {
        await backupSafetyService.BackupConfigurationAsync();
        var bytes = await cleanerService.CleanAsync(CleaningTargets);
        StatusMessage = $"Cleanup completed. Freed {bytes / (1024.0 * 1024.0):N2} MB.";
        await loggerService.LogAsync(StatusMessage);
    }

    private async Task BrowserCleanAsync()
    {
        var bytes = await browserCleanerService.CleanBrowsersAsync(includeCookies: true, includeHistory: true, includeAutofill: false);
        StatusMessage = $"Browser cleanup completed. Freed {bytes / (1024.0 * 1024.0):N2} MB.";
        await loggerService.LogAsync(StatusMessage);
    }

    private async Task ScanMalwareAsync()
    {
        Findings.Clear();
        foreach (var finding in await malwareScannerService.QuickScanAsync()) Findings.Add(finding);

        StatusMessage = Findings.Count == 0 ? "Quick scan complete. No threats found." : $"Quick scan complete. {Findings.Count} suspicious files found.";
        await loggerService.LogAsync(StatusMessage);
    }

    private async Task OneClickOptimizeAsync()
    {
        try
        {
            await backupSafetyService.CreateRestorePointAsync("WinOptimizePro One-Click");
            await AnalyzeAsync();
            await CleanAsync();
            await BrowserCleanAsync();
            await privacySecurityService.ApplyPrivacyFixesAsync();
            await schedulerService.ConfigureWeeklyCleaningAsync();
            var boost = await performanceBoosterService.ApplyBoostAsync(gamingMode: false);
            StatusMessage = $"One-click optimization complete. {boost}";
            await loggerService.LogAsync(StatusMessage);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Optimization failed: {ex.Message}";
            await loggerService.LogAsync(StatusMessage);
            MessageBox.Show(StatusMessage, "WinOptimizePro", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private async Task RunCommandAsync(Func<Task> action)
    {
        try
        {
            await action();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Operation failed: {ex.Message}";
            await loggerService.LogAsync(StatusMessage);
            MessageBox.Show(StatusMessage, "WinOptimizePro", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }
}
