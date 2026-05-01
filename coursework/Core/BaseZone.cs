using coursework.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace coursework.Core
{
    public abstract class BaseZone : FestivalElement
    {
        private double _attractiveness;
        private string _theme = string.Empty;
        private int _capacity;

        public double Attractiveness
        {
            get => _attractiveness;
            set => SetProperty(ref _attractiveness, value);
        }
        public string Theme
        {
            get => _theme;
            set => SetProperty(ref _theme, value);
        }
        public int Capacity
        {
            get => _capacity;
            set => SetProperty(ref _capacity, value);
        }

        public ObservableCollection<BaseShop> Shops { get; private set; }

        public decimal TotalRevenue => Shops.Sum(s => s.Revenue);
        public decimal TotalNetProfit => Shops.Sum(s => s.NetProfit);

        public int CurrentVisitors => Shops.Sum(s => s.CurrentQueue);
        public double AverageQueueLength => Shops.Any() ? Shops.Average(s => s.CurrentQueue) : 0;
        public double OccupancyRate => Capacity > 0 ? (double)CurrentVisitors / Capacity : 0;
        private CuisineType _zoneCuisine;
        public CuisineType ZoneCuisine
        {
            get => _zoneCuisine;
            set => SetProperty(ref _zoneCuisine, value);
        }
        protected BaseZone()
        {
            Shops = new ObservableCollection<BaseShop>();

            Shops.CollectionChanged += (s, e) => {
                if (e.NewItems != null)
                {
                    foreach (BaseShop shop in e.NewItems)
                        shop.PropertyChanged += Shop_PropertyChanged;
                }
                if (e.OldItems != null)
                {
                    foreach (BaseShop shop in e.OldItems)
                        shop.PropertyChanged -= Shop_PropertyChanged;
                }

                UpdateZoneMetrics();
            };
        }
        private void Shop_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(BaseShop.Revenue) ||
                e.PropertyName == nameof(BaseShop.NetProfit) ||
                e.PropertyName == nameof(BaseShop.CurrentQueue))
            {
                UpdateZoneMetrics();
            }
        }
        private void UpdateZoneMetrics()
        {
            OnPropertyChanged(nameof(TotalRevenue));
            OnPropertyChanged(nameof(TotalNetProfit));
            OnPropertyChanged(nameof(CurrentVisitors));
            OnPropertyChanged(nameof(AverageQueueLength));
            OnPropertyChanged(nameof(OccupancyRate)); 
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
