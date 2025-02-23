using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;

namespace Converse.Services.Chat
{
    public class ConnectionService
    {
        private static readonly ConcurrentDictionary<string, string> _userConnections = new();
        private static readonly HashSet<string> _onlineUsers = new();

        public void AddConnection(string userId, string connectionId)
        {
            _userConnections[userId] = connectionId;
            lock (_onlineUsers)
            {
                _onlineUsers.Add(userId);
            }
            Console.WriteLine($"User {userId} connected with ConnectionId: {connectionId}");
        }

        public string RemoveConnection(string connectionId)
        {
            var userId = _userConnections.FirstOrDefault(x => x.Value == connectionId).Key;
            if (!string.IsNullOrEmpty(userId))
            {
                _userConnections.TryRemove(userId, out _);
                lock (_onlineUsers)
                {
                    _onlineUsers.Remove(userId);
                }
                Console.WriteLine($"User {userId} disconnected.");
            }
            return userId;
        }

        public string GetConnectionId(string userId)
        {
            _userConnections.TryGetValue(userId, out var connectionId);
            return connectionId;
        }

        public string GetUserIdByConnection(string connectionId)
        {
            return _userConnections.FirstOrDefault(x => x.Value == connectionId).Key;
        }

        public bool IsUserOnline(string userId)
        {
            lock (_onlineUsers)
            {
                return _onlineUsers.Contains(userId);
            }
        }

        public List<string> GetOnlineUsers()
        {
            lock (_onlineUsers)
            {
                return _onlineUsers.ToList();
            }
        }

        public string GetUserId(HubCallerContext context)
        {
            return context.User?.FindFirst(ClaimTypes.Name)?.Value;
        }
    }
}