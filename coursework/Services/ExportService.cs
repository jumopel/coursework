using System;
using System.Collections.Generic;
using System.Linq;
using ClosedXML.Excel;
using Microsoft.Win32;
using coursework.DTO;

namespace coursework.Services
{
    public class ExportService
    {
        public void GenerateOverallReport(IEnumerable<ZoneStateDto> zonesSnapshot)
        {
            var dialog = new SaveFileDialog
            {
                Title = "Зберегти розширений звіт",
                Filter = "Excel Files (*.xlsx)|*.xlsx",
                DefaultExt = ".xlsx",
                FileName = $"_Аналітика_{DateTime.Now:yyyyMMdd_HHmm}"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    using (var workbook = new XLWorkbook())
                    {
                        var wsZones = workbook.Worksheets.Add("Статистика Зон");
                        wsZones.Cell("A1").Value = "Фінансовий звіт по зонах фестивалю";
                        wsZones.Range("A1:G1").Merge().Style.Font.SetBold().Font.SetFontSize(16).Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

                        var headersZ = new[] { "Назва Зони", "Тематика", "К-сть Закладів", "Відвідувачів", "Загальний Дохід", "Загальні Витрати", "Чистий Прибуток" };
                        for (int i = 0; i < headersZ.Length; i++) wsZones.Cell(3, i + 1).Value = headersZ[i];
                        wsZones.Range("A3:G3").Style.Font.SetBold().Fill.SetBackgroundColor(XLColor.LightGray);

                        int rowZ = 4;
                        foreach (var zone in zonesSnapshot)
                        {
                            decimal zoneExpenses = zone.ShopsData.Sum(s => s.TotalExpenses);
                            decimal zoneNetProfit = zone.ShopsData.Sum(s => s.NetProfit);

                            wsZones.Cell(rowZ, 1).Value = zone.ZoneName;
                            wsZones.Cell(rowZ, 2).Value = zone.Theme.ToString();
                            wsZones.Cell(rowZ, 3).Value = zone.ShopsData.Count;
                            wsZones.Cell(rowZ, 4).Value = zone.CurrentVisitors;
                            wsZones.Cell(rowZ, 5).Value = zone.TotalRevenue;
                            wsZones.Cell(rowZ, 6).Value = zoneExpenses;
                            wsZones.Cell(rowZ, 7).Value = zoneNetProfit;

                            if (zoneNetProfit < 0) wsZones.Cell(rowZ, 7).Style.Font.SetFontColor(XLColor.Red);
                            else if (zoneNetProfit > 0) wsZones.Cell(rowZ, 7).Style.Font.SetFontColor(XLColor.Green);

                            rowZ++;
                        }
                        wsZones.Columns().AdjustToContents();

                        var wsShops = workbook.Worksheets.Add("Аналітика Закладів");
                        wsShops.Cell("A1").Value = "Детальна бізнес-аналітика кожного закладу";
                        wsShops.Range("A1:J1").Merge().Style.Font.SetBold().Font.SetFontSize(16).Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

                        var headersS = new[] { "Заклад", "Зона", "Дохід (грн)", "Чистий Прибуток", "К-сть Замовлень", "Середній Чек", "Сер. Час Очікування (хв)", "Топ Страва", "Аутсайдер", "Персонал (Кас/Кух)" };
                        for (int i = 0; i < headersS.Length; i++) wsShops.Cell(3, i + 1).Value = headersS[i];
                        wsShops.Range("A3:J3").Style.Font.SetBold().Fill.SetBackgroundColor(XLColor.LightBlue);

                        int rowS = 4;
                        foreach (var zone in zonesSnapshot)
                        {
                            foreach (var shop in zone.ShopsData)
                            {
                                wsShops.Cell(rowS, 1).Value = shop.ShopName;
                                wsShops.Cell(rowS, 2).Value = zone.ZoneName;
                                wsShops.Cell(rowS, 3).Value = shop.CurrentRevenue;
                                wsShops.Cell(rowS, 4).Value = shop.NetProfit;
                                wsShops.Cell(rowS, 5).Value = shop.TotalOrders;
                                wsShops.Cell(rowS, 6).Value = shop.AverageTicket;
                                wsShops.Cell(rowS, 7).Value = shop.AverageWaitTimeMinutes;
                                wsShops.Cell(rowS, 8).Value = shop.TopDishName;
                                wsShops.Cell(rowS, 9).Value = shop.WorstDishName;
                                wsShops.Cell(rowS, 10).Value = $"{shop.CashiersCount} / {shop.CooksCount}";

                                if (shop.NetProfit < 0) wsShops.Cell(rowS, 4).Style.Font.SetFontColor(XLColor.Red);
                                else if (shop.NetProfit > 0) wsShops.Cell(rowS, 4).Style.Font.SetFontColor(XLColor.Green);

                                rowS++;
                            }
                        }
                        wsShops.Columns().AdjustToContents();

                        workbook.SaveAs(dialog.FileName);
                    }

                    System.Windows.MessageBox.Show("Розширений звіт успішно згенеровано! Перевірте ваші показники.", "Успіх",
                        System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show($"Помилка при створенні звіту: {ex.Message}", "Помилка",
                        System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                }
            }
        }
    }
}