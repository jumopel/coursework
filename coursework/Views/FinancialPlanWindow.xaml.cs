using System.Collections.Generic;
using System.Windows;
using coursework.DTO;

namespace coursework.Views
{
    public partial class FinancialPlanWindow : Window
    {
        public IEnumerable<ShopStateDto> ShopsData { get; set; }

        public FinancialPlanWindow(IEnumerable<ShopStateDto> shopsData)
        {
            InitializeComponent();
            ShopsData = shopsData;
            DataContext = this; 
        }
    }
}