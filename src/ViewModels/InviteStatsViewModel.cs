using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using VRCGroupTools.Services;

namespace VRCGroupTools.ViewModels;
 
public partial class InviteStatsViewModel : ObservableObject
{
    private readonly InviteHistoryService _history;
    private string? _groupId;

    public ObservableCollection<InviteStatsRow> Stats { get; } = new();

    public InviteStatsViewModel(InviteHistoryService history)
    {
        _history = history;
    }

    public async Task LoadAsync(string groupId)
    {
        _groupId = groupId;
        Stats.Clear();

        var records = await _history.GetInviteHistoryAsync(groupId);

        var grouped = records
            .GroupBy(r => r.InviterId)
            .Select(g =>
            {
                var first = g.First();
                return new InviteStatsRow
                {
                    InviterName = first.InviterName ?? "Unknown",
                    Sent = g.Count(),
                    Accepted = g.Count(x => x.Outcome == "Accepted"),
                    Expired = g.Count(x => x.Outcome == "Expired")
                };
            })
            .OrderByDescending(r => r.Accepted);

        foreach (var row in grouped)
            Stats.Add(row);
    }
}