using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SubscriptionTracker.Models;
using SubscriptionTracker.Services;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

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

        [ObservableProperty]
        private ObservableCollection<Category> _filterCategories;

        [ObservableProperty]
        private Category _selectedCategory;

        [ObservableProperty]
        private string _selectedStatus = "Wszystkie";

        [ObservableProperty]
        private string _selectedSortOption = "Domyślnie";

        public ObservableCollection<string> StatusOptions { get; } = new() { "Wszystkie", "Aktywna", "Nieaktywna" };
        
        public ObservableCollection<string> SortOptions { get; } = new() { 
            "Domyślnie", 
            "Cena: od najniższej", 
            "Cena: od najwyższej", 
            "Data płatności: najwcześniej", 
            "Data płatności: najpóźniej" 
        };

        public SubscriptionsViewModel()
        {
            _dataService = new DataService();
            _allSubscriptions = new ObservableCollection<SubscriptionItemViewModel>();
            DisplayedSubscriptions = new ObservableCollection<SubscriptionItemViewModel>();
            FilterCategories = new ObservableCollection<Category>();

            LoadSubscriptionsCommand.Execute(null);
        }

        partial void OnSearchTextChanged(string value) => FilterList();
        partial void OnSelectedCategoryChanged(Category value) => FilterList();
        partial void OnSelectedStatusChanged(string value) => FilterList();
        partial void OnSelectedSortOptionChanged(string value) => FilterList();

        [RelayCommand]
        private async Task LoadSubscriptionsAsync()
        {
            var data = await _dataService.GetSubscriptionsAsync();
            _allSubscriptions.Clear();
            foreach (var sub in data)
            {
                _allSubscriptions.Add(new SubscriptionItemViewModel(sub, LoadSubscriptionsAsync));
            }

            var cats = await _dataService.GetCategoriesAsync();
            FilterCategories.Clear();
            
            var allCatsDummy = new Category { Id = 0, Name = "Wszystkie kategorie" };
            FilterCategories.Add(allCatsDummy);
            foreach (var c in cats)
            {
                FilterCategories.Add(c);
            }

            if (SelectedCategory == null)
            {
                SelectedCategory = allCatsDummy;
            }
            else
            {
                var existing = FilterCategories.FirstOrDefault(c => c.Id == SelectedCategory.Id);
                SelectedCategory = existing ?? allCatsDummy;
            }

            FilterList();
        }

        private void FilterList()
        {
            var query = _allSubscriptions.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var lower = SearchText.ToLower();
                query = query.Where(s => !string.IsNullOrEmpty(s.Name) && s.Name.ToLower().Contains(lower));
            }

            if (SelectedCategory != null && SelectedCategory.Id != 0)
            {
                query = query.Where(s => s.Model.CategoryId == SelectedCategory.Id);
            }

            if (SelectedStatus != "Wszystkie")
            {
                query = query.Where(s => s.Model.Status == SelectedStatus);
            }

            if (SelectedSortOption == "Cena: od najniższej")
            {
                query = query.OrderBy(s => s.Model.SplitPrice);
            }
            else if (SelectedSortOption == "Cena: od najwyższej")
            {
                query = query.OrderByDescending(s => s.Model.SplitPrice);
            }
            else if (SelectedSortOption == "Data płatności: najwcześniej")
            {
                query = query.OrderBy(s => s.Model.NextPaymentDate);
            }
            else if (SelectedSortOption == "Data płatności: najpóźniej")
            {
                query = query.OrderByDescending(s => s.Model.NextPaymentDate);
            }

            DisplayedSubscriptions.Clear();
            foreach (var item in query)
            {
                DisplayedSubscriptions.Add(item);
            }
        }

        [RelayCommand]
        private async Task OpenAddSubscriptionDialogAsync()
        {
            var addWindow = new AddSubscriptionWindow();
            addWindow.Owner = Application.Current.MainWindow;
            if (addWindow.ShowDialog() == true)
            {
                await LoadSubscriptionsAsync();
            }
        }

        [RelayCommand]
        private async Task EditSubscriptionAsync(SubscriptionItemViewModel item)
        {
            if (item == null) return;
            var editWindow = new AddSubscriptionWindow(item.Model);
            editWindow.Owner = Application.Current.MainWindow;
            if (editWindow.ShowDialog() == true)
            {
                await LoadSubscriptionsAsync();
            }
        }

        [RelayCommand]
        private async Task DeleteSubscriptionAsync(SubscriptionItemViewModel item)
        {
            if (item == null) return;
            var result = CustomMessageBox.Show(
                $"Czy na pewno chcesz usunąć subskrypcję {item.Name}?",
                "Potwierdzenie",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                await _dataService.DeleteSubscriptionAsync(item.Model.Id);
                await LoadSubscriptionsAsync();
            }
        }
    }
}

