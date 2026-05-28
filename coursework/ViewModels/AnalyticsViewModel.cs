using coursework.Core;
using coursework.DTO;
using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;

namespace coursework.ViewModels
{
    public class AnalyticsViewModel : ObservableObject
    {
        private readonly IFestivalDataProvider _dataProvider;
        private readonly coursework.Services.ExportService _exportService;

        public SeriesCollection ZoneComparisonChart { get; set; } = new SeriesCollection();
        public List<string> ZoneLabels { get; set; } = new List<string>();

        private SeriesCollection _topShopsChart = new SeriesCollection();
        public SeriesCollection TopShopsChart { get => _topShopsChart; set => SetProperty(ref _topShopsChart, value); }

        private List<string> _topShopsLabels = new List<string>();
        public List<string> TopShopsLabels { get => _topShopsLabels; set => SetProperty(ref _topShopsLabels, value); }
        public SeriesCollection LoadHeatmapChart { get; set; } = new SeriesCollection();
        public List<string> TimeLabels { get; set; } = new List<string>();

        public Func<double, string> Formatter { get; set; } = value => value.ToString("C0");

        public decimal TotalNetProfit => _dataProvider.GetZonesSnapshot().SelectMany(z => z.ShopsData).Sum(s => s.NetProfit);

        private decimal _totalRevenue;
        public decimal TotalRevenue { get => _totalRevenue; set => SetProperty(ref _totalRevenue, value); }

        private int _totalVisitors;
        public int TotalVisitors { get => _totalVisitors; set => SetProperty(ref _totalVisitors, value); }

        private System.Collections.ObjectModel.ObservableCollection<coursework.DTO.AlertMessage> _businessAlerts;
        public System.Collections.ObjectModel.ObservableCollection<coursework.DTO.AlertMessage> BusinessAlerts
        {
            get => _businessAlerts;
            set => SetProperty(ref _businessAlerts, value);
        }
        public System.Windows.Input.ICommand ExportOverallReportCommand { get; }
        public System.Windows.Input.ICommand OpenAbcAnalysisCommand { get; }
        public System.Windows.Input.ICommand OpenFinancialPlanCommand { get; }

        public AnalyticsViewModel(IFestivalDataProvider dataProvider)
        {
            _dataProvider = dataProvider;
            _exportService = new coursework.Services.ExportService();

            ExportOverallReportCommand = new coursework.Commands.RelayCommand(_ =>
            {
                var snap = _dataProvider.GetZonesSnapshot();
                _exportService.GenerateOverallReport(snap);
            });

            OpenAbcAnalysisCommand = new coursework.Commands.RelayCommand(_ =>
            {
                var vm = new coursework.ViewModels.AbcAnalysisViewModel(_dataProvider);
                var win = new coursework.Views.AbcAnalysisWindow { DataContext = vm };
                win.Closed += (s, args) => vm.StopTimer();

                win.ShowDialog();
            });

            OpenFinancialPlanCommand = new coursework.Commands.RelayCommand(_ =>
            {
                var win = new coursework.Views.FinancialPlanWindow(_dataProvider);
                win.ShowDialog();
            });

            var snapshot = _dataProvider.GetZonesSnapshot().ToList();

            GenerateZoneComparison(snapshot);
            GenerateLoadHeatmap(snapshot);
            GenerateCharts(snapshot);

            var analyzer = new coursework.Services.BusinessAnalyzerService();
            BusinessAlerts = new System.Collections.ObjectModel.ObservableCollection<coursework.DTO.AlertMessage>(analyzer.Analyze(snapshot));

            var timer = new System.Windows.Threading.DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(2); 
            timer.Tick += (s, e) =>
            {
                var snap = _dataProvider.GetZonesSnapshot().ToList();

                GenerateZoneComparison(snap);
                GenerateLoadHeatmap(snap);
                GenerateCharts(snap); 

                var analyzer = new coursework.Services.BusinessAnalyzerService();
                BusinessAlerts = new System.Collections.ObjectModel.ObservableCollection<coursework.DTO.AlertMessage>(analyzer.Analyze(snap));

                OnPropertyChanged(nameof(TotalNetProfit));
            };
            timer.Start();
        }

        private void GenerateCharts(List<ZoneStateDto> zonesSnapshot)
        {
            TotalRevenue = zonesSnapshot.Sum(z => z.TotalRevenue);
            TotalVisitors = zonesSnapshot.Sum(z => z.CurrentVisitors);

            var topShops = zonesSnapshot.SelectMany(z => z.ShopsData)
                                        .OrderByDescending(s => s.CurrentRevenue)
                                        .Take(5)
                                        .ToList();

            TopShopsChart.Clear();
            TopShopsChart.Add(new ColumnSeries
            {
                Title = "Прибуток (грн)",
                Values = new ChartValues<decimal>(topShops.Select(s => s.CurrentRevenue)),
                DataLabels = true,
                LabelPoint = point => point.Y.ToString("C0")
            });

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

        private void GenerateZoneComparison(IEnumerable<ZoneStateDto> snapshot)
        {
            ZoneComparisonChart.Clear();
            ZoneLabels.Clear();

            var revenues = new ChartValues<double>();
            var expenses = new ChartValues<double>();
            var profits = new ChartValues<double>();

            foreach (var zone in snapshot)
            {
                ZoneLabels.Add(zone.ZoneName);

                double rev = (double)zone.ShopsData.Sum(s => s.CurrentRevenue);
                double prof = (double)zone.ShopsData.Sum(s => s.NetProfit);
                double exp = rev - prof;

                revenues.Add(rev);
                expenses.Add(exp);
                profits.Add(prof);
            }

            ZoneComparisonChart.Add(new ColumnSeries { Title = "Виручка", Values = revenues, Fill = System.Windows.Media.Brushes.DodgerBlue });
            ZoneComparisonChart.Add(new ColumnSeries { Title = "Витрати", Values = expenses, Fill = System.Windows.Media.Brushes.Tomato });
            ZoneComparisonChart.Add(new ColumnSeries { Title = "Прибуток", Values = profits, Fill = System.Windows.Media.Brushes.MediumSeaGreen });
        }
    }
}