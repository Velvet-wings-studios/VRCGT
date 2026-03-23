namespace VRCGroupTools.Models;

public enum InviteOutcome
{
	Pending,
	Accepted,
	Expired
}

public class InviteHistoryRecord
{
	public string InviteId { get; set; } = "";
	public string GroupId { get; set; } = "";

	public string InviterId { get; set; } = "";
	public string InviterName { get; set; } = "";

	public string TargetUserId { get; set; } = "";
	public string TargetName { get; set; } = "";

	public DateTime SentAtUtc { get; set; }

	public InviteOutcome Outcome { get; set; } = InviteOutcome.Pending;
	public DateTime? AcceptedAtUtc { get; set; }
}