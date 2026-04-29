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

        public string Cycle { get; set; } // np. Miesięcznie, Rocznie

        public DateTime StartDate { get; set; }
        public DateTime NextPaymentDate { get; set; }

        public int CategoryId { get; set; }
        public Category Category { get; set; }

        public string Note { get; set; }

        public string Status { get; set; } // Aktywna / Nieaktywna
    }
}
