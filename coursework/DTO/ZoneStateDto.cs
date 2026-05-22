using System.Collections.Generic;
using coursework.Core;
using System;

namespace coursework.DTO
{
    public class ZoneStateDto : ObservableObject
    {
        private int _currentVisitors;
        private double _occupancyRate;
        private decimal _totalRevenue;

        public string ZoneName { get; set; } = string.Empty;
        public string Theme { get; set; } = string.Empty;
        public Guid ZoneId { get; set; }
        public int CurrentVisitors { get => _currentVisitors; set => SetProperty(ref _currentVisitors, value); }
        public double OccupancyRate { get => _occupancyRate; set => SetProperty(ref _occupancyRate, value); }
        public decimal TotalRevenue { get => _totalRevenue; set => SetProperty(ref _totalRevenue, value); }

        public List<ShopStateDto> ShopsData { get; set; } = new List<ShopStateDto>();
    }
}