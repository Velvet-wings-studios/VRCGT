using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VRCGroupTools.Data;
using VRCGroupTools.Models;

namespace VRCGroupTools.Services;

public class InviteHistoryService
{
    private readonly IDatabaseService _db;
    private static readonly TimeSpan InviteTtl = TimeSpan.FromDays(7);

    public InviteHistoryService(IDatabaseService db)
    {
        _db = db;
    }

    public Task RecordInviteSentAsync(string groupId, AuditLogEntry inviteLog)
    {
        if (string.IsNullOrWhiteSpace(inviteLog.ActorId) ||
            string.IsNullOrWhiteSpace(inviteLog.TargetId))
            return Task.CompletedTask;

        return _db.UpsertInviteHistoryAsync(
            groupId,
            inviteLog.Id,
            inviteLog.ActorId!,
            inviteLog.ActorName,
            inviteLog.TargetId!,
            inviteLog.TargetName,
            inviteLog.CreatedAt);
    }

    public Task MarkInviteAcceptedAsync(
        string groupId,
        string targetUserId,
        DateTime acceptedAtUtc)
        => _db.MarkInviteAcceptedAsync(groupId, targetUserId, acceptedAtUtc);

    public Task ExpireOldInvitesAsync()
        => _db.MarkExpiredInvitesAsync(DateTime.UtcNow - InviteTtl);

    public Task<List<InviteHistoryRecord>> GetInviteHistoryAsync(string groupId)
        => _db.GetInviteHistoryAsync(groupId);

    public async Task<List<InviteHistoryRecord>> GetPendingInvitesAsync(string groupId)
    {
        await ExpireOldInvitesAsync();

        var all = await _db.GetInviteHistoryAsync(groupId);

        return all
            .Where(r => r.Outcome == InviteOutcome.Pending)
            .OrderByDescending(r => r.SentAtUtc)
            .ToList();
    }

    public async Task<List<InviteHistoryRecord>> GetExpiringSoonAsync(
        string groupId,
        TimeSpan window)
    {
        await ExpireOldInvitesAsync();

        var all = await _db.GetInviteHistoryAsync(groupId);
        var now = DateTime.UtcNow;

        return all
            .Where(r => r.Outcome == InviteOutcome.Pending)
            .Where(r => (r.SentAtUtc + InviteTtl - now) <= window)
            .OrderBy(r => r.SentAtUtc)
            .ToList();
    }

    public async Task<Dictionary<string, int>> GetRepeatedNonAcceptsAsync(
        string groupId,
        int threshold = 3)
    {
        await ExpireOldInvitesAsync();

        var all = await _db.GetInviteHistoryAsync(groupId);

        return all
            .Where(r => r.Outcome == InviteOutcome.Expired)
            .GroupBy(r => r.TargetUserId)
            .Where(g => g.Count() >= threshold)
            .ToDictionary(g => g.Key, g => g.Count());
    }
}