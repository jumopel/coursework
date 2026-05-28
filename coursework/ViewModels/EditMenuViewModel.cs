using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows; 
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
        private decimal _newCostPrice = 40;
        private double _newPrepTime = 3;
        private ProductCategory _selectedCategory = ProductCategory.MainCourse;
        private DietaryType _selectedDietaryTag = DietaryType.Standard;

        public string NewName { get => _newName; set => SetProperty(ref _newName, value); }
        public decimal NewPrice
        {
            get => _newPrice;
            set
            {
                SetProperty(ref _newPrice, value);
                if (!_costPriceEditedManually)
                    NewCostPrice = Math.Round(value * 0.4m, 2);
                
                (SubmitProductCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }
        public decimal NewCostPrice
        {
            get => _newCostPrice;
            set
            {
                _costPriceEditedManually = true;
                SetProperty(ref _newCostPrice, value);
                OnPropertyChanged(nameof(MarginPercent));
                OnPropertyChanged(nameof(MarginHint));
                
                (SubmitProductCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }
        public double NewPrepTime { get => _newPrepTime; set => SetProperty(ref _newPrepTime, value); }
        public ProductCategory SelectedCategory { get => _selectedCategory; set => SetProperty(ref _selectedCategory, value); }
        public DietaryType SelectedDietaryTag { get => _selectedDietaryTag; set => SetProperty(ref _selectedDietaryTag, value); }

        public string MarginPercent => NewPrice > 0
            ? $"{Math.Round((NewPrice - NewCostPrice) / NewPrice * 100, 1)}%"
            : "—";
        public string MarginHint => NewPrice > 0
            ? $"Маржа: {NewPrice - NewCostPrice:N2} грн ({MarginPercent})"
            : "Введіть ціну";

        public List<ProductCategory> Categories { get; } = Enum.GetValues(typeof(ProductCategory)).Cast<ProductCategory>().ToList();
        public List<DietaryType> DietaryTags { get; } = Enum.GetValues(typeof(DietaryType)).Cast<DietaryType>().ToList();

        private bool _isEditing = false;
        private Product? _editingProduct;
        private bool _costPriceEditedManually = false;

        public bool IsEditing { get => _isEditing; set => SetProperty(ref _isEditing, value); }

        private string _submitButtonText = " Додати";
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
                !string.IsNullOrWhiteSpace(NewName) && NewPrice > 0 && NewPrepTime > 0 && NewCostPrice >= 0);

            RemoveProductCommand = new RelayCommand(param => RemoveProduct(param as Product));
            StartEditCommand = new RelayCommand(param => StartEdit(param as Product));
            CancelEditCommand = new RelayCommand(_ => CancelEdit());
        }

        private void SubmitProduct()
        {
            if (NewCostPrice >= NewPrice)
            {
                var result = MessageBox.Show(
                    "Собівартість цієї страви більша або дорівнює ціні продажу.\nВи впевнені, що хочете продавати її у збиток (чи в нуль)?",
                    "Попередження про маржинальність",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.No)
                {
                    return; 
                }
            }

            if (IsEditing && _editingProduct != null)
            {
                _editingProduct.Name = NewName;
                _editingProduct.Price = NewPrice;
                _editingProduct.CostPrice = NewCostPrice;
                _editingProduct.PreparationTime = TimeSpan.FromMinutes(NewPrepTime);
                _editingProduct.Category = SelectedCategory;
                _editingProduct.DietaryTag = SelectedDietaryTag;

                int index = MenuItems.IndexOf(_editingProduct);
                if (index >= 0)
                    MenuItems[index] = _editingProduct;

                CancelEdit();
            }
            else
            {
                var product = new Product
                {
                    Name = NewName,
                    Price = NewPrice,
                    CostPrice = NewCostPrice,
                    PreparationTime = TimeSpan.FromMinutes(NewPrepTime),
                    Cuisine = _shop.ShopCuisine,
                    Category = SelectedCategory,
                    DietaryTag = SelectedDietaryTag
                };
                MenuItems.Add(product);
                NewName = "Ще одна страва";
                _costPriceEditedManually = false;
                NewPrice = 100;
            }
        }

        private void StartEdit(Product? p)
        {
            if (p == null) return;

            _editingProduct = p;
            _costPriceEditedManually = true; 

            NewName = p.Name;
            NewPrice = p.Price;
            NewCostPrice = p.CostPrice;
            NewPrepTime = p.PreparationTime.TotalMinutes;
            SelectedCategory = p.Category;
            SelectedDietaryTag = p.DietaryTag;

            IsEditing = true;
            SubmitButtonText = "Зберегти";
            SubmitButtonColor = "#F39C12";
            
            (SubmitProductCommand as RelayCommand)?.RaiseCanExecuteChanged();
        }

        private void CancelEdit()
        {
            _editingProduct = null;
            IsEditing = false;
            _costPriceEditedManually = false;

            SubmitButtonText = " Додати";
            SubmitButtonColor = "#27AE60";

            NewName = "Нова страва";
            NewPrice = 100;
            NewCostPrice = 40;
            NewPrepTime = 3;
            
            (SubmitProductCommand as RelayCommand)?.RaiseCanExecuteChanged();
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