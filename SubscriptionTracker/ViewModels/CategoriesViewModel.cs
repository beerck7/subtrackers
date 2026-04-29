using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SubscriptionTracker.Models;
using SubscriptionTracker.Services;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;

namespace SubscriptionTracker.ViewModels
{
    public partial class CategoriesViewModel : ObservableObject
    {
        private readonly DataService _dataService;

        [ObservableProperty]
        private ObservableCollection<CategoryItemViewModel> _categories;

        [ObservableProperty]
        private string _newCategoryName;

        public CategoriesViewModel()
        {
            _dataService = new DataService();
            Categories = new ObservableCollection<CategoryItemViewModel>();
            LoadCategoriesCommand.Execute(null);
        }

        [RelayCommand]
        private async Task LoadCategoriesAsync()
        {
            var data = await _dataService.GetCategoriesAsync();
            Categories.Clear();
            foreach (var category in data)
            {
                Categories.Add(new CategoryItemViewModel(category));
            }
        }

        [RelayCommand]
        private async Task AddCategoryAsync()
        {
            if (string.IsNullOrWhiteSpace(NewCategoryName))
            {
                MessageBox.Show("Podaj nazwę kategorii.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var category = new Category
            {
                Name = NewCategoryName,
                Color = "#3b82f6" // Default blue
            };

            await _dataService.AddCategoryAsync(category);
            NewCategoryName = string.Empty;
            
            // Odśwież listę
            await LoadCategoriesAsync();
        }
    }
}
