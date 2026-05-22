using coursework.Services;
using coursework.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace coursework.Views
{
    public partial class MapWindow : Window
    {
        public MapWindow(SimulationEngine engine)
        {
            InitializeComponent();
            DataContext = new MapViewModel(engine);
        }
        private void Canvas_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (DataContext is MapViewModel vm)
            {
                var pos = e.GetPosition((System.Windows.IInputElement)sender);

                if (vm.IsPlacingZone && vm.ZoneToPlace != null)
                {
                    System.Windows.Controls.Canvas.SetLeft(PlacementPreview, pos.X - vm.ZoneToPlace.Width / 2);
                    System.Windows.Controls.Canvas.SetTop(PlacementPreview, pos.Y - vm.ZoneToPlace.Height / 2);
                }
                else if (vm.IsPlacingShop && vm.ShopToPlace != null)
                {
                    System.Windows.Controls.Canvas.SetLeft(ShopPlacementPreview, pos.X - 10);
                    System.Windows.Controls.Canvas.SetTop(ShopPlacementPreview, pos.Y - 10);
                }
            }
        }

        private void Canvas_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (DataContext is MapViewModel vm)
            {
                var pos = e.GetPosition((System.Windows.IInputElement)sender);

                if (vm.IsPlacingZone && vm.ZoneToPlace != null)
                {
                    vm.ZoneToPlace.X = pos.X - vm.ZoneToPlace.Width / 2;
                    vm.ZoneToPlace.Y = pos.Y - vm.ZoneToPlace.Height / 2;
                    vm.Zones.Add(vm.ZoneToPlace);

                    vm.IsPlacingZone = false;
                    e.Handled = true; 
                }
                else if (vm.IsPlacingShop && vm.ShopToPlace != null)
                {
                    var targetZone = vm.Zones.FirstOrDefault(z =>
                        pos.X >= z.X && pos.X <= (z.X + z.Width) &&
                        pos.Y >= z.Y && pos.Y <= (z.Y + z.Height));

                    if (targetZone != null)
                    {
                        vm.ShopToPlace.X = pos.X;
                        vm.ShopToPlace.Y = pos.Y;
                        vm.ShopToPlace.ShopCuisine = targetZone.ZoneCuisine;
                        var dataService = new coursework.Services.DataService(); 
                        dataService.InitializeDefaultMenu(vm.ShopToPlace, targetZone.ZoneCuisine);

                        targetZone.AddShop(vm.ShopToPlace);

                        vm.IsPlacingShop = false;
                        e.Handled = true;
                    }
                    else
                    {
                        vm.IsPlacingShop = false;
                        e.Handled = true;

                        System.Windows.MessageBox.Show(
                            "Заклад не додано: ви клікнули повз зону. Спробуйте ще раз.",
                            "Скасовано",
                            System.Windows.MessageBoxButton.OK,
                            System.Windows.MessageBoxImage.Information);
                    }
                }
            }
        }
    }
}