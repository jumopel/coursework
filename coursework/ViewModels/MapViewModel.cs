using System.Windows.Media.Imaging;
using coursework.Core;
using coursework.Services;

namespace coursework.ViewModels
{
    public class MapViewModel : ObservableObject
    {
        private readonly SimulationEngine _engine;
        private readonly HeatmapRenderer _renderer;

        private WriteableBitmap? _heatmapImage;

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
    }
}