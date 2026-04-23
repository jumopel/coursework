using System;
using System.Collections.Generic;
using System.Text;

namespace coursework.Core
{
    public interface ICalculatable
    {
        double CalculateKPI(); 
        string GetKPIDescription();
    }
}
