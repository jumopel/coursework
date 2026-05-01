using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Timers;
using coursework.Core;
using coursework.Models;

namespace coursework.Services
{
    internal class SimulationEngine : ObservableObject
    {
        private readonly System.Timers.Timer _simulationTimer;
        private readonly Random _random = new Random();
        private const double BaseIntervalMs = 1000.0;
        public const double GameMinutesPerTick = 2.0;
        private double _timeScale = 1.0;
        private TimeSpan _elapsedGameTime = TimeSpan.Zero;
        private bool _isRunning;
        private readonly Dictionary<Guid, KitchenState> _kitchenStates = new Dictionary<Guid, KitchenState>();

        public ObservableCollection<BaseZone> Zones { get; } = new ObservableCollection<BaseZone>();
        public ObservableCollection<Visitor> Visitors { get; } = new ObservableCollection<Visitor>();

        public event Action TickCompleted;

        public double TimeScale
        {
            get => _timeScale;
            private set
            {
                if (SetProperty(ref _timeScale, value))
                {
                    UpdateTimerInterval();
                }
            }
        }

        public TimeSpan ElapsedGameTime
        {
            get => _elapsedGameTime;
            private set => SetProperty(ref _elapsedGameTime, value);
        }

        public bool IsRunning
        {
            get => _isRunning;
            private set => SetProperty(ref _isRunning, value);
        }

        public SimulationEngine()
        {
            _simulationTimer = new System.Timers.Timer(BaseIntervalMs);
            _simulationTimer.Elapsed += OnTimerElapsed;
            _simulationTimer.AutoReset = true;
        }

        private void UpdateTimerInterval()
        {
            if (_timeScale > 0)
            {
                _simulationTimer.Interval = BaseIntervalMs / _timeScale;
            }
        }

        public void Start()
        {
            if (!IsRunning)
            {
                _simulationTimer.Start();
                IsRunning = true;
            }
        }

        public void Pause()
        {
            if (IsRunning)
            {
                _simulationTimer.Stop();
                IsRunning = false;
            }
        }

        public void ProcessCommand(string command)
        {
            if (command == "START") Start();
            else if (command == "PAUSE") Pause();
            else if (command.StartsWith("SET_SPEED_"))
            {
                string speedString = command.Replace("SET_SPEED_", "");
                if (double.TryParse(speedString, out double scale) && scale > 0)
                {
                    TimeScale = scale;
                }
            }
        }

        private void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            ElapsedGameTime = ElapsedGameTime.Add(TimeSpan.FromMinutes(GameMinutesPerTick));
            UpdateShopsAndZones();
            ProcessVisitorsStochasticExit();
            TickCompleted?.Invoke();
        }

        private void UpdateShopsAndZones()
        {
            foreach (var zone in Zones)
            {
                foreach (var shop in zone.Shops)
                {
                    if (!_kitchenStates.ContainsKey(shop.Id))
                    {
                        _kitchenStates[shop.Id] = new KitchenState();
                    }
                    var kitchen = _kitchenStates[shop.Id];

                    for (int i = 0; i < shop.CashiersCount; i++)
                    {
                        if (shop.CashierQueue > 0)
                        {
                            // Отримати visitor з черги 
                            // var order = visitor.MakeOrder(shop.Menu)
                            // Розрахувати orderTotal 
                            // Розрахувати cost 
                            // Розрахувати individualPrepTime 
                            // Тимчасова  логіка 
                            decimal orderTotal = shop.AverageCheck * (decimal)(0.8 + _random.NextDouble() * 0.4);
                            decimal cost = orderTotal * 0.3m;
                            double individualPrepTime = Math.Max(1.0, shop.FoodPreparationTime.TotalMinutes);

                            kitchen.PendingOrderTimes.Enqueue(individualPrepTime);
                            shop.ProcessCashier(orderTotal, cost);
                        }
                    }

                    for (int i = kitchen.ActiveCooksRemainingTime.Count - 1; i >= 0; i--)
                    {
                        kitchen.ActiveCooksRemainingTime[i] -= GameMinutesPerTick;

                        if (kitchen.ActiveCooksRemainingTime[i] <= 0)
                        {
                            kitchen.ActiveCooksRemainingTime.RemoveAt(i);
                            shop.ProcessKitchen(GameMinutesPerTick);
                        }
                    }

                    int availableCooks = shop.CooksCount - kitchen.ActiveCooksRemainingTime.Count;
                    while (availableCooks > 0 && kitchen.PendingOrderTimes.Count > 0)
                    {
                        double nextOrderTime = kitchen.PendingOrderTimes.Dequeue();
                        kitchen.ActiveCooksRemainingTime.Add(nextOrderTime);
                        availableCooks--;
                    }
                }
            }
        }

        private void ProcessVisitorsStochasticExit()
        {
            for (int i = Visitors.Count - 1; i >= 0; i--)
            {
                var visitor = Visitors[i];
                visitor.Hunger += 0.8;

                double exitProbability = 0.01;

                if (visitor.Hunger > 100)
                {
                    exitProbability += 0.04;
                }

                if (_random.NextDouble() <= exitProbability)
                {
                    Visitors.RemoveAt(i);
                }
            }
        }

        private class KitchenState
        {
            public Queue<double> PendingOrderTimes { get; } = new Queue<double>();
            public List<double> ActiveCooksRemainingTime { get; } = new List<double>();
        }
    }
}