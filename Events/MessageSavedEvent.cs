using Converse.Models;

namespace Converse.Events
{
    public class MessageSavedEvent
    {
        public MessageData Message { get; }

        public MessageSavedEvent(MessageData message)
        {
            Message = message;
        }
    }
}