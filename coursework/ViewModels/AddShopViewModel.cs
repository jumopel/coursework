using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using coursework.Core;
using coursework.Models;
using coursework.Commands;

namespace coursework.ViewModels
{
    public class AddShopViewModel : ObservableObject
    {
        private string _shopName = "Новий заклад";
        private int _cashiers = 1;
        private int _cooks = 1;
        private decimal _rent = 500;
        private decimal _salary = 300;

        public string ShopName { get => _shopName; set => SetProperty(ref _shopName, value); }
        public int Cashiers { get => _cashiers; set => SetProperty(ref _cashiers, value); }
        public int Cooks { get => _cooks; set => SetProperty(ref _cooks, value); }
        public decimal Rent { get => _rent; set => SetProperty(ref _rent, value); }
        public decimal Salary { get => _salary; set => SetProperty(ref _salary, value); }


        public ICommand SaveCommand { get; }

        public event Action<BaseShop>? ShopCreated;
        public event Action? RequestClose;

        public AddShopViewModel()
        {
            SaveCommand = new RelayCommand(_ => SaveShop(), _ =>
                !string.IsNullOrWhiteSpace(ShopName) && Cashiers > 0 && Cooks > 0);
        }

        private void SaveShop()
        {
            var newShop = new BaseShop
            {
                Name = ShopName,
                CashiersCount = Cashiers,
                CooksCount = Cooks,
                BaseRent = Rent,
                StaffsDailySalary = Salary,
            };

            ShopCreated?.Invoke(newShop);
            RequestClose?.Invoke();
        }
    }
}