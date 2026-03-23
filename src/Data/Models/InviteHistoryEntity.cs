using System;

namespace VRCGroupTools.Data.Models;

public class InviteHistoryEntity
{
    public string InviteId { get; set; } = "";      // invite log id (or synthetic)
    public string GroupId { get; set; } = "";

    public string InviterId { get; set; } = "";
    public string InviterName { get; set; } = "";
     
    public string TargetUserId { get; set; } = "";
    public string TargetName { get; set; } = "";

    public DateTime SentAtUtc { get; set; }
    public string Outcome { get; set; } = "Pending"; // Pending|Accepted|Expired
    public DateTime? AcceptedAtUtc { get; set; }

    public DateTime UpdatedAtUtc { get; set; }
}