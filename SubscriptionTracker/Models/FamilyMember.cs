using System.ComponentModel.DataAnnotations;

namespace SubscriptionTracker.Models
{
    public class FamilyMember
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Imię/nazwa członka rodziny jest wymagana")]
        public string Name { get; set; }

        public string Relationship { get; set; } // np. Rodzina, Przyjaciel, Partner, Współlokator

        public int UserId { get; set; }
        public User User { get; set; }
    }
}
