using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using VRCGroupTools.Models;
using VRCGroupTools.Services;
 
namespace VRCGroupTools.ViewModels;

public class InviteStatsRow
{
    public string InviterName { get; set; } = "Unknown";
    public int Sent { get; set; }
    public int Accepted { get; set; }
    public int Expired { get; set; }

    public double AcceptanceRate =>
        Sent == 0 ? 0 : (double)Accepted / Sent;

    public bool HasLowAcceptance =>
        Sent >= 3 && Accepted == 0;
}
