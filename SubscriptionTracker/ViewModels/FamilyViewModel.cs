using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SubscriptionTracker.Models;
using SubscriptionTracker.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace SubscriptionTracker.ViewModels
{
    public partial class FamilyViewModel : ObservableObject
    {
        private readonly DataService _dataService;

        [ObservableProperty]
        private ObservableCollection<FamilyMemberItemViewModel> _members;

        [ObservableProperty]
        private string _newMemberName;

        [ObservableProperty]
        private string _newMemberRelationship;

        public FamilyViewModel()
        {
            _dataService = new DataService();
            Members = new ObservableCollection<FamilyMemberItemViewModel>();
            LoadMembersCommand.Execute(null);
        }

        [RelayCommand]
        private async Task LoadMembersAsync()
        {
            var data = await _dataService.GetFamilyMembersAsync();
            var subscriptions = await _dataService.GetSubscriptionsAsync();

            Members.Clear();
            foreach (var member in data)
            {
                // Calculate sharing statistics
                // SharedWith is comma-separated co-payer names, let's check for case-insensitive matches
                var sharedSubs = subscriptions.Where(s => 
                    s.IsShared && 
                    !string.IsNullOrWhiteSpace(s.SharedWith) && 
                    s.SharedWith.Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(name => name.Trim().ToLower())
                        .Contains(member.Name.Trim().ToLower())
                ).ToList();

                int count = sharedSubs.Count;
                decimal totalShare = sharedSubs.Sum(s => s.SplitPrice);

                Members.Add(new FamilyMemberItemViewModel(member, count, totalShare));
            }
        }

        [RelayCommand]
        private async Task AddMemberAsync()
        {
            if (string.IsNullOrWhiteSpace(NewMemberName))
            {
                CustomMessageBox.Show("Podaj imię lub pseudonim członka rodziny / znajomego.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Check if name is unique
            var existing = Members.FirstOrDefault(m => m.Name.Trim().Equals(NewMemberName.Trim(), StringComparison.OrdinalIgnoreCase));
            if (existing != null)
            {
                CustomMessageBox.Show("Członek o takiej nazwie już istnieje.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var member = new FamilyMember
            {
                Name = NewMemberName.Trim(),
                Relationship = string.IsNullOrWhiteSpace(NewMemberRelationship) ? "Rodzina" : NewMemberRelationship.Trim()
            };

            await _dataService.AddFamilyMemberAsync(member);
            NewMemberName = string.Empty;
            NewMemberRelationship = string.Empty;

            await LoadMembersAsync();
        }

        [RelayCommand]
        private async Task DeleteMemberAsync(FamilyMemberItemViewModel item)
        {
            if (item == null) return;

            var result = CustomMessageBox.Show(
                $"Czy na pewno chcesz usunąć użytkownika {item.Name}?",
                "Potwierdzenie",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                await _dataService.DeleteFamilyMemberAsync(item.Id);
                await LoadMembersAsync();
            }
        }
    }
}
