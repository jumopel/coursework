using System;
using System.Collections.Generic;
using System.Text;

namespace coursework.Core
{
    public abstract class BaseShop : FestivalElement
    {
        public decimal Revenue { get; protected set; } 
        public decimal CostPrice { get; protected set; }
        public decimal AverageCheck { get; set; } 
        public double MarginMultiplier { get; set; }
        public decimal NetProfit => Revenue - CostPrice;
        public int CurrentQueue { get; protected set; }
        public int CooksCount { get; set; }
        public TimeSpan BaseServiceTime { get; set; }
        public double BaseAttractiveness { get; set; } = 1.0;
        public double CurrentAttractiveness
        {
            get
            {
                int activeCooks = CooksCount > 0 ? CooksCount : 1;
                double estimatedWaitTimeMinutes = (CurrentQueue * BaseServiceTime.TotalMinutes) / activeCooks;
                double penalty = estimatedWaitTimeMinutes * 0.025;
                double actual = BaseAttractiveness - penalty;
                return actual > 0.3 ? actual : 0.3;
            }
        }
        public int TotalCustomersServed { get; protected set; } 
        public double TotalWaitTimeMinutes { get; protected set; }
        public double AverageServiceSpeedMinutes => TotalCustomersServed > 0 ? TotalWaitTimeMinutes / TotalCustomersServed : 0;
        protected BaseShop()
        {
            Revenue = 0;
            CostPrice = 0;
            CurrentQueue = 0;
            TotalCustomersServed = 0;
            TotalWaitTimeMinutes = 0;
        }
    }
}
