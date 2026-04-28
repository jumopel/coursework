using coursework.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace coursework.Models
{
    public class Visitor : ObservableObject
    {
        public enum VisitorState
        {
            Wandering,   
            Searching, 
            Waiting,   
            Eating,    
            Leaving     
        }
        private static readonly Random _rnd = new Random();
        public string PreferredCuisine { get; private set; }
        public List<string> FavoriteDishes { get; private set; } 
        public double Satisfaction { get; private set; } = 1.0;
        public FestivalElement TargetDestination { get; set; }
        public Visitor(double startX, double startY) // доробить вподобання страв та кухонь
        {
            var cuisines = new[] { "" };
            PreferredCuisine = cuisines[_rnd.Next(cuisines.Length)];
            FavoriteDishes = new List<string> { "" };
        }

        private double _x;
        private double _y;
        private double _hunger;
        private VisitorState _state;
        private decimal _balance;
        public Guid Id { get; }   = Guid.NewGuid();
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
        public double Patience { get; private set; } 
        public double MovementSpeed { get; private set; }
        public bool IsHungry => Hunger > 55;
        public BaseZone ChooseZone(IEnumerable<BaseZone> zones)
        {
            if (!zones.Any()) return null;
            return zones.OrderByDescending(z => 
            {
                double score = z.Attractiveness;
                if (z.Theme == PreferredCuisine) score *= 1.5;
                score += _rnd.NextDouble() * 0.5;
                return score;
            }).First();
        }
        public void EvaluateZone(BaseZone currentZone)
        {
            var bestShop = currentZone.Shops
                .Where(s => s.AverageCheck <= Balance)
                .OrderByDescending(s => CalculateShopScore(s))
                .FirstOrDefault();

            if (bestShop == null || IsTooCrowded(bestShop))
            {
                Satisfaction -= 0.2;

                if (Satisfaction < 0.4 || Balance < 50)
                    State = VisitorState.Leaving;
                else
                    State = VisitorState.Searching;

                return;
            }
            TargetDestination = bestShop;
            State = VisitorState.Waiting;
        }
        public BaseShop ChooseShop(IEnumerable<BaseShop> availableShops)
        {
            if (!availableShops.Any()) return null;
            var affordableShops = availableShops.Where(s => s.AverageCheck <= Balance);
            if (!affordableShops.Any()) return null;
            return affordableShops.OrderByDescending(shop => CalculateUtility(shop)).First();
        }
        private double CalculateShopScore(BaseShop shop)
        {
            double score = shop.CurrentAttractiveness;
            //доробить перевірку на улюблені страви 
            double estimatedWait = (shop.CashierQueue * shop.OrderTakingTime.TotalMinutes) / (shop.CashiersCount > 0 ? shop.CashiersCount : 1);
            if (estimatedWait > Patience) score -= 2.0;

            return score;
        }
        private bool IsTooCrowded(BaseShop shop)
        {
            double estimatedWait = (shop.CashierQueue * shop.OrderTakingTime.TotalMinutes) / (shop.CashiersCount > 0 ? shop.CashiersCount : 1);
            return estimatedWait > Patience * 2;
        }
       
        private double CalculateUtility(BaseShop shop)
        {
            double waitTime = (shop.CurrentQueue * shop.OrderTakingTime.TotalMinutes) / (shop.CashiersCount > 0 ? shop.CashiersCount : 1);
            double patienceFactor = waitTime / Patience;

            return shop.CurrentAttractiveness - (patienceFactor * 0.5);
        }
        public void SpendMoney(decimal amount)
        {
            Balance -= amount;
            if (Balance < 50) State = VisitorState.Leaving; 
        }
    }
}
