using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using coursework.Core;
using coursework.Models;

namespace coursework.Services
{
    internal class DataService
    {
        public BaseZone CreateZoneFromUser(string name, string theme, int capacity, CuisineType cuisine)
        {
            var zone = new FoodZone
            {
                Name = name,
                Theme = theme,
                Capacity = capacity,
                ZoneCuisine = cuisine,
                Attractiveness = 1.0 
            };
            return zone;
        }
        public BaseShop CreateShop(string name, int cashiers, int cooks, decimal rent, decimal salary, double orderTakingMinutes, double prepMinutes, double x, double y)
        {
            var shop = new FastFoodShop
            {
                Name = name,
                X = x,
                Y = y,
                CashiersCount = cashiers,
                CooksCount = cooks,
                DailyRent = rent,
                StaffsDailySalary = salary,
                OrderTakingTime = TimeSpan.FromMinutes(orderTakingMinutes),
                FoodPreparationTime = TimeSpan.FromMinutes(prepMinutes)
            };
            return shop;
        }
        public void AddProductToShop(BaseShop shop, string name, decimal price, decimal cost)
        {
            var product = new Product(
                name,
                price,
                cost,
                TimeSpan.FromMinutes(5), 
                ProductCategory.MainCourse,
                DietaryType.Standard,
                CuisineType.Universal
            );
            shop.Menu.Add(product);
        }
       
    }
}
