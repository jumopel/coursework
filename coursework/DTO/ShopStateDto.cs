using System;
using System.Collections.Generic;
using coursework.Core; 

namespace coursework.DTO
{
    public class ShopStateDto : ObservableObject
    {
        public Guid ShopId { get; set; }
        public string ShopName { get; set; } = string.Empty;


        private decimal _currentRevenue;
        public decimal CurrentRevenue
        {
            get => _currentRevenue;
            set => SetProperty(ref _currentRevenue, value);
        }

        private decimal _fixedCosts;
        public decimal FixedCosts
        {
            get => _fixedCosts;
            set => SetProperty(ref _fixedCosts, value);
        }

        private string _estimatedPayoff = string.Empty;
        public string EstimatedPayoff
        {
            get => _estimatedPayoff;
            set => SetProperty(ref _estimatedPayoff, value);
        }

        private double _breakEvenProgress;
        public double BreakEvenProgress
        {
            get => _breakEvenProgress;
            set
            {
                if (SetProperty(ref _breakEvenProgress, value))
                {
                    OnPropertyChanged(nameof(BreakEvenStatus));
                    OnPropertyChanged(nameof(ProgressColor));
                }
            }
        }

        public string BreakEvenStatus => BreakEvenProgress >= 100 ? "Окупився" : "В мінусі";
        public string ProgressColor => BreakEvenProgress >= 100 ? "#27AE60" : "#E74C3C";

        public int CurrentQueue { get; set; }
        public double CongestionLevel { get; set; }
        public double Attractiveness { get; set; }
        public int CashiersCount { get; set; }
        public int CooksCount { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal NetProfit { get; set; }
        public int TotalOrders { get; set; }
        public decimal AverageTicket { get; set; }
        public double AverageWaitTimeMinutes { get; set; }
        public string TopDishName { get; set; } = string.Empty;
        public string WorstDishName { get; set; } = string.Empty;
        public List<ProductStatsDto> MenuStats { get; set; } = new List<ProductStatsDto>();
    }
}