using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SubscriptionTracker.Models
{
    public class PaymentLog
    {
        public int Id { get; set; }

        public int SubscriptionId { get; set; }
        public Subscription Subscription { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }

        public DateTime PaymentDate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal AmountPaid { get; set; }

        public string Note { get; set; } = string.Empty;
    }
}
