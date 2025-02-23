using Converse.Services.Chat;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Converse.Hubs
{
    [Authorize] // Require authentication before connection
    public class ChatHub : Hub
    {
        private readonly ChatService _chatService;
        private readonly ConnectionService _connectionService;

        public ChatHub(ChatService chatService, ConnectionService connectionService)
        {
            _chatService = chatService;
            _connectionService = connectionService;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = _connectionService.GetUserId(Context);
            if (string.IsNullOrWhiteSpace(userId))
            {
                Context.Abort();
                return;
            }

            _connectionService.AddConnection(userId, Context.ConnectionId);
            Console.WriteLine($"User Connected: {userId} with ConnectionId: {Context.ConnectionId}");

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var userId = _connectionService.RemoveConnection(Context.ConnectionId);
            if (!string.IsNullOrEmpty(userId))
                Console.WriteLine($"User Disconnected: {userId}");

            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessageToUser(string receiver, string messageContent)
        {
            var sender = _connectionService.GetUserId(Context);
            if (string.IsNullOrWhiteSpace(sender)) return;

            await _chatService.SendMessageAsync(sender, receiver, messageContent);
        }

        public async Task MarkMessagesAsRead(string senderPhone)
        {
            var receiverPhone = _connectionService.GetUserId(Context);
            if (!string.IsNullOrWhiteSpace(receiverPhone))
                await _chatService.MarkMessagesAsReadAsync(senderPhone, receiverPhone);
        }

        public async Task SendMessageToGroup(string groupId, string messageContent)
        {
            var sender = _connectionService.GetUserId(Context);
            if (!string.IsNullOrWhiteSpace(sender))
                await _chatService.SendGroupMessageAsync(sender, groupId, messageContent);
        }

        public async Task JoinGroup(string groupId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupId);
        }

        public async Task LeaveGroup(string groupId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupId);
        }
    }
}