using coursework.Core;
using System;

namespace coursework.DTO
{
    public class ShopStateDto : ObservableObject
    {
        private int _currentQueue;
        private double _congestionLevel;
        private double _attractiveness;
        private decimal _currentRevenue;
        private int _cashiersCount;
        private int _cooksCount;
        public string ShopName { get; set; } = string.Empty;
        public Guid ShopId { get; set; }
        public int CurrentQueue { get => _currentQueue; set => SetProperty(ref _currentQueue, value); }
        public double CongestionLevel { get => _congestionLevel; set => SetProperty(ref _congestionLevel, value); }
        public double Attractiveness { get => _attractiveness; set => SetProperty(ref _attractiveness, value); }
        public decimal CurrentRevenue { get => _currentRevenue; set => SetProperty(ref _currentRevenue, value); }
        public int CashiersCount { get => _cashiersCount; set => SetProperty(ref _cashiersCount, value); }
        public int CooksCount { get => _cooksCount; set => SetProperty(ref _cooksCount, value); }
        public decimal TotalExpenses { get; set; }
        public decimal NetProfit { get; set; }
        public int TotalOrders { get; set; }
        public decimal AverageTicket { get; set; }
        public double AverageWaitTimeMinutes { get; set; }
        public string TopDishName { get; set; }
        public string WorstDishName { get; set; }
        public decimal FixedCosts { get; set; } 
        public double BreakEvenProgress { get; set; } 
        public string BreakEvenStatus => BreakEvenProgress >= 100 ? "Окупився" : "В мінусі";
        public string ProgressColor => BreakEvenProgress >= 100 ? "#27AE60" : "#E74C3C";
        public string EstimatedPayoff { get; set; } = string.Empty;
        public System.Collections.Generic.List<ProductStatsDto> MenuStats { get; set; } = new System.Collections.Generic.List<ProductStatsDto>();
    }
}