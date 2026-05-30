namespace SubscriptionTracker.Models
{
    public class FamilyConnection
    {
        public int Id { get; set; }

        public int SenderUserId { get; set; }
        public User SenderUser { get; set; }

        public int ReceiverUserId { get; set; }
        public User ReceiverUser { get; set; }

        public bool IsAccepted { get; set; }

        public string Relationship { get; set; } // np. Rodzina, Przyjaciel, Partner, Współlokator
    }
}
