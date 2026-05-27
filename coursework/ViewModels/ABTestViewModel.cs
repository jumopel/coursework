using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows.Input;
using System.Windows.Media;
using LiveCharts;
using LiveCharts.Wpf;
using coursework.Commands;
using coursework.Core;

namespace coursework.ViewModels
{
    public class ABTestViewModel : ObservableObject
    {
        private string _file1Name = "Файл 1 не вибрано";
        public string File1Name { get => _file1Name; set => SetProperty(ref _file1Name, value); }

        private string _file2Name = "Файл 2 не вибрано";
        public string File2Name { get => _file2Name; set => SetProperty(ref _file2Name, value); }

        private string _conclusion = "Завантажте два файли для порівняння";
        public string Conclusion { get => _conclusion; set => SetProperty(ref _conclusion, value); }

        private Brush _conclusionBackground = Brushes.White;
        public Brush ConclusionBackground { get => _conclusionBackground; set => SetProperty(ref _conclusionBackground, value); }

        private Brush _conclusionBorder = new SolidColorBrush(Color.FromRgb(189, 195, 199));
        public Brush ConclusionBorder { get => _conclusionBorder; set => SetProperty(ref _conclusionBorder, value); }

        private Brush _conclusionForeground = new SolidColorBrush(Color.FromRgb(127, 140, 141));
        public Brush ConclusionForeground { get => _conclusionForeground; set => SetProperty(ref _conclusionForeground, value); }

        public SeriesCollection ShopsChart { get; set; } = new SeriesCollection();
        public SeriesCollection TicketChart { get; set; } = new SeriesCollection();
        public SeriesCollection RevenueChart { get; set; } = new SeriesCollection();

        public List<string> PlanLabels { get; set; } = new List<string> { "А", "Б" };
        public Func<double, string> IntFormatter { get; set; } = v => v.ToString("N0");
        public Func<double, string> MoneyFormatter { get; set; } = v => v.ToString("N0") + " ₴";

        private string _shopsA = "—"; public string ShopsA { get => _shopsA; set => SetProperty(ref _shopsA, value); }
        private string _shopsB = "—"; public string ShopsB { get => _shopsB; set => SetProperty(ref _shopsB, value); }
        private string _shopsDiff = "—"; public string ShopsDiff { get => _shopsDiff; set => SetProperty(ref _shopsDiff, value); }
        private Brush _shopsDiffColor = Brushes.Gray; public Brush ShopsDiffColor { get => _shopsDiffColor; set => SetProperty(ref _shopsDiffColor, value); }

        private string _ticketA = "—"; public string TicketA { get => _ticketA; set => SetProperty(ref _ticketA, value); }
        private string _ticketB = "—"; public string TicketB { get => _ticketB; set => SetProperty(ref _ticketB, value); }
        private string _ticketDiff = "—"; public string TicketDiff { get => _ticketDiff; set => SetProperty(ref _ticketDiff, value); }
        private Brush _ticketDiffColor = Brushes.Gray; public Brush TicketDiffColor { get => _ticketDiffColor; set => SetProperty(ref _ticketDiffColor, value); }

        private string _revenueA = "—"; public string RevenueA { get => _revenueA; set => SetProperty(ref _revenueA, value); }
        private string _revenueB = "—"; public string RevenueB { get => _revenueB; set => SetProperty(ref _revenueB, value); }
        private string _revenueDiff = "—"; public string RevenueDiff { get => _revenueDiff; set => SetProperty(ref _revenueDiff, value); }
        private Brush _revenueDiffColor = Brushes.Gray; public Brush RevenueDiffColor { get => _revenueDiffColor; set => SetProperty(ref _revenueDiffColor, value); }

        public ICommand LoadFile1Command { get; }
        public ICommand LoadFile2Command { get; }

        private double[] _planAStats = new double[3];
        private double[] _planBStats = new double[3];
        private bool _plan1Loaded = false;
        private bool _plan2Loaded = false;

        public ABTestViewModel()
        {
            LoadFile1Command = new RelayCommand(_ => LoadFile(1));
            LoadFile2Command = new RelayCommand(_ => LoadFile(2));
        }

        private void LoadFile(int fileNumber)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog { Filter = "JSON Files (*.json)|*.json" };
            if (dialog.ShowDialog() == true)
            {
                var stats = AnalyzePlan(dialog.FileName);
                if (fileNumber == 1)
                {
                    File1Name = Path.GetFileName(dialog.FileName);
                    _planAStats = stats;
                    _plan1Loaded = true;
                }
                else
                {
                    File2Name = Path.GetFileName(dialog.FileName);
                    _planBStats = stats;
                    _plan2Loaded = true;
                }
                UpdateCharts();
            }
        }

        private double[] AnalyzePlan(string filePath)
        {
            try
            {
                string json = File.ReadAllText(filePath);
                using (JsonDocument doc = JsonDocument.Parse(json))
                {
                    int totalShops = 0;
                    double totalMenuPrice = 0;
                    int menuItemsCount = 0;
                    double hourlyRevenuePotential = 0;

                    foreach (var zone in doc.RootElement.EnumerateArray())
                    {
                        if (zone.TryGetProperty("Shops", out JsonElement shops))
                        {
                            totalShops += shops.GetArrayLength();
                            foreach (var shop in shops.EnumerateArray())
                            {
                                int cooks = shop.TryGetProperty("CooksCount", out var c) ? c.GetInt32() : 1;

                                double totalShopTime = 0;
                                double totalShopPrice = 0;
                                int menuCount = 0;

                                if (shop.TryGetProperty("Menu", out JsonElement menu))
                                {
                                    foreach (var item in menu.EnumerateArray())
                                    {
                                        double price = item.TryGetProperty("Price", out var p) ? p.GetDouble() : 0;
                                        double time = item.TryGetProperty("PrepTime", out var t) ? t.GetDouble() : 5.0;

                                        totalShopPrice += price;
                                        totalShopTime += time;
                                        menuCount++;
                                    }
                                }

                                totalMenuPrice += totalShopPrice;
                                menuItemsCount += menuCount;

                                if (menuCount > 0)
                                {
                                    double avgTime = totalShopTime / menuCount;
                                    double avgPrice = totalShopPrice / menuCount;
                                    hourlyRevenuePotential += (60.0 / avgTime) * cooks * avgPrice;
                                }
                            }
                        }
                    }

                    double avgTicket = menuItemsCount > 0 ? totalMenuPrice / menuItemsCount : 0;
                    return new double[] { totalShops, avgTicket, hourlyRevenuePotential };
                }
            }
            catch
            {
                return new double[] { 0, 0, 0 };
            }
        }

        private void UpdateCharts()
        {
            var colorA = new SolidColorBrush(Color.FromRgb(52, 152, 219));   
            var colorB = new SolidColorBrush(Color.FromRgb(231, 76, 60));    

            // Графік закладів
            ShopsChart.Clear();
            ShopsChart.Add(new ColumnSeries { Title = "А", Values = new ChartValues<double> { _planAStats[0] }, Fill = colorA, DataLabels = true, LabelPoint = p => p.Y.ToString("N0") });
            ShopsChart.Add(new ColumnSeries { Title = "Б", Values = new ChartValues<double> { _planBStats[0] }, Fill = colorB, DataLabels = true, LabelPoint = p => p.Y.ToString("N0") });

            // Графік середнього чека
            TicketChart.Clear();
            TicketChart.Add(new ColumnSeries { Title = "А", Values = new ChartValues<double> { _planAStats[1] }, Fill = colorA, DataLabels = true, LabelPoint = p => p.Y.ToString("N0") + " ₴" });
            TicketChart.Add(new ColumnSeries { Title = "Б", Values = new ChartValues<double> { _planBStats[1] }, Fill = colorB, DataLabels = true, LabelPoint = p => p.Y.ToString("N0") + " ₴" });

            // Графік доходу
            RevenueChart.Clear();
            RevenueChart.Add(new ColumnSeries { Title = "А", Values = new ChartValues<double> { _planAStats[2] }, Fill = colorA, DataLabels = true, LabelPoint = p => p.Y.ToString("N0") + " ₴" });
            RevenueChart.Add(new ColumnSeries { Title = "Б", Values = new ChartValues<double> { _planBStats[2] }, Fill = colorB, DataLabels = true, LabelPoint = p => p.Y.ToString("N0") + " ₴" });

            // Таблиця
            ShopsA = _planAStats[0].ToString("N0");
            ShopsB = _planBStats[0].ToString("N0");
            UpdateDiff(_planAStats[0], _planBStats[0], v => v.ToString("N0"),
                out string sd, out Brush sc); ShopsDiff = sd; ShopsDiffColor = sc;

            TicketA = _planAStats[1].ToString("N2") + " ₴";
            TicketB = _planBStats[1].ToString("N2") + " ₴";
            UpdateDiff(_planAStats[1], _planBStats[1], v => v.ToString("N2") + " ₴",
                out string td, out Brush tc); TicketDiff = td; TicketDiffColor = tc;

            RevenueA = _planAStats[2].ToString("N0") + " ₴";
            RevenueB = _planBStats[2].ToString("N0") + " ₴";
            UpdateDiff(_planAStats[2], _planBStats[2], v => v.ToString("N0") + " ₴",
                out string rd, out Brush rc); RevenueDiff = rd; RevenueDiffColor = rc;

            if (_plan1Loaded && _plan2Loaded)
            {
                double a = _planAStats[2], b = _planBStats[2];
                if (a > b)
                {
                    Conclusion = $"✅ План А вигідніший! Дохід більший на {Math.Round(a - b, 0):N0} ₴/год ({Math.Round((a - b) / b * 100, 1)}%)";
                    ConclusionBackground = new SolidColorBrush(Color.FromRgb(234, 250, 241));
                    ConclusionBorder = new SolidColorBrush(Color.FromRgb(46, 204, 113));
                    ConclusionForeground = new SolidColorBrush(Color.FromRgb(39, 174, 96));
                }
                else if (b > a)
                {
                    Conclusion = $"✅ План Б вигідніший! Дохід більший на {Math.Round(b - a, 0):N0} ₴/год ({Math.Round((b - a) / a * 100, 1)}%)";
                    ConclusionBackground = new SolidColorBrush(Color.FromRgb(253, 237, 236));
                    ConclusionBorder = new SolidColorBrush(Color.FromRgb(231, 76, 60));
                    ConclusionForeground = new SolidColorBrush(Color.FromRgb(192, 57, 43));
                }
                else
                {
                    Conclusion = "⚖️ Обидва плани однаково прибуткові.";
                    ConclusionBackground = new SolidColorBrush(Color.FromRgb(234, 250, 241));
                    ConclusionBorder = new SolidColorBrush(Color.FromRgb(46, 204, 113));
                    ConclusionForeground = new SolidColorBrush(Color.FromRgb(39, 174, 96));
                }
            }
        }

        private void UpdateDiff(double a, double b, Func<double, string> fmt, out string text, out Brush color)
        {
            if (!_plan1Loaded || !_plan2Loaded) { text = "—"; color = Brushes.Gray; return; }
            double diff = b - a;
            if (diff > 0) { text = "+" + fmt(diff); color = new SolidColorBrush(Color.FromRgb(39, 174, 96)); }
            else if (diff < 0) { text = fmt(diff); color = new SolidColorBrush(Color.FromRgb(192, 57, 43)); }
            else { text = "="; color = Brushes.Gray; }
        }
    }
}