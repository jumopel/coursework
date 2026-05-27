using System.Collections.Generic;
using System.Linq;
using LiveCharts;
using LiveCharts.Wpf;
using coursework.Core;
using coursework.DTO;

namespace coursework.ViewModels
{
    public class AnalyticsViewModel : ObservableObject
    {
        private readonly IFestivalDataProvider _dataProvider;

        public SeriesCollection RevenueByZoneChart { get; set; }
        public SeriesCollection TopShopsChart { get; set; }
        public List<string> TopShopsLabels { get; set; }

        public decimal TotalRevenue { get; set; }
        public int TotalVisitors { get; set; }
        public System.Windows.Input.ICommand ExportOverallReportCommand { get; }
        private readonly coursework.Services.ExportService _exportService;
        public AnalyticsViewModel(IFestivalDataProvider dataProvider)
        {
            _dataProvider = dataProvider;
            _exportService = new coursework.Services.ExportService();
            ExportOverallReportCommand = new coursework.Commands.RelayCommand(_ =>
            {
                var snapshot = _dataProvider.GetZonesSnapshot();
                _exportService.GenerateOverallReport(snapshot);
            });
            GenerateCharts();   
        }

        public void GenerateCharts()
        {
            var zonesSnapshot = _dataProvider.GetZonesSnapshot().ToList();
            TotalRevenue = zonesSnapshot.Sum(z => z.TotalRevenue);
            TotalVisitors = zonesSnapshot.Sum(z => z.CurrentVisitors);
            RevenueByZoneChart = new SeriesCollection();
            foreach (var zone in zonesSnapshot)
            {
                if (zone.TotalRevenue > 0)
                {
                    RevenueByZoneChart.Add(new PieSeries
                    {
                        Title = zone.ZoneName,
                        Values = new ChartValues<decimal> { zone.TotalRevenue },
                        DataLabels = true
                    });
                }
            }
            var topShops = zonesSnapshot.SelectMany(z => z.ShopsData)
                                        .OrderByDescending(s => s.CurrentRevenue)
                                        .Take(5)
                                        .ToList();

            TopShopsChart = new SeriesCollection
            {
                new ColumnSeries
                {
                    Title = "Прибуток (грн)",
                    Values = new ChartValues<decimal>(topShops.Select(s => s.CurrentRevenue))
                }
            };

            TopShopsLabels = topShops.Select(s => s.ShopName).ToList();
        }
    }
}