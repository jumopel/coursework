using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using coursework.Core;
using coursework.DTO;

namespace coursework.Services
{
    public class CsvDataProvider : IFestivalDataProvider
    {
        public string ProviderName => "CSV Import Provider";
        public event Action? DataUpdated;

        private List<ZoneStateDto> _zones = new List<ZoneStateDto>();

        public CsvDataProvider(string filePath)
        {
            LoadDataFromCsv(filePath);
        }

        private void LoadDataFromCsv(string filePath)
        {
            try
            {
                var lines = File.ReadAllLines(filePath).Skip(1);
                var zoneDict = new Dictionary<string, ZoneStateDto>();

                foreach (var line in lines)
                {
                    var parts = line.Split(',');
                    if (parts.Length >= 6)
                    {
                        string zoneName = parts[0].Trim();
                        string shopName = parts[1].Trim();
                        decimal revenue = ParseDecimal(parts[2]);
                        int orders = int.Parse(parts[3].Trim());
                        decimal expenses = ParseDecimal(parts[4]);
                        string topDish = parts[5].Trim();

                        if (!zoneDict.ContainsKey(zoneName))
                        {
                            zoneDict[zoneName] = new ZoneStateDto
                            {
                                ZoneId = Guid.NewGuid(),
                                ZoneName = zoneName,
                                Theme = "Імпортована зона",
                                ShopsData = new List<ShopStateDto>()
                            };
                        }

                        var shop = new ShopStateDto
                        {
                            ShopId = Guid.NewGuid(),
                            ShopName = shopName,
                            CurrentRevenue = revenue,
                            TotalOrders = orders,
                            TotalExpenses = expenses,
                            NetProfit = revenue - expenses,
                            AverageTicket = orders > 0 ? Math.Round(revenue / orders, 2) : 0,
                            TopDishName = topDish,
                            WorstDishName = "Невідомо (з CSV)"
                        };

                        zoneDict[zoneName].ShopsData.Add(shop);
                    }
                }

                _zones = zoneDict.Values.ToList();

                foreach (var zone in _zones)
                {
                    zone.TotalRevenue = zone.ShopsData.Sum(s => s.CurrentRevenue);
                    zone.CurrentVisitors = zone.ShopsData.Sum(s => s.TotalOrders); 
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Помилка читання CSV: {ex.Message}", "Помилка", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        private decimal ParseDecimal(string value)
        {
            value = value.Trim().Replace(".", ","); 
            decimal.TryParse(value, out decimal result);
            return result;
        }

        public IEnumerable<ZoneStateDto> GetZonesSnapshot() => _zones;
        public IEnumerable<ShopStateDto> GetShopsSnapshot() => _zones.SelectMany(z => z.ShopsData);
        public void SendCommand(string command) { } 
    }
}