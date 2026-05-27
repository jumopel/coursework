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

        private bool _isEditing = false;
        private Product? _editingProduct;

        public bool IsEditing { get => _isEditing; set => SetProperty(ref _isEditing, value); }

        private string _submitButtonText = "Додати";
        public string SubmitButtonText { get => _submitButtonText; set => SetProperty(ref _submitButtonText, value); }

        private string _submitButtonColor = "#27AE60"; 
        public string SubmitButtonColor { get => _submitButtonColor; set => SetProperty(ref _submitButtonColor, value); }

        public ICommand SubmitProductCommand { get; }
        public ICommand RemoveProductCommand { get; }
        public ICommand StartEditCommand { get; }
        public ICommand CancelEditCommand { get; }

        public EditMenuViewModel(BaseShop shop)
        {
            _shop = shop;

            SubmitProductCommand = new RelayCommand(_ => SubmitProduct(), _ =>
                !string.IsNullOrWhiteSpace(NewName) && NewPrice > 0 && NewPrepTime > 0);

            RemoveProductCommand = new RelayCommand(param => RemoveProduct(param as Product));
            StartEditCommand = new RelayCommand(param => StartEdit(param as Product));
            CancelEditCommand = new RelayCommand(_ => CancelEdit());
        }

        private void SubmitProduct()
        {
            if (IsEditing && _editingProduct != null)
            {
                _editingProduct.Name = NewName;
                _editingProduct.Price = NewPrice;
                _editingProduct.CostPrice = NewPrice * 0.4m;
                _editingProduct.PreparationTime = TimeSpan.FromMinutes(NewPrepTime);
                _editingProduct.Category = SelectedCategory;
                _editingProduct.DietaryTag = SelectedDietaryTag;

                int index = MenuItems.IndexOf(_editingProduct);
                if (index >= 0)
                {
                    MenuItems[index] = _editingProduct;
                }

                CancelEdit(); 
            }
            else
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
        }

        private void StartEdit(Product? p)
        {
            if (p == null) return;

            _editingProduct = p;

            NewName = p.Name;
            NewPrice = p.Price;
            NewPrepTime = p.PreparationTime.TotalMinutes;
            SelectedCategory = p.Category;
            SelectedDietaryTag = p.DietaryTag;

            IsEditing = true;
            SubmitButtonText = "Зберегти";
            SubmitButtonColor = "#F39C12";
        }

        private void CancelEdit()
        {
            _editingProduct = null;
            IsEditing = false;

            SubmitButtonText = " Додати";
            SubmitButtonColor = "#27AE60";

            NewName = "Нова страва";
            NewPrice = 100;
            NewPrepTime = 3;
        }

        private void RemoveProduct(Product? p)
        {
            if (p != null && MenuItems.Contains(p))
            {
                MenuItems.Remove(p);

                if (p == _editingProduct) CancelEdit();
            }
        }
    }
}