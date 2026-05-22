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
        private int _capacity = 100;
        private double _zoneWidth = 200;
        private double _zoneHeight = 100;
       
        private CuisineType _selectedCuisine = CuisineType.Universal;

        public string ZoneName { get => _zoneName; set => SetProperty(ref _zoneName, value); }
        public int Capacity { get => _capacity; set => SetProperty(ref _capacity, value); }
        public double ZoneWidth { get => _zoneWidth; set => SetProperty(ref _zoneWidth, value); }
        public double ZoneHeight { get => _zoneHeight; set => SetProperty(ref _zoneHeight, value); }

        public CuisineType SelectedCuisine { get => _selectedCuisine; set => SetProperty(ref _selectedCuisine, value); }
        public List<CuisineType> Cuisines { get; } = Enum.GetValues(typeof(CuisineType)).Cast<CuisineType>().ToList();

        public ICommand SaveCommand { get; }
        public event Action<BaseZone>? ZoneCreated;
        public event Action? RequestClose;

        private readonly SimulationEngine _engine;

        public AddZoneViewModel(SimulationEngine engine)
        {
            _engine = engine;

            SaveCommand = new RelayCommand(_ => SaveZone(), _ =>
                !string.IsNullOrWhiteSpace(ZoneName) && Capacity > 0 && ZoneWidth > 0 && ZoneHeight > 0);
        }

        private void SaveZone()
        {
            var newZone = new BaseZone
            {
                Name = ZoneName,
                Theme = SelectedCuisine.ToString(),
                Capacity = Capacity,
                Width = ZoneWidth,
                Height = ZoneHeight,
                ZoneCuisine = SelectedCuisine
            };

            ZoneCreated?.Invoke(newZone);

            RequestClose?.Invoke();
        }
    }
}