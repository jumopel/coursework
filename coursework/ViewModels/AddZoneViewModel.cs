using coursework.Commands;
using coursework.Core;
using coursework.Models;
using coursework.Services;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace coursework.ViewModels
{
    public class AddZoneViewModel : ObservableObject
    {
        private string _zoneName = "Нова зона";
        private string _theme = "Загальна тематика";
        private string _selectedTypeStr = "Фуд-Корт";
        private CuisineType _selectedCuisine = CuisineType.Universal;
        private int _capacity = 100;
        private double _zoneWidth = 200;
        private double _zoneHeight = 200;
        public string ZoneName { get => _zoneName; set => SetProperty(ref _zoneName, value); }
        public string Theme { get => _theme; set => SetProperty(ref _theme, value); }
        public int Capacity { get => _capacity; set => SetProperty(ref _capacity, value); }
        public double ZoneWidth { get => _zoneWidth; set => SetProperty(ref _zoneWidth, value); }
        public double ZoneHeight { get => _zoneHeight; set => SetProperty(ref _zoneHeight, value); }
        public CuisineType SelectedCuisine
        {
            get => _selectedCuisine;
            set => SetProperty(ref _selectedCuisine, value);
        }
        public List<CuisineType> Cuisines { get; } = Enum.GetValues(typeof(CuisineType)).Cast<CuisineType>().ToList();

        public List<string> ZoneTypes { get; } = new List<string> { "Фуд-Корт", "Лаунж/Chill Зона" };
        public string SelectedTypeStr { get => _selectedTypeStr; set => SetProperty(ref _selectedTypeStr, value); }

        public ICommand SaveCommand { get; }
        public event Action? RequestClose;

        private readonly SimulationEngine _engine;

        public AddZoneViewModel(SimulationEngine engine)
        {
            _engine = engine;

            SaveCommand = new RelayCommand(_ => SaveZone(), _ =>
                !string.IsNullOrWhiteSpace(ZoneName) &&
                !string.IsNullOrWhiteSpace(Theme) &&
                Capacity > 0);
        }

        private void SaveZone()
        {
            BaseZone newZone;

            if (SelectedTypeStr == "Фуд-Корт")
                newZone = new FoodZone { Name = ZoneName, Theme = SelectedCuisine.ToString(), Capacity = Capacity, Width = ZoneWidth, Height = ZoneHeight, ZoneCuisine = SelectedCuisine };
            else
                newZone = new ChillZone { Name = ZoneName, Theme = SelectedCuisine.ToString(), Capacity = Capacity, Width = ZoneWidth, Height = ZoneHeight, ZoneCuisine = SelectedCuisine };

            _engine.Zones.Add(newZone);
            RequestClose?.Invoke();
        }
    }
}