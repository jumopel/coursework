using coursework.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace coursework.Models
{
    public enum ProductCategory
    {
        MainCourse, 
        Snack,      
        Drink,     
        Dessert     
    }

    public enum DietaryType
    {
        Standard,   
        Vegetarian, 
        Vegan       
    }

    public enum CuisineType
    {
        Universal,  
        Ukrainian,
        American,   
        Asian,      
        Italian     
    }

    public class Product : ObservableObject
    {
        private string _name = string.Empty;
        private decimal _price;
        private TimeSpan _preparationTime;
        private ProductCategory _category;
        private DietaryType _dietaryTag;
        private CuisineType _cuisine;
        private decimal _costPrice;
        public Guid Id { get; } = Guid.NewGuid();

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public decimal Price
        {
            get => _price;
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException(nameof(Price), "Ціна не може бути від'ємною.");
                SetProperty(ref _price, value);
            }
        }
        public decimal CostPrice
        {
            get => _costPrice;
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException(nameof(CostPrice), "Собівартість не може бути від'ємною.");
                SetProperty(ref _costPrice, value);
            }
        }

        public TimeSpan PreparationTime
        {
            get => _preparationTime;
            set => SetProperty(ref _preparationTime, value);
        }

        public ProductCategory Category
        {
            get => _category;
            set => SetProperty(ref _category, value);
        }

        public DietaryType DietaryTag
        {
            get => _dietaryTag;
            set => SetProperty(ref _dietaryTag, value);
        }

        public CuisineType Cuisine
        {
            get => _cuisine;
            set => SetProperty(ref _cuisine, value);
        }

        public Product() { }

        public Product(string name, decimal price, decimal costPrice, TimeSpan prepTime, ProductCategory category, DietaryType dietaryTag, CuisineType cuisine)
        {
            Name = name;
            Price = price;
            CostPrice = costPrice;
            PreparationTime = prepTime;
            Category = category;
            DietaryTag = dietaryTag;
            Cuisine = cuisine;
        }
    }
}
