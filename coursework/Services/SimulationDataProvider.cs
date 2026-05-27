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

        public string ProviderName => "Simulation Provider";

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

                    decimal fixedCosts = shop.BaseRent + shop.StaffsDailySalary;
                    decimal remainingCosts = fixedCosts - shop.Revenue;
                    decimal avgCheck = shop.TotalOrders > 0 ? shop.Revenue / shop.TotalOrders : 0;

                    string payoffForecast;
                    if (remainingCosts <= 0)
                    {
                        payoffForecast = "✅ Приносить чистий прибуток";
                    }
                    else if (avgCheck > 0)
                    {
                        int clientsNeeded = (int)Math.Ceiling(remainingCosts / avgCheck);
                        payoffForecast = $"⏳ Ще ~{clientsNeeded} замовлень";
                    }
                    else
                    {
                        payoffForecast = "⚠️ Чекаємо першого клієнта";
                    }

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
                        TotalExpenses = fixedCosts,
                        NetProfit = shop.Revenue - fixedCosts,
                        TotalOrders = shop.TotalOrders,
                        AverageTicket = avgCheck > 0 ? Math.Round(avgCheck, 2) : 0,
                        AverageWaitTimeMinutes = Math.Round(shop.OrderTakingTime.TotalMinutes + shop.FoodPreparationTime.TotalMinutes, 1),
                        TopDishName = topDish != null ? $"{topDish.Name} ({topDish.SalesCount} шт)" : "Немає даних",
                        WorstDishName = worstDish != null ? $"{worstDish.Name} ({worstDish.SalesCount} шт)" : "Немає даних",

                        FixedCosts = fixedCosts,
                        BreakEvenProgress = fixedCosts > 0 ? Math.Min((double)(shop.Revenue / fixedCosts) * 100, 100) : 100,
                        EstimatedPayoff = payoffForecast,

                        MenuStats = shop.Menu.Select(p => new ProductStatsDto
                        {
                            ShopName = shop.Name,
                            ProductName = p.Name,
                            Price = p.Price,
                            CostPrice = p.CostPrice, 
                            SalesCount = p.SalesCount
                        }).ToList()
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
                    decimal fixedCosts = shop.BaseRent + shop.StaffsDailySalary;
                    decimal remainingCosts = fixedCosts - shop.Revenue;
                    decimal avgCheck = shop.TotalOrders > 0 ? shop.Revenue / shop.TotalOrders : 0;

                    string payoffForecast;
                    if (remainingCosts <= 0)
                    {
                        payoffForecast = "✅ Приносить чистий прибуток";
                    }
                    else if (avgCheck > 0)
                    {
                        int clientsNeeded = (int)Math.Ceiling(remainingCosts / avgCheck);
                        payoffForecast = $"⏳ Ще ~{clientsNeeded} замовлень";
                    }
                    else
                    {
                        payoffForecast = "⚠️ Чекаємо першого клієнта";
                    }

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

                        FixedCosts = fixedCosts,
                        BreakEvenProgress = fixedCosts > 0 ? Math.Min((double)(shop.Revenue / fixedCosts) * 100, 100) : 100,
                        EstimatedPayoff = payoffForecast,

                        MenuStats = shop.Menu.Select(p => new ProductStatsDto
                        {
                            ShopName = shop.Name,
                            ProductName = p.Name,
                            Price = p.Price,
                            CostPrice = p.CostPrice, 
                            SalesCount = p.SalesCount
                        }).ToList()
                    });
                }
            }
            return dtos;
        }
    }
}