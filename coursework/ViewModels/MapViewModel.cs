using System.Windows.Media.Imaging;
using coursework.Core;
using coursework.Services;

namespace coursework.ViewModels
{
    public class MapViewModel : ObservableObject
    {
        private readonly SimulationEngine _engine;
        private readonly HeatmapRenderer _renderer;
        private bool _isPlacingZone = false;
        private BaseZone? _zoneToPlace;
        private WriteableBitmap? _heatmapImage;
        private bool _isPlacingShop = false;
        private coursework.Core.BaseShop? _shopToPlace;
        private string _targetZoneName = string.Empty;
        public bool IsPlacingZone { get => _isPlacingZone; set => SetProperty(ref _isPlacingZone, value); }
        public BaseZone? ZoneToPlace { get => _zoneToPlace; set => SetProperty(ref _zoneToPlace, value); }
        public bool IsPlacingShop { get => _isPlacingShop; set => SetProperty(ref _isPlacingShop, value); }
        public coursework.Core.BaseShop? ShopToPlace { get => _shopToPlace; set => SetProperty(ref _shopToPlace, value); }
        public string TargetZoneName { get => _targetZoneName; set => SetProperty(ref _targetZoneName, value); }

        public System.Collections.ObjectModel.ObservableCollection<coursework.Core.BaseZone> Zones => _engine.Zones;
        public WriteableBitmap? HeatmapImage
        {
            get => _heatmapImage;
            private set => SetProperty(ref _heatmapImage, value);
        }

        private const int MapWidth = 800;
        private const int MapHeight = 600;

        public MapViewModel(SimulationEngine engine)
        {
            _engine = engine;
            _renderer = new HeatmapRenderer(MapWidth, MapHeight);

            _engine.TickCompleted += OnTick;

            OnTick();
        }

        private void OnTick()
        {
            HeatmapImage = _renderer.Render(_engine.Visitors, _engine.Zones);
        }
        public void ActivatePlacementMode(BaseZone newZone)
        {
            ZoneToPlace = newZone;
            IsPlacingZone = true;
        }
        public void ActivateShopPlacementMode(coursework.Core.BaseShop newShop, string zoneName)
        {
            ShopToPlace = newShop;
            TargetZoneName = zoneName;
            IsPlacingShop = true;
        }
    }
}