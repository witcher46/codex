# WinOptimizePro

WinOptimizePro is a professional Windows desktop optimization and maintenance application built with **C#**, **.NET**, and **WPF** using a **layered MVVM architecture**.

## Highlights

- System analysis dashboard with CPU/RAM/Disk metrics, startup time, health score, running processes, and services.
- Temp and junk cleaner for `%TEMP%`, `C:\Windows\Temp`, dumps, reports, and recycle-bin placeholder cleanup.
- Browser cleaner support for Chrome, Edge, and Firefox cache locations.
- Startup optimizer that discovers startup entries and classifies impact.
- Safe-mode registry analyzer skeleton with explicit restore point workflow.
- Basic malware scanner using signature pattern matching in temp/downloads.
- Performance booster module with gaming-mode option.
- Privacy and security risk analyzer with a fix workflow.
- Scheduling abstraction for weekly cleaning/startup scan automation.
- Safety-first flow with backup and restore-point service hooks and structured logs.
- Modern dashboard UX with tabbed modules and one-click optimization.

## Folder Structure

```text
/workspace/codex
├── WinOptimizePro.sln
├── README.md
└── WinOptimizePro/
    ├── App.xaml
    ├── App.xaml.cs
    ├── WinOptimizePro.csproj
    ├── Core/
    │   ├── ObservableObject.cs
    │   └── RelayCommand.cs
    ├── Models/
    │   ├── CleaningTarget.cs
    │   ├── ScanFinding.cs
    │   ├── StartupItem.cs
    │   └── SystemMetrics.cs
    ├── Services/
    │   ├── Interfaces.cs
    │   └── WindowsServices.cs
    ├── ViewModels/
    │   └── MainViewModel.cs
    └── Views/
        ├── MainWindow.xaml
        └── MainWindow.xaml.cs
```

## Module-by-Module Explanation

### 1. Core (`Core/`)
- `ObservableObject`: Shared `INotifyPropertyChanged` implementation.
- `RelayCommand`: Generic MVVM command wrapper for async/sync command binding.

### 2. Models (`Models/`)
- `SystemMetrics`: Captures health and utilization snapshot fields.
- `CleaningTarget`: Describes a cleanable item with selection and size estimate.
- `StartupItem`: Represents startup apps + impact and recommendation.
- `ScanFinding`: Represents suspicious detection output and risk.

### 3. Services (`Services/`)
- `Interfaces.cs`: Contracts for all modules (analysis, cleaning, security, scheduler, backup, logging).
- `WindowsServices.cs`: Concrete service implementations.
  - `SystemAnalysisService`: Uses process/service APIs and simulated utilization values.
  - `CleanerService`: Estimates and deletes files from configured junk targets.
  - `BrowserCleanerService`: Cleans major browser cache directories.
  - `StartupOptimizerService`: Reads startup entries from registry run key.
  - `RegistryAnalyzerService`: Safe placeholder for broken-entry detection/fix.
  - `MalwareScannerService`: Signature-style filename scanning in high-risk folders.
  - `PerformanceBoosterService`: Applies and reports optimization mode.
  - `PrivacySecurityService`: Detects and applies privacy hardening tasks.
  - `SchedulerService`: Entry point for Task Scheduler automation.
  - `BackupSafetyService`: Restore-point + backup hooks before destructive tasks.
  - `FileLoggerService`: Daily local log files in `%LocalAppData%`.

### 4. ViewModels (`ViewModels/`)
- `MainViewModel`: Orchestrates the complete workflow.
  - Commands: Analyze, One-Click Optimize, Clean, Browser Clean, Malware Scan, Theme Toggle.
  - Aggregates data from all service modules and drives dashboard state.

### 5. Views (`Views/`)
- `MainWindow.xaml`: Modern dashboard with sidebar actions, status panel, metric cards, and module tabs.
- `MainWindow.xaml.cs`: Injects the `MainViewModel` into the view.

## One-Click Optimization Flow

1. Create restore point.
2. Analyze system.
3. Clean temp/junk targets.
4. Clean browsers.
5. Apply privacy fixes.
6. Configure weekly cleaning schedule.
7. Apply performance boost.
8. Log every stage.

## Build Instructions

```bash
dotnet restore WinOptimizePro.sln
dotnet build WinOptimizePro.sln -c Release
```

> Requires Windows with .NET 8 SDK (or compatible Visual Studio workload) due to WPF target framework `net8.0-windows`.

## Safety Notes

- Registry and startup modification operations are conservative by default.
- Restore-point and backup calls are structured first in optimization workflows.
- Cleaner is best-effort and intentionally resilient to locked/system files.
- This is a **basic** scanner, not a full antivirus engine.
