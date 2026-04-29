using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SubscriptionTracker.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Nazwa kategorii jest wymagana")]
        public string Name { get; set; }

        public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
    }
}
