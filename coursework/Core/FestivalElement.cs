using System;
using System.Collections.Generic;
using System.Text;

namespace coursework.Core
{
    public abstract class FestivalElement : ObservableObject, ICalculatable
    {
        private string _name = string.Empty;
        private string _description = string.Empty;
        public Guid Id { get; set; } = Guid.NewGuid();
        public double X { get; set; }
        public double Y { get; set; }
        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }
        public abstract string GetReport();
        public abstract double CalculateKPI();
        public abstract string GetKPIDescription();
    }
}
