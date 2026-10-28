using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace JobAppHR.Hubs
{
    public class ScreeningHub : Hub
    {
        // Track which rows are locked by which connection
        // Key: "groupName::applicationCode", Value: { ConnectionId, UserId, UserName, LockedAt }
        private static readonly ConcurrentDictionary<string, RowLock> _rowLocks = new();

        // Track which group each connection belongs to (for cleanup on disconnect)
        private static readonly ConcurrentDictionary<string, string> _connectionGroups = new();

        // Track active users per group: Key: groupName, Value: dict of connectionId -> { UserId, UserName }
        private static readonly ConcurrentDictionary<string, ConcurrentDictionary<string, ActiveUser>> _activeUsers = new();

        public async Task JoinIntakeGroup(string intakeCode, string stage, string status)
        {
            var groupName = BuildGroupName(intakeCode, stage, status);
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            _connectionGroups[Context.ConnectionId] = groupName;

            var userId = GetUserId();
            var userName = GetUserName();

            // Add to active users
            var groupUsers = _activeUsers.GetOrAdd(groupName, _ => new ConcurrentDictionary<string, ActiveUser>());
            groupUsers[Context.ConnectionId] = new ActiveUser { UserId = userId, UserName = userName };

            // Broadcast updated user list to the group
            var userList = groupUsers.Values
                .GroupBy(u => u.UserId)
                .Select(g => new { userId = g.Key, userName = g.First().UserName })
                .ToList();

            await Clients.Group(groupName).SendAsync("ReceiveActiveUsers", userList);

            // Send current row locks to the joining user
            var currentLocks = _rowLocks
                .Where(kv => kv.Key.StartsWith(groupName + "::"))
                .Select(kv => new
                {
                    applicationCode = kv.Key.Split("::")[1],
                    userId = kv.Value.UserId,
                    userName = kv.Value.UserName
                })
                .ToList();

            await Clients.Caller.SendAsync("ReceiveCurrentLocks", currentLocks);
        }

        public async Task LeaveIntakeGroup(string intakeCode, string stage, string status)
        {
            var groupName = BuildGroupName(intakeCode, stage, status);
            await CleanupConnection(Context.ConnectionId, groupName);
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        }

        public async Task UpdateApplicantStatus(string intakeCode, string stage, string status,
            string applicationCode, string newStatus, string newRemarks, string userId, string userName)
        {
            var groupName = BuildGroupName(intakeCode, stage, status);

            // Broadcast to all OTHER clients in the group
            await Clients.OthersInGroup(groupName).SendAsync("ReceiveStatusUpdate",
                applicationCode, newStatus, newRemarks, userId, userName);
        }

        public async Task LockRow(string intakeCode, string stage, string status,
            string applicationCode)
        {
            var groupName = BuildGroupName(intakeCode, stage, status);
            var lockKey = groupName + "::" + applicationCode;
            var userId = GetUserId();
            var userName = GetUserName();

            var lockInfo = new RowLock
            {
                ConnectionId = Context.ConnectionId,
                UserId = userId,
                UserName = userName,
                LockedAt = DateTime.UtcNow
            };

            // Only lock if not already locked by someone else
            var existing = _rowLocks.GetOrAdd(lockKey, lockInfo);
            if (existing.ConnectionId != Context.ConnectionId)
            {
                // Already locked by another user — check if it's stale (>30s)
                if ((DateTime.UtcNow - existing.LockedAt).TotalSeconds > 30)
                {
                    // Force-override stale lock
                    _rowLocks[lockKey] = lockInfo;
                }
                else
                {
                    // Still actively locked — notify caller
                    await Clients.Caller.SendAsync("ReceiveLockRejected", applicationCode, existing.UserId, existing.UserName);
                    return;
                }
            }

            await Clients.OthersInGroup(groupName).SendAsync("ReceiveRowLock",
                applicationCode, userId, userName);
        }

        public async Task UnlockRow(string intakeCode, string stage, string status,
            string applicationCode)
        {
            var groupName = BuildGroupName(intakeCode, stage, status);
            var lockKey = groupName + "::" + applicationCode;

            // Only the lock owner can unlock
            if (_rowLocks.TryGetValue(lockKey, out var lockInfo) &&
                lockInfo.ConnectionId == Context.ConnectionId)
            {
                _rowLocks.TryRemove(lockKey, out _);
                await Clients.OthersInGroup(groupName).SendAsync("ReceiveRowUnlock", applicationCode);
            }
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            if (_connectionGroups.TryRemove(Context.ConnectionId, out var groupName))
            {
                await CleanupConnection(Context.ConnectionId, groupName);
            }

            await base.OnDisconnectedAsync(exception);
        }

        private async Task CleanupConnection(string connectionId, string groupName)
        {
            // Release all locks held by this connection
            var locksToRemove = _rowLocks
                .Where(kv => kv.Value.ConnectionId == connectionId)
                .Select(kv => kv.Key)
                .ToList();

            foreach (var lockKey in locksToRemove)
            {
                if (_rowLocks.TryRemove(lockKey, out _))
                {
                    var applicationCode = lockKey.Split("::")[1];
                    await Clients.Group(groupName).SendAsync("ReceiveRowUnlock", applicationCode);
                }
            }

            // Remove from active users
            if (_activeUsers.TryGetValue(groupName, out var groupUsers))
            {
                groupUsers.TryRemove(connectionId, out _);

                // Broadcast updated user list
                var userList = groupUsers.Values
                    .GroupBy(u => u.UserId)
                    .Select(g => new { userId = g.Key, userName = g.First().UserName })
                    .ToList();

                await Clients.Group(groupName).SendAsync("ReceiveActiveUsers", userList);
            }
        }

        private string GetUserId()
        {
            return Context.User?.FindFirst("UserId")?.Value ?? "unknown";
        }

        private string GetUserName()
        {
            return Context.User?.FindFirst("UserName")?.Value ?? "Unknown User";
        }

        private static string BuildGroupName(string intakeCode, string stage, string status)
        {
            // Normalize to a safe group name
            return $"{intakeCode}_{stage}_{status}".Replace("/", "_").Replace(" ", "_");
        }

        private class RowLock
        {
            public string ConnectionId { get; set; } = "";
            public string UserId { get; set; } = "";
            public string UserName { get; set; } = "";
            public DateTime LockedAt { get; set; }
        }

        private class ActiveUser
        {
            public string UserId { get; set; } = "";
            public string UserName { get; set; } = "";
        }
    }
}
