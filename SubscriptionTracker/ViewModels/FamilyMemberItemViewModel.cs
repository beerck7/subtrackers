using CommunityToolkit.Mvvm.ComponentModel;
using SubscriptionTracker.Models;
using System.Linq;

namespace SubscriptionTracker.ViewModels
{
    public partial class FamilyMemberItemViewModel : ObservableObject
    {
        public FamilyMember Model { get; }

        public int Id => Model.Id;
        public string Name => Model.Name;
        public string Relationship => string.IsNullOrWhiteSpace(Model.Relationship) ? "Znajomy" : Model.Relationship;

        [ObservableProperty]
        private int _sharedSubscriptionsCount;

        [ObservableProperty]
        private string _sharedSubscriptionsCountText;

        [ObservableProperty]
        private string _totalShareAmountText;

        [ObservableProperty]
        private string _avatarColor;

        public string Initials
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Name)) return "?";
                var parts = Name.Split(' ', System.StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 2)
                {
                    return (parts[0][0].ToString() + parts[1][0].ToString()).ToUpper();
                }
                return Name[0].ToString().ToUpper();
            }
        }

        public FamilyMemberItemViewModel(FamilyMember model, int count, decimal totalShare)
        {
            Model = model;
            SharedSubscriptionsCount = count;
            
            // Polish grammar helper for plural nouns
            string subText;
            if (count == 1) subText = "subskrypcję";
            else if (count % 10 >= 2 && count % 10 <= 4 && (count % 100 < 10 || count % 100 >= 20)) subText = "subskrypcje";
            else subText = "subskrypcji";
            
            SharedSubscriptionsCountText = $"Współdzieli {count} {subText}";
            TotalShareAmountText = $"Udział: {totalShare:F2} PLN / mies.";

            // Pick a beautiful pastel color based on the name hash
            string[] colors = new string[] { "#ef4444", "#3b82f6", "#22c55e", "#eab308", "#a855f7", "#ec4899", "#f97316", "#06b6d4" };
            int hash = Name.GetHashCode();
            AvatarColor = colors[System.Math.Abs(hash) % colors.Length];
        }
    }
}
