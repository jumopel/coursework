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
        public BaseShop CreateShop(string name, int cashiers, int cooks, decimal rent, decimal salary, double orderTakingMinutes, double prepMinutes, double x, double y)
        {
            BaseShop shop = new BaseShop();

            shop.Name = name;
            shop.X = x;
            shop.Y = y;
            shop.CashiersCount = cashiers;
            shop.CooksCount = cooks;
            shop.BaseRent = rent;
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
        public void InitializeDefaultMenu(coursework.Core.BaseShop shop, coursework.Models.CuisineType cuisine)
        {
            shop.Menu.Clear();

            switch (cuisine)
            {
                case CuisineType.Italian:
                    AddProductToShop(shop, "Піца Маргарита", 180, 60, TimeSpan.FromMinutes(3), ProductCategory.MainCourse, DietaryType.Standard, cuisine);
                    AddProductToShop(shop, "Паста Карбонара", 210, 75, TimeSpan.FromMinutes(4), ProductCategory.MainCourse, DietaryType.Standard, cuisine);
                    AddProductToShop(shop, "Домашнє Лімончелло", 90, 30, TimeSpan.FromMinutes(1), ProductCategory.Drink, DietaryType.Standard, cuisine);
                    break;

                case CuisineType.Asian:
                    AddProductToShop(shop, "Суші Рол Філадельфія", 240, 90, TimeSpan.FromMinutes(5), ProductCategory.MainCourse, DietaryType.Standard, cuisine);
                    AddProductToShop(shop, "Локшина WOK з куркою", 160, 50, TimeSpan.FromMinutes(3), ProductCategory.MainCourse, DietaryType.Standard, cuisine);
                    AddProductToShop(shop, "Чай Матча", 70, 20, TimeSpan.FromMinutes(1.5), ProductCategory.Drink, DietaryType.Standard, cuisine);
                    break;

                case CuisineType.American:
                    AddProductToShop(shop, "Дабл Бургер Меню", 195, 65, TimeSpan.FromMinutes(2.5), ProductCategory.MainCourse, DietaryType.Standard, cuisine);
                    AddProductToShop(shop, "Картопля Фрі з соусом", 65, 15, TimeSpan.FromMinutes(2), ProductCategory.Snack, DietaryType.Standard, cuisine);
                    AddProductToShop(shop, "Холодна Кола", 40, 10, TimeSpan.FromMinutes(0.5), ProductCategory.Drink, DietaryType.Standard, cuisine);
                    break;

                case CuisineType.Ukrainian:
                    AddProductToShop(shop, "Борщ з пампушками", 140, 45, TimeSpan.FromMinutes(4), ProductCategory.MainCourse, DietaryType.Standard, cuisine);
                    AddProductToShop(shop, "Вареники з лівером", 110, 35, TimeSpan.FromMinutes(3.5), ProductCategory.MainCourse, DietaryType.Standard, cuisine);
                    AddProductToShop(shop, "Холодний Узвар", 35, 8, TimeSpan.FromMinutes(0.5), ProductCategory.Drink, DietaryType.Standard, cuisine);
                    break;

                default: 
                    AddProductToShop(shop, "Стандартний Сендвіч", 95, 30, TimeSpan.FromMinutes(2), ProductCategory.MainCourse, DietaryType.Standard, CuisineType.Universal);
                    AddProductToShop(shop, "Кава Американо", 45, 12, TimeSpan.FromMinutes(1), ProductCategory.Drink, DietaryType.Standard, CuisineType.Universal); break;
            }
        }
    }
}
