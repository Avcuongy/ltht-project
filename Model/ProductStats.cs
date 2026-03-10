using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ltht_project.Model
{
    internal class ProductStats
    {
        private string productId;   // Mã sản phẩm
        private int totalPurchasedQuantity;   // Tổng số lượng đã nhập
        private int totalSoldQuantity;   // Tổng số lượng đã bán
        private decimal totalPurchaseCost;   // Tổng chi phí nhập hàng
        private List<DateTime> salesDates;   // Danh sách ngày có bán hàng
        private List<PurchaseOrder> purchaseHistory;   // Lịch sử nhập hàng

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
