using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SubscriptionTracker.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Nazwa użytkownika jest wymagana")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Hasło jest wymagane")]
        public string PasswordHash { get; set; }

        public string Email { get; set; }

        public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
        public ICollection<Category> Categories { get; set; } = new List<Category>();
        public ICollection<FamilyMember> FamilyMembers { get; set; } = new List<FamilyMember>();
    }
}
