using System;
using VRCGroupTools.Services; // or wherever AuditLogEntry lives in your solution


namespace VRCGroupTools.Helpers;

public record NormalizedAuditUsers(
    string PrimaryName,
    string PrimaryId,
    string? SecondaryName,
    string? SecondaryId,
    string? InitiatorName,
    string? InitiatorId
);

public static class AuditLogNormalizer
{
    public static NormalizedAuditUsers Normalize(AuditLogEntry log)
    {
        var actorName  = Clean(log.ActorName);
        var actorId    = Clean(log.ActorId);
        var targetName = Clean(log.TargetName);
        var targetId   = Clean(log.TargetId);

        return log.EventType switch
        {
            // Join events: actor = joiner (target often null)
            "group.user.join" => new NormalizedAuditUsers(
                PrimaryName: actorName ?? "User",
                PrimaryId: actorId ?? "",
                SecondaryName: null,
                SecondaryId: null,
                InitiatorName: actorName,
                InitiatorId: actorId
            ),

            // Invite events: actor = inviter, target = invitee
            "group.invite.create" or "group.user.invite" => new NormalizedAuditUsers(
                PrimaryName: targetName ?? "User",
                PrimaryId: targetId ?? "",
                SecondaryName: null,
                SecondaryId: null,
                InitiatorName: actorName,
                InitiatorId: actorId
            ),

            // Default: prefer target (acted-on), otherwise actor
            _ => new NormalizedAuditUsers(
                PrimaryName: targetName ?? actorName ?? "User",
                PrimaryId: targetId ?? actorId ?? "",
                SecondaryName: null,
                SecondaryId: null,
                InitiatorName: actorName,
                InitiatorId: actorId
            )
        };
    }

    private static string? Clean(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}