using System;
using System.Collections.Generic;
using System.Text;

namespace coursework.Core
{
    public abstract class BaseShop : FestivalElement
    {
        public double BaseAttractiveness { get; set; } = 1.0;

        public decimal Revenue { get; protected set; } 
        public decimal TotalConsumablesCost { get; protected set; }
        public decimal AverageCheck { get; set; }
        public decimal DailyRent { get; set; }
        public decimal StaffsDailySalary { get; set; }
        public double MarginMultiplier { get; set; }
        public decimal NetProfit => Revenue - TotalConsumablesCost - DailyRent - StaffsDailySalary;


        public int CashiersCount { get; set; }
        public int CooksCount { get; set; }

        public int CashierQueue { get; protected set; }
        public int KitchenQueue { get; protected set; }

        public TimeSpan BaseServiceTime { get; set; }
        public TimeSpan FoodPreparationTime { get; set; }
        public TimeSpan OrderTakingTime { get; set; }

        public int CurrentQueue => CashierQueue + KitchenQueue;
        public double CurrentAttractiveness
        {
            get
            {
                int activeCooks = CooksCount > 0 ? CooksCount : 1;
                int activeCashiers = CashiersCount > 0 ? CashiersCount : 1;

                double waitAtCashier = (CashierQueue * OrderTakingTime.TotalMinutes) / activeCashiers;
                double waitAtKitchen = (KitchenQueue * FoodPreparationTime.TotalMinutes) / activeCooks;
                double totalEstimatedWait = waitAtCashier + waitAtKitchen;
                double penalty = totalEstimatedWait * 0.02;
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
            TotalConsumablesCost = 0;
            CashierQueue = 0;
            KitchenQueue = 0;
            TotalCustomersServed = 0;
            TotalWaitTimeMinutes = 0;
        }
        public virtual void JoinQueue()
        {
            CashierQueue++;
        }
        public virtual void ProcessCashier(decimal orderTotal, decimal consumableCost)
        {
            if (CashierQueue > 0)
            {
                CashierQueue--; 
                Revenue += orderTotal;
                TotalConsumablesCost += consumableCost; 
                KitchenQueue++;
            }
        }
        public virtual void ProcessKitchen(double actualWaitTimeMinutes)
        {
            if (KitchenQueue > 0)
            {
                KitchenQueue--; 
                TotalCustomersServed++; 
                TotalWaitTimeMinutes += actualWaitTimeMinutes; 
            }
        }
        public override double CalculateKPI()
        {
            if (TotalCustomersServed == 0 || NetProfit <= 0)
                return 0;
            double profitPerCustomer = (double)(NetProfit / TotalCustomersServed);
            double kpi = (profitPerCustomer * CurrentAttractiveness) / (1 + AverageServiceSpeedMinutes * 0.1);

            return Math.Round(kpi, 2);
        }
        public override string GetKPIDescription()
        {
            return $"Показник ефективності (KPI: {CalculateKPI()}) розраховується як " +
                   $"відношення чистого середнього прибутку на одного клієнта до середньої швидкості " +
                   $"обслуговування ({Math.Round(AverageServiceSpeedMinutes, 1)} хв), помножене на привабливість.";
        }

        public override string GetReport()
        {
            return $"Магазин: '{Name}'. " +
                   $"Чистий прибуток: {NetProfit} грн. " +
                   $"Обслуговано: {TotalCustomersServed} осіб. " +
                   $"Зараз у черзі: {CurrentQueue} (Каса: {CashierQueue}, Видача: {KitchenQueue}). " +
                   $"Привабливість: {Math.Round(CurrentAttractiveness, 2)}.";
        }
    }
}
