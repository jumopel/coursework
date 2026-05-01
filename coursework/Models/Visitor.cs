using coursework.Core;
using coursework.Models; 
using System;
using System.Collections.Generic;
using System.Linq; 

namespace coursework.Models
{
    public class Visitor : ObservableObject
    {
        public enum VisitorState { Wandering, Searching, Waiting, Eating, Leaving }

        private static readonly Random _rnd = new Random();

        // Агентні властивості
        public DietaryType DietaryPreference { get; private set; }
        public CuisineType PreferredCuisine { get; private set; }
        public double Patience { get; private set; }
        public double Satisfaction { get; private set; } = 1.0;
        public double MovementSpeed { get; private set; }
        public bool IsHungry => Hunger > 55;
        public FestivalElement? TargetDestination { get; set; }

        private double _x;
        private double _y;
        private double _hunger;
        private VisitorState _state;
        private decimal _balance;

        public Guid Id { get; } = Guid.NewGuid();
        public double X { get => _x; set => SetProperty(ref _x, value); }
        public double Y { get => _y; set => SetProperty(ref _y, value); }

        public double Hunger
        {
            get => _hunger;
            set { if (SetProperty(ref _hunger, value)) OnPropertyChanged(nameof(IsHungry)); }
        }

        public VisitorState State { get => _state; set => SetProperty(ref _state, value); }

        public decimal Balance
        {
            get => _balance;
            private set => SetProperty(ref _balance, value);
        }

        public Visitor(decimal initialBalance, DietaryType diet, CuisineType cuisine, double startX = 0, double startY = 0)
        {
            Balance = initialBalance;
            DietaryPreference = diet;
            PreferredCuisine = cuisine;
            X = startX;
            Y = startY;
            Patience = 10.0 + _rnd.NextDouble() * 35.0;
            MovementSpeed = 1.0 + _rnd.NextDouble() * 1.5;
            State = VisitorState.Wandering;
            _hunger = 0;
            TargetDestination = null;
        }

        public BaseZone? ChooseZone(IEnumerable<BaseZone> zones)
        {
            if (zones == null || !zones.Any()) return null;

            return zones.OrderByDescending(z =>
            {
                double score = z.Attractiveness;
                if (z.ZoneCuisine == PreferredCuisine || z.ZoneCuisine == CuisineType.Universal)
                    score *= 1.5;
                if (z.OccupancyRate > 0.9)
                    score -= 2.0;
                score += _rnd.NextDouble() * 0.5;
                return score;
            }).FirstOrDefault();
        }

        public BaseShop? ChooseShop(BaseZone currentZone)
        {
            if (currentZone == null || !currentZone.Shops.Any()) return null;

            var suitableShops = currentZone.Shops.Where(s =>
                s.Menu.Any(p =>
                    (DietaryPreference == DietaryType.Standard || p.DietaryTag == DietaryPreference) &&
                    p.Price <= Balance)
            ).ToList();

            if (!suitableShops.Any())
            {
                Satisfaction -= 0.2;
                State = (Satisfaction < 0.4 || Balance < 50) ? VisitorState.Leaving : VisitorState.Searching;
                return null;
            }

            var bestShop = suitableShops.OrderByDescending(CalculateShopScore).FirstOrDefault();

            if (bestShop != null)
            {
                TargetDestination = bestShop;
                State = VisitorState.Waiting;
            }

            return bestShop;
        }

        private double CalculateShopScore(BaseShop shop)
        {
            double score = shop.CurrentAttractiveness;
            int suitableDishesCount = shop.Menu.Count(p =>
                (DietaryPreference == DietaryType.Standard || p.DietaryTag == DietaryPreference) &&
                p.Price <= Balance);

            score += suitableDishesCount * 0.1;

            double estimatedWait = (shop.CurrentQueue * shop.OrderTakingTime.TotalMinutes) / (shop.CashiersCount > 0 ? shop.CashiersCount : 1);

            if (estimatedWait > Patience * 1.5) score -= 5.0;
            else if (estimatedWait > Patience) score -= 2.0;

            return score;
        }

        public List<Product> MakeOrder(BaseShop shop)
        {
            var order = new List<Product>();
            var availableMenu = shop.Menu
                .Where(p => DietaryPreference == DietaryType.Standard || p.DietaryTag == DietaryPreference)
                .OrderBy(_ => _rnd.Next())
                .ToList();

            foreach (var product in availableMenu)
            {
                if (product.Price <= Balance)
                {
                    order.Add(product);
                    SpendMoney(product.Price);
                    Hunger = Math.Max(0, Hunger - 40);
                }
            }

            if (order.Any()) Satisfaction = Math.Min(1.0, Satisfaction + 0.1);
            return order;
        }

        public void SpendMoney(decimal amount)
        {
            Balance -= amount;
            if (Balance < 50) State = VisitorState.Leaving;
        }
    }
}