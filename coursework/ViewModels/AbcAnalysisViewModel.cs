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
                                   .OrderByDescending(p => p.TotalRevenue)
                                   .ToList();

            decimal totalFestivalRevenue = allProducts.Sum(p => p.TotalRevenue);
            decimal cumulativeRevenue = 0;
            AbcItems = new List<AbcItem>();

            foreach (var p in allProducts)
            {
                cumulativeRevenue += p.TotalRevenue;
                double cumulativePercent = totalFestivalRevenue > 0
                    ? (double)(cumulativeRevenue / totalFestivalRevenue) * 100
                    : 0;

                string category, color;

                if (cumulativePercent <= 80)
                {
                    category = "A (Флагман)";
                    color = "#27AE60"; 
                }
                else if (cumulativePercent <= 95)
                {
                    category = "B (Середняк)";
                    color = "#F39C12"; 
                }
                else
                {
                    category = "C (Аутсайдер)";
                    color = "#E74C3C"; 
                }

                AbcItems.Add(new AbcItem
                {
                    ShopName = p.ShopName,
                    ProductName = p.ProductName,
                    SalesCount = p.SalesCount,
                    TotalRevenue = p.TotalRevenue,
                    CumulativePercentage = Math.Round(cumulativePercent, 1),
                    Category = category,
                    CategoryColor = color
                });
            }
        }
    }
}