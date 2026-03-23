using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace VRCGroupTools.ViewModels;

public partial class InviterHubViewModel : ObservableObject
{
    [ObservableProperty]
    private string _selectedMode = "GroupInviter";

    [ObservableProperty]
    private InviteToGroupViewModel? _inviteToGroupVM;

    [ObservableProperty]
    private InstanceInviterViewModel? _instanceInviterVM;

    [ObservableProperty]
    private FriendInviterViewModel? _friendInviterVM;

    [ObservableProperty]
    private GameLogViewModel? _gameLogVM;

    [ObservableProperty]
    private InviteStatsViewModel? _inviteStatsVM;

    [ObservableProperty]
    private PendingInvitesViewModel? _pendingInvitesVM;

    public InviterHubViewModel(
     InviteToGroupViewModel inviteToGroupVM,
     InstanceInviterViewModel instanceInviterVM,
     FriendInviterViewModel friendInviterVM,
     GameLogViewModel gameLogVM,
     InviteStatsViewModel inviteStatsVM,
     PendingInvitesViewModel pendingInvitesVM)
    {
        InviteToGroupVM = inviteToGroupVM;
        InstanceInviterVM = instanceInviterVM;
        FriendInviterVM = friendInviterVM;
        GameLogVM = gameLogVM;
        InviteStatsVM = inviteStatsVM;
        PendingInvitesVM = pendingInvitesVM;
    }

    [RelayCommand]
    private void SelectMode(string mode)
    {
        SelectedMode = mode;
    }
}
