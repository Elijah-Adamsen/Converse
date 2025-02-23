namespace Converse.Events
{
    public class MessagesMarkedAsReadEvent
    {
        public string ReceiverId { get; }
        public string SenderId { get; }
        public List<string> MessageIds { get; }

        public MessagesMarkedAsReadEvent(string receiverPhone, string senderPhone, List<string> messageIds)
        {
            ReceiverId = receiverPhone;
            SenderId = senderPhone;
            MessageIds = messageIds ?? new List<string>();
        }
    }
}