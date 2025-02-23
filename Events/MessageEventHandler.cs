using Converse.Models;

namespace Converse.Events
{
    public class MessageEventHandler
    {
        public async Task HandleMessageSavedAsync(MessageSavedEvent messageEvent)
        {
            // Handle logic when a message is saved (e.g., notifications, logging)
            await Task.CompletedTask;
        }

        public async Task HandleMessagesMarkedAsReadAsync(MessagesMarkedAsReadEvent eventData)
        {
            // Handle logic when messages are marked as read (e.g., updating UI, notifications)
            await Task.CompletedTask;
        }
    }
}