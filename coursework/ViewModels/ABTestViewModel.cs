using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows.Input;
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

        public SeriesCollection ComparisonChart { get; set; } = new SeriesCollection();
        public List<string> Labels { get; set; } = new List<string> { "К-сть закладів", "Середній чек (грн)", "Теоретичний дохід/год" };
        public Func<double, string> Formatter { get; set; } = value => value.ToString("N0");

        public ICommand LoadFile1Command { get; }
        public ICommand LoadFile2Command { get; }

        private double[] _planAStats = new double[3];
        private double[] _planBStats = new double[3];

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
                }
                else
                {
                    File2Name = Path.GetFileName(dialog.FileName);
                    _planBStats = stats;
                }
                UpdateChart();
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

        private void UpdateChart()
        {
            ComparisonChart.Clear();
            ComparisonChart.Add(new ColumnSeries
            {
                Title = "План А (" + File1Name + ")",
                Values = new ChartValues<double>(_planAStats),
                Fill = System.Windows.Media.Brushes.DodgerBlue
            });
            ComparisonChart.Add(new ColumnSeries
            {
                Title = "План Б (" + File2Name + ")",
                Values = new ChartValues<double>(_planBStats),
                Fill = System.Windows.Media.Brushes.Tomato
            });

            if (_planAStats[2] > 0 && _planBStats[2] > 0)
            {
                if (_planAStats[2] > _planBStats[2])
                    Conclusion = $"План А вигідніший! Очікуваний дохід на {Math.Round(_planAStats[2] - _planBStats[2], 2)} грн/год більший.";
                else if (_planBStats[2] > _planAStats[2])
                    Conclusion = $"План Б вигідніший! Очікуваний дохід на {Math.Round(_planBStats[2] - _planAStats[2], 2)} грн/год більший.";
                else
                    Conclusion = "Обидва плани однаково прибуткові.";
            }
        }
    }
}