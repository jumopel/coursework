using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Timers;
using coursework.Core;
using coursework.Models;
using System.Windows.Threading;
using System.Linq;
using System.Collections.Specialized;

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
        private readonly Dictionary<Guid, ShopState> _shopStates = new Dictionary<Guid, ShopState>();
        private readonly HashSet<Guid> _visitorsToRemove = new HashSet<Guid>();

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
            Zones.CollectionChanged += OnZonesCollectionChanged;
        }

        private void OnZonesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (BaseZone zone in e.NewItems)
                {
                    zone.Shops.CollectionChanged += OnShopsCollectionChanged;
                }
            }

            if (e.OldItems != null)
            {
                foreach (BaseZone zone in e.OldItems)
                {
                    zone.Shops.CollectionChanged -= OnShopsCollectionChanged;
                    foreach (var shop in zone.Shops)
                    {
                        _shopStates.Remove(shop.Id);
                    }
                }
            }
        }

        private void OnShopsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (BaseShop shop in e.OldItems)
                {
                    if (_shopStates.ContainsKey(shop.Id))
                    {
                        _shopStates.Remove(shop.Id);
                    }
                }
            }
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

            _visitorsToRemove.Clear();

            GenerateNewVisitors();
            ProcessVisitorDecisions();
            ProcessPhysicalMovement();      
            UpdateShopsAndZones();          
            ProcessVisitorsStochasticExit();
            for (int i = Visitors.Count - 1; i >= 0; i--)
            {
                if (_visitorsToRemove.Contains(Visitors[i].Id))
                    Visitors.RemoveAt(i);
            }

            foreach (var zone in Zones)
                zone.CurrentVisitors = 0;

            foreach (var visitor in Visitors)
            {
                if (visitor.CurrentZone != null)
                    visitor.CurrentZone.CurrentVisitors++;
            }

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
                            visitor.TargetDestination = shop;
                            visitor.State = Visitor.VisitorState.MovingToShop;
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
                    if (!_shopStates.ContainsKey(shop.Id))
                        _shopStates[shop.Id] = new ShopState();

                    var state = _shopStates[shop.Id];

                    for (int i = state.ActiveCashiersRemainingTime.Count - 1; i >= 0; i--)
                    {
                        state.ActiveCashiersRemainingTime[i] -= GameMinutesPerTick;
                        if (state.ActiveCashiersRemainingTime[i] <= 0)
                        {
                            state.ActiveCashiersRemainingTime.RemoveAt(i);
                        }
                    }

                    int availableCashiers = shop.CashiersCount - state.ActiveCashiersRemainingTime.Count;

                    if (availableCashiers > 0)
                    {
                        var waitingVisitors = Visitors
                            .Where(v => v.TargetDestination == shop && v.State == Visitor.VisitorState.Waiting)
                            .Take(availableCashiers)
                            .ToList();

                        foreach (var visitor in waitingVisitors)
                        {
                            state.ActiveCashiersRemainingTime.Add(shop.OrderTakingTime.TotalMinutes);

                            var order = visitor.MakeOrder(shop);

                            if (order.Any())
                            {
                                shop.LeaveQueue();
                                decimal orderTotal = order.Sum(p => p.Price);
                                decimal orderCost = order.Sum(p => p.CostPrice);
                                double totalPrepTime = order.Sum(p => p.PreparationTime.TotalMinutes);
                                state.PendingOrderTimes.Enqueue(totalPrepTime);
                                shop.RegisterSale(orderTotal, orderCost);
                                shop.JoinKitchenQueue();

                                visitor.State = Visitor.VisitorState.Eating;
                            }
                            else
                            {
                                shop.LeaveQueue();
                                state.ActiveCashiersRemainingTime.RemoveAt(
                                state.ActiveCashiersRemainingTime.Count - 1);
                                visitor.State = Visitor.VisitorState.Searching;
                                visitor.TargetDestination = null;
                                visitor.DecreaseSatisfaction(0.15);
                            }
                        }
                    }
                    for (int i = state.ActiveCooksRemainingTime.Count - 1; i >= 0; i--)
                    {
                         state.ActiveCooksRemainingTime[i] -= GameMinutesPerTick;

                        if (state.ActiveCooksRemainingTime[i] <= 0)
                        {
                           state.ActiveCooksRemainingTime.RemoveAt(i);
                           shop.ProcessKitchen(shop.OrderTakingTime.TotalMinutes + shop.FoodPreparationTime.TotalMinutes);
                        }
                    }
                    int availableCooks = shop.CooksCount - state.ActiveCooksRemainingTime.Count;
                    while (availableCooks > 0 && state.PendingOrderTimes.Count > 0)
                    {
                        state.ActiveCooksRemainingTime.Add(state.PendingOrderTimes.Dequeue());
                        availableCooks--;
                    }
                    
                }
            }
        }
           

        private void ProcessVisitorsStochasticExit()
        {
            foreach (var visitor in Visitors)
            {
                if (_visitorsToRemove.Contains(visitor.Id)) continue;

                if (visitor.State == Visitor.VisitorState.Leaving)
                    continue; 

                if (visitor.State != Visitor.VisitorState.Eating)
                    visitor.Hunger += 0.8;

                if (visitor.State == Visitor.VisitorState.Eating)
                {
                    visitor.EatingTimer -= GameMinutesPerTick;
                    if (visitor.EatingTimer <= 0)
                        visitor.State = visitor.Balance < 50
                            ? Visitor.VisitorState.Leaving
                            : Visitor.VisitorState.Wandering;
                    continue;
                }

                if (visitor.State == Visitor.VisitorState.Waiting
                    && visitor.TargetDestination is BaseShop waitShop
                    && waitShop.CashierQueue < 4)
                    continue;

                double exitProbability = 0.01;
                if (visitor.Hunger  > 100) exitProbability += 0.04;
                if (visitor.Balance < 20) exitProbability += 0.2;

                if (_random.NextDouble() <= exitProbability)
                {
                    visitor.State = Visitor.VisitorState.Leaving;

                    if (visitor.TargetDestination is BaseShop abandonedShop
                        && abandonedShop.CashierQueue > 0)
                        abandonedShop.LeaveQueue();

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
            private void ProcessPhysicalMovement()
            {
                const double exitGateX = 0.0;
                const double exitGateY = 0.0;
                const double mapWidth = 800.0;
                const double mapHeight = 600.0;
                const double shopRadius = 40.0;

                foreach (var visitor in Visitors)
                {
                    if (_visitorsToRemove.Contains(visitor.Id)) continue;

                    if (visitor.State == Visitor.VisitorState.Leaving)
                    {
                        MoveTowards(visitor, exitGateX, exitGateY);

                        double distToExit = Math.Sqrt(
                            Math.Pow(exitGateX - visitor.X, 2) +
                            Math.Pow(exitGateY - visitor.Y, 2));

                        if (distToExit < 5.0)
                            _visitorsToRemove.Add(visitor.Id); 

                        continue; 
                    }

                    if (visitor.State == Visitor.VisitorState.MovingToShop
                        && visitor.TargetDestination is BaseShop targetShop)
                    {
                        MoveTowards(visitor, targetShop.X, targetShop.Y);

                        double distToShop = Math.Sqrt(
                            Math.Pow(targetShop.X - visitor.X, 2) +
                            Math.Pow(targetShop.Y - visitor.Y, 2));

                        if (distToShop <= shopRadius + 2.0)
                        {
                            visitor.State = Visitor.VisitorState.Waiting;
                            targetShop.JoinQueue();
                        }
                    }
                    else if (visitor.State == Visitor.VisitorState.Wandering)
                    {
                        double distToWander = Math.Sqrt(
                            Math.Pow(visitor.WanderTargetX - visitor.X, 2) +
                            Math.Pow(visitor.WanderTargetY - visitor.Y, 2));

                        if (distToWander < 5.0
                            || (visitor.WanderTargetX == 0 && visitor.WanderTargetY == 0))
                        {
                            visitor.WanderTargetX = _random.NextDouble() * mapWidth;
                            visitor.WanderTargetY = _random.NextDouble() * mapHeight;
                        }

                        MoveTowards(visitor, visitor.WanderTargetX, visitor.WanderTargetY);
                    }

                    foreach (var zone in Zones)
                    {
                        foreach (var shop in zone.Shops)
                        {
                            if ((visitor.State == Visitor.VisitorState.MovingToShop
                                 || visitor.State == Visitor.VisitorState.Waiting)
                                && visitor.TargetDestination == shop)
                                continue;

                            double dx = visitor.X - shop.X;
                            double dy = visitor.Y - shop.Y;
                            double distance = Math.Sqrt(dx * dx + dy * dy);

                            if (distance < shopRadius)
                            {
                                if (distance == 0) { dx = 1; distance = 1; }
                                double overlap = shopRadius - distance;
                                visitor.X += (dx / distance) * overlap;
                                visitor.Y += (dy / distance) * overlap;

                                if (visitor.State == Visitor.VisitorState.Wandering)
                                {
                                    visitor.WanderTargetX = _random.NextDouble() * mapWidth;
                                    visitor.WanderTargetY = _random.NextDouble() * mapHeight;
                                }
                            }
                        }
                    }
                }
            }


        private class ShopState
        {
            public Queue<double> PendingOrderTimes { get; } = new Queue<double>();
            public List<double> ActiveCooksRemainingTime { get; } = new List<double>();
            public List<double> ActiveCashiersRemainingTime { get; } = new List<double>();
        }
    }
}