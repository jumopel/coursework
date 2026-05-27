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
        public ICommand HireCashierCommand { get; }
        public ICommand HireCookCommand { get; }
        public ICommand OpenAddZoneCommand { get; }
        public ICommand DeleteZoneCommand { get; }
        public ICommand OpenMenuCommand { get; }
        public ICommand SaveMapCommand { get; }
        public ICommand LoadMapCommand { get; }
        public System.Windows.Input.ICommand ImportCsvAndAnalyzeCommand { get; }
        public System.Windows.Input.ICommand OpenAnalyticsCommand { get; }

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
        public System.Windows.Input.ICommand ImportApiCommand { get; }

        public MainViewModel()
        {
            _engine = new SimulationEngine();
            _dataProvider = new SimulationDataProvider(_engine);
            _dataService = new DataService();
            OpenMenuCommand = new RelayCommand(param => OpenMenu(param as coursework.DTO.ShopStateDto)); 
            StartCommand = new RelayCommand(_ =>
            {
                _dataProvider.SendCommand("START");
                UpdateSnapshot(); 
            }, _ => !IsSimulationRunning);

            PauseCommand = new RelayCommand(_ =>
            {
                _dataProvider.SendCommand("PAUSE");
                UpdateSnapshot(); 
            }, _ => IsSimulationRunning);
            SetSpeed1XCommand = new RelayCommand(_ => ChangeSpeed(1.0));
            SetSpeed2XCommand = new RelayCommand(_ => ChangeSpeed(2.0));
            SetSpeed4XCommand = new RelayCommand(_ => ChangeSpeed(4.0));
            HireCashierCommand = new RelayCommand(param => HireStaff(param as coursework.DTO.ShopStateDto, true));
            HireCookCommand = new RelayCommand(param => HireStaff(param as coursework.DTO.ShopStateDto, false));
            OpenMapCommand = new RelayCommand(_ =>
            {
                var mapWindow = new coursework.Views.MapWindow(_engine);
                mapWindow.Show();
            });
            OpenAddShopCommand = new RelayCommand(_ =>
            {
                var addShopVm = new AddShopViewModel();
                var window = new coursework.Views.AddShopWindow(addShopVm);
                window.Owner = System.Windows.Application.Current.MainWindow;

                addShopVm.ShopCreated += (newShop) =>
                {
                    var mapVm = new MapViewModel(_engine);
                    mapVm.ActivateShopPlacementMode(newShop); 

                    var mapWindow = new coursework.Views.MapWindow(_engine)
                    {
                        DataContext = mapVm,
                        Owner = System.Windows.Application.Current.MainWindow
                    };

                    mapWindow.ShowDialog();
                    UpdateSnapshot(); 
                };

                window.ShowDialog();
            });
            SaveMapCommand = new RelayCommand(_ => SaveMap());
            LoadMapCommand = new RelayCommand(_ => LoadMap(), _ => !_engine.IsRunning); 
            OpenAddZoneCommand = new RelayCommand(_ =>
            {
                var addZoneVm = new AddZoneViewModel(_engine);
                var window = new coursework.Views.AddZoneWindow(addZoneVm);
                window.Owner = System.Windows.Application.Current.MainWindow;

                addZoneVm.ZoneCreated += (newZone) =>
                {
                    var mapVm = new MapViewModel(_engine);
                    mapVm.ActivatePlacementMode(newZone);

                    var mapWindow = new coursework.Views.MapWindow(_engine);
                    mapWindow.DataContext = mapVm; 
                    mapWindow.Owner = System.Windows.Application.Current.MainWindow;

                    mapWindow.ShowDialog(); 

                    UpdateSnapshot();
                };

                window.ShowDialog(); 
            });
            OpenAnalyticsCommand = new coursework.Commands.RelayCommand(_ => OpenAnalytics());
            DeleteZoneCommand = new RelayCommand(param => DeleteZone(param as coursework.DTO.ZoneStateDto));
            ImportCsvAndAnalyzeCommand = new coursework.Commands.RelayCommand(_ => ImportCsvAndAnalyze());
            ImportApiCommand = new coursework.Commands.RelayCommand(_ => ImportFromApi());
            _dataProvider.DataUpdated += OnSimulationDataUpdated;
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

            if (Zones.Count == 0 || Zones.Count != freshZonesSnapshot.Count ||
                Zones.Any(z => z.ShopsData.Count != freshZonesSnapshot.First(f => f.ZoneId == z.ZoneId).ShopsData.Count))
            {
                Zones.Clear();
                foreach (var zoneDto in freshZonesSnapshot) Zones.Add(zoneDto);
                return;
            }

            for (int i = 0; i < Zones.Count; i++)
            {
                var currentZone = Zones[i];

                var freshZone = freshZonesSnapshot.FirstOrDefault(z => z.ZoneId == currentZone.ZoneId);
                if (freshZone != null)
                {
                    currentZone.CurrentVisitors = freshZone.CurrentVisitors;
                    currentZone.OccupancyRate = freshZone.OccupancyRate;
                    currentZone.TotalRevenue = freshZone.TotalRevenue;

                    for (int j = 0; j < currentZone.ShopsData.Count; j++)
                    {
                        var currentShop = currentZone.ShopsData[j];

                        var freshShop = freshZone.ShopsData.FirstOrDefault(s => s.ShopId == currentShop.ShopId);
                        if (freshShop != null)
                        {
                            currentShop.CurrentQueue = freshShop.CurrentQueue;
                            currentShop.CongestionLevel = freshShop.CongestionLevel;
                            currentShop.Attractiveness = freshShop.Attractiveness;
                            currentShop.CurrentRevenue = freshShop.CurrentRevenue;
                            currentShop.CashiersCount = freshShop.CashiersCount;
                            currentShop.CooksCount = freshShop.CooksCount;
                        }
                    }
                }
            }
        }


        private void HireStaff(coursework.DTO.ShopStateDto? dto, bool isCashier)
        {
            if (dto == null) return;

            var shop = _engine.Zones.SelectMany(z => z.Shops).FirstOrDefault(s => s.Id == dto.ShopId);
            if (shop != null)
            {
                if (isCashier)
                {
                    shop.CashiersCount++;
                    shop.StaffsDailySalary += 500;
                }
                else
                {
                    shop.CooksCount++;
                    shop.StaffsDailySalary += 700;
                }
                UpdateSnapshot();
            }
        }
        private void DeleteZone(coursework.DTO.ZoneStateDto? dto)
        {
            if (dto == null) return;

            var zoneToRemove = _engine.Zones.FirstOrDefault(z => z.Id == dto.ZoneId);
            if (zoneToRemove != null)
            {
                _engine.Zones.Remove(zoneToRemove);
                UpdateSnapshot();
            }
        }
        private void OpenMenu(coursework.DTO.ShopStateDto? dto)
        {
            if (dto == null) return;

            var shop = _engine.Zones.SelectMany(z => z.Shops).FirstOrDefault(s => s.Id == dto.ShopId);
            if (shop != null)
            {
                var vm = new EditMenuViewModel(shop);
                var window = new coursework.Views.EditMenuWindow
                {
                    DataContext = vm,
                    Owner = System.Windows.Application.Current.MainWindow
                };
                window.ShowDialog();
            }
        }
        private void SaveMap()
        {
            var saveService = new coursework.Services.SaveLoadService();
            saveService.SaveMapToFile(_engine.Zones);
        }

        private void LoadMap()
        {
            var saveService = new coursework.Services.SaveLoadService();
            var loadedZones = saveService.LoadMapFromFile();

            if (loadedZones != null)
            {
                _engine.Zones.Clear();
                foreach (var zone in loadedZones)
                {
                    _engine.Zones.Add(zone);
                }
                UpdateSnapshot();
                System.Windows.MessageBox.Show("Карту успішно завантажено! Можете відкривати редактор.", "Успіх", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
            }
        }
        private void OpenAnalytics()
        {
            UpdateSnapshot();

            var analyticsVm = new AnalyticsViewModel(_dataProvider);
            var window = new coursework.Views.AnalyticsWindow
            {
                DataContext = analyticsVm,
                Owner = System.Windows.Application.Current.MainWindow
            };
            window.ShowDialog();
        }
        private void ImportCsvAndAnalyze()
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Title = "Імпорт касового звіту",
                Filter = "CSV Files (*.csv)|*.csv",
                DefaultExt = ".csv"
            };

            if (dialog.ShowDialog() == true)
            {
                var csvProvider = new coursework.Services.CsvDataProvider(dialog.FileName);

                var analyticsVm = new AnalyticsViewModel(csvProvider);
                var window = new coursework.Views.AnalyticsWindow
                {
                    DataContext = analyticsVm,
                    Owner = System.Windows.Application.Current.MainWindow
                };
                window.ShowDialog();
            }
        }
        private async void ImportFromApi()
        {
            string apiUrl = "https://api.gastro-metric.com/v1/festival/live-stats";// недійсний URL, замініть на реальний

            var apiProvider = new coursework.Services.ApiDataProvider(apiUrl);

            await apiProvider.LoadDataFromApiAsync();

            if (apiProvider.GetZonesSnapshot().Any())
            {
                var analyticsVm = new AnalyticsViewModel(apiProvider);
                var window = new coursework.Views.AnalyticsWindow
                {
                    DataContext = analyticsVm,
                    Owner = System.Windows.Application.Current.MainWindow
                };
                window.ShowDialog();
            }
        }
    }
}