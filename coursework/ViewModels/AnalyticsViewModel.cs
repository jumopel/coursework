using coursework.Core;
using coursework.DTO;
using DocumentFormat.OpenXml.Office2013.WebExtension;
using LiveCharts;
using LiveCharts.Wpf;
using System.Collections.Generic;
using System.Linq;

namespace coursework.ViewModels
{
    public class AnalyticsViewModel : ObservableObject
    {
        private readonly IFestivalDataProvider _dataProvider;

        public SeriesCollection RevenueByZoneChart { get; set; }
        public SeriesCollection TopShopsChart { get; set; }
        public List<string> TopShopsLabels { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TotalNetProfit => _dataProvider.GetZonesSnapshot().SelectMany(z => z.ShopsData).Sum(s => s.NetProfit);
        public int TotalVisitors { get; set; }
        public System.Windows.Input.ICommand ExportOverallReportCommand { get; }
        public System.Windows.Input.ICommand OpenAbcAnalysisCommand { get; }
        public System.Windows.Input.ICommand OpenFinancialPlanCommand { get; }
        public SeriesCollection LoadHeatmapChart { get; set; } = new SeriesCollection();
        public List<string> TimeLabels { get; set; } = new List<string> { "10:00", "12:00", "14:00", "16:00", "18:00", "20:00", "22:00" };
        public System.Collections.ObjectModel.ObservableCollection<coursework.DTO.AlertMessage> BusinessAlerts { get; set; }

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
            OpenAbcAnalysisCommand = new coursework.Commands.RelayCommand(_ =>
            {
                var snapshot = _dataProvider.GetZonesSnapshot();
                var vm = new coursework.ViewModels.AbcAnalysisViewModel(snapshot);
                var win = new coursework.Views.AbcAnalysisWindow { DataContext = vm };
                win.ShowDialog();
            });
            OpenFinancialPlanCommand = new coursework.Commands.RelayCommand(_ =>
            {
                var snapshot = _dataProvider.GetShopsSnapshot();
                var win = new coursework.Views.FinancialPlanWindow(snapshot);
                win.ShowDialog();
            });
            var snapshot = _dataProvider.GetZonesSnapshot();
            var analyzer = new coursework.Services.BusinessAnalyzerService();
            BusinessAlerts = new System.Collections.ObjectModel.ObservableCollection<coursework.DTO.AlertMessage>(analyzer.Analyze(snapshot));
            GenerateCharts();
            GenerateLoadHeatmap(snapshot);
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
        private void GenerateLoadHeatmap(IEnumerable<ZoneStateDto> snapshot)
        {
            LoadHeatmapChart.Clear();
            TimeLabels.Clear();
            var values = new ChartValues<double>();
            foreach (var zone in snapshot)
            {
                int totalQueueInZone = zone.ShopsData.Sum(s => s.CurrentQueue);
                values.Add(totalQueueInZone);
                TimeLabels.Add(zone.ZoneName);
            }

            LoadHeatmapChart.Add(new ColumnSeries
            {
                Title = "Людей у чергах",
                Values = values,
                Fill = System.Windows.Media.Brushes.Tomato,
                DataLabels = true, 
                LabelPoint = point => point.Y.ToString()
            });
        }
    }
}