using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SubscriptionTracker.Services;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace SubscriptionTracker.ViewModels
{
    public partial class SubscriptionsViewModel : ObservableObject
    {
        private readonly DataService _dataService;
        private ObservableCollection<SubscriptionItemViewModel> _allSubscriptions;

        [ObservableProperty]
        private ObservableCollection<SubscriptionItemViewModel> _displayedSubscriptions;

        [ObservableProperty]
        private string _searchText = "";

        public SubscriptionsViewModel()
        {
            _dataService = new DataService();
            _allSubscriptions = new ObservableCollection<SubscriptionItemViewModel>();
            DisplayedSubscriptions = new ObservableCollection<SubscriptionItemViewModel>();

            LoadSubscriptionsCommand.Execute(null);
        }

        partial void OnSearchTextChanged(string value)
        {
            FilterList();
        }

        [RelayCommand]
        private async Task LoadSubscriptionsAsync()
        {
            var data = await _dataService.GetSubscriptionsAsync();
            _allSubscriptions.Clear();
            foreach (var sub in data)
            {
                _allSubscriptions.Add(new SubscriptionItemViewModel(sub));
            }
            FilterList();
        }

        private void FilterList()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                DisplayedSubscriptions.Clear();
                foreach (var item in _allSubscriptions) DisplayedSubscriptions.Add(item);
            }
            else
            {
                var lowerQuery = SearchText.ToLower();
                var filtered = _allSubscriptions.Where(s => s.Name.ToLower().Contains(lowerQuery)).ToList();
                DisplayedSubscriptions.Clear();
                foreach (var item in filtered) DisplayedSubscriptions.Add(item);
            }
        }

        [RelayCommand]
        private async Task OpenAddSubscriptionDialogAsync()
        {
            var addWindow = new AddSubscriptionWindow();
            
            // To pass parent window for centering, but let's just show it.
            if (addWindow.ShowDialog() == true)
            {
                await LoadSubscriptionsAsync();
            }
        }
    }
}
