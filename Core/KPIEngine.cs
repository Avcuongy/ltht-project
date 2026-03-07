using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ltht_project.Model;

namespace ltht_project.Core
{
    internal class KPIEngine : IKPICalculator
    {
        private ConcurrentDictionary<string, ProductStats> productStatsDict;
        public ConcurrentDictionary<string, ProductStats> ProductStatsDict { get => productStatsDict; set => productStatsDict = value; }

        public KPIEngine()
        {
            ProductStatsDict = new ConcurrentDictionary<string, ProductStats>();
        }

        public void ProcessPurchaseOrder(PurchaseOrder order)
        {
            var stats = ProductStatsDict.GetOrAdd(order.ProductId, new ProductStats(order.ProductId));

            lock (stats)
            {
                stats.TotalPurchasedQuantity += order.Quantity;
                stats.TotalPurchaseCost += order.Quantity * order.UnitCost;
                stats.PurchaseHistory.Add(order);
            }
        }

        public void ProcessInvoice(Invoice invoice)
        {
            var stats = ProductStatsDict.GetOrAdd(invoice.ProductId, new ProductStats(invoice.ProductId));

            lock (stats)
            {
                stats.TotalSoldQuantity += invoice.Quantity;
                if (!stats.SalesDates.Contains(invoice.InvoiceDate.Date))
                {
                    stats.SalesDates.Add(invoice.InvoiceDate.Date);
                }
            }
        }

        public int CalculateTotalSKUs()
        {
            return ProductStatsDict.Keys.Count;
        }

        public decimal CalculateStockValue()
        {
            decimal totalValue = 0;

            foreach (var stats in ProductStatsDict.Values)
            {
                int unsoldQuantity = stats.CurrentStock;

                if (unsoldQuantity > 0 && stats.PurchaseHistory.Count > 0)
                {
                    decimal avgUnitCost = stats.TotalPurchaseCost / stats.TotalPurchasedQuantity;
                    totalValue += unsoldQuantity * avgUnitCost;
                }
            }

            return totalValue;
        }

        public int CalculateOutOfStock()
        {
            return ProductStatsDict.Values.Count(stats => stats.CurrentStock <= 0);
        }

        public double CalculateAvgDailySales()
        {
            var allSalesDates = ProductStatsDict.Values
                .SelectMany(stats => stats.SalesDates)
                .Distinct()
                .ToList();

            if (allSalesDates.Count == 0)
            {
                return 0;
            }

            int totalQuantitySold = ProductStatsDict.Values.Sum(stats => stats.TotalSoldQuantity);
            int numberOfSalesDays = allSalesDates.Count;

            return (double)totalQuantitySold / numberOfSalesDays;
        }

        public double CalculateAvgInventoryAge()
        {
            var unsoldItems = ProductStatsDict.Values
                .Where(stats => stats.CurrentStock > 0 && stats.PurchaseHistory.Count > 0)
                .ToList();

            if (unsoldItems.Count == 0)
            {
                return 0;
            }

            DateTime currentDate = DateTime.Now;
            double totalAge = 0;
            int totalUnsoldQuantity = 0;

            foreach (var stats in unsoldItems)
            {
                int unsoldQuantity = stats.CurrentStock;

                var sortedPurchases = stats.PurchaseHistory
                    .OrderBy(p => p.PurchaseDate)
                    .ToList();

                int remainingUnsold = unsoldQuantity;

                foreach (var purchase in sortedPurchases)
                {
                    if (remainingUnsold <= 0) break;

                    int quantityFromThisPurchase = Math.Min(remainingUnsold, purchase.Quantity);
                    double ageInDays = (currentDate - purchase.PurchaseDate).TotalDays;

                    totalAge += ageInDays * quantityFromThisPurchase;
                    totalUnsoldQuantity += quantityFromThisPurchase;
                    remainingUnsold -= quantityFromThisPurchase;
                }
            }

            return totalUnsoldQuantity > 0 ? totalAge / totalUnsoldQuantity : 0;
        }

        public ConcurrentDictionary<string, ProductStats> GetProductStats()
        {
            return ProductStatsDict;
        }
    }
}
