using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Windows.Threading;
using VRCGroupTools.Services;
using VRCGroupTools.Views;

namespace VRCGroupTools.ViewModels;

public partial class PendingInviteRow : ObservableObject
{
    public string TargetUserId { get; set; } = "";
    public string TargetName { get; set; } = "Unknown";
    public string InviterName { get; set; } = "Unknown";
    public string? TargetAvatarUrl { get; set; }

    public DateTime SentAtUtc { get; set; }
    public DateTime ExpiresAtUtc { get; set; }

    public TimeSpan PendingFor => DateTime.UtcNow - SentAtUtc;
    public TimeSpan ExpiresIn => ExpiresAtUtc - DateTime.UtcNow;

    public bool IsExpired => ExpiresIn <= TimeSpan.Zero;
    public bool IsExpiringSoon => !IsExpired && ExpiresIn <= TimeSpan.FromHours(24);

    public void Tick()
    {
        OnPropertyChanged(nameof(PendingFor));
        OnPropertyChanged(nameof(ExpiresIn));
        OnPropertyChanged(nameof(IsExpired));
        OnPropertyChanged(nameof(IsExpiringSoon));
    }
}

public partial class PendingInvitesViewModel : ObservableObject
{
    private readonly InviteHistoryService _history;
    private readonly IVRChatApiService _api;
    private readonly DispatcherTimer _timer;

    private string _groupId = "";

    public ObservableCollection<PendingInviteRow> Pending { get; } = new();

    public PendingInvitesViewModel(InviteHistoryService history, IVRChatApiService api)
    {
        _history = history;
        _api = api;

        _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(30) };
        _timer.Tick += async (_, _) =>
        {
            foreach (var row in Pending) row.Tick();

            // Auto-remove expired from UI
            for (int i = Pending.Count - 1; i >= 0; i--)
            {
                if (Pending[i].IsExpired)
                    Pending.RemoveAt(i);
            }

            // Keep DB clean too
            await _history.ExpireOldInvitesAsync();
        };
        _timer.Start();
    }

    public async Task LoadAsync(string groupId)
    {
        _groupId = groupId;

        await _history.ExpireOldInvitesAsync();

        var pending = await _history.GetPendingInvitesAsync(groupId);

        Pending.Clear();
        foreach (var p in pending)
        {
            Pending.Add(new PendingInviteRow
            {
                TargetUserId = p.TargetUserId,
                TargetName = p.TargetName ?? "Unknown",
                InviterName = p.InviterName ?? "Unknown",
                SentAtUtc = p.SentAtUtc,
                ExpiresAtUtc = p.SentAtUtc.AddDays(7),
                TargetAvatarUrl = null
            });
        }

        _ = HydrateAvatarsAsync();
    }

    private async Task HydrateAvatarsAsync()
    {
        foreach (var row in Pending)
        {
            if (string.IsNullOrWhiteSpace(row.TargetUserId)) continue;

            try
            {
                var user = await _api.GetUserAsync(row.TargetUserId);


                // TODO: map UserDetails avatar property names correctly
                row.TargetAvatarUrl = null;


                row.Tick();
            }
            catch
            {
                // ignore API failures
            }
        }
    }

    [RelayCommand]
    private Task OpenProfileAsync(PendingInviteRow? row)
    {
        if (row == null || string.IsNullOrWhiteSpace(row.TargetUserId))
            return Task.CompletedTask;

        try
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(
                $"https://vrchat.com/home/user/{row.TargetUserId}")
            { UseShellExecute = true });
        }
        catch { }

        return Task.CompletedTask;
    }
}  