using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace coursework.Core
{
    public abstract class BaseZone : FestivalElement
    {
        public ObservableCollection<BaseShop> Shops { get; private set; }

        public double Attractiveness { get; set; }
        public string Theme { get; set; } = string.Empty;
        public int Capacity { get;  set; }

        public decimal TotalRevenue => Shops.Sum(s => s.Revenue);
        public decimal TotalNetProfit => Shops.Sum(s => s.NetProfit);

        public int CurrentVisitors => Shops.Sum(s => s.CurrentQueue);
        public double AverageQueueLength => Shops.Any() ? Shops.Average(s => s.CurrentQueue) : 0;

        protected BaseZone()
        {
            Shops = new ObservableCollection<BaseShop>();
        }
        public void AddShop(BaseShop shop) =>  Shops.Add(shop);
        public void RemoveShop(BaseShop shop) => Shops.Remove(shop);

        public override double CalculateKPI()
        {
            if (!Shops.Any()) return 0;
            double avgShopKPI = Shops.Average(s => s.CalculateKPI());
            double crowdingPenalty = Capacity > 0 ? (double)CurrentVisitors / Capacity : 0;
            if (crowdingPenalty > 1) crowdingPenalty = 1;
            double finalKPI = (avgShopKPI * Attractiveness) * (1 - (crowdingPenalty * 0.5));

            return Math.Round(finalKPI, 2);
        }
        public override string GetKPIDescription()
        {
            return "KPI зони базується на середньому KPI її магазинів, " +
             "помноженому на базову привабливість зони. Показник динамічно знижується " +
             $"до 50%, якщо кількість відвідувачів перевищує місткість ({Capacity} осіб).";
        }
        public override string GetReport()
        {
            return $"Зона: '{Name}' [{Theme}]. " +
                               $"Магазинів: {Shops.Count}. " +
                               $"Чистий прибуток: {TotalNetProfit} грн. " +
                               $"Людей у зоні: {CurrentVisitors}/{Capacity} (Сер. черга: {Math.Round(AverageQueueLength, 1)}). " +
                               $"KPI: {CalculateKPI()}.";
        }



    }
}
