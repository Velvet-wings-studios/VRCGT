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

    public async Task LoadAsync(string groupId)
    {
        // Only load the new tabs. The other VMs either don't have LoadAsync
        // or their LoadAsync is not public.
        if (InviteStatsVM != null)
            await InviteStatsVM.LoadAsync(groupId);

        if (PendingInvitesVM != null)
            await PendingInvitesVM.LoadAsync(groupId);
    }

    [RelayCommand]
    private void SelectMode(string mode)
    {
        SelectedMode = mode;
    }
}
