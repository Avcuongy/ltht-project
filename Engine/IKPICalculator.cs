using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ltht_project.Engine
{
    internal interface IKPICalculator
    {
        int CalculateTotalSKUs();
        decimal CalculateStockValue();
        int CalculateOutOfStock();
        double CalculateAvgDailySales();
        double CalculateAvgInventoryAge();
    }
}
