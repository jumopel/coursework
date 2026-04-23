using System;
using System.Collections.Generic;
using System.Text;

namespace coursework.Core
{
    public abstract class FestivalElement : ICalculatable
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Description { get; set; } = "Опис відсутній";
        public abstract string GetReport();
        public abstract double CalculateKPI();
        public abstract string GetKPIDescription();
    }
}
