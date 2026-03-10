using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ltht_project.Engine
{
    internal interface IKPICalculator
    {
        int CalculateTotalSKUs();   // KPI 1: Tổng số SKU
        decimal CalculateStockValue();   // KPI 2: Tổng giá trị tồn kho
        int CalculateOutOfStock();   // KPI 3: Số sản phẩm hết hàng
        double CalculateAvgDailySales();   // KPI 4: Doanh số TB/ngày
        double CalculateAvgInventoryAge();   // KPI 5: Tuổi kho TB
    }
}
