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
    public partial class UserSearchResult : ObservableObject
    {
        public User User { get; }
        public string Username => User.Username;
        public string Email => User.Email ?? "Brak e-mail";
        public string Initials => string.IsNullOrWhiteSpace(Username) ? "?" : Username[0].ToString().ToUpper();

        [ObservableProperty]
        private string _statusText = "Wyślij zaproszenie";

        [ObservableProperty]
        private bool _canSend = true;

        public UserSearchResult(User user, string status, bool canSend)
        {
            User = user;
            StatusText = status;
            CanSend = canSend;
        }
    }

    public partial class FamilyViewModel : ObservableObject
    {
        private readonly DataService _dataService;

        [ObservableProperty]
        private ObservableCollection<FamilyMemberItemViewModel> _members;

        [ObservableProperty]
        private ObservableCollection<FamilyMemberItemViewModel> _pendingReceived;

        [ObservableProperty]
        private ObservableCollection<FamilyMemberItemViewModel> _pendingSent;

        [ObservableProperty]
        private ObservableCollection<UserSearchResult> _searchResults;

        [ObservableProperty]
        private string _searchQuery;

        public FamilyViewModel()
        {
            _dataService = new DataService();
            Members = new ObservableCollection<FamilyMemberItemViewModel>();
            PendingReceived = new ObservableCollection<FamilyMemberItemViewModel>();
            PendingSent = new ObservableCollection<FamilyMemberItemViewModel>();
            SearchResults = new ObservableCollection<UserSearchResult>();
            
            LoadMembersCommand.Execute(null);
        }

        [RelayCommand]
        private async Task LoadMembersAsync()
        {
            var connections = await _dataService.GetFamilyConnectionsAsync();
            var subscriptions = await _dataService.GetSubscriptionsAsync();
            int currentUserId = SessionManager.CurrentUser?.Id ?? 0;

            Members.Clear();
            PendingReceived.Clear();
            PendingSent.Clear();

            foreach (var conn in connections)
            {
                // Find the other user
                var otherUser = conn.SenderUserId == currentUserId ? conn.ReceiverUser : conn.SenderUser;

                // Calculate sharing statistics
                var sharedSubs = subscriptions.Where(s => 
                    s.IsShared && 
                    !string.IsNullOrWhiteSpace(s.SharedWith) && 
                    s.SharedWith.Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(name => name.Trim().ToLower())
                        .Contains(otherUser.Username.Trim().ToLower())
                ).ToList();

                int count = sharedSubs.Count;
                decimal totalShare = sharedSubs.Sum(s => s.SplitPrice);

                var vm = new FamilyMemberItemViewModel(conn, currentUserId, count, totalShare);

                if (conn.IsAccepted)
                {
                    Members.Add(vm);
                }
                else if (conn.SenderUserId == currentUserId)
                {
                    PendingSent.Add(vm);
                }
                else
                {
                    PendingReceived.Add(vm);
                }
            }

            // Refresh search results statuses if any query is active
            if (!string.IsNullOrWhiteSpace(SearchQuery))
            {
                await SearchAsync();
            }
        }

        [RelayCommand]
        private async Task SearchAsync()
        {
            if (string.IsNullOrWhiteSpace(SearchQuery))
            {
                SearchResults.Clear();
                return;
            }

            var users = await _dataService.SearchUsersAsync(SearchQuery);
            var connections = await _dataService.GetFamilyConnectionsAsync();
            int currentUserId = SessionManager.CurrentUser?.Id ?? 0;

            SearchResults.Clear();

            foreach (var user in users)
            {
                // Determine connection state
                var conn = connections.FirstOrDefault(c => 
                    (c.SenderUserId == currentUserId && c.ReceiverUserId == user.Id) ||
                    (c.SenderUserId == user.Id && c.ReceiverUserId == currentUserId));

                string statusText = "Wyślij zaproszenie";
                bool canSend = true;

                if (conn != null)
                {
                    canSend = false;
                    if (conn.IsAccepted)
                    {
                        statusText = "Połączono";
                    }
                    else if (conn.SenderUserId == currentUserId)
                    {
                        statusText = "Wysłano zaproszenie";
                    }
                    else
                    {
                        statusText = "Oczekuje na akceptację";
                    }
                }

                SearchResults.Add(new UserSearchResult(user, statusText, canSend));
            }
        }

        [RelayCommand]
        private async Task SendInvitationAsync(UserSearchResult result)
        {
            if (result == null || !result.CanSend) return;

            bool success = await _dataService.SendInvitationAsync(result.User.Id);
            if (success)
            {
                result.StatusText = "Wysłano zaproszenie";
                result.CanSend = false;
                await LoadMembersAsync();
            }
            else
            {
                CustomMessageBox.Show("Nie udało się wysłać zaproszenia.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task AcceptInvitationAsync(FamilyMemberItemViewModel item)
        {
            if (item == null) return;

            bool success = await _dataService.AcceptInvitationAsync(item.ConnectionId);
            if (success)
            {
                await LoadMembersAsync();
            }
            else
            {
                CustomMessageBox.Show("Nie udało się zaakceptować zaproszenia.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task DeclineOrRemoveAsync(FamilyMemberItemViewModel item)
        {
            if (item == null) return;

            string message = item.IsAccepted 
                ? $"Czy na pewno chcesz usunąć użytkownika {item.Name} z rodziny?" 
                : (item.IsPendingReceived ? $"Czy chcesz odrzucić zaproszenie od użytkownika {item.Name}?" : $"Czy chcesz anulować zaproszenie do użytkownika {item.Name}?");

            var result = CustomMessageBox.Show(message, "Potwierdzenie", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                await _dataService.DeleteConnectionAsync(item.ConnectionId);
                await LoadMembersAsync();
            }
        }
    }
}
