using coursework.Commands;
using coursework.Core;
using coursework.Models;
using coursework.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using static coursework.Core.BaseShop;

namespace coursework.ViewModels
{
    public class AddShopViewModel : ObservableObject
    {
        private string _shopName = "Новий заклад";
        private ShopType _selectedType = ShopType.FastFood;
        private CuisineType _selectedCuisine = CuisineType.Universal;
        private int _cashiersCount = 2;
        private int _cooksCount = 2;
        private decimal _dailyRent = 500;
        private decimal _staffsDailySalary = 300;
        private double _orderTakingMinutes = 1.5;
        private double _prepMinutes = 4.0;
        private double _x = 300;
        private double _y = 300;
        private BaseZone? _selectedZone;

        public string ShopName { get => _shopName; set => SetProperty(ref _shopName, value); }
        public ShopType SelectedType { get => _selectedType; set => SetProperty(ref _selectedType, value); }
        public List<ShopType> ShopTypes { get; } = Enum.GetValues(typeof(ShopType)).Cast<ShopType>().ToList();

        public int CashiersCount { get => _cashiersCount; set => SetProperty(ref _cashiersCount, value); }
        public int CooksCount { get => _cooksCount; set => SetProperty(ref _cooksCount, value); }
        public decimal DailyRent { get => _dailyRent; set => SetProperty(ref _dailyRent, value); }
        public decimal StaffsDailySalary { get => _staffsDailySalary; set => SetProperty(ref _staffsDailySalary, value); }
        public double OrderTakingMinutes { get => _orderTakingMinutes; set => SetProperty(ref _orderTakingMinutes, value); }
        public double PrepMinutes { get => _prepMinutes; set => SetProperty(ref _prepMinutes, value); }
        public double X { get => _x; set => SetProperty(ref _x, value); }
        public double Y { get => _y; set => SetProperty(ref _y, value); }
        public CuisineType SelectedCuisine
        {
            get => _selectedCuisine;
            set => SetProperty(ref _selectedCuisine, value);
        }

        public List<CuisineType> Cuisines { get; } = Enum.GetValues(typeof(CuisineType)).Cast<CuisineType>().ToList();

        public BaseZone? SelectedZone { get => _selectedZone; set => SetProperty(ref _selectedZone, value); }
        public List<BaseZone> AvailableZones { get; }

        public ICommand SaveCommand { get; }
        public event Action? RequestClose;

        public AddShopViewModel(IEnumerable<BaseZone> zones)
        {
            AvailableZones = zones.ToList();
            SelectedZone = AvailableZones.FirstOrDefault();

            SaveCommand = new RelayCommand(_ => SaveShop(), _ => !string.IsNullOrWhiteSpace(ShopName) && SelectedZone != null);
        }

        private void SaveShop()
        {
            if (SelectedZone == null) return;

            var dataService = new DataService();
            var newShop = dataService.CreateShop(
                ShopName, SelectedType, CashiersCount, CooksCount,
                DailyRent, StaffsDailySalary, OrderTakingMinutes, PrepMinutes, X, Y
            );

            if (SelectedType == ShopType.FastFood)
            {
                dataService.AddProductToShop(newShop, $"Комбо меню ({SelectedCuisine})", 150, 50, TimeSpan.FromMinutes(PrepMinutes), ProductCategory.MainCourse, DietaryType.Standard, SelectedCuisine);
                dataService.AddProductToShop(newShop, "Фірмовий Напій", 45, 12, TimeSpan.FromMinutes(0.5), ProductCategory.Drink, DietaryType.Standard, CuisineType.Universal);
            }
            else
            {
                dataService.AddProductToShop(newShop, $"Спешл страва ({SelectedCuisine})", 240, 80, TimeSpan.FromMinutes(PrepMinutes), ProductCategory.MainCourse, DietaryType.Standard, SelectedCuisine);
                dataService.AddProductToShop(newShop, "Келих вина", 95, 35, TimeSpan.FromMinutes(1), ProductCategory.Drink, DietaryType.Standard, CuisineType.Universal);
            }

            SelectedZone.AddShop(newShop);

            RequestClose?.Invoke();
        }
    }
}