using System;
using System.Collections.Generic;
using System.Linq;
using coursework.DTO;

namespace coursework.ViewModels
{
    public class AbcItem
    {
        public string ShopName { get; set; }
        public string ProductName { get; set; }
        public int SalesCount { get; set; }
        public decimal TotalRevenue { get; set; }
        public double CumulativePercentage { get; set; }
        public string Category { get; set; }
        public string CategoryColor { get; set; }
    }

    public class AbcAnalysisViewModel
    {
        public List<AbcItem> AbcItems { get; set; }

        public AbcAnalysisViewModel(IEnumerable<ZoneStateDto> zones)
        {
            var allProducts = zones.SelectMany(z => z.ShopsData)
                                   .SelectMany(s => s.MenuStats)
                                   .Where(p => p.SalesCount > 0)
                                   .OrderByDescending(p => p.TotalProfit)
                                   .ToList();

            decimal totalFestivalProfit = allProducts.Sum(p => p.TotalProfit);
            decimal cumulativeProfit = 0;
            AbcItems = new List<AbcItem>();

            foreach (var p in allProducts)
            {
                cumulativeProfit += p.TotalProfit;
                double cumulativePercent = totalFestivalProfit > 0
                    ? (double)(cumulativeProfit / totalFestivalProfit) * 100
                    : 0;

                string category = cumulativePercent <= 80 ? "A (Флагман)" : cumulativePercent <= 95 ? "B (Середняк)" : "C (Аутсайдер)";
                string color = cumulativePercent <= 80 ? "#27AE60" : cumulativePercent <= 95 ? "#F39C12" : "#E74C3C";

                AbcItems.Add(new AbcItem
                {
                    ShopName = p.ShopName,
                    ProductName = p.ProductName,
                    SalesCount = p.SalesCount,
                    TotalRevenue = Math.Round(p.TotalProfit, 2), 
                    CumulativePercentage = Math.Round(cumulativePercent, 1),
                    Category = category,
                    CategoryColor = color
                });
            }
        }
    }
}