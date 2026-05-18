using System.Windows;
using coursework.Services;
using coursework.ViewModels;

namespace coursework.Views
{
    public partial class MapWindow : Window
    {
        public MapWindow(SimulationEngine engine)
        {
            InitializeComponent();
            DataContext = new MapViewModel(engine);
        }
    }
}