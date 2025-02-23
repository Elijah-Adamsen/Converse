using Converse.Hubs;
using Converse.Models;
using Converse.Services.Message;
using Converse.Services.Group;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Converse.Services.Chat
{
    public class ChatService
    {
        private readonly IHubContext<ChatHub> _chatHub;
        private readonly MessageService _messageService;
        private readonly GroupManagementService _groupManagementService;
        private readonly ConnectionService _connectionService;

        public ChatService(IHubContext<ChatHub> chatHub, MessageService messageService, GroupManagementService groupManagementService, ConnectionService connectionService)
        {
            _chatHub = chatHub;
            _messageService = messageService;
            _groupManagementService = groupManagementService;
            _connectionService = connectionService;
        }

        public async Task<bool> SendMessageAsync(string senderPhone, string receiverPhone, string content, string? groupId = null)
        {
            var message = new MessageData
            {
                SenderPhone = senderPhone,
                ReceiverPhone = receiverPhone,
                GroupID = groupId,
                Content = content,
                SentAt = DateTime.UtcNow,
                Read = false,
                Delivered = false
            };

            var saved = await _messageService.SaveAndSendMessageAsync(message);
            if (!saved) return false;

            if (_connectionService.IsUserOnline(receiverPhone))
            {
                await BroadcastMessageAsync(senderPhone, receiverPhone, content, groupId);
                await MarkMessagesAsDeliveredAsync(receiverPhone);
            }

            return true;
        }

        public async Task<bool> SendGroupMessageAsync(string senderPhone, string groupId, string content)
        {
            var group = _groupManagementService.GetGroupById(groupId);
            if (group == null || !group.Members.Contains(senderPhone))
                return false;

            var message = new MessageData
            {
                SenderPhone = senderPhone,
                GroupID = groupId,
                Content = content,
                SentAt = DateTime.UtcNow,
                Read = false,
                Delivered = false
            };

            var saved = await _messageService.SaveAndSendMessageAsync(message);
            if (!saved) return false;

            await _chatHub.Clients.Group(groupId).SendAsync("ReceiveMessage", senderPhone, content);
            return true;
        }

        private async Task BroadcastMessageAsync(string senderPhone, string receiverPhone, string content, string? groupId)
        {
            if (!string.IsNullOrEmpty(groupId))
            {
                await _chatHub.Clients.Group(groupId).SendAsync("ReceiveMessage", senderPhone, content);
            }
            else
            {
                var receiverConnectionId = _connectionService.GetConnectionId(receiverPhone);
                if (!string.IsNullOrWhiteSpace(receiverConnectionId))
                {
                    await _chatHub.Clients.Client(receiverConnectionId).SendAsync("ReceiveMessage", senderPhone, content);
                }
            }
        }

        public async Task<bool> MarkMessagesAsReadAsync(string senderPhone, string receiverPhone)
        {
            var updated = await _messageService.MarkMessagesAsReadAsync(receiverPhone, senderPhone);
            if (updated)
            {
                await _chatHub.Clients.User(senderPhone).SendAsync("MessagesMarkedAsRead", receiverPhone);
                return true;
            }
            return false;
        }

        public async Task MarkMessagesAsDeliveredAsync(string userId)
        {
            await _messageService.MarkMessagesAsDeliveredAsync(userId);
        }
    }
}