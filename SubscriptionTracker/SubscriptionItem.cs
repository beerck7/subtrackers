namespace SubscriptionTracker
{
    public class SubscriptionItem
    {
        public string Initials { get; set; }
        public string IconColor { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public string Cycle { get; set; }
        public decimal Price { get; set; }
        public string FormattedPrice => $"{Price:0.00} PLN";
    }
}
