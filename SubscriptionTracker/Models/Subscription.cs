using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SubscriptionTracker.Models
{
    public class Subscription
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Nazwa jest wymagana")]
        public string Name { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Cena musi być większa od zera")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        public string Cycle { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime NextPaymentDate { get; set; }

        public int CategoryId { get; set; }
        public Category Category { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }

        public string Note { get; set; }

        public string Status { get; set; }


        public bool IsShared { get; set; } = false;
        public int NumberOfMembers { get; set; } = 1;
        public string SharedWith { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;

        [NotMapped]
        public decimal SplitPrice => IsShared && NumberOfMembers > 1 ? Math.Round(Price / NumberOfMembers, 2) : Price;


        public ICollection<PaymentLog> PaymentLogs { get; set; }
    }
}
