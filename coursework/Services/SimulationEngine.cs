using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Timers;
using coursework.Core;
using coursework.Models;
using System.Windows.Threading;
using System.Linq;

namespace coursework.Services
{
    internal class SimulationEngine : ObservableObject
    {
        private readonly DispatcherTimer _simulationTimer;
        private readonly Random _random = new Random();
        private const double BaseIntervalMs = 1000.0;
        public const double GameMinutesPerTick = 2.0;
        private double _timeScale = 1.0;
        private TimeSpan _elapsedGameTime = TimeSpan.Zero;
        private bool _isRunning;
        private readonly Dictionary<Guid, KitchenState> _kitchenStates = new Dictionary<Guid, KitchenState>();

        public ObservableCollection<BaseZone> Zones { get; } = new ObservableCollection<BaseZone>();
        public ObservableCollection<Visitor> Visitors { get; } = new ObservableCollection<Visitor>();

        public event Action? TickCompleted;

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
            _simulationTimer = new DispatcherTimer();
            _simulationTimer.Interval = TimeSpan.FromMilliseconds(BaseIntervalMs);
            _simulationTimer.Tick += OnTimerElapsed;
        }

        private void UpdateTimerInterval()
        {
            if (_timeScale > 0)
                _simulationTimer.Interval = TimeSpan.FromMilliseconds(BaseIntervalMs / _timeScale);
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

        private void OnTimerElapsed(object? sender, EventArgs e)
        {
            ElapsedGameTime = ElapsedGameTime.Add(TimeSpan.FromMinutes(GameMinutesPerTick));
            GenerateNewVisitors();
            ProcessVisitorDecisions();
            UpdateShopsAndZones();
            ProcessVisitorsStochasticExit();
            TickCompleted?.Invoke();
        }
        private void ProcessVisitorDecisions()
        {
            foreach (var visitor in Visitors)
            {
                if (visitor.IsHungry && (visitor.State == Visitor.VisitorState.Wandering || visitor.State == Visitor.VisitorState.Searching))
                {
                    var zone = visitor.ChooseZone(Zones);
                    if (zone != null)
                    {
                        var shop = visitor.ChooseShop(zone);
                        if (shop != null)
                        {
                            shop.JoinQueue();
                        }
                    }
                }
            }
        }
        private void UpdateShopsAndZones()
        {
            foreach (var zone in Zones)
            {
                foreach (var shop in zone.Shops)
                {
                    if (!_kitchenStates.ContainsKey(shop.Id))
                        _kitchenStates[shop.Id] = new KitchenState();

                    var kitchen = _kitchenStates[shop.Id];
                    var waitingVisitors = Visitors
                        .Where(v => v.TargetDestination == shop && v.State == Visitor.VisitorState.Waiting)
                        .Take(shop.CashiersCount) 
                        .ToList();

                    foreach (var visitor in waitingVisitors)
                    {
                        var order = visitor.MakeOrder(shop);
                        shop.LeaveQueue();
                        if (order.Any())
                        {
                            decimal orderTotal = order.Sum(p => p.Price);
                            decimal orderCost = order.Sum(p => p.CostPrice);
                            double totalPrepTime = order.Sum(p => p.PreparationTime.TotalMinutes);
                            kitchen.PendingOrderTimes.Enqueue(totalPrepTime);
                            shop.RegisterSale(orderTotal, orderCost);
                            shop.JoinKitchenQueue();
                            visitor.State = Visitor.VisitorState.Eating;
                        }
                        else
                        {
                            visitor.State = Visitor.VisitorState.Searching;
                            visitor.TargetDestination = null;
                        }
                    }
                    for (int i = kitchen.ActiveCooksRemainingTime.Count - 1; i >= 0; i--)
                    {
                        kitchen.ActiveCooksRemainingTime[i] -= GameMinutesPerTick;

                        if (kitchen.ActiveCooksRemainingTime[i] <= 0)
                        {
                            kitchen.ActiveCooksRemainingTime.RemoveAt(i);
                            shop.ProcessKitchen(shop.OrderTakingTime.TotalMinutes + shop.FoodPreparationTime.TotalMinutes);
                        }
                    }

                    int availableCooks = shop.CooksCount - kitchen.ActiveCooksRemainingTime.Count;
                    while (availableCooks > 0 && kitchen.PendingOrderTimes.Count > 0)
                    {
                        kitchen.ActiveCooksRemainingTime.Add(kitchen.PendingOrderTimes.Dequeue());
                        availableCooks--;
                    }
                }
            }
        }

        private void ProcessVisitorsStochasticExit()
        {
            const double exitGateX = 0.0;
            const double exitGateY = 0.0;

            for (int i = Visitors.Count - 1; i >= 0; i--)
            {
                var visitor = Visitors[i];
                if (visitor.State == Visitor.VisitorState.Leaving)
                {
                    MoveTowards(visitor, exitGateX, exitGateY);

                    double distToExit = Math.Sqrt(Math.Pow(exitGateX - visitor.X, 2) + Math.Pow(exitGateY - visitor.Y, 2));
                    if (distToExit < 5.0)
                    {
                        Visitors.RemoveAt(i);
                    }
                    continue;
                }
                if (visitor.State != Visitor.VisitorState.Eating) visitor.Hunger += 0.8;
                if (visitor.State == Visitor.VisitorState.Eating) continue;
                if (visitor.State == Visitor.VisitorState.Waiting && visitor.TargetDestination is BaseShop shop)
                {
                    if (shop.CashierQueue < 4)
                        continue;
                }
                double exitProbability = 0.01;
                if (visitor.Hunger > 100) exitProbability += 0.04;
                if (visitor.Balance < 20) exitProbability += 0.2;
                if (_random.NextDouble() <= exitProbability)
                {
                    visitor.State = Visitor.VisitorState.Leaving;
                    if (visitor.TargetDestination is BaseShop abandonedShop && abandonedShop.CashierQueue > 0)
                    {
                        abandonedShop.LeaveQueue();
                    }
                    visitor.TargetDestination = null;
                }
            }
        }
        private void GenerateNewVisitors()
        {
            int maxCapacity = Zones.Sum(z => z.Capacity);
            if (maxCapacity <= 0) maxCapacity = 200;
            if (Visitors.Count < maxCapacity)
            {
                int newVisitorsCount = _random.Next(0, 5);

                for (int i = 0; i < newVisitorsCount; i++)
                {
                    if (Visitors.Count >= maxCapacity) break;

                    decimal initialBalance = _random.Next(150, 5000);
                    Array diets = Enum.GetValues(typeof(DietaryType));
                    DietaryType randomDiet = _random.NextDouble() > 0.3
                        ? DietaryType.Standard
                        : (DietaryType)diets.GetValue(_random.Next(diets.Length));

                    Array cuisines = Enum.GetValues(typeof(CuisineType));

                    CuisineType randomCuisine = (CuisineType)cuisines.GetValue(_random.Next(cuisines.Length));

                    double startX = 0.0;
                    double startY = 0.0;
                    var newVisitor = new Visitor(initialBalance, randomDiet, randomCuisine, startX, startY);
                    Visitors.Add(newVisitor);
                }
            }
        }
        private void MoveTowards(Visitor visitor, double targetX, double targetY)
        {
            double dx = targetX - visitor.X;
            double dy = targetY - visitor.Y;
            double distance = Math.Sqrt(dx * dx + dy * dy);

            if (distance > 0)
            {
                double moveX = (dx / distance) * visitor.MovementSpeed;
                double moveY = (dy / distance) * visitor.MovementSpeed;
                visitor.X += moveX;
                visitor.Y += moveY;
            }
        }
        private class KitchenState
        {
            public Queue<double> PendingOrderTimes { get; } = new Queue<double>();
            public List<double> ActiveCooksRemainingTime { get; } = new List<double>();
        }
    }
}