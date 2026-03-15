using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MaterialDesignThemes.Wpf;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using VRCGroupTools.Services;

namespace VRCGroupTools.ViewModels;

public partial class AppSettingsViewModel : ObservableObject
{
    private readonly ISettingsService _settingsService;
    private readonly IUpdateService _updateService;

    public ObservableCollection<string> Themes { get; } = new(new[] { "Dark", "Light" });
    public ObservableCollection<string> Colors { get; } = new(new[] { "DeepPurple", "Indigo", "Blue", "Teal", "Green", "Amber", "Orange", "DeepOrange", "Red", "Pink", "Purple", "BlueGrey", "Grey" });
    public ObservableCollection<string> Regions { get; } = new(new[] { "US West", "US East", "Europe", "Japan" });
    public ObservableCollection<string> Languages { get; } = new(new[] { "EN", "ES", "FR", "DE", "IT", "PT", "RU", "JA", "ZH", "KO" });
    public ObservableCollection<string> UpdateActions { get; } = new(new[] { "Off", "Notify", "Auto Download" });
    public ObservableCollection<string> TimeZones { get; }
        = new(TimeZoneInfo.GetSystemTimeZones().Select(tz => tz.Id));

    [ObservableProperty] private string _selectedTheme = "Dark";
    [ObservableProperty] private string _selectedPrimaryColor = "DeepPurple";
    [ObservableProperty] private string _selectedSecondaryColor = "Teal";
    [ObservableProperty] private string _selectedTimeZoneId = TimeZoneInfo.Local.Id;
    [ObservableProperty] private string _defaultRegion = "US West";
    [ObservableProperty] private bool _startWithWindows;
    [ObservableProperty] private string _status = string.Empty;

    // Language & Translation
    [ObservableProperty] private string _selectedLanguage = "EN";
    [ObservableProperty] private bool _autoTranslateEnabled;

    // UI Settings
    [ObservableProperty] private double _uiZoom = 1.0;
    [ObservableProperty] private bool _showTrayNotificationDot = true;

    // Application Behavior
    [ObservableProperty] private bool _startMinimized;
    [ObservableProperty] private bool _minimizeToTray = true;
    [ObservableProperty] private bool _showConsoleWindow = false;

    // Update Settings
    [ObservableProperty] private string _updateAction = "Notify";

    // App Info (Read-only) — FIXED to use your fork + your version
    public string AppVersion { get; }
    public string RepositoryUrl { get; }
    public string SupportUrl { get; }

    public string LegalNotice { get; } =
        "VRCGT is an assistant tool for VRChat that provides information and manages groups. " +
        "This application makes use of the unofficial VRChat API and is not endorsed by VRChat Inc. " +
        "VRCGT does not reflect the views or opinions of VRChat or anyone officially involved in " +
        "producing or managing VRChat properties. VRChat and all associated properties are trademarks " +
        "or registered trademarks of VRChat Inc. Use of this tool is at your own risk. " +
        "The developers of VRCGT are not responsible for any account actions taken by VRChat Inc.";

    public AppSettingsViewModel()
    {
        _settingsService = App.Services.GetRequiredService<ISettingsService>();
        _updateService   = App.Services.GetRequiredService<IUpdateService>();

        // ✅ Use your hard-coded app version so it matches updater logic
        AppVersion = App.Version;

        // ✅ Point to your fork automatically
        RepositoryUrl = $"https://github.com/{App.GitHubRepo}";

        // ✅ Safe default support link: Issues page on your fork
        SupportUrl = $"https://github.com/{App.GitHubRepo}/issues";

        Load();
    }

    private void Load()
    {
        var settings = _settingsService.Settings;
        SelectedTheme = string.IsNullOrWhiteSpace(settings.Theme) ? "Dark" : settings.Theme;
        SelectedPrimaryColor = string.IsNullOrWhiteSpace(settings.PrimaryColor) ? "DeepPurple" : settings.PrimaryColor;
        SelectedSecondaryColor = string.IsNullOrWhiteSpace(settings.SecondaryColor) ? "Teal" : settings.SecondaryColor;
        SelectedTimeZoneId = settings.TimeZoneId;
        DefaultRegion = string.IsNullOrWhiteSpace(settings.DefaultRegion) ? "US West" : settings.DefaultRegion;
        StartWithWindows = settings.StartWithWindows;

        // Language & Translation
        SelectedLanguage = string.IsNullOrWhiteSpace(settings.Language) ? "EN" : settings.Language;
        AutoTranslateEnabled = settings.AutoTranslateEnabled;

        // UI Settings
        UiZoom = settings.UIZoom;
        ShowTrayNotificationDot = settings.ShowTrayNotificationDot;

        // Application Behavior
        StartMinimized = settings.StartMinimized;
        MinimizeToTray = settings.MinimizeToTray;
        ShowConsoleWindow = settings.ShowConsoleWindow;

        // Update Settings
        UpdateAction = string.IsNullOrWhiteSpace(settings.UpdateAction) ? "Notify" : settings.UpdateAction;

        ApplyTheme(SelectedTheme, SelectedPrimaryColor, SelectedSecondaryColor);
    }

    [RelayCommand]
    private void Save()
    {
        var settings = _settingsService.Settings;
        settings.Theme = SelectedTheme;
        settings.PrimaryColor = SelectedPrimaryColor;
        settings.SecondaryColor = SelectedSecondaryColor;
        settings.TimeZoneId = SelectedTimeZoneId;
        settings.DefaultRegion = DefaultRegion;
        settings.StartWithWindows = StartWithWindows;

        // Language & Translation
        settings.Language = SelectedLanguage;
        settings.AutoTranslateEnabled = AutoTranslateEnabled;

        // UI Settings
        settings.UIZoom = UiZoom;
        settings.ShowTrayNotificationDot = ShowTrayNotificationDot;

        // Application Behavior
        settings.StartMinimized = StartMinimized;
        settings.MinimizeToTray = MinimizeToTray;
        settings.ShowConsoleWindow = ShowConsoleWindow;

        // Update Settings
        settings.UpdateAction = UpdateAction;

        _settingsService.Save();
        ApplyTheme(SelectedTheme, SelectedPrimaryColor, SelectedSecondaryColor);
        SetStartupWithWindows(StartWithWindows);
        Status = "✅ Settings saved successfully!";
    }

    // ✅ This is the command your XAML Button is binding to:
    // Command="{Binding CheckForUpdatesCommand}"
    [RelayCommand]
    private async System.Threading.Tasks.Task CheckForUpdatesAsync()
    {
        try
        {
            Status = "🔍 Checking for updates...";

            var hasUpdate = await _updateService.CheckForUpdateAsync();

            if (!hasUpdate)
            {
                Status = "✅ You are on the latest version.";
                MessageBox.Show("You are already running the latest version.",
                    "No Updates Available",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            Status = $"⬆️ Update available: v{_updateService.LatestVersion}";

            var result = MessageBox.Show(
                $"Update available: v{_updateService.LatestVersion}\n\nDownload and install now?",
                "Update Available",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                Status = "⬇️ Downloading update...";
                await _updateService.DownloadAndInstallUpdateAsync();
            }
            else
            {
                Status = "⏸ Update postponed.";
            }
        }
        catch (Exception ex)
        {
            Status = $"❌ Update check failed: {ex.Message}";
            MessageBox.Show($"Update check failed:\n{ex.Message}",
                "Update Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private static void ApplyTheme(string themeName, string primary, string secondary)
    {
        var theme = Application.Current.Resources.MergedDictionaries
            .OfType<BundledTheme>()
            .FirstOrDefault();

        if (theme == null) return;

        theme.BaseTheme = themeName.Equals("Light", StringComparison.OrdinalIgnoreCase)
            ? BaseTheme.Light
            : BaseTheme.Dark;

        // Primary/Secondary color dynamic mapping intentionally left as your original comment.
    }

    private static void SetStartupWithWindows(bool enable)
    {
        try
        {
            const string appName = "VRCGroupTools";
            var startupKey = Registry.CurrentUser.OpenSubKey(
                "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

            if (startupKey == null) return;

            if (enable)
            {
                var exePath = Environment.ProcessPath ?? AppContext.BaseDirectory;
                if (exePath.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
                    exePath = exePath.Replace(".dll", ".exe");

                if (System.IO.Directory.Exists(exePath))
                    exePath = System.IO.Path.Combine(exePath, "VRCGroupTools.exe");

                startupKey.SetValue(appName, $"\"{exePath}\"");
            }
            else
            {
                startupKey.DeleteValue(appName, false);
            }

            startupKey.Close();
        }
        catch
        {
            // keep your original behavior: fail silently to avoid breaking settings UI
        }
    }
}