using Converse.Data;
using Converse.Events;
using Converse.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Converse.Services.Message
{
    public class MessageService
    {
        private readonly MessageDb _messageDb;
        private readonly MessageEventHandler _eventHandler;

        public MessageService(MessageDb messageDb, MessageEventHandler eventHandler)
        {
            _messageDb = messageDb;
            _eventHandler = eventHandler;
        }

        // Save a message before sending it
        public async Task<bool> SaveAndSendMessageAsync(MessageData message)
        {
            try
            {
                await _messageDb.SaveMessageAsync(message);
                await _eventHandler.HandleMessageSavedAsync(new MessageSavedEvent(message));
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving message: {ex.Message}");
                return false;
            }
        }

        // Retrieve message history between two users
        public async Task<List<MessageData>> GetMessageHistoryAsync(string user1, string user2)
        {
            return await _messageDb.GetMessageHistoryAsync(user1, user2);
        }

        // Retrieve group message history
        public async Task<List<MessageData>> GetGroupMessageHistoryAsync(string groupId)
        {
            return await _messageDb.GetGroupMessageHistoryAsync(groupId);
        }

        // Get unread messages for a user
        public async Task<List<MessageData>> GetUnreadMessagesAsync(string userId)
        {
            return await _messageDb.GetUnreadMessagesAsync(userId);
        }

        // Mark messages as read
        public async Task<bool> MarkMessagesAsReadAsync(string receiverPhone, string senderPhone)
        {
            var updatedMessageIds = await _messageDb.MarkMessagesAsReadAsync(receiverPhone, senderPhone);
            if (updatedMessageIds.Count > 0)
            {
                await _eventHandler.HandleMessagesMarkedAsReadAsync(new MessagesMarkedAsReadEvent(receiverPhone, senderPhone, updatedMessageIds));
                return true;
            }
            return false;
        }

        // Mark messages as delivered
        public async Task MarkMessagesAsDeliveredAsync(string userId)
        {
            await _messageDb.MarkMessagesAsDeliveredAsync(userId);
        }
    }
}