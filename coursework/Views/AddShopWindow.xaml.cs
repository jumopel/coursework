using System.Windows;
using coursework.ViewModels;

namespace coursework.Views
{
    public partial class AddShopWindow : Window
    {
        public AddShopWindow(AddShopViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;

            viewModel.RequestClose += () => this.Close();
        }
    }
}