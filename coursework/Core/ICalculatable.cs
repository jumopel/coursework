using System;
using System.Collections.Generic;
using System.Text;

namespace coursework.Core
{
    internal interface ICalculatable
    {
        double CalculateKPI(); 
        string GetKPIDescription();
    }
}
