using System;
using System.Collections.Generic;
using System.Linq;
using VRCGroupTools.Data;
using VRCGroupTools.Data.Models;

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
        if (string.IsNullOrWhiteSpace(inviteLog.ActorId) || string.IsNullOrWhiteSpace(inviteLog.TargetId))
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

    public Task MarkInviteAcceptedAsync(string groupId, string targetUserId, DateTime acceptedAtUtc)
        => _db.MarkInviteAcceptedAsync(groupId, targetUserId, acceptedAtUtc);

    public Task ExpireOldInvitesAsync()
        => _db.MarkExpiredInvitesAsync(DateTime.UtcNow - InviteTtl);

    // Note: this now returns the DB entity type your DB already returns
    public Task<List<InviteHistoryEntity>> GetInviteHistoryAsync(string groupId)
        => _db.GetInviteHistoryAsync(groupId);

    public async Task<List<InviteHistoryEntity>> GetPendingInvitesAsync(string groupId)
    {
        await ExpireOldInvitesAsync();
        var all = await _db.GetInviteHistoryAsync(groupId);
        return all.Where(r => r.Outcome == "Pending")
                  .OrderByDescending(r => r.SentAtUtc)
                  .ToList();
    }

    public async Task<List<InviteHistoryEntity>> GetExpiringSoonAsync(string groupId, TimeSpan window)
    {
        await ExpireOldInvitesAsync();
        var all = await _db.GetInviteHistoryAsync(groupId);
        var now = DateTime.UtcNow;

        return all.Where(r => r.Outcome == "Pending")
                  .Where(r => (r.SentAtUtc + InviteTtl - now) <= window)
                  .OrderBy(r => r.SentAtUtc)
                  .ToList();
    }

    public async Task<Dictionary<string, int>> GetRepeatedNonAcceptsAsync(string groupId, int threshold = 3)
    {
        await ExpireOldInvitesAsync();
        var all = await _db.GetInviteHistoryAsync(groupId);

        return all.Where(r => r.Outcome == "Expired")
                  .GroupBy(r => r.TargetUserId)
                  .Where(g => g.Count() >= threshold)
                  .ToDictionary(g => g.Key, g => g.Count());
    }
} 