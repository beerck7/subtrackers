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
        private readonly Subscription _editingSubscription;

        public bool IsEditMode => _editingSubscription != null;
        public string TitleText => IsEditMode ? "Edytuj subskrypcję" : "Dodaj nową subskrypcję";
        public string ButtonText => IsEditMode ? "Zapisz zmiany" : "Dodaj subskrypcję";

        public AddSubscriptionViewModel() : this(null)
        {
        }

        public AddSubscriptionViewModel(Subscription subscriptionToEdit)
        {
            _dataService = new DataService();
            Categories = new ObservableCollection<Category>();
            _editingSubscription = subscriptionToEdit;

            if (IsEditMode)
            {
                Name = _editingSubscription.Name;
                Price = _editingSubscription.Price;
                Cycle = _editingSubscription.Cycle;
                StartDate = _editingSubscription.StartDate;
                NextPaymentDate = _editingSubscription.NextPaymentDate;
            }
            else
            {
                StartDate = DateTime.Now;
                NextPaymentDate = DateTime.Now.AddMonths(1);
                Cycle = "Miesięcznie";
            }

            LoadCategoriesCommand.Execute(null);
        }

        public ObservableCollection<string> CycleOptions { get; } = new() { "Miesięcznie", "Rocznie" };

        partial void OnCycleChanged(string value)
        {
            if (value == "Rocznie")
            {
                NextPaymentDate = StartDate.AddYears(1);
            }
            else if (value == "Miesięcznie")
            {
                NextPaymentDate = StartDate.AddMonths(1);
            }
        }

        partial void OnStartDateChanged(DateTime value)
        {
            if (Cycle == "Rocznie")
            {
                NextPaymentDate = value.AddYears(1);
            }
            else if (Cycle == "Miesięcznie")
            {
                NextPaymentDate = value.AddMonths(1);
            }
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

            if (IsEditMode && _editingSubscription != null)
            {
                SelectedCategory = Categories.FirstOrDefault(c => c.Id == _editingSubscription.CategoryId);
            }
        }

        [RelayCommand]
        private async Task SaveAsync(Window window)
        {
            ValidateAllProperties();
            if (HasErrors)
            {
                var firstError = string.Join("\n", GetErrors().Select(e => e.ErrorMessage));
                CustomMessageBox.Show(firstError, "Błąd walidacji", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var subs = await _dataService.GetSubscriptionsAsync();
            bool alreadyExists = subs.Any(s => !string.IsNullOrEmpty(s.Name) && string.Equals(s.Name, Name, StringComparison.OrdinalIgnoreCase) && (!IsEditMode || s.Id != _editingSubscription.Id));
            if (alreadyExists)
            {
                CustomMessageBox.Show("Subskrypcja o tej nazwie już istnieje w systemie.", "Błąd walidacji", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (IsEditMode && _editingSubscription != null)
            {
                _editingSubscription.Name = Name;
                _editingSubscription.Price = Price;
                _editingSubscription.Cycle = Cycle ?? "Miesięcznie";
                _editingSubscription.StartDate = StartDate;
                _editingSubscription.NextPaymentDate = NextPaymentDate;
                _editingSubscription.Category = null;
                _editingSubscription.CategoryId = SelectedCategory != null ? SelectedCategory.Id : 0;
                _editingSubscription.Note ??= "";

                await _dataService.UpdateSubscriptionAsync(_editingSubscription);
            }
            else
            {
                var sub = new Subscription
                {
                    Name = Name,
                    Price = Price,
                    Cycle = Cycle ?? "Miesięcznie",
                    StartDate = StartDate,
                    NextPaymentDate = NextPaymentDate,
                    CategoryId = SelectedCategory != null ? SelectedCategory.Id : 0,
                    Status = "Aktywna",
                    Note = ""
                };

                await _dataService.AddSubscriptionAsync(sub);
            }
            
            window.DialogResult = true;
        }

        [RelayCommand]
        private void Cancel(Window window)
        {
            window.DialogResult = false;
        }
    }
}
