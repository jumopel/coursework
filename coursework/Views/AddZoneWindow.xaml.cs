using System.Windows;
using coursework.ViewModels;

namespace coursework.Views
{
    public partial class AddZoneWindow : Window
    {
        public AddZoneWindow(AddZoneViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            viewModel.RequestClose += () => this.Close();
        }
    }
}