using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ltht_project.Model
{
    internal class ProductStats
    {
        private string productId;
        private int totalPurchasedQuantity;
        private int totalSoldQuantity;
        private decimal totalPurchaseCost;
        private List<DateTime> salesDates;
        private List<PurchaseOrder> purchaseHistory;

        public ProductStats(string productId)
        {
            this.productId = productId;
            this.totalPurchasedQuantity = 0;
            this.totalSoldQuantity = 0;
            this.totalPurchaseCost = 0;
            this.salesDates = new List<DateTime>();
            this.purchaseHistory = new List<PurchaseOrder>();
        }

        public string ProductId { get => productId; set => productId = value; }
        public int TotalPurchasedQuantity { get => totalPurchasedQuantity; set => totalPurchasedQuantity = value; }
        public int TotalSoldQuantity { get => totalSoldQuantity; set => totalSoldQuantity = value; }
        public decimal TotalPurchaseCost { get => totalPurchaseCost; set => totalPurchaseCost = value; }
        public List<DateTime> SalesDates { get => salesDates; set => salesDates = value; }
        public List<PurchaseOrder> PurchaseHistory { get => purchaseHistory; set => purchaseHistory = value; }
        public int CurrentStock => totalPurchasedQuantity - totalSoldQuantity;
    }
}
