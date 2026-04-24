using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace coursework.Core
{
    public abstract class BaseZone : FestivalElement
    {
        public ObservableCollection<BaseShop> Shops { get; private set; }
        public double Attractiveness { get; set; }
        public string Theme { get; set; } = string.Empty;
        public int Capacity { get; private set; }
        protected BaseZone()
        {
            Shops = new ObservableCollection<BaseShop>();
        }
        public void AddShop(BaseShop shop) =>  Shops.Add(shop);
        public void RemoveShop(BaseShop shop) => Shops.Remove(shop);

        //public double AverageQueueLength()
        //public decimal CalculateTotalRevenue()
        //public override double CalculateKPI()
        public override string GetKPIDescription()
        {
            return "";
        }
        public override string GetReport()
        {
            return "";
        }



    }
}
