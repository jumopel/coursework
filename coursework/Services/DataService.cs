using coursework.Core;
using coursework.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Policy;
using System.Text.Json;
using static coursework.Core.BaseShop;

namespace coursework.Services
{
    internal class DataService
    {
        public BaseZone CreateZoneFromUser(string name, string theme, int capacity, CuisineType cuisine)
        {
            var zone = new BaseZone
            {
                Name = name,
                Theme = theme,
                Capacity = capacity,
                ZoneCuisine = cuisine,
                Attractiveness = 1.0
            };
            return zone;
        }
        public BaseShop CreateShop(string name, ShopType type, int cashiers, int cooks, decimal rent, decimal salary, double orderTakingMinutes, double prepMinutes, double x, double y)
        {
            BaseShop shop = type == ShopType.FastFood ? new FastFoodShop() : new RestaurantShop();

            shop.Name = name;
            shop.X = x;
            shop.Y = y;
            shop.CashiersCount = cashiers;
            shop.CooksCount = cooks;
            shop.DailyRent = rent;
            shop.StaffsDailySalary = salary;
            shop.OrderTakingTime = TimeSpan.FromMinutes(orderTakingMinutes);
            shop.FoodPreparationTime = TimeSpan.FromMinutes(prepMinutes);

            return shop;
        }
        public void AddProductToShop(BaseShop shop, string name, decimal price, decimal cost,
          TimeSpan preparationTime, ProductCategory category = ProductCategory.MainCourse,
          DietaryType dietaryType = DietaryType.Standard, CuisineType cuisineType = CuisineType.Universal)
        {
            var product = new Product(
                name,
                price,
                cost,
                preparationTime,
                category,
                dietaryType,
                cuisineType
            );
            shop.Menu.Add(product);
        }

    }
}
