using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Threading;
using coursework.Core;
using coursework.DTO;

namespace coursework.Views
{
    public partial class FinancialPlanWindow : Window
    {
        private readonly IFestivalDataProvider _dataProvider;
        private readonly DispatcherTimer _timer;

        public ObservableCollection<ShopStateDto> ShopsData { get; set; }

        public FinancialPlanWindow(IFestivalDataProvider dataProvider)
        {
            InitializeComponent();
            _dataProvider = dataProvider;

            ShopsData = new ObservableCollection<ShopStateDto>(_dataProvider.GetShopsSnapshot());
            DataContext = this;

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(2);
            _timer.Tick += (s, e) => UpdateData();
            _timer.Start();

            this.Closed += (s, e) => _timer.Stop();
        }

        private void UpdateData()
        {
            var freshData = _dataProvider.GetShopsSnapshot();

            ShopsData.Clear();
            foreach (var shop in freshData)
            {
                ShopsData.Add(shop);
            }
        }
    }
}