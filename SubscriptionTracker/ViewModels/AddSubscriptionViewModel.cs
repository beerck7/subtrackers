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

        public ObservableCollection<SelectableFamilyMember> FamilyMembersList { get; } = new();

        public AddSubscriptionViewModel(Subscription subscriptionToEdit)
        {
            _dataService = new DataService();
            Categories = new ObservableCollection<Category>();
            _editingSubscription = subscriptionToEdit;

            if (IsEditMode)
            {
                Name = _editingSubscription.Name;
                PriceText = _editingSubscription.Price.ToString("0.00");
                Cycle = _editingSubscription.Cycle;
                StartDate = _editingSubscription.StartDate;
                NextPaymentDate = _editingSubscription.NextPaymentDate;
                IsShared = _editingSubscription.IsShared;
                NumberOfMembers = _editingSubscription.NumberOfMembers;
                SharedWith = _editingSubscription.SharedWith;
                SelectedColor = _editingSubscription.Color;
            }
            else
            {
                StartDate = DateTime.Now;
                NextPaymentDate = DateTime.Now.AddMonths(1);
                Cycle = "Miesięcznie";
                IsShared = false;
                NumberOfMembers = 1;
                SharedWith = string.Empty;
                PriceText = string.Empty;
                SelectedColor = string.Empty;
            }

            LoadCategoriesCommand.Execute(null);
            LoadFamilyMembersCommand.Execute(null);
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
        [NotifyPropertyChangedFor(nameof(CalculatedSplitPrice))]
        private decimal _price;

        private string _priceText;

        [Required(ErrorMessage = "Podaj cenę")]
        [CustomValidation(typeof(AddSubscriptionViewModel), nameof(ValidatePriceText))]
        public string PriceText
        {
            get => _priceText;
            set
            {
                if (SetProperty(ref _priceText, value))
                {
                    string normalized = value?.Replace(',', '.') ?? "";
                    if (decimal.TryParse(normalized, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal parsedPrice))
                    {
                        Price = parsedPrice;
                    }
                    else
                    {
                        Price = 0;
                    }
                    ValidateProperty(value, nameof(PriceText));
                }
            }
        }

        public ObservableCollection<string> AvailableColors { get; } = new()
        {
            "#3b82f6", // Niebieski
            "#22c55e", // Zielony
            "#f43f5e", // Czerwony
            "#eab308", // Żółty
            "#a855f7", // Fioletowy
            "#ec4899", // Różowy
            "#14b8a6", // Turkusowy
            "#6b7280"  // Szary
        };

        [ObservableProperty]
        private string _selectedColor = string.Empty;

        public static ValidationResult ValidatePriceText(string priceText, ValidationContext context)
        {
            if (string.IsNullOrWhiteSpace(priceText))
            {
                return new ValidationResult("Cena jest wymagana");
            }
            string normalized = priceText.Replace(',', '.');
            if (!decimal.TryParse(normalized, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal parsedPrice))
            {
                return new ValidationResult("Niepoprawny format ceny");
            }
            if (parsedPrice <= 0)
            {
                return new ValidationResult("Cena musi być większa od zera");
            }
            if (parsedPrice > 99999.99m)
            {
                return new ValidationResult("Cena jest zbyt wysoka");
            }
            return ValidationResult.Success;
        }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CalculatedSplitPrice))]
        private bool _isShared = false;

        [ObservableProperty]
        [Range(1, 100, ErrorMessage = "Liczba osób musi być między 1 a 100")]
        [NotifyPropertyChangedFor(nameof(CalculatedSplitPrice))]
        private int _numberOfMembers = 1;

        [ObservableProperty]
        private string _sharedWith = string.Empty;

        public decimal CalculatedSplitPrice => IsShared && NumberOfMembers > 1 ? Math.Round(Price / NumberOfMembers, 2) : Price;

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
                _editingSubscription.IsShared = IsShared;
                _editingSubscription.NumberOfMembers = IsShared ? NumberOfMembers : 1;
                _editingSubscription.SharedWith = IsShared ? (SharedWith ?? string.Empty) : string.Empty;
                _editingSubscription.Color = SelectedColor ?? string.Empty;

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
                    Note = "",
                    IsShared = IsShared,
                    NumberOfMembers = IsShared ? NumberOfMembers : 1,
                    SharedWith = IsShared ? (SharedWith ?? string.Empty) : string.Empty,
                    Color = SelectedColor ?? string.Empty
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

        [RelayCommand]
        private async Task LoadFamilyMembersAsync()
        {
            var connections = await _dataService.GetFamilyConnectionsAsync();
            FamilyMembersList.Clear();

            int currentUserId = SessionManager.CurrentUser?.Id ?? 0;
            var acceptedConnections = connections.Where(fc => fc.IsAccepted).ToList();

            var sharedNames = string.IsNullOrWhiteSpace(SharedWith)
                ? Array.Empty<string>()
                : SharedWith.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(n => n.Trim().ToLower()).ToArray();

            foreach (var conn in acceptedConnections)
            {
                var otherUser = conn.SenderUserId == currentUserId ? conn.ReceiverUser : conn.SenderUser;
                string otherUsername = otherUser.Username;

                var selectable = new SelectableFamilyMember
                {
                    Name = otherUsername,
                    IsSelected = sharedNames.Contains(otherUsername.Trim().ToLower())
                };

                selectable.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(SelectableFamilyMember.IsSelected))
                    {
                        UpdateSharedStatus();
                    }
                };

                FamilyMembersList.Add(selectable);
            }
        }

        private void UpdateSharedStatus()
        {
            var selected = FamilyMembersList.Where(m => m.IsSelected).Select(m => m.Name).ToList();
            if (selected.Any())
            {
                SharedWith = string.Join(", ", selected);
                NumberOfMembers = selected.Count + 1;
            }
            else
            {
                SharedWith = string.Empty;
                NumberOfMembers = 1;
            }
        }
    }

    public partial class SelectableFamilyMember : ObservableObject
    {
        public string Name { get; set; }

        [ObservableProperty]
        private bool _isSelected;
    }
}
