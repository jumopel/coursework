using System;
using System.Collections.Generic;
using System.Text;

namespace coursework.Core
{
    public abstract class BaseShop : FestivalElement
    {
        private double _baseAttractiveness = 1.0;
        private decimal _revenue;
        private decimal _totalConsumablesCost;
        private decimal _averageCheck;
        private decimal _dailyRent;
        private decimal _staffsDailySalary;
        private double _marginMultiplier;

        private int _cashiersCount;
        private int _cooksCount;
        private int _cashierQueue;
        private int _kitchenQueue;

        private TimeSpan _foodPreparationTime;
        private TimeSpan _orderTakingTime;
        private TimeSpan _baseServiceTime;

        private int _totalCustomersServed;
        private double _totalWaitTimeMinutes;

        public double BaseAttractiveness
        {
            get => _baseAttractiveness;
            set => SetProperty(ref _baseAttractiveness, value);
        }

        public decimal AverageCheck
        {
            get => _averageCheck;
            set => SetProperty(ref _averageCheck, value);
        }
        public decimal DailyRent
        {
            get => _dailyRent;
            set => SetProperty(ref _dailyRent, value);
        }
        public decimal StaffsDailySalary
        {
            get => _staffsDailySalary;
            set => SetProperty(ref _staffsDailySalary, value);
        }
        public int CashiersCount
        {
            get => _cashiersCount;
            set => SetProperty(ref _cashiersCount, value);
        }
        public int CooksCount
        {
            get => _cooksCount;
            set => SetProperty(ref _cooksCount, value);
        }
        public TimeSpan FoodPreparationTime
        {
            get => _foodPreparationTime;
            set => SetProperty(ref _foodPreparationTime, value);
        }
        public TimeSpan OrderTakingTime
        {
            get => _orderTakingTime;
            set => SetProperty(ref _orderTakingTime, value);
        }
        public decimal Revenue
        {
            get => _revenue;
            protected set
            {
                if (SetProperty(ref _revenue, value)) OnPropertyChanged(nameof(NetProfit));
            }
        }
        public decimal TotalConsumablesCost
        {
            get => _totalConsumablesCost;
            protected set
            {
                if (SetProperty(ref _totalConsumablesCost, value)) OnPropertyChanged(nameof(NetProfit));
            }
        }
        public double MarginMultiplier { get; set; }
        public decimal NetProfit => Revenue - TotalConsumablesCost - DailyRent - StaffsDailySalary;
        public int CashierQueue
        {
            get => _cashierQueue;
            protected set
            {
                if (SetProperty(ref _cashierQueue, value))
                {
                    OnPropertyChanged(nameof(CurrentQueue));
                    OnPropertyChanged(nameof(CurrentAttractiveness));
                    OnPropertyChanged(nameof(CongestionLevel));
                }
            }
        }
        public int KitchenQueue
        {
            get => _kitchenQueue;
            protected set
            {
                if (SetProperty(ref _kitchenQueue, value))
                {
                    OnPropertyChanged(nameof(CurrentQueue));
                    OnPropertyChanged(nameof(CurrentAttractiveness));
                }
            }
        }
        public TimeSpan BaseServiceTime { get; set; }
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
        public int TotalCustomersServed
        {
            get => _totalCustomersServed;
            protected set
            {
                if (SetProperty(ref _totalCustomersServed, value)) OnPropertyChanged(nameof(AverageServiceSpeedMinutes));
            }
        }
        public double TotalWaitTimeMinutes
        {
            get => _totalWaitTimeMinutes;
            protected set
            {
                if (SetProperty(ref _totalWaitTimeMinutes, value))
                    OnPropertyChanged(nameof(AverageServiceSpeedMinutes));
            }
        }
        public double CongestionLevel
        {
            get
            {
                int activeCashiers = CashiersCount > 0 ? CashiersCount : 1;
                return (CashierQueue * OrderTakingTime.TotalMinutes) / activeCashiers;
            }
        }
        public double AverageServiceSpeedMinutes => TotalCustomersServed > 0 ? _totalWaitTimeMinutes / TotalCustomersServed : 0;
        protected BaseShop()
        {
            Revenue = 0;
            TotalConsumablesCost = 0;
            CashierQueue = 0;
            KitchenQueue = 0;
            TotalCustomersServed = 0;
            _totalWaitTimeMinutes = 0;
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
