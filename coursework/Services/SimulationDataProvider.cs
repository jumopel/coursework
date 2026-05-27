using coursework.Core;
using coursework.DTO;
using System;
using System.Collections.Generic;
using System.Linq;

namespace coursework.Services
{
    internal class SimulationDataProvider : IFestivalDataProvider
    {
        private readonly SimulationEngine _engine;

        public string ProviderName => " ";

        public event Action? DataUpdated;

        public SimulationDataProvider(SimulationEngine engine)
        {
            _engine = engine;
            _engine.TickCompleted += () => DataUpdated?.Invoke();
        }

        public void SendCommand(string command)
        {
            _engine.ProcessCommand(command);
        }

        public IEnumerable<ZoneStateDto> GetZonesSnapshot()
        {
            var dtos = new List<ZoneStateDto>();
            foreach (var zone in _engine.Zones)
            {
                var zoneDto = new ZoneStateDto
                {
                    ZoneId = zone.Id,                 
                    ZoneName = zone.Name,             
                    Theme = zone.Theme,               
                    TotalRevenue = zone.TotalRevenue, 
                    CurrentVisitors = zone.CurrentVisitors, 
                    OccupancyRate = Math.Round(zone.OccupancyRate * 100, 1), 
                    ShopsData = new List<ShopStateDto>()
                };

                foreach (var shop in zone.Shops)
                {
                    var topDish = shop.Menu.OrderByDescending(p => p.SalesCount).FirstOrDefault();
                    var worstDish = shop.Menu.OrderBy(p => p.SalesCount).FirstOrDefault();

                    zoneDto.ShopsData.Add(new ShopStateDto
                    {
                        ShopId = shop.Id,
                        ShopName = shop.Name,
                        CurrentRevenue = shop.Revenue,
                        CurrentQueue = shop.CurrentQueue,
                        CongestionLevel = Math.Round(shop.CongestionLevel, 2),
                        Attractiveness = Math.Round(shop.CurrentAttractiveness, 2),
                        CashiersCount = shop.CashiersCount,
                        CooksCount = shop.CooksCount,

                        TotalExpenses = shop.BaseRent + shop.StaffsDailySalary,
                        NetProfit = shop.Revenue - (shop.BaseRent + shop.StaffsDailySalary),
                        TotalOrders = shop.TotalOrders,
                        AverageTicket = shop.TotalOrders > 0 ? Math.Round(shop.Revenue / shop.TotalOrders, 2) : 0,
                        AverageWaitTimeMinutes = Math.Round(shop.OrderTakingTime.TotalMinutes + shop.FoodPreparationTime.TotalMinutes, 1),
                        TopDishName = topDish != null ? $"{topDish.Name} ({topDish.SalesCount} шт)" : "Немає даних",
                        WorstDishName = worstDish != null ? $"{worstDish.Name} ({worstDish.SalesCount} шт)" : "Немає даних"
                    });
                }

                dtos.Add(zoneDto);
            }
            return dtos;
        }

        public IEnumerable<ShopStateDto> GetShopsSnapshot()
        {
            var dtos = new List<ShopStateDto>();
            foreach (var zone in _engine.Zones)
            {
                foreach (var shop in zone.Shops)
                {
                    dtos.Add(new ShopStateDto
                    {
                        ShopId = shop.Id,                        
                        ShopName = shop.Name,                    
                        CurrentRevenue = shop.Revenue,           
                        CurrentQueue = shop.CurrentQueue,        
                        CongestionLevel = Math.Round(shop.CongestionLevel, 2),       
                        Attractiveness = Math.Round(shop.CurrentAttractiveness, 2),
                        CashiersCount = shop.CashiersCount,
                        CooksCount = shop.CooksCount,

                    });
                }
            }
            return dtos;
        }
    }
}