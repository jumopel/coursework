using System.Collections.Generic;
using System.Linq;
using coursework.DTO;

namespace coursework.Services
{
    public class BusinessAnalyzerService
    {
        public List<AlertMessage> Analyze(IEnumerable<ZoneStateDto> zones)
        {
            var alerts = new List<AlertMessage>();
            var allShops = zones.SelectMany(z => z.ShopsData).ToList();

            if (!allShops.Any())
            {
                alerts.Add(new AlertMessage { Icon = "ℹ️", Title = "Немає даних", Message = "Запустіть симуляцію, щоб отримати поради.", BackgroundColor = "#E8F8F5", BorderColor = "#1ABC9C" });
                return alerts;
            }

            foreach (var shop in allShops)
            {
                if (shop.NetProfit < 0)
                {
                    alerts.Add(new AlertMessage
                    {
                        Icon = "🔴",
                        Title = "Збитковість",
                        Message = $"Заклад '{shop.ShopName}' працює в мінус ({shop.NetProfit} грн). Витрати перевищують доходи! Зменште кількість персоналу.",
                        BackgroundColor = "#FDEDEC",
                        BorderColor = "#E74C3C"
                    });
                }

                if (shop.CurrentQueue >= 5)
                {
                    alerts.Add(new AlertMessage
                    {
                        Icon = "🟠",
                        Title = "Критична черга",
                        Message = $"У закладі '{shop.ShopName}' в черзі {shop.CurrentQueue} людей. Ви втрачаєте клієнтів! Найміть додаткового кухаря.",
                        BackgroundColor = "#FEF9E7",
                        BorderColor = "#F1C40F"
                    });
                }
                if (shop.CurrentRevenue == 0)
                {
                    alerts.Add(new AlertMessage
                    {
                        Title = "Нульові продажі",
                        Message = $"Заклад '{shop.ShopName}' досі нічого не продав. Перевірте розташування або привабливість меню.",
                        Icon = "⚠️",
                        BackgroundColor = "#FEF9E7",
                        BorderColor = "#F1C40F"
                    });
                }
                var lossMakers = shop.MenuStats.Where(m => m.Price <= m.CostPrice).ToList();
                foreach (var item in lossMakers)
                {
                    alerts.Add(new AlertMessage
                    {
                        Title = "Критична ціна",
                        Message = $"У '{shop.ShopName}' страва '{item.ProductName}' продається у збиток (Собівартість: {item.CostPrice} ₴, Ціна: {item.Price} ₴).",
                        Icon = "❌",
                        BackgroundColor = "#FDEDEC",
                        BorderColor = "#E74C3C"
                    });
                }
            }

            var topShop = allShops.OrderByDescending(s => s.NetProfit).FirstOrDefault();
            if (topShop != null && topShop.NetProfit > 0)
            {
                alerts.Add(new AlertMessage
                {
                    Icon = "🟢",
                    Title = "Лідер продажів",
                    Message = $"Заклад '{topShop.ShopName}' генерує найбільший прибуток. Це ваш найуспішніший проєкт на фестивалі!",
                    BackgroundColor = "#EAFAF1",
                    BorderColor = "#2ECC71"
                });
            }

            return alerts;
        }
    }
}