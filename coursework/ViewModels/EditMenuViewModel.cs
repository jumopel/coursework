using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using coursework.Core;
using coursework.Models;
using coursework.Commands;

namespace coursework.ViewModels
{
    public class EditMenuViewModel : ObservableObject
    {
        private readonly BaseShop _shop;

        public ObservableCollection<Product> MenuItems => _shop.Menu;

        private string _newName = "Нова страва";
        private decimal _newPrice = 100;
        private double _newPrepTime = 3;
        private ProductCategory _selectedCategory = ProductCategory.MainCourse;
        private DietaryType _selectedDietaryTag = DietaryType.Standard;

        public string NewName { get => _newName; set => SetProperty(ref _newName, value); }
        public decimal NewPrice { get => _newPrice; set => SetProperty(ref _newPrice, value); }
        public double NewPrepTime { get => _newPrepTime; set => SetProperty(ref _newPrepTime, value); }
        public ProductCategory SelectedCategory { get => _selectedCategory; set => SetProperty(ref _selectedCategory, value); }
        public DietaryType SelectedDietaryTag { get => _selectedDietaryTag; set => SetProperty(ref _selectedDietaryTag, value); }

        public List<ProductCategory> Categories { get; } = Enum.GetValues(typeof(ProductCategory)).Cast<ProductCategory>().ToList();
        public List<DietaryType> DietaryTags { get; } = Enum.GetValues(typeof(DietaryType)).Cast<DietaryType>().ToList();

        public ICommand AddProductCommand { get; }
        public ICommand RemoveProductCommand { get; }

        public EditMenuViewModel(BaseShop shop)
        {
            _shop = shop;

            AddProductCommand = new RelayCommand(_ => AddProduct(), _ =>
                !string.IsNullOrWhiteSpace(NewName) && NewPrice > 0 && NewPrepTime > 0);

            RemoveProductCommand = new RelayCommand(param => RemoveProduct(param as Product));
        }

        private void AddProduct()
        {
            var product = new Product
            {
                Name = NewName,
                Price = NewPrice,
                CostPrice = NewPrice * 0.4m,
                PreparationTime = TimeSpan.FromMinutes(NewPrepTime),
                Cuisine = _shop.ShopCuisine,
                Category = SelectedCategory,
                DietaryTag = SelectedDietaryTag
            };

            MenuItems.Add(product); 

            NewName = "Ще одна страва"; 
        }

        private void RemoveProduct(Product? p)
        {
            if (p != null && MenuItems.Contains(p))
            {
                MenuItems.Remove(p);
            }
        }
    }
}