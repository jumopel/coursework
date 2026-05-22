using coursework.Commands;
using coursework.Core;
using coursework.DTO;
using coursework.Models;
using coursework.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using static coursework.Core.BaseShop;

namespace coursework.ViewModels
{
    public class MainViewModel : ObservableObject
    {
        private readonly SimulationEngine _engine;
        private readonly SimulationDataProvider _dataProvider;
        private readonly DataService _dataService;

        private string _elapsedTimeText = "00:00";
        private bool _isSimulationRunning;
        private double _currentSpeed = 1.0;

        public ObservableCollection<ZoneStateDto> Zones { get; } = new ObservableCollection<ZoneStateDto>();
        public ICommand OpenMapCommand { get; }
        public ICommand OpenAddShopCommand { get; }

        public string ElapsedTimeText
        {
            get => _elapsedTimeText;
            set => SetProperty(ref _elapsedTimeText, value);
        }

        public bool IsSimulationRunning
        {
            get => _isSimulationRunning;
            set => SetProperty(ref _isSimulationRunning, value);
        }

        public double CurrentSpeed
        {
            get => _currentSpeed;
            set => SetProperty(ref _currentSpeed, value);
        }

        public ICommand StartCommand { get; }
        public ICommand PauseCommand { get; }
        public ICommand SetSpeed1XCommand { get; }
        public ICommand SetSpeed2XCommand { get; }
        public ICommand SetSpeed4XCommand { get; }

        public MainViewModel()
        {
            _engine = new SimulationEngine();
            _dataProvider = new SimulationDataProvider(_engine);
            _dataService = new DataService();
            StartCommand = new RelayCommand(_ => _dataProvider.SendCommand("START"), _ => !IsSimulationRunning);
            PauseCommand = new RelayCommand(_ => _dataProvider.SendCommand("PAUSE"), _ => IsSimulationRunning);
            SetSpeed1XCommand = new RelayCommand(_ => ChangeSpeed(1.0));
            SetSpeed2XCommand = new RelayCommand(_ => ChangeSpeed(2.0));
            SetSpeed4XCommand = new RelayCommand(_ => ChangeSpeed(4.0));
            OpenMapCommand = new RelayCommand(_ =>
            {
                var mapWindow = new coursework.Views.MapWindow(_engine);
                mapWindow.Show();
            });
            OpenAddShopCommand = new RelayCommand(_ =>
            {
                var addShopVm = new AddShopViewModel(_engine.Zones);
                var window = new coursework.Views.AddShopWindow(addShopVm);
                window.Owner = System.Windows.Application.Current.MainWindow;
                window.ShowDialog();
            });
            _dataProvider.DataUpdated += OnSimulationDataUpdated;
            InitializeDemoData();
            UpdateSnapshot();
        }

        private void ChangeSpeed(double speed)
        {
            _dataProvider.SendCommand($"SET_SPEED_{speed}");
            CurrentSpeed = speed;
        }

        private void OnSimulationDataUpdated()
        {
            UpdateSnapshot();
        }

        private void UpdateSnapshot()
        {
            IsSimulationRunning = _engine.IsRunning;
            ElapsedTimeText = $"{(int)_engine.ElapsedGameTime.TotalHours:00}:{_engine.ElapsedGameTime.Minutes:00}";
            var freshZonesSnapshot = _dataProvider.GetZonesSnapshot().ToList();
            Zones.Clear();
            foreach (var zoneDto in freshZonesSnapshot)
            {
                Zones.Add(zoneDto);
            }
        }

        private void InitializeDemoData()
        {
            var foodZone = _dataService.CreateZoneFromUser(
                name: "Центральний Фуд-Корт",
                theme: "Американська та Італійська кухня",
                capacity: 120,
                cuisine: CuisineType.Universal
            );

            var burgerShop = _dataService.CreateShop(
                name: "Бургерна 'Краш'",
                cashiers: 2,
                cooks: 3,
                rent: 600,
                salary: 350,
                orderTakingMinutes: 1.5,
                prepMinutes: 4.0,
                x: 150,
                y: 200,
                type: ShopType.FastFood
            );

            _dataService.AddProductToShop(burgerShop, "Краш Бургер XXL", 145, 55, TimeSpan.FromMinutes(3), ProductCategory.MainCourse, DietaryType.Standard, CuisineType.American);
            _dataService.AddProductToShop(burgerShop, "Картопля Фрі", 65, 18, TimeSpan.FromMinutes(2), ProductCategory.Snack, DietaryType.Vegetarian, CuisineType.Universal);
            _dataService.AddProductToShop(burgerShop, "Фірмовий Лимонад", 45, 12, TimeSpan.FromMinutes(0.5), ProductCategory.Drink, DietaryType.Standard, CuisineType.Universal);

            var pizzaShop = _dataService.CreateShop(
                name: "Піцерія 'Vicini'",
                cashiers: 1,
                cooks: 2,
                rent: 900,
                salary: 450,
                orderTakingMinutes: 2.5,
                prepMinutes: 8.0,
                x: 400,
                y: 200,
                type: ShopType.Restaurant
            );

            _dataService.AddProductToShop(pizzaShop, "Піца Маргарита", 300, 85, TimeSpan.FromMinutes(6), ProductCategory.MainCourse, DietaryType.Vegetarian, CuisineType.Italian);
            _dataService.AddProductToShop(pizzaShop, "Піца Пепероні", 340, 110, TimeSpan.FromMinutes(7), ProductCategory.MainCourse, DietaryType.Standard, CuisineType.Italian);
            _dataService.AddProductToShop(pizzaShop, "Сік прямого віджиму", 100, 20, TimeSpan.FromMinutes(1), ProductCategory.Drink, DietaryType.Standard, CuisineType.Universal);
            foodZone.AddShop(burgerShop);
            foodZone.AddShop(pizzaShop);
            _engine.Zones.Add(foodZone);
        }
    }
}