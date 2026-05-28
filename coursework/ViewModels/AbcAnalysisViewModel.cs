using coursework.Core;
using coursework.DTO;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Threading;

namespace coursework.ViewModels
{
    public class AbcItem
    {
        public string ShopName { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public int SalesCount { get; set; }
        public decimal TotalRevenue { get; set; }
        public double CumulativePercentage { get; set; }
        public string Category { get; set; } = string.Empty;
        public string CategoryColor { get; set; } = string.Empty;
    }

    public class AbcAnalysisViewModel : ObservableObject
    {
        private readonly IFestivalDataProvider _dataProvider;
        private readonly DispatcherTimer _timer;

        public ObservableCollection<AbcItem> AbcItems { get; set; }

        public AbcAnalysisViewModel(IFestivalDataProvider dataProvider)
        {
            _dataProvider = dataProvider;
            AbcItems = new ObservableCollection<AbcItem>();

            UpdateData();

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(5);
            _timer.Tick += (s, e) => UpdateData();
            _timer.Start();
        }

        public void StopTimer()
        {
            _timer.Stop();
        }

        private void UpdateData()
        {
            var zones = _dataProvider.GetZonesSnapshot();

            var allProducts = zones.SelectMany(z => z.ShopsData)
                                   .SelectMany(s => s.MenuStats)
                                   .Where(p => p.SalesCount > 0)
                                   .OrderByDescending(p => p.TotalProfit)
                                   .ToList();

            decimal totalFestivalProfit = allProducts.Sum(p => p.TotalProfit);
            decimal cumulativeProfit = 0;

            AbcItems.Clear();

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