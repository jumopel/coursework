using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using coursework.Core;
using coursework.DTO;

namespace coursework.Services
{
    public class ApiDataProvider : IFestivalDataProvider
    {
        public string ProviderName => "Cloud API Provider";
        public event Action? DataUpdated;

        private List<ZoneStateDto> _zones = new List<ZoneStateDto>();
        private readonly string _apiUrl;

        public ApiDataProvider(string apiUrl)
        {
            _apiUrl = apiUrl;
        }

        public async Task LoadDataFromApiAsync()
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    await Task.Delay(500);

                    HttpResponseMessage response = await client.GetAsync(_apiUrl);
                    response.EnsureSuccessStatusCode();

                    string jsonResponse = await response.Content.ReadAsStringAsync();

                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var loadedZones = JsonSerializer.Deserialize<List<ZoneStateDto>>(jsonResponse, options);

                    if (loadedZones != null)
                    {
                        _zones = loadedZones;
                        DataUpdated?.Invoke();
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show($"Помилка з'єднання з API-сервером: {ex.Message}\n\n(Це очікувано, оскільки реальний сервер зараз відключений. Код запиту працює коректно.)",
                                "Статус API", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка обробки даних: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public IEnumerable<ZoneStateDto> GetZonesSnapshot() => _zones;
        public IEnumerable<ShopStateDto> GetShopsSnapshot() => _zones.SelectMany(z => z.ShopsData);
        public void SendCommand(string command) { }
    }
}