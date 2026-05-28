using coursework.Core;
using coursework.DTO;
using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Threading;

namespace coursework.ViewModels
{
    public class AnalyticsViewModel : ObservableObject
    {
        private readonly IFestivalDataProvider _dataProvider;
        private readonly coursework.Services.ExportService _exportService;
        private readonly DispatcherTimer _timer;

        public SeriesCollection ZoneComparisonChart { get; set; } = new SeriesCollection();
        public List<string> ZoneLabels { get; set; } = new List<string>();

        private SeriesCollection _topShopsChart = new SeriesCollection();
        public SeriesCollection TopShopsChart { get => _topShopsChart; set => SetProperty(ref _topShopsChart, value); }

        private List<string> _topShopsLabels = new List<string>();
        public List<string> TopShopsLabels { get => _topShopsLabels; set => SetProperty(ref _topShopsLabels, value); }

        public SeriesCollection ProfitTrendChart { get; set; }
        public ChartValues<decimal> ProfitHistory { get; set; } = new ChartValues<decimal>();

        private List<string> _trendTimeLabels = new List<string>();
        public List<string> TrendTimeLabels { get => _trendTimeLabels; set => SetProperty(ref _trendTimeLabels, value); }

        public Func<double, string> Formatter { get; set; } = value => value.ToString("C0");

        private decimal _totalRevenue;
        public decimal TotalRevenue { get => _totalRevenue; set => SetProperty(ref _totalRevenue, value); }

        public decimal TotalNetProfit => _dataProvider.GetZonesSnapshot().SelectMany(z => z.ShopsData).Sum(s => s.NetProfit);

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

            ProfitTrendChart = new SeriesCollection
            {
                new LineSeries
                {
                    Title = "Чистий прибуток",
                    Values = ProfitHistory,
                    PointGeometrySize = 10,
                    LineSmoothness = 0.4, 
                    StrokeThickness = 3,
                    Stroke = System.Windows.Media.Brushes.MediumSeaGreen,
                    Fill = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(50, 60, 179, 113)), // Напівпрозорий зелений фон під лінією
                    DataLabels = true,
                    LabelPoint = point => point.Y.ToString("C0")
                }
            };

            var snapshot = _dataProvider.GetZonesSnapshot().ToList();
            GenerateZoneComparison(snapshot);
            GenerateCharts(snapshot);
            UpdateTrendChart(snapshot); 

            var analyzer = new coursework.Services.BusinessAnalyzerService();
            BusinessAlerts = new System.Collections.ObjectModel.ObservableCollection<coursework.DTO.AlertMessage>(analyzer.Analyze(snapshot));

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(2);
            _timer.Tick += (s, e) =>
            {
                var snap = _dataProvider.GetZonesSnapshot().ToList();

                GenerateZoneComparison(snap);
                GenerateCharts(snap);
                UpdateTrendChart(snap); 

                BusinessAlerts = new System.Collections.ObjectModel.ObservableCollection<coursework.DTO.AlertMessage>(analyzer.Analyze(snap));
                OnPropertyChanged(nameof(TotalNetProfit));
            };
            _timer.Start();
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

        private void UpdateTrendChart(List<ZoneStateDto> snapshot)
        {
            decimal currentProfit = snapshot.SelectMany(z => z.ShopsData).Sum(s => s.NetProfit);
            string currentTime = DateTime.Now.ToString("HH:mm:ss");

            ProfitHistory.Add(currentProfit);

            var newLabels = new List<string>(TrendTimeLabels);
            newLabels.Add(currentTime);

            if (ProfitHistory.Count > 60)
            {
                ProfitHistory.RemoveAt(0);
                newLabels.RemoveAt(0);
            }

            TrendTimeLabels = newLabels; 
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