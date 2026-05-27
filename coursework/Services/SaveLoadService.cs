using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Win32;
using coursework.Core;

namespace coursework.Services
{
    public class SaveLoadService
    {
        private readonly JsonSerializerOptions _options = new JsonSerializerOptions
        {
            WriteIndented = true,
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            PropertyNameCaseInsensitive = true
        };

        public void SaveMapToFile(ObservableCollection<BaseZone> zones)
        {
            var dialog = new SaveFileDialog
            {
                Title = "Зберегти карту фестивалю",
                Filter = "GastroMetric Map (*.json)|*.json",
                DefaultExt = ".json",
                FileName = "MyFestivalMap"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    string jsonString = JsonSerializer.Serialize(zones, _options);
                    File.WriteAllText(dialog.FileName, jsonString);
                    System.Windows.MessageBox.Show("Карту успішно збережено!", "Успіх", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show($"Помилка при збереженні: {ex.Message}", "Помилка", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                }
            }
        }

        public ObservableCollection<BaseZone>? LoadMapFromFile()
        {
            var dialog = new OpenFileDialog
            {
                Title = "Завантажити карту фестивалю",
                Filter = "GastroMetric Map (*.json)|*.json",
                DefaultExt = ".json"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    string jsonString = File.ReadAllText(dialog.FileName);
                    var loadedZones = JsonSerializer.Deserialize<ObservableCollection<BaseZone>>(jsonString, _options);
                    return loadedZones;
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show($"Помилка при завантаженні: {ex.Message}", "Помилка", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                }
            }
            return null;
        }
    }
}