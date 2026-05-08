using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SubscriptionTracker.Models;
using SubscriptionTracker.Services;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace SubscriptionTracker.ViewModels
{
    public partial class AddSubscriptionViewModel : ObservableValidator
    {
        private readonly DataService _dataService;

        public AddSubscriptionViewModel()
        {
            _dataService = new DataService();
            Categories = new ObservableCollection<Category>();
            LoadCategoriesCommand.Execute(null);

            // Default values
            StartDate = DateTime.Now;
            NextPaymentDate = DateTime.Now.AddMonths(1);
            Cycle = "Miesięcznie";
        }

        [ObservableProperty]
        private ObservableCollection<Category> _categories;

        [ObservableProperty]
        [Required(ErrorMessage = "Wybierz kategorię")]
        private Category _selectedCategory;

        [ObservableProperty]
        [Required(ErrorMessage = "Podaj nazwę subskrypcji")]
        private string _name;

        [ObservableProperty]
        [Range(0.01, 99999.99, ErrorMessage = "Cena musi być większa od zera")]
        private decimal _price;

        [ObservableProperty]
        private string _cycle;

        [ObservableProperty]
        private DateTime _startDate;

        [ObservableProperty]
        [CustomValidation(typeof(AddSubscriptionViewModel), nameof(ValidateNextPaymentDate))]
        private DateTime _nextPaymentDate;

        public static ValidationResult ValidateNextPaymentDate(DateTime nextPaymentDate, ValidationContext context)
        {
            var vm = (AddSubscriptionViewModel)context.ObjectInstance;
            if (nextPaymentDate < vm.StartDate)
            {
                return new ValidationResult("Następna płatność nie może być wcześniejsza niż data rozpoczęcia.");
            }
            return ValidationResult.Success;
        }

        [RelayCommand]
        private async Task LoadCategoriesAsync()
        {
            var cats = await _dataService.GetCategoriesAsync();
            Categories.Clear();
            foreach (var c in cats) Categories.Add(c);
        }

        [RelayCommand]
        private async Task SaveAsync(Window window)
        {
            ValidateAllProperties();
            if (HasErrors)
            {
                var firstError = string.Join("\n", GetErrors().Select(e => e.ErrorMessage));
                MessageBox.Show(firstError, "Błąd walidacji", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var sub = new Subscription
            {
                Name = Name,
                Price = Price,
                Cycle = Cycle,
                StartDate = StartDate,
                NextPaymentDate = NextPaymentDate,
                Category = SelectedCategory,
                CategoryId = SelectedCategory.Id,
                Status = "Aktywna"
            };

            await _dataService.AddSubscriptionAsync(sub);
            
            window.DialogResult = true;
        }

        [RelayCommand]
        private void Cancel(Window window)
        {
            window.DialogResult = false;
        }
    }
}
